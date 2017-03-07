namespace LocalFileSystem.IO
{
    public abstract class FileSystemItem
    {
        public abstract string Name { get; }
        public abstract FileSystemItemType Type { get; }
        public abstract string FileName { get; }
        public abstract string FullPath { get; }

        public override string ToString()
        {
            return Type.ToString() + " " + FullPath;
        }
    }
}
