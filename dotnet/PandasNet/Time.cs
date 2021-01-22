using System;
    
namespace PandasNet
{
    public static class Time
    {
        public static readonly DateTime Origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly long OriginTicks = Origin.Ticks;
    }
}
