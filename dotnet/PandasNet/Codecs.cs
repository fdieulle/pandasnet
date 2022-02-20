using Python.Runtime;
using System;
using System.Collections.Generic;

namespace PandasNet
{
    public class Codecs : IPyObjectDecoder, IPyObjectEncoder
    {
        public static Codecs Instance { get; } = new Codecs();

        public static void Initialize(bool force = false) => Instance.Setup(force);

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

        public bool CanDecode(PyType objectType, Type targetType)
        {
            var type = objectType.ToString();
            return _decoders.ContainsKey(type);
        }

        public bool TryDecode<T>(PyObject pyObj, out T value)
        {
            var type = pyObj.GetPythonType().ToString();
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
            return _encoders.ContainsKey(type);
        }

        public PyObject TryEncode(object value)
        {
            if (value == null) return null; // Todo: Should I return None instead ?

            if (!_encoders.TryGetValue(value.GetType(), out var encoder))
                return null; // Todo: Should I return None instead ?

            return encoder(value);
        }

        public static PyObject Encode(object value) => Instance.TryEncode(value);
    }
}