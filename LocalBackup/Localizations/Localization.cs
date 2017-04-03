using System;
using static System.FormattableString;

namespace LocalBackup.Localizations
{
    public static class Localization
    {
        public static string GetPlural(int count, string word)
        {
            if (word == null)
                throw new ArgumentNullException(nameof(word));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 1)
                return "1 " + word;

            return Invariant($"{count} {word}s");
        }

        public static string GetPlural(long count, string word)
        {
            if (word == null)
                throw new ArgumentNullException(nameof(word));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 1)
                return "1 " + word;

            return Invariant($"{count} {word}s");
        }

        public static string GetHumanReadableTimeSpan(TimeSpan ts)
        {
            var ticks = ts.Ticks;

            if (ticks < TimeSpan.TicksPerSecond)
                return "less than a second";

            if (ticks < TimeSpan.TicksPerMinute)
            {
                var seconds = (ticks / TimeSpan.TicksPerSecond);

                return GetPlural(seconds, "second");
            }

            if (ticks < TimeSpan.TicksPerHour)
            {
                var minutes = ticks / TimeSpan.TicksPerMinute;
                var seconds = ticks % TimeSpan.TicksPerMinute / TimeSpan.TicksPerSecond;

                return GetPlural(minutes, "minute") + " " + GetPlural(seconds, "second");
            }

            if (ticks < TimeSpan.TicksPerDay)
            {
                var hours = ticks / TimeSpan.TicksPerHour;
                var minutes = ticks % TimeSpan.TicksPerHour / TimeSpan.TicksPerMinute;

                return GetPlural(hours, "hour") + " " + GetPlural(minutes, "minute");
            }

            {
                var days = ticks / TimeSpan.TicksPerDay;
                var hours = ticks % TimeSpan.TicksPerDay / TimeSpan.TicksPerHour;

                return GetPlural(days, "day") + " " + GetPlural(hours, "hour");
            }
        }
    }
}
