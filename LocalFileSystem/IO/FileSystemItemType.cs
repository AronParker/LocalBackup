namespace LocalFileSystem.IO
{
    public enum FileSystemItemType
    {
        CreateDirectory = 0,
        EditDirectory = 1,
        DestroyDirectory = 2,

        CopyFile = 3,
        EditFile = 4,
        DeleteFile = 5,
        
        DirectoryError = 6,
        FileError = 7,
    }
}
