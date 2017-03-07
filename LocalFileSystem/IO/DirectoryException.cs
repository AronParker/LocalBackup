using System;
using System.IO;
using System.Runtime.Serialization;

namespace LocalFileSystem.IO
{
    [Serializable]
    public class DirectoryException : Exception
    {
        public DirectoryException(DirectoryInfo directory, Exception ex) : base(ex.Message, ex)
        {
            Directory = directory;
        }

        protected DirectoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Directory = (DirectoryInfo)info.GetValue("dir", typeof(FileInfo));
        }

        public DirectoryInfo Directory { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("dir", Directory);

            base.GetObjectData(info, context);
        }
    }
}
