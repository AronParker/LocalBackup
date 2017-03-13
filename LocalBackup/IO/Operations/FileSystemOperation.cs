namespace LocalBackup.IO.Operations
{
    public abstract class FileSystemOperation
    {
        public abstract string FileName { get; }
        public abstract string FilePath { get; }
        public virtual long Weight => 1;

        public abstract void Perform();
    }
}
