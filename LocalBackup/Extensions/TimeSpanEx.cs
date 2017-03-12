using System;

namespace LocalBackup.Extensions
{
    public static class TimeSpanEx
    {
        public static string ToHumanReadableString(this TimeSpan ts)
        {
            var ticks = ts.Ticks;

            if (ticks < TimeSpan.TicksPerSecond)
                return "less than a second";

            if (ticks < TimeSpan.TicksPerMinute)
                return (ticks / TimeSpan.TicksPerSecond).ToString() + " second(s)";

            if (ticks < TimeSpan.TicksPerHour)
                return (ticks / TimeSpan.TicksPerMinute).ToString() + " minute(s) " + (ticks % TimeSpan.TicksPerMinute / TimeSpan.TicksPerSecond).ToString() + " second(s)";

            if (ticks < TimeSpan.TicksPerDay)
                return (ticks / TimeSpan.TicksPerHour).ToString() + " hour(s) " + (ticks % TimeSpan.TicksPerHour / TimeSpan.TicksPerMinute).ToString() + " minute(s)";

            return (ticks / TimeSpan.TicksPerDay).ToString() + " day(s) " + (ticks % TimeSpan.TicksPerDay / TimeSpan.TicksPerHour).ToString() + " hour(s)";
        }
    }
}
