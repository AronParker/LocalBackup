using System;
using System.IO;

namespace LocalFileSystem.IO.FileComparers
{
    public class FileComparer : IFileEqualityComprarer
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

            using (var fs1 = FileSystem.OpenFile(f1))
            using (var fs2 = FileSystem.OpenFile(f2))
                return InternalEquals(f1, fs1, f2, fs2);
        }

        public bool Equals(FileInfo f1, FileStream fs1, FileInfo f2, FileStream fs2)
        {
            if (f1 == null)
                throw new ArgumentNullException(nameof(f1));
            if (fs1 == null)
                throw new ArgumentNullException(nameof(fs1));
            if (f2 == null)
                throw new ArgumentNullException(nameof(f2));
            if (fs2 == null)
                throw new ArgumentNullException(nameof(fs2));

            if (f1.Length != f2.Length)
                return false;

            fs1.Seek(0, SeekOrigin.Begin);
            fs2.Seek(0, SeekOrigin.Begin);

            return InternalEquals(f1, fs1, f2, fs2);
        }

        private bool InternalEquals(FileInfo f1, FileStream fs1, FileInfo f2, FileStream fs2)
        {
            var bytesLeft = f1.Length;

            if (bytesLeft > 0)
            {
                do
                {
                    var len = (int)Math.Min(bytesLeft, BufferSize);
                    
                    FileSystem.ReadFile(f1, fs1, _buffer1, 0, len);
                    FileSystem.ReadFile(f2, fs2, _buffer2, 0, len);

                    if (NativeMethods.memcmp(_buffer1, _buffer2, new UIntPtr((uint)len)) != 0)
                        return false;

                    bytesLeft -= len;
                } while (bytesLeft > 0);
            }

            return true;
        }
    }
}
