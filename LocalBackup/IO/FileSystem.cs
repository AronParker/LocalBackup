using LocalBackup.Extensions;
using System;
using System.IO;
using System.Security;

namespace LocalBackup.IO
{
    internal static class FileSystem
    {
        public static FileStream OpenFile(FileInfo file)
        {
            try
            {
                return new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 1, FileOptions.SequentialScan);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)
            {
                throw new FileException(file, ex);
            }
        }

        public static void ReadFile(FileInfo file, FileStream fs, byte[] buffer, int offset, int count)
        {
            try
            {
                fs.SafeRead(buffer, offset, count);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)
            {
                throw new FileException(file, ex);
            }
        }

        public static FileAttributes GetMeaningfulAttributes(FileAttributes attributes)
        {
            return attributes & ~(FileAttributes.Archive | FileAttributes.ReparsePoint);
        }

        public static bool AttributesEqual(FileAttributes attributes1, FileAttributes attributes2)
        {
            return ((attributes1 ^ attributes2) & ~(FileAttributes.Archive | FileAttributes.ReparsePoint)) == 0;
        }

        public static void UnsetReadOnlyIfSet(FileSystemInfo fsi)
        {
            var attributes = fsi.Attributes;

            if (attributes != (FileAttributes)(-1) && (attributes & FileAttributes.ReadOnly) != 0)
                fsi.Attributes = attributes & ~FileAttributes.ReadOnly;
        }
    }
}
