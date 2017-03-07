using System;
using System.Collections.Generic;
using System.IO;

namespace LocalBackup.IO.FileComparers
{
    public class FATFileComparer : IFileInfoEqualityComparer
    {
        public bool Equals(FileInfo f1, FileInfo f2)
        {
            if (f1 == null)
                throw new ArgumentNullException(nameof(f1));
            if (f2 == null)
                throw new ArgumentNullException(nameof(f2));

            if (f1.Length == f2.Length)
            {
                var f1Ticks = (f1.LastWriteTimeUtc.Ticks + (2 * TimeSpan.TicksPerSecond - 1)) / (2 * TimeSpan.TicksPerSecond) * (2 * TimeSpan.TicksPerSecond);
                var f2Ticks = (f2.LastWriteTimeUtc.Ticks + (2 * TimeSpan.TicksPerSecond - 1)) / (2 * TimeSpan.TicksPerSecond) * (2 * TimeSpan.TicksPerSecond);

                return f1Ticks == f2Ticks;
            }

            return false;
        }

        public int GetHashCode(FileInfo obj)
        {
            throw new NotSupportedException();
        }
    }
}
