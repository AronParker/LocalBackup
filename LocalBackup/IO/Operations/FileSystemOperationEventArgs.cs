namespace LocalBackup.IO.Operations
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
