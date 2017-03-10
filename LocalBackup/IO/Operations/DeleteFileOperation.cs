using System.IO;

namespace LocalBackup.IO.Operations
{
    public class DeleteFileOperation : FileSystemOperation
    {
        private FileInfo _file;

        public DeleteFileOperation(FileInfo file)
        {
            _file = file;
        }

        public override string Name => "Delete file";
        public override FileSystemOperationType Type => FileSystemOperationType.DeleteFile;
        public override string FileName => _file.Name;
        public override string FilePath => _file.FullName;
        public FileInfo File => _file;

        public override void Perform()
        {
            FileSystem.UnsetReadOnlyIfSet(_file);
            _file.Delete();
        }
    }
}
