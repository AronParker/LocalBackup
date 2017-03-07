﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
            File = (FileInfo)info.GetValue("file", typeof(FileInfo));
        }

        public FileInfo File { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("file", File);

            base.GetObjectData(info, context);
        }
    }
}
