using System.IO;

namespace LocalBackup.IO.Operations
{
    public class CopyFileOperation : FileSystemOperation
    {
        private FileInfo _srcFile;
        private FileInfo _dstFile;

        public CopyFileOperation(FileInfo srcFile, FileInfo dstFile)
        {
            _srcFile = srcFile;
            _dstFile = dstFile;
        }

        public override string Name => "Copy file";
        public override FileSystemOperationType Type => FileSystemOperationType.CopyFile;
        public override string FileName => _dstFile.Name;
        public override string FullPath => _dstFile.FullName;
        public override long Weight => SourceFile.Length / 4096 + 1;
        public FileInfo SourceFile => _srcFile;
        public FileInfo DestinationFile => _dstFile;

        public override void Perform()
        {
            _srcFile.CopyTo(_dstFile.FullName, false);
        }
    }
}
