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

        public override string OperationName => "Edit file";
        public override string FileName => DestinationFile.Name;
        public override string FilePath => DestinationFile.FullName;
        public override long Weight => SourceFile.Length / 8192 + 1;
        public FileInfo SourceFile { get; }
        public FileInfo DestinationFile { get; }

        public override void Perform()
        {
            FileSystem.UnsetReadOnlyIfSet(DestinationFile);
            SourceFile.CopyTo(DestinationFile.FullName, true);
        }
    }
}
