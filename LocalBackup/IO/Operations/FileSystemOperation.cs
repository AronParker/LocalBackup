using System.IO;

namespace LocalBackup.IO.Operations
{
    public abstract class FileSystemOperation
    {
        public abstract string FileName { get; }
        public abstract string FilePath { get; }

        public abstract void Perform();

        protected static void UnsetReadOnlyIfSet(FileSystemInfo fsi)
        {
            var attributes = fsi.Attributes;

            if ((attributes & FileAttributes.ReadOnly) != 0)
                fsi.Attributes = attributes & ~FileAttributes.ReadOnly;
        }
    }
}
