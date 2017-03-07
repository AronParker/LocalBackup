using System.IO;

namespace LocalFileSystem.IO.FileComparers
{
    public interface IFileInfoEqualityComparer
    {
        bool Equals(FileInfo f1, FileInfo f2);
    }
}
