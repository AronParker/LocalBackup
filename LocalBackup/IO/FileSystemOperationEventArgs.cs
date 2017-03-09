using LocalBackup.IO.Operations;

namespace LocalBackup.IO
{
    public class FileSystemOperationEventArgs
    {
        public FileSystemOperationEventArgs(FileSystemOperation operation)
        {
            Operation = operation;
        }

        public FileSystemOperation Operation { get; }
    }
}
