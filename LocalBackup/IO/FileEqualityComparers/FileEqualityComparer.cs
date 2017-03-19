using System;
using System.IO;
using System.Security;
using LocalBackup.Extensions;

namespace LocalBackup.IO.FileEqualityComparers
{
    public class FileEqualityComparer : IFileEqualityComprarer
    {
        private const int BufferSize = 512 * 1024;

        private byte[] _buffer1 = new byte[BufferSize];
        private byte[] _buffer2 = new byte[BufferSize];

        public bool Equals(FileInfo f1, FileInfo f2)
        {
            if (f1 == null)
                throw new ArgumentNullException(nameof(f1));
            if (f2 == null)
                throw new ArgumentNullException(nameof(f2));

            if (f1.Length != f2.Length)
                return false;
            if (f1.Length == 0)
                return true;

            using (var fs1 = OpenFile(f1))
            using (var fs2 = OpenFile(f2))
                return InternalEquals(fs1, fs2);
        }

        public bool Equals(FileInfoStream fs1, FileInfoStream fs2)
        {
            if (fs1 == null)
                throw new ArgumentNullException(nameof(fs1));
            if (fs2 == null)
                throw new ArgumentNullException(nameof(fs2));

            if (fs1.File.Length != fs2.File.Length)
                return false;
            if (fs1.File.Length == 0)
                return true;

            fs1.Seek(0, SeekOrigin.Begin);
            fs2.Seek(0, SeekOrigin.Begin);

            return InternalEquals(fs1, fs2);
        }

        private static FileInfoStream OpenFile(FileInfo file)
        {
            try
            {
                return new FileInfoStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 1, FileOptions.SequentialScan);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)
            {
                throw new FileSystemInfoException(file, ex);
            }
        }

        private static void ReadFile(FileInfoStream fs, byte[] buffer, int offset, int count)
        {
            try
            {
                fs.SafeRead(buffer, offset, count);
            }
            catch (IOException ex)
            {
                throw new FileSystemInfoException(fs.File, ex);
            }
        }

        private bool InternalEquals(FileInfoStream fs1, FileInfoStream fs2)
        {
            var bytesLeft = fs1.Length;

            do
            {
                var len = (int)Math.Min(bytesLeft, BufferSize);

                ReadFile(fs1, _buffer1, 0, len);
                ReadFile(fs2, _buffer2, 0, len);

                if (NativeMethods.memcmp(_buffer1, _buffer2, new UIntPtr((uint)len)) != 0)
                    return false;

                bytesLeft -= len;
            } while (bytesLeft > 0);

            return true;
        }
    }
}
