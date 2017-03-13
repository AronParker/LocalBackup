using System;
using System.IO;
using System.Runtime.Serialization;

namespace LocalBackup.IO
{
    [Serializable]
    public class FileException : Exception
    {
        public FileException(FileInfo file, Exception ex) : base(ex.Message,ex)
        {
            File = file;
        }

        protected FileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            File = (FileInfo)info.GetValue(nameof(File), typeof(FileInfo));
        }

        public FileInfo File { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(File), File);

            base.GetObjectData(info, context);
        }
    }
}
