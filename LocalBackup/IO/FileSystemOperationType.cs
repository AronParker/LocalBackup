namespace LocalBackup.IO
{
    public enum FileSystemOperationType
    {
        CreateDirectory = 0,
        EditDirectory = 1,
        DestroyDirectory = 2,

        CopyFile = 3,
        EditFile = 4,
        DeleteFile = 5,
    }
}
