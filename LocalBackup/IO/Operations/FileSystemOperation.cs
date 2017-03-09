namespace LocalBackup.IO.Operations
{
    public abstract class FileSystemOperation
    {
        public abstract string Name { get; }
        public abstract FileSystemOperationType Type { get; }
        public abstract string FileName { get; }
        public abstract string FullPath { get; }

        public override string ToString()
        {
            return Type.ToString() + " " + FullPath;
        }

        public virtual long Weight => 1;

        public abstract void Perform();
    }
}
