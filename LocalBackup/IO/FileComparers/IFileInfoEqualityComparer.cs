using System.IO;

namespace LocalBackup.IO.FileComparers
{
    public interface IFileInfoEqualityComparer
    {
        bool Equals(FileInfo f1, FileInfo f2);
    }
}
