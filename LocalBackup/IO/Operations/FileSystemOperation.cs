namespace LocalBackup.IO.Operations
{
    public abstract class FileSystemOperation : FileSystemItem
    {
        public virtual long Weight => 1;

        public abstract void Perform();
    }
}
