using System.IO;

namespace LocalBackup.IO.Operations
{
    public class EditFileOperation : FileSystemOperation
    {
        private FileInfo srcFile;
        private FileInfo dstFile;

        public EditFileOperation(FileInfo srcFile, FileInfo dstFile)
        {
            this.srcFile = srcFile;
            this.dstFile = dstFile;
        }

        public override string Name => "Edit file";
        public override FileSystemOperationType Type => FileSystemOperationType.EditFile;
        public override string FileName => dstFile.Name;
        public override string FullPath => dstFile.FullName;
        public override long Weight => SourceFile.Length / 4096 + 1;
        public FileInfo SourceFile => srcFile;
        public FileInfo DestinationFile => dstFile;

        public override void Perform()
        {
            FileSystem.UnsetReadOnlyIfSet(dstFile);
            srcFile.CopyTo(dstFile.FullName, true);
        }
    }
}
