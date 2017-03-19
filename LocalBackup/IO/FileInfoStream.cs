using System;
using System.IO;
using System.Security.AccessControl;

namespace LocalBackup.IO
{
    public class FileInfoStream : FileStream
    {
        public FileInfoStream(FileInfo file, FileMode mode) : base(file.FullName, mode)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            File = file;
        }

        public FileInfoStream(FileInfo file, FileMode mode, FileAccess access) : base(file.FullName, mode, access)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            File = file;
        }

        public FileInfoStream(FileInfo file, FileMode mode, FileAccess access, FileShare share) : base(file.FullName, mode, access, share)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            File = file;
        }

        public FileInfoStream(FileInfo file, FileMode mode, FileAccess access, FileShare share, int bufferSize) : base(file.FullName, mode, access, share, bufferSize)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            File = file;
        }

        public FileInfoStream(FileInfo file, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) : base(file.FullName, mode, access, share, bufferSize, useAsync)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            File = file;
        }

        public FileInfoStream(FileInfo file, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) : base(file.FullName, mode, access, share, bufferSize, options)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            File = file;
        }

        public FileInfoStream(FileInfo file, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options) : base(file.FullName, mode, rights, share, bufferSize, options)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            File = file;
        }

        public FileInfoStream(FileInfo file, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options, FileSecurity fileSecurity) : base(file.FullName, mode, rights, share, bufferSize, options, fileSecurity)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            File = file;
        }
        
        public FileInfo File { get; }
    }
}
