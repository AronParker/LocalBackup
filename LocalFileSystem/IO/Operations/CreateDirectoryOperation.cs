using System.IO;

namespace LocalFileSystem.IO.Operations
{
    public class CreateDirectoryOperation : FileSystemOperation
    {
        private DirectoryInfo _dir;

        public CreateDirectoryOperation(DirectoryInfo dir)
        {
            _dir = dir;
        }

        public override string Name => "Create directory";
        public override FileSystemItemType Type => FileSystemItemType.CreateDirectory;
        public override string FileName => _dir.Name;
        public override string FullPath => _dir.FullName;
        public DirectoryInfo Directory => _dir;

        public override void Perform()
        {
            _dir.Create();
        }
    }
}
