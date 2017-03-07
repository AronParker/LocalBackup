using LocalFileSystem.Extensions;
using System;
using System.IO;
using System.Security;

namespace LocalFileSystem.IO
{
    public static class FileSystem
    {
        public static FileStream OpenFile(FileInfo file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

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
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            if (fs == null)
                throw new ArgumentNullException(nameof(fs));
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if ((uint)offset > (uint)buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if ((uint)count > (uint)buffer.Length - (uint)offset)
                throw new ArgumentOutOfRangeException(nameof(count));

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

        public static FileAttributes ClearArchiveAttribute(FileAttributes attributes)
        {
            return attributes & ~(FileAttributes.Archive);
        }

        public static void UnsetReadOnlyIfSet(FileSystemInfo fsi)
        {
            if (fsi == null)
                throw new ArgumentNullException(nameof(fsi));

            var attributes = fsi.Attributes;

            if (attributes != (FileAttributes)(-1) && (attributes & FileAttributes.ReadOnly) != 0)
                fsi.Attributes = attributes & ~FileAttributes.ReadOnly;
        }
    }
}
