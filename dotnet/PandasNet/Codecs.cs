using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace PandasNet
{
    public class Codecs : IPyObjectDecoder, IPyObjectEncoder
    {
        private static readonly Codecs instance = new Codecs();

        public static void Initialize(bool force = false) => instance.Setup(force);

        private bool _isInitialized = false;
        private readonly Dictionary<string, object> _decoders = new Dictionary<string, object>();
        private readonly Dictionary<Type, Func<object, PyObject>> _encoders = new Dictionary<Type, Func<object, PyObject>>();

        private Codecs() 
        {
            Setup(); 
        }

        private void Setup(bool force = false)
        {
            if (_isInitialized && !force) return;
            _isInitialized = true;

            DateTimeCodec.Initialize(this);
            NumpyCodec.Initialize(this);
            PandasCodec.Initialize(this);


            PyObjectConversions.RegisterEncoder(this);
            PyObjectConversions.RegisterDecoder(this);
        }

        public void Register<T>(Func<T, PyObject> encoder) => _encoders[typeof(T)] = p => encoder((T)p);

        public void Register<T>(string pyType, Func<PyObject, T> decoder) => _decoders[pyType] = decoder;

        public bool CanDecode(PyObject objectType, Type targetType)
        {
            var type = objectType.ToString();
            //Write($"CanDecode: {type}, ObjectType: {objectType}, targetType: {targetType}");
            return _decoders.ContainsKey(type);
        }

        public bool TryDecode<T>(PyObject pyObj, out T value)
        {
            var type = pyObj.GetPythonType().ToString();
            //Write($"TryDecode: {type}, targetType: {typeof(T)}");
            if (!_decoders.TryGetValue(type, out var decoder))
            {
                value = default;
                return false;
            }
            
            value = ((Func<PyObject, T>)decoder)(pyObj);
            return true;
        }

        public bool CanEncode(Type type)
        {
            //Write($"CanEncode type: {type}");
            return _encoders.ContainsKey(type);
        }

        public PyObject TryEncode(object value)
        {
            if (value == null) return null; // Todo: Should I return None instead ?

            //Write($"Encode type: {value.GetType()}");

            if (!_encoders.TryGetValue(value.GetType(), out var encoder))
                return null; // Todo: Should I return None instead ?

            return encoder(value);
        }

        //private static void Write(string text)
        //{
        //    const string file = @"C:\OtherDrive\Workspace\Git\fdieulle\pandasnet\log.txt";
        //    if (!File.Exists(file))
        //        File.WriteAllLines(file, new[] { text });
        //    else File.AppendAllLines(file, new[] { text });
        //}

        public static PyObject Encode(object value) => instance.TryEncode(value);
    }
}