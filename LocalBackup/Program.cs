using LocalBackup.IO;
using LocalBackup.IO.FileComparers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace LocalBackup
{
    public static class Program
    {
        public static void Main()
        {
            /*var detector = new ChangesDetector();
            detector.ItemFound += (o, e) =>
            {
                Console.WriteLine(e.FileSystemItem);
            };
            detector.RunAsync(new DirectoryInfo(@"C:\Users\Aron\Desktop\1"),
                              new DirectoryInfo(@"C:\Users\Aron\Desktop\2"),
                              new FileComparer());*/
            var finder = new DuplicateFinder();
            var dirs = new List<DirectoryInfo>();
            dirs.Add(new DirectoryInfo(@"C:\Users\Aron\Desktop\1"));
            finder.InternalStart(dirs, new FileComparer());
            
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
    }
}
