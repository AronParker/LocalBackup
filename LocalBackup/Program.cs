using LocalBackup.IO;
using LocalBackup.IO.FileComparers;
using System;
using System.IO;

namespace LocalBackup
{
    public static class Program
    {
        public static void Main()
        {
            var detector = new DirectoryMirrorer();
            detector.ItemFound += (o, e) =>
            {
                Console.WriteLine(e.FileSystemItem);
            };
            detector.RunAsync(new DirectoryInfo(@"C:\Users\Aron\Desktop\1"),
                              new DirectoryInfo(@"C:\Users\Aron\Desktop\2"),
                              new FileComparer());
            /*var finder = new DuplicateFinder();
            var dirs = new List<DirectoryInfo>();
            dirs.Add(new DirectoryInfo(@"C:\Users\Aron\Desktop\1"));*/

            Console.ReadLine();

        }
        /*private IFileComparer CreateFileComparer(DirectoryInfo dstDir)
{
   if (_fullScan)
       return new FileComparer();

   var driveFormat = new DriveInfo(dstDir.FullName).DriveFormat;

   switch (driveFormat)
   {
       case "FAT32":
       case "exFAT":
           return new FATFileComparer();
       case "NTFS":
           return new NTFSFileComparer();
       default:
           throw new NotSupportedException("Destination uses a file system where quick scan is not supported.");
   }
}*/

        /*public class DirectoryError : FileSystemError
        {
        private DirectoryInfo _dir;

        public DirectoryError(DirectoryInfo dir, Exception ex) : base(ex)
        {
            _dir = dir;
        }

        public override string Name => "Directory error";
        public override FileSystemItemType Type => FileSystemItemType.DirectoryError;
        public override string FileName => _dir.Name;
        public override string FullPath => _dir.FullName;
        public DirectoryInfo Directory => _dir;
        }*/

        /*    public class FileError : FileSystemError
        {
        private FileInfo _file;

        public FileError(FileInfo file, Exception ex) : base(ex)
        {
            _file = file;
        }

        public override string Name => "File error";
        public override FileSystemItemType Type => FileSystemItemType.FileError;
        public override string FileName => _file.Name;
        public override string FullPath => _file.FullName;
        public FileInfo File => _file;
        }*/
    }
}