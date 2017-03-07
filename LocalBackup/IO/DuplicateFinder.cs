﻿using LocalBackup.IO.FileComparers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LocalBackup.Extensions;

namespace LocalBackup.IO
{
    public class DuplicateFinder
    {
        private List<FileInfo> _files = new List<FileInfo>();
        private CancellationToken _token;
        private Task _task;

        public bool IsRunning => _task != null && !_task.IsCompleted;

        public Task Start(IEnumerable<DirectoryInfo> dirs, IFileInfoEqualityComparer fileInfoComparer, CancellationToken token)
        {
            if (dirs == null)
                throw new ArgumentNullException(nameof(dirs));
            if (fileInfoComparer == null)
                throw new ArgumentNullException(nameof(fileInfoComparer));

            _token = token;
            _task = Task.Run(() => InternalStart(dirs, fileInfoComparer));

            return _task;
        }

#if DEBUG
        public
#else
        private
#endif
        void InternalStart(IEnumerable<DirectoryInfo> dirs, IFileInfoEqualityComparer fileComparer)
        {
            AddDirs(dirs);
            SortFiles();
            FindDuplicates(fileComparer);
        }

        private void AddDirs(IEnumerable<DirectoryInfo> dirs)
        {
            foreach (DirectoryInfo dir in dirs)
            {
                _token.ThrowIfCancellationRequested();

                if (dir == null)
                {
                    OnDirectoryError(new DirectoryException(dir, new ArgumentNullException()));
                    continue;
                }

                AddFiles(dir);
            }
        }

        private void AddFiles(DirectoryInfo parent)
        {
            try
            {
                foreach (var fsi in parent.EnumerateFileSystemInfos())
                {
                    _token.ThrowIfCancellationRequested();

                    if (fsi is FileInfo file)
                        _files.Add(file);
                    else if (fsi is DirectoryInfo dir)
                        AddFiles(dir);
                }
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)
            {
                OnDirectoryError(new DirectoryException(parent, ex));
            }
        }

        private void SortFiles()
        {
            _files.Sort((x, y) => y.Length.CompareTo(x.Length));
        }

        private void FindDuplicates(IFileInfoEqualityComparer fileComparer)
        {
            var i = 0;

            while (i < _files.Count)
            {
                var start = i++;

                while (i < _files.Count && _files[start].Length == _files[i].Length)
                    i++;

                var length = i - start;

                if (length > 1)
                    FindDuplicates(start, length, fileComparer);
            }
        }

        private void FindDuplicates(int start, int length, IFileInfoEqualityComparer fileInfoComparer)
        {
            try
            {
                if (length == 2)
                {
                    if (fileInfoComparer.Equals(_files[start], _files[start + 1]))
                        OnDuplicateFound(start, length);
                }
                else
                {
                    if (fileInfoComparer is IFileEqualityComprarer fileComparer)
                        GroupEqualAtTop(start, length, fileComparer);
                    else
                        GroupEqualAtTop(start, length, fileInfoComparer);
                }
            }
            catch (FileException ex)
            {
                OnFileError(ex);
            }
        }

        private void GroupEqualAtTop(int start, int length, IFileInfoEqualityComparer fileInfoComparer)
        {
            for (var i = 0; i < length;)
            {
                var equalFiles = 1;

                for (int j = i + 1; j < length; j++)
                {
                    var f1 = _files[start + i];
                    var f2 = _files[start + j];

                    if (fileInfoComparer.Equals(f1, f2))
                    {
                        equalFiles++;
                        _files.Swap(start + i, start + j);
                    }
                }

                if (equalFiles > 1)
                    OnDuplicateFound(start + i, equalFiles);

                i += equalFiles;
            }
        }

        private void GroupEqualAtTop(int start, int length, IFileEqualityComprarer fileComparer)
        {
            var fs = new FileStream[length];

            try
            {
                for (var i = 0; i < fs.Length; i++)
                    fs[i] = FileSystem.OpenFile(_files[start + i]);

                for (var i = 0; i < length;)
                {
                    var equalFiles = 1;

                    for (int j = i + 1; j < length; j++)
                    {
                        var f1 = _files[start + i];
                        var fs1 = fs[i];
                        var f2 = _files[start + j];
                        var fs2 = fs[j];

                        if (fileComparer.Equals(f1, fs1, f2, fs2))
                        {
                            equalFiles++;

                            _files.Swap(start + i, start + j);
                            fs.Swap(i, j);
                        }
                    }

                    if (equalFiles > 1)
                        OnDuplicateFound(start + i, equalFiles);

                    i += equalFiles;
                }
            }
            finally
            {
                for (var i = 0; i < fs.Length; i++)
                    if (fs[i] != null)
                        fs[i].Dispose();
            }
        }

        protected virtual void OnDuplicateFound(int start, int length)
        {
            for (int i = start; i < start + length; i++)
                Console.WriteLine(_files[i].FullName);

            Console.WriteLine();
        }

        protected virtual void OnDirectoryError(DirectoryException ex)
        {

        }

        protected virtual void OnFileError(FileException ex)
        {

        }
    }
}
