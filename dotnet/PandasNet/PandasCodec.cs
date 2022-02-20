using Python.Runtime;
using System;
using System.Collections.Generic;

namespace PandasNet
{
    public static class PandasCodec
    {
        public static void Initialize(Codecs codecs)
        {
            // .Net -> Python
            codecs.Register<Dictionary<string, Array>>(Encode);

            // Python -> .Net
            codecs.Register("<class 'pandas.core.frame.DataFrame'>", DecodeDataFrame);
            codecs.Register("<class 'pandas.core.series.Series'>", DecodeSeries);
            codecs.Register("<class 'pandas._libs.tslibs.timestamps.Timestamp'>", DecodeTimestamp);
            codecs.Register("<class 'pandas._libs.tslibs.timedeltas.TimeDelta'>", DateTimeCodec.DecodeTimeDelta);
        }

        private static PyObject Encode(Dictionary<string, Array> data)
        {
            using(var scope = Py.CreateScope())
            {
                dynamic pandas = scope.Import("pandas");
                var pyDict = new PyDict();
                foreach(var pair in data)
                {
                    var series = pandas.Series(Codecs.Encode(pair.Value));
                    if (pair.Value is string[])
                        series = series.astype("string");

                    pyDict.SetItem(pair.Key, series);
                }

                return pandas.DataFrame(pyDict);
            }
        }

        private static Dictionary<string, Array> DecodeDataFrame(PyObject py)
        {
            var result = new Dictionary<string, Array>();
            var columns = py.GetAttr("columns");
            var length = columns.Length();
            for (var i=0; i < length; i++)
            {
                var columnName = columns.GetItem(i).As<string>();
                result[columnName] = py.GetItem(columnName).DecodeSeries();
            }

            return result;
        }

        private static Array DecodeSeries(this PyObject py) 
            => (Array)NumpyCodec.Decode(py.InvokeMethod("to_numpy"));

        private static object DecodeTimestamp(this PyObject py)
        {
            var posix = py.InvokeMethod("timestamp").As<double>();
            var timestamp = Time.Origin.AddSeconds(posix);

            var tz = py.GetAttr("tzinfo");
            if (tz.HasAttr("utcoffset"))
            {
                var utcOffset = py.InvokeMethod("utcoffset")
                    .DecodeTimeDelta();
                timestamp = timestamp.Add(utcOffset);
            }
            else
            {
                timestamp = new DateTime(timestamp.Ticks, DateTimeKind.Utc);
            }

            return timestamp;
        }
    }
}
