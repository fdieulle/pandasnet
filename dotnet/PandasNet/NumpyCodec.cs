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

        private static PyObject EncodeFast<T>(T[] array, Action<T[], UIntPtr> encode)
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
                    encode(array, new UIntPtr(address.As<ulong>()));

                return py;
            }
        }

        private static unsafe void Encode<T>(T[] array, UIntPtr ptr) where T : unmanaged
        {
            var size = sizeof(T) * array.Length;
            fixed (T* src = array)
                Buffer.MemoryCopy(src, (T*)ptr, size, size);
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
                return "timedelta64[us]";
            if (typeof(T) == typeof(TimeSpan))
                return "datetime64[us]";
            if (typeof(T) == typeof(string))
                return "str";
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
                    break;
                case 'M': // datetime
                    break;
                case 'O': // object
                    break;
                case 'S': // string
                    return py.DecodeSafe<string>();
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

        private static T[] DecodeFast<T>(this PyObject py, Action<UIntPtr, T[]> decode)
        {
            var length = py.Length();
            var result = new T[length];
            if (length == 0) return result;

            using (var arrayInterface = py.GetAttr("__array_interface__"))
            using (var data = arrayInterface.GetItem("data"))
            using (var address = data.GetItem(0))
                decode(new UIntPtr(address.As<ulong>()), result);

            return result;
        }

        private static unsafe void Decode<T>(UIntPtr ptr, T[] result) where T : unmanaged
        {
            var src = (T*)ptr;
            var size = sizeof(T) * result.Length;
            fixed (T* dst = result)
                Buffer.MemoryCopy(src, dst, size, size);
        }

        #endregion
    }
}
