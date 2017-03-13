using System.IO;

namespace LocalBackup.IO.Operations
{
    public class CreateDirectoryOperation : FileSystemOperation
    {
        private DirectoryInfo _dir;

        public CreateDirectoryOperation(DirectoryInfo dir)
        {
            _dir = dir;
        }
        
        public override string FileName => _dir.Name;
        public override string FilePath => _dir.FullName;
        public DirectoryInfo Directory => _dir;

        public override void Perform()
        {
            _dir.Create();
        }
    }
}
