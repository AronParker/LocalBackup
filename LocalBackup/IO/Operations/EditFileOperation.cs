using System.IO;

namespace LocalBackup.IO.Operations
{
    public class EditFileOperation : FileSystemOperation
    {
        public EditFileOperation(FileInfo srcFile, FileInfo dstFile)
        {
            SourceFile = srcFile;
            DestinationFile = dstFile;
        }
        
        public override string FileName => DestinationFile.Name;
        public override string FilePath => DestinationFile.FullName;
        public FileInfo SourceFile { get; }
        public FileInfo DestinationFile { get; }

        public override void Perform()
        {
            FileSystem.UnsetReadOnlyIfSet(DestinationFile);
            SourceFile.CopyTo(DestinationFile.FullName, true);
        }
    }
}
