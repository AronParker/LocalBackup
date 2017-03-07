using System.IO;

namespace LocalFileSystem.IO.Operations
{
    public class DestroyDirectoryOperation : FileSystemOperation
    {
        private DirectoryInfo _dir;

        public DestroyDirectoryOperation(DirectoryInfo dir)
        {
            _dir = dir;
        }

        public override string Name => "Destroy directory";
        public override FileSystemItemType Type => FileSystemItemType.DestroyDirectory;
        public override string FileName => _dir.Name;
        public override string FullPath => _dir.FullName;
        public DirectoryInfo Directory => _dir;

        public override void Perform()
        {
            FileSystem.UnsetReadOnlyIfSet(_dir);
            _dir.Delete(false);
        }
    }
}
