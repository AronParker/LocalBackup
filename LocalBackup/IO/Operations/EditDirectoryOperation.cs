using System.IO;

namespace LocalBackup.IO.Operations
{
    public class EditAttributesOperation : FileSystemOperation
    {
        public EditAttributesOperation(FileSystemInfo fsi, FileAttributes attributes)
        {
            FileSystemInfo = fsi;
            Attributes = attributes;
        }
        
        public override string FileName => FileSystemInfo.Name;
        public override string FilePath => FileSystemInfo.FullName;
        public FileSystemInfo FileSystemInfo { get; }
        public FileAttributes Attributes { get; }

        public override void Perform()
        {
            FileSystemInfo.Attributes = Attributes;
        }
    }
}
