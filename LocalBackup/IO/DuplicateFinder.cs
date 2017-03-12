using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using LocalBackup.Extensions;
using LocalBackup.IO.FileComparers;

namespace LocalBackup.IO
{
    public class DuplicateFinder
    {
        private List<FileInfo> _files = new List<FileInfo>();
        private CancellationToken _token;
        private Task _task;
        private int _state;

        public event EventHandler StateChanged;
        public event FileInfoEventHandler FileAdded;
        public event FileInfoEnumerableEventHandler FilesProcessed;
        public event FileInfoEnumerableEventHandler DuplicateFound;
        public event ErrorEventHandler Error;

        public bool IsRunning => _task != null && !_task.IsCompleted;

        public DuplicateFinderState State
        {
            get => (DuplicateFinderState)Volatile.Read(ref _state);
            set
            {
                Volatile.Write(ref _state, (int)value);

                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Task RunAsync(IEnumerable<DirectoryInfo> dirs, IFileInfoEqualityComparer fileInfoComparer)
        {
            return RunAsync(dirs, fileInfoComparer, CancellationToken.None);
        }

        public Task RunAsync(IEnumerable<DirectoryInfo> dirs, IFileInfoEqualityComparer fileInfoComparer, CancellationToken token)
        {
            if (dirs == null)
                throw new ArgumentNullException(nameof(dirs));
            if (fileInfoComparer == null)
                throw new ArgumentNullException(nameof(fileInfoComparer));
            if (IsRunning)
                throw new InvalidOperationException("DuplicateFinder is already running.");

            _token = token;
            _task = Task.Run(() => InternalStart(dirs, fileInfoComparer));

            return _task;
        }

        protected virtual void OnFileAdded(FileInfo file)
        {
            FileAdded?.Invoke(this, new FileInfoEventArgs(file));
        }

        protected virtual void OnFileProcessed(IEnumerable<FileInfo> processed)
        {
            FilesProcessed?.Invoke(this, new FileInfoEnumerableEventArgs(processed));
        }

        protected virtual void OnDuplicateFound(IEnumerable<FileInfo> duplicates)
        {
            DuplicateFound?.Invoke(this, new FileInfoEnumerableEventArgs(duplicates));
        }

        protected virtual void OnError(Exception ex)
        {
            Error?.Invoke(this, new ErrorEventArgs(ex));
        }

        private void InternalStart(IEnumerable<DirectoryInfo> dirs, IFileInfoEqualityComparer fileComparer)
        {
            try
            {
                State = DuplicateFinderState.FindingDuplicates;
                new FileInfoEnumerator(this).AddDirectories(dirs);

                State = DuplicateFinderState.SortingFiles;
                _files.Sort((x, y) => y.Length.CompareTo(x.Length));

                State = DuplicateFinderState.FindingDuplicates;
                FindDuplicates(fileComparer);
            }
            finally
            {
                State = DuplicateFinderState.Idle;
            }
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
                        MarkAsDuplicates(start, length);
                }
                else
                {
                    if (fileInfoComparer is IFileEqualityComprarer fileComparer)
                        new MultiFileEqualityComparer(this, start, length, fileComparer).FindDuplicates();
                    else
                        GroupEqualFileInfosAtTop(start, length, fileInfoComparer);
                }
            }
            catch (FileException ex)
            {
                OnError(ex);
            }

            MarkAsProcessed(start, length);
        }

        private void GroupEqualFileInfosAtTop(int start, int length, IFileInfoEqualityComparer fileInfoComparer)
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
                    MarkAsDuplicates(start + i, equalFiles);

