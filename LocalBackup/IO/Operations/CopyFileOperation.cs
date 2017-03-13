using System.IO;

namespace LocalBackup.IO.Operations
{
    public class CopyFileOperation : FileSystemOperation
    {
        public CopyFileOperation(FileInfo srcFile, FileInfo dstFile)
        {
            SourceFile = srcFile;
            DestinationFile = dstFile;
        }
        
        public override string FileName => DestinationFile.Name;
        public override string FilePath => DestinationFile.FullName;
        public override long Weight => SourceFile.Length / 8192 + 1;
        public FileInfo SourceFile { get; }
        public FileInfo DestinationFile { get; }

        public override void Perform()
        {
            SourceFile.CopyTo(DestinationFile.FullName, false);
        }
    }
}
