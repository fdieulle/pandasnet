using Python.Runtime;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PandasNet
{
    public static class NumpyCodec
    {
        public static void Initialize(Codecs codecs)
        {
            // .Net -> Python
            codecs.Register<sbyte[]>(a => EncodeFast(a, Encode));
            codecs.Register<short[]>(a => EncodeFast(a, Encode));
            codecs.Register<int[]>(a => EncodeFast(a, Encode));
            codecs.Register<long[]>(a => EncodeFast(a, Encode));
            codecs.Register<bool[]>(a => EncodeFast(a, Encode));
            codecs.Register<byte[]>(a => EncodeFast(a, Encode));
            codecs.Register<ushort[]>(a => EncodeFast(a, Encode));
            codecs.Register<uint[]>(a => EncodeFast(a, Encode));
            codecs.Register<ulong[]>(a => EncodeFast(a, Encode));
            codecs.Register<float[]>(a => EncodeFast(a, Encode));
            codecs.Register<double[]>(a => EncodeFast(a, Encode));
            codecs.Register<DateTime[]>(a => EncodeFast(a, Encode));
            codecs.Register<TimeSpan[]>(a => EncodeFast(a, Encode));
            codecs.Register<string[]>(a => EncodeSafe(a));

            // Python -> .Net
            codecs.Register("<class 'numpy.ndarray'>", Decode);
            codecs.Register("<class 'numpy.bool_'>", p => p.As<bool>());
            codecs.Register("<class 'numpy.uint8'>", p => p.As<byte>());
            codecs.Register("<class 'numpy.uint16'>", p => p.As<ushort>());
            codecs.Register("<class 'numpy.uint32'>", p => p.As<uint>());
            codecs.Register("<class 'numpy.uint64'>", p => p.As<ulong>());
            codecs.Register("<class 'numpy.int8'>", p => p.As<sbyte>());
            codecs.Register("<class 'numpy.int16'>", p => p.As<short>());
            codecs.Register("<class 'numpy.int32'>", p => p.As<int>());
            codecs.Register("<class 'numpy.int64'>", p => p.As<long>());
            codecs.Register("<class 'numpy.float32'>", p => p.As<float>());
            codecs.Register("<class 'numpy.float64'>", p => p.As<double>());
        }

        #region Encode 

        private static PyObject EncodeSafe<T>(this T[] array, Func<T, PyObject> select = null)
        {
            select = select ?? (p => p.ToPython());
            using (var scope = Py.CreateScope())
            {
                var list = new PyList(array.Select(select).ToArray());
                dynamic numpy = scope.Import("numpy");
                return numpy.array(list);
            }
        }

        private static PyObject EncodeFast<T>(T[] array, Action<T[], IntPtr> encode)
        {
            using (var scope = Py.CreateScope())
            {
                scope.Import("numpy");
                scope.Exec($"a = numpy.empty({array.Length}, dtype='{GetDType<T>()}')");
                var py = scope.Get("a");
                if (array.Length == 0) return py;

                using (var arrayInterface = py.GetAttr("__array_interface__"))
                using (var data = arrayInterface.GetItem("data"))
                using (var address = data.GetItem(0))
                    encode(array, new IntPtr(address.As<long>()));

                return py;
            }
        }

        private static unsafe void Encode<T>(T[] array, IntPtr ptr) where T : unmanaged
        {
            var size = sizeof(T) * array.Length;
            fixed (T* src = array)
                Buffer.MemoryCopy(src, (T*)ptr, size, size);
        }

        private static unsafe void Encode(DateTime[] array, IntPtr ptr)
        {
            var dst = (long*)ptr;

            fixed (DateTime* ptSrc = array)
            {
                var src = ptSrc;
                var end = src + array.Length;
                var deltaTicksToUtc = src->ToUniversalTime().Ticks - src->Ticks;
                var nextDayTicks = src->Date.AddDays(1).Ticks;
                while (src < end)
                {
                    // This is enough to detect the day change to handle DST changes without hit performances
                    if (src->Ticks >= nextDayTicks)
                    {
                        deltaTicksToUtc = src->ToUniversalTime().Ticks - src->Ticks;
                        nextDayTicks = src->Date.AddDays(1).Ticks;
                    }
                    
                    *dst = (src->Ticks + deltaTicksToUtc - Time.OriginTicks) / 10;
                    ++src; ++dst;
                }
            }
        }

        private static unsafe void Encode(TimeSpan[] array, IntPtr ptr)
        {
            var dst = (long*)ptr;
            
            fixed(TimeSpan* ptSrc = array)
            {
                var src = ptSrc;
                var end = src + array.Length;
                while (src < end)
                {
                    *dst = src->Ticks / 10;
                    ++src; ++dst;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string PyCreateNumpyArray<T>(int length)
            => $"a = numpy.empty({length}, dtype='{GetDType<T>()}')";

        // Should support constant propagation mechanism
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetDType<T>()
        {
            if (typeof(T) == typeof(sbyte))
                return "int8";
            if (typeof(T) == typeof(short))
                return "int16";
            if (typeof(T) == typeof(int))
                return "int32";
            if (typeof(T) == typeof(long))
                return "int64";
            if (typeof(T) == typeof(byte))
                return "uint8"; 
            if (typeof(T) == typeof(ushort))
                return "uint16";
            if (typeof(T) == typeof(uint))
                return "uint32";
            if (typeof(T) == typeof(ulong))
                return "uint64";
            if (typeof(T) == typeof(float))
                return "float32";
            if (typeof(T) == typeof(double))
                return "float64";
            if (typeof(T) == typeof(DateTime))
                return "datetime64[us]";
            if (typeof(T) == typeof(TimeSpan))
                return "timedelta64[us]";
            if (typeof(T) == typeof(string))
                return "str";
            if (typeof(T) == typeof(bool))
                return "bool_";
            // Todo: maybe 'V' for raw data (void) based on custom struct
            return "O";
        }

        #endregion

        #region Decode

        public static object Decode(PyObject py)
        {
            // Let's aasume that when python use big endian, dotnet also and respectively with little endian
            var dType = py.GetAttr("dtype");
            var kind = dType.GetAttr("kind").As<char>();
            var itemSize = dType.GetAttr("itemsize").As<int>();
            
            switch(kind)
            {
                case 'i': // integer
                    switch (itemSize)
                    {
                        case 1: // sbyte
                            return py.DecodeFast<sbyte>(Decode);
                        case 2: // short
                            return py.DecodeFast<short>(Decode);
                        case 4: // int
                            return py.DecodeFast<int>(Decode);
                        case 8: // long
                            return py.DecodeFast<long>(Decode);
                    }
                    break;
                case 'b': // boolean
                    return py.DecodeFast<bool>(Decode);
                case 'u': // unsigned integer
                    switch (itemSize)
                    {
                        case 1: // byte
                            return py.DecodeFast<byte>(Decode);
                        case 2: // ushort
                            return py.DecodeFast<ushort>(Decode);
                        case 4: // uint
                            return py.DecodeFast<uint>(Decode);
                        case 8: // ulong
                            return py.DecodeFast<ulong>(Decode);
                    }
                    break;
                case 'f': // floating number
                    switch (itemSize)
                    {
                        case 4: // float
                            return py.DecodeFast<float>(Decode);
                        case 8: // double
                            return py.DecodeFast<double>(Decode);
                    }
                    break;
                case 'c': // complex number
                    break;
                case 'm': // timedelta
                    return py.DecodeFast<TimeSpan>((p, a) => Decode(p, dType.GetTimeFactor(), a));
                case 'M': // datetime
                    return py.DecodeFast<DateTime>((p, a) => Decode(p, dType.GetTimeFactor(), a));
                case 'O': // object
                    return py.DecodeSafe<string>(); // Todo: do better here if it's not necessary a string
                case 'S': // string
                    return py.DecodeSafe<string>(); // Obsolete since python 3
                case 'U': // string as unicode
                    return py.DecodeSafe<string>();
                case 'V': // fixed chunk of memory (void)
                    break;
            }

            throw new NotSupportedException($"numpy dtypr: {dType}, Kind: {kind}, ItemSize: {itemSize} is not supported");
        }

        private static T[] DecodeSafe<T>(this PyObject py, Func<PyObject, T> select = null)
        {
            var length = py.Length();
            var result = new T[length];
            if (length == 0) return result;

            select = select ?? (p => p.As<T>());
            for (var i = 0; i < length; i++)
                result[i] = select(py.GetItem(i));

            return result;
        }

        private static T[] DecodeFast<T>(this PyObject py, Action<IntPtr, T[]> decode)
        {
            var length = py.Length();
            var result = new T[length];
            if (length == 0) return result;

            using (var arrayInterface = py.GetAttr("__array_interface__"))
            using (var data = arrayInterface.GetItem("data"))
            using (var address = data.GetItem(0))
                decode(new IntPtr(address.As<long>()), result);

            return result;
        }

        private static unsafe void Decode<T>(IntPtr ptr, T[] result) where T : unmanaged
        {
            var src = (T*)ptr;
            var size = sizeof(T) * result.Length;
            fixed (T* dst = result)
                Buffer.MemoryCopy(src, dst, size, size);
        }

        private static unsafe void Decode(IntPtr ptr, double factor, DateTime[] result)
        {
            var src = (long*)ptr;
            fixed (DateTime* ptDst = result)
            {
                var dst = (long*)ptDst;
                var end = src + result.Length;

                while(src<end)
                {
                    *dst = ((long)(*src * factor) + Time.OriginTicks) | Time.KindUtcTicks;
                    ++src; ++dst;
                }
            }
        }

        private static unsafe void Decode(IntPtr ptr, double factor, TimeSpan[] result)
        {
            var src = (long*)ptr;
            fixed (TimeSpan* ptDst = result)
            {
                var dst = (long*)ptDst;
                var end = src + result.Length;

                while (src < end)
                {
                    *dst = (long)(*src * factor);
                    ++src; ++dst;
                }
            }
        }

        private static double GetTimeFactor(this PyObject dType)
        {
            dynamic numpy = Py.Import("numpy");
            var data = numpy.datetime_data(dType);
            var unit = data[0].As<string>();
            var count = data[1].As<int>();
            switch (unit)
            {
                case "W":
                    return Time.TICKS_BY_WEEK * count;
                case "D":
                    return Time.TICKS_BY_DAY * count;
                case "h":
                    return Time.TICKS_BY_HOUR * count;
                case "m":
                    return Time.TICKS_BY_MIN * count;
                case "s":
                    return Time.TICKS_BY_SEC * count;
                case "ms":
                    return Time.TICKS_BY_MS * count;
                case "us":
                    return Time.TICKS_BY_US * count;
                case "ns":
                    return Time.TICKS_BY_NS * count;
                default:
                    throw new NotSupportedException($"time unit {unit} isn't supported");
            }
        }

        #endregion
    }
}
