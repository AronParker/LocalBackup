using System.IO;

namespace LocalBackup.IO.FileEqualityComparers
{
    public interface IFileInfoEqualityComparer
    {
        bool Equals(FileInfo f1, FileInfo f2);
    }
}
