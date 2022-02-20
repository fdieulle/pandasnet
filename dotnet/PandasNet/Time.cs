using System;
    
namespace PandasNet
{
    public static class Time
    {
        public static readonly DateTime Origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly long OriginTicks = Origin.Ticks;

        public static readonly long KindLocalTicks = (long)DateTimeKind.Local << 62;
        public static readonly long KindUtcTicks = (long)DateTimeKind.Utc << 62;

        public const double TICKS_BY_NS = 0.01;
        public const long TICKS_BY_US = 10;
        public const long TICKS_BY_MS = 1000 * TICKS_BY_US;
        public const long TICKS_BY_SEC = 1000 * TICKS_BY_MS;
        public const long TICKS_BY_MIN = 60 * TICKS_BY_SEC;
        public const long TICKS_BY_HOUR = 60 * TICKS_BY_MIN;
        public const long TICKS_BY_DAY = 24 * TICKS_BY_HOUR;
        public const long TICKS_BY_WEEK = 7 * TICKS_BY_DAY;
    }
}
