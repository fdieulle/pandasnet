using Python.Runtime;
using System;

namespace PandasNet
{
    public static class DateTimeCodec
    {
        public static void Initialize(Codecs codecs)
        {
            // .Net -> Python
            codecs.Register<DateTime>(EncodeDateTime);
            codecs.Register<TimeSpan>(EncodeTimeSpan);

            // Python -> .Net
            codecs.Register("<class 'datetime.date'>", DecodeDate);
            codecs.Register("<class 'datetime.datetime'>", DecodeDateTime);
            codecs.Register("<class 'datetime.time'>", DecodeTime);
            codecs.Register("<class 'datetime.timedelta'>", DecodeTimeDelta);
        }

        private static PyObject EncodeDateTime(DateTime x)
        {
            using(var scope = Py.CreateScope())
            {
                scope.Import("datetime");
                var tz = string.Empty;
                if (x.Kind == DateTimeKind.Utc)
                {
                    scope.Import("pytz");
                    tz = ", tzinfo=pytz.utc";
                }
                var microseconds = x.Ticks / 10L % 1000000L;
                scope.Exec($"a = datetime.datetime({x:yyyy, M, d, H, m, s}, {microseconds}{tz})");
                return scope.Get("a");
            }
        }

        private static PyObject EncodeTimeSpan(TimeSpan x)
        {
            using (var scope = Py.CreateScope())
            {
                scope.Import("datetime");
                var microseconds = x.Ticks / 10L % 1000000L;
                scope.Exec($"a = datetime.timedelta(days={x.Days}, hours={x.Hours}, minutes={x.Minutes}, seconds={x.Seconds}, microseconds={microseconds})");
                return scope.Get("a");
            }
        }

        private static DateTime DecodeDate(PyObject py) 
            => new DateTime(
                py.GetAttr("year").As<int>(),
                py.GetAttr("month").As<int>(),
                py.GetAttr("day").As<int>())
                .Date;

        private static DateTime DecodeDateTime(PyObject py)
        {
            var utcDelta = TimeSpan.Zero;
            var kind = DateTimeKind.Local;

            var tz = py.GetAttr("tzinfo");
            if (tz.HasAttr("utcoffset"))
            {
                kind = DateTimeKind.Utc;
                utcDelta = py.InvokeMethod("utcoffset")
                    .DecodeTimeDelta();
            }
            
            var result = new DateTime(
                py.GetAttr("year").As<int>(),
                py.GetAttr("month").As<int>(),
                py.GetAttr("day").As<int>(),
                py.GetAttr("hour").As<int>(),
                py.GetAttr("minute").As<int>(),
                py.GetAttr("second").As<int>(),
                kind);

            return result.AddTicks(-utcDelta.Ticks + py.GetAttr("microsecond").As<int>() * 10)
                .ToLocalTime();
        }

        private static TimeSpan DecodeTime(PyObject py)
        {
            var result = new TimeSpan(
                py.GetAttr("hour").As<int>(),
                py.GetAttr("minute").As<int>(),
                py.GetAttr("second").As<int>());

            return TimeSpan.FromTicks(
                result.Ticks +
                py.GetAttr("microsecond").As<int>() * 10);
        }

        public static TimeSpan DecodeTimeDelta(this PyObject py)
        {
            var seconds = py.InvokeMethod("total_seconds").As<double>();
            return TimeSpan.FromSeconds(seconds);
        }

    }
}
