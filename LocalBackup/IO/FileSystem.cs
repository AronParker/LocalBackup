﻿using System;
using System.IO;
using System.Security;
using LocalBackup.Extensions;
using LocalBackup.IO.Operations;

namespace LocalBackup.IO
{
    internal static class FileSystem
    {
        public static FileStream OpenFile(FileInfo file)
        {
            try
            {
                return new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 1, FileOptions.SequentialScan);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)
            {
                throw new FileException(file, ex);
            }
        }

        public static void ReadFile(FileInfo file, FileStream fs, byte[] buffer, int offset, int count)
        {
            try
            {
                fs.SafeRead(buffer, offset, count);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)
            {
                throw new FileException(file, ex);
            }
        }
        
        public static bool ContainsSpecialDirectoryAttributes(FileAttributes attributes)
        {
            return (attributes & ~(FileAttributes.Directory | FileAttributes.Archive | FileAttributes.ReparsePoint)) != 0;
        }

        public static bool AttributesEqual(FileAttributes attributes1, FileAttributes attributes2)
        {
            return ((attributes1 ^ attributes2) & ~(FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Normal | FileAttributes.ReparsePoint)) == 0;
        }

        public static void UnsetReadOnlyIfSet(FileSystemInfo fsi)
        {
            var attributes = fsi.Attributes;

            if ((attributes & FileAttributes.ReadOnly) != 0)
                fsi.Attributes = attributes & ~FileAttributes.ReadOnly;
        }

        public static long GetWeight(FileSystemOperation op)
        {
            switch (op)
            {
                case CreateDirectoryOperation _:
                case DestroyDirectoryOperation _:
                case EditAttributesOperation _:
                case DeleteFileOperation _:
                    return 1;
                case CopyFileOperation copy:
                    return 1 + copy.SourceFile.Length / 8192;
                case EditFileOperation edit:
                    return 1 + edit.SourceFile.Length / 8192;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
