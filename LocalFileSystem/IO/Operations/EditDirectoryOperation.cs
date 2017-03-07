﻿using System.IO;

namespace LocalFileSystem.IO.Operations
{
    public class EditDirectoryOperation : FileSystemOperation
    {
        private DirectoryInfo _dir;
        private FileAttributes _attributes;

        public EditDirectoryOperation(DirectoryInfo dir, FileAttributes attributes)
        {
            _dir = dir;
            _attributes = attributes;
        }

        public override string Name => "Edit directory";
        public override FileSystemItemType Type => FileSystemItemType.EditDirectory;
        public override string FileName => _dir.Name;
        public override string FullPath => _dir.FullName;
        public DirectoryInfo Directory => _dir;

        public override void Perform()
        {
            _dir.Attributes = _attributes;
        }
    }
}