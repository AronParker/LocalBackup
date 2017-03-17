using System.IO;

namespace LocalBackup.IO.Operations
{
    public class DestroyDirectoryOperation : FileSystemOperation
    {
        private DirectoryInfo _dir;

        public DestroyDirectoryOperation(DirectoryInfo dir)
        {
            _dir = dir;
        }
        
        public override string FileName => _dir.Name;
        public override string FilePath => _dir.FullName;
        public DirectoryInfo Directory => _dir;

        public override void Perform()
        {
            UnsetReadOnlyIfSet(_dir);
            _dir.Delete(false);
        }
    }
}
