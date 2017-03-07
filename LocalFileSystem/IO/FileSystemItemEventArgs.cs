namespace LocalFileSystem.IO
{
    public class FileSystemItemEventArgs
    {
        private FileSystemItem _fsi;

        public FileSystemItemEventArgs(FileSystemItem fileSystemItem)
        {
            _fsi = fileSystemItem;
        }

        public FileSystemItem FileSystemItem
        {
            get { return _fsi; }
        }
    }
}