                i += equalFiles;
            }
        }

        private void MarkAsDuplicates(int start, int length)
        {
            OnDuplicateFound(CreateView(start, length));
        }

        private void MarkAsProcessed(int start, int length)
        {
            OnFileProcessed(CreateView(start, length));
        }

        private IEnumerable<FileInfo> CreateView(int start, int length)
        {
            var maxExclusive = start + length;
            
            for (var i = start; i < maxExclusive; i++)
                yield return _files[i];
        }

        private struct FileInfoEnumerator
        {
            private DuplicateFinder _df;
            private List<DirectoryInfo> _stack;

            public FileInfoEnumerator(DuplicateFinder df)
            {
                _df = df;
                _stack = new List<DirectoryInfo>();
            }
            
            public void AddDirectories(IEnumerable<DirectoryInfo> dirs)
            {
                foreach (DirectoryInfo dir in dirs)
                {
                    _df._token.ThrowIfCancellationRequested();

                    if (dir == null)
                    {
                        _df.OnError(new DirectoryException(dir, new ArgumentNullException()));
                        continue;
                    }

                    _stack.Add(dir);

                    while (_stack.Count > 0)
                    {
                        _df._token.ThrowIfCancellationRequested();

                        var index = _stack.Count - 1;
                        var curDir = _stack[index];
                        _stack.RemoveAt(index);

                        AddDirectory(curDir);

                        if (_stack.Count > index)
                            _stack.Reverse(index, _stack.Count - index);
                    }
                }
            }

            private void AddDirectory(DirectoryInfo curDir)
            {
                try
                {
                    foreach (var fsi in curDir.EnumerateFileSystemInfos())
                    {
                        _df._token.ThrowIfCancellationRequested();

                        if (fsi is FileInfo file)
                        {
                            _df._files.Add(file);
                            _df.OnFileAdded(file);
                        }
                        else if (fsi is DirectoryInfo dir)
                        {
                            _stack.Add(dir);
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException ||
                                           ex is UnauthorizedAccessException ||
                                           ex is SecurityException)
                {
                    _df.OnError(new DirectoryException(curDir, ex));
                }
            }
        }

        private struct MultiFileEqualityComparer
        {
            private DuplicateFinder _df;
            private int _start;
            private int _length;
            private IFileEqualityComprarer _fileComparer;

            private FileStream[] _fs;
            private int _fileStreams;

            public MultiFileEqualityComparer(DuplicateFinder df, int start, int length, IFileEqualityComprarer fileComparer)
            {
                _df = df;
                _start = start;
                _length = length;
                _fileComparer = fileComparer;

                _fs = new FileStream[length];
                _fileStreams = _fs.Length;
            }

            public void FindDuplicates()
            {
                try
                {
                    OpenFiles();

                    for (var i = 0; i < _fileStreams;)
                    {
                        int equalFiles = GetEqualFiles(i);

                        if (equalFiles > 1)
                            _df.MarkAsDuplicates(_start + i, equalFiles);

                        i += equalFiles;
                    }
                }
                finally
                {
                    CloseFiles();
                }
            }

            private void OpenFiles()
            {
                for (var i = 0; i < _fileStreams;)
                {
                    try
                    {
                        _fs[i] = FileSystem.OpenFile(_df._files[_start + i]);
                        i++;
                    }
                    catch (FileException ex)
                    {
                        _df.OnError(ex);

                        _fileStreams--;
                        _df._files.Swap(_start + i, _start + _fileStreams);
                    }
                }
            }

            private int GetEqualFiles(int i)
            {
                var f1 = GetFileInfo(i);
                var fs1 = GetFileStream(i);

                var equalFiles = 1;

                for (int j = i + 1; j < _fileStreams; j++)
                {
                    var f2 = GetFileInfo(j);
                    var fs2 = GetFileStream(j);

                    try
                    {
                        if (_fileComparer.Equals(f1, fs1, f2, fs2))
                        {
                            equalFiles++;

                            Swap(i, j);
                        }
                    }
                    catch (FileException ex) when (ex.File == f1)
                    {
                        _df.OnError(ex);
                        break;
                    }
                    catch (FileException ex) when (ex.File == f2)
                    {
                        _df.OnError(ex);
                        _fileStreams--;

                        if (j != _fileStreams)
                            Swap(j, _fileStreams);
                    }
                }

                return equalFiles;
            }

            private void CloseFiles()
            {
                for (var i = 0; i < _fs.Length; i++)
                    if (_fs[i] != null)
                        _fs[i].Dispose();
            }

            private FileInfo GetFileInfo(int index)
            {
                return _df._files[_start + index];
            }

            private FileStream GetFileStream(int index)
            {
                return _fs[index];
            }

            private void Swap(int i, int j)
            {
                _df._files.Swap(_start + i, _start + j);
                _fs.Swap(i, j);
            }
        }
    }
}
