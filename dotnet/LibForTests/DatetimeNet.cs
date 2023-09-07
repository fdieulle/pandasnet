using System;

namespace LibForTests
{
    public class DatetimeNet
    {
        public DateTime DateTimePassThrough(DateTime x) => x;

        public TimeSpan TimeSpanPassThrough(TimeSpan x) => x;
    }
}
