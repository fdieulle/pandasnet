using System;

namespace LibForTests
{
    public class DatetimeNet
    {
        public static DateTime DateTimePassThrough(DateTime x) => x;

        public static TimeSpan TimeSpanPassThrough(TimeSpan x) => x;
    }
}
