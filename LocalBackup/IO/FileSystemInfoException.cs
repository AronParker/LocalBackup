using System;
using System.IO;
using System.Runtime.Serialization;

namespace LocalBackup.IO
{
    [Serializable]
    public class FileSystemInfoException : Exception
    {
        public FileSystemInfoException(FileSystemInfo fsi, Exception ex) : base(ex.Message,ex)
        {
            FileSystemInfo = fsi;
        }

        protected FileSystemInfoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            FileSystemInfo = (FileSystemInfo)info.GetValue(nameof(FileSystemInfo), typeof(FileSystemInfo));
        }

        public FileSystemInfo FileSystemInfo { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(FileSystemInfo), FileSystemInfo);

            base.GetObjectData(info, context);
        }
    }
}
