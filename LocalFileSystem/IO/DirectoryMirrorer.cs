using LocalFileSystem.IO.Errors;
using LocalFileSystem.IO.FileComparers;
using LocalFileSystem.IO.Operations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace LocalFileSystem.IO
{
    public class DirectoryMirrorer
    {
        private const int BufferSize = 512 * 1024;

        private List<DirectoryInfo> _stack = new List<DirectoryInfo>();
        private Dictionary<string, FileSystemInfo> _dstLookup = new Dictionary<string, FileSystemInfo>(StringComparer.OrdinalIgnoreCase);
        private DirectoryCopier _copier;
        private DirectoryDeleter _deleter;
        
        private CancellationToken _token;
        private Task _task;
        
        public DirectoryMirrorer()
        {
            _copier = new DirectoryCopier(this);
            _deleter = new DirectoryDeleter(this);
        }

        public event FileSystemItemHandler ItemFound;
        
        public bool IsRunning => _task != null && !_task.IsCompleted;

        public Task RunAsync(DirectoryInfo srcDir, DirectoryInfo dstDir, IFileInfoEqualityComparer fileInfoComparer)
        {
            return RunAsync(srcDir, dstDir, fileInfoComparer, CancellationToken.None);
        }

        public Task RunAsync(DirectoryInfo srcDir, DirectoryInfo dstDir, IFileInfoEqualityComparer fileInfoComparer, CancellationToken token)
        {
            if (srcDir == null)
                throw new ArgumentNullException(nameof(srcDir));
            if (dstDir == null)
                throw new ArgumentNullException(nameof(dstDir));
            if (fileInfoComparer == null)
                throw new ArgumentNullException(nameof(fileInfoComparer));
            if (!srcDir.Exists)
                throw new DirectoryNotFoundException("Source directory does not exist: " + srcDir.FullName + ".");
            if (!dstDir.Exists)
                throw new DirectoryNotFoundException("Destination directory does not exist: " + dstDir.FullName + ".");
            
            _token = token;
            _task = Task.Run(() => InternalStart(srcDir, dstDir, fileInfoComparer));

            return _task;
        }

        protected virtual void OnItemFound(FileSystemItem item)
        {
            ItemFound?.Invoke(this, new FileSystemItemEventArgs(item));
        }

        private void InternalStart(DirectoryInfo curSrcDir, DirectoryInfo curDstDir, IFileInfoEqualityComparer fileInfoComparer)
        {
            _stack.Clear();
            _stack.Add(curDstDir);
            _stack.Add(curSrcDir);

            PostOperations(fileInfoComparer);

            _dstLookup.Clear();
        }

        private void PostOperations(IFileInfoEqualityComparer fileInfoComparer)
        {
            while (_stack.Count > 0)
            {
                _token.ThrowIfCancellationRequested();

                var index = _stack.Count;
                var srcDir = _stack[--index];
                var dstDir = _stack[--index];

                _stack.RemoveRange(index, 2);

                if (FileSystem.ClearArchiveAttribute(srcDir.Attributes) != FileSystem.ClearArchiveAttribute(dstDir.Attributes))
                    OnItemFound(new EditDirectoryOperation(dstDir, srcDir.Attributes));

                PostDirectoryChanges(srcDir, dstDir, fileInfoComparer);

                if (_stack.Count > index)
                    _stack.Reverse(index, _stack.Count - index);
            }
        }

        private void PostDirectoryChanges(DirectoryInfo srcDir, DirectoryInfo dstDir, IFileInfoEqualityComparer fileInfoComparer)
        {
            BuildDestinationLookup(dstDir);

            try
            {
                foreach (var srcFsi in srcDir.EnumerateFileSystemInfos())
                    ProcessSourceFileSystemInfo(srcFsi, dstDir, fileInfoComparer);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)
            {
                OnItemFound(new DirectoryError(srcDir, ex));
            }

            RemoveLeftovers();
        }

        private void BuildDestinationLookup(DirectoryInfo dstDir)
        {
            _dstLookup.Clear();

            try
            {
                foreach (var dstFsi in dstDir.EnumerateFileSystemInfos())
                    _dstLookup.Add(dstFsi.Name, dstFsi);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)
            {
                OnItemFound(new DirectoryError(dstDir, ex));
            }
        }

        private void ProcessSourceFileSystemInfo(FileSystemInfo srcFsi, DirectoryInfo dstDir, IFileInfoEqualityComparer fileInfoComparer)
        {
            var fsiExistsInDst = _dstLookup.TryGetValue(srcFsi.Name, out var dstFsi);

            if (fsiExistsInDst)
            {
                _dstLookup.Remove(srcFsi.Name);
                CompareFileSystenInfo(srcFsi, dstFsi, fileInfoComparer);
            }
            else
            {
                if (srcFsi is FileInfo srcFile)
                    OnItemFound(new CopyFileOperation(srcFile, new FileInfo(Path.Combine(dstDir.FullName, srcFile.Name))));
                else if (srcFsi is DirectoryInfo srcDir)
                    _copier.CopyDirectory(srcDir, new DirectoryInfo(Path.Combine(dstDir.FullName, srcDir.Name)));
            }
        }

        private void RemoveLeftovers()
        {
            foreach (var dstFsi in _dstLookup.Values)
            {
                if (dstFsi is FileInfo fileInDst)
                    OnItemFound(new DeleteFileOperation(fileInDst));
                else if (dstFsi is DirectoryInfo dirInDst)
                    _deleter.DeleteDirectory(dirInDst);
            }
        }

        private void CompareFileSystenInfo(FileSystemInfo srcFsi, FileSystemInfo dstFsi, IFileInfoEqualityComparer fileInfoComparer)
        {
            if (srcFsi is FileInfo srcFile)
            {
                if (dstFsi is FileInfo dstFile)
                {
                    try
                    {
                        if (!fileInfoComparer.Equals(srcFile, dstFile))
                            OnItemFound(new EditFileOperation(srcFile, dstFile));
                    }
                    catch (FileException ex)
                    {
                        OnItemFound(new FileError(ex.File, ex.InnerException));
                    }
                }
                else if (dstFsi is DirectoryInfo dstDir)
                {
                    _deleter.DeleteDirectory(dstDir);
                    OnItemFound(new CopyFileOperation(srcFile, new FileInfo(dstFsi.FullName)));
                }
            }
            else if (srcFsi is DirectoryInfo srcDir)
            {
                if (dstFsi is FileInfo dstFile)
                {
                    OnItemFound(new DeleteFileOperation(dstFile));

                    _copier.CopyDirectory(srcDir, new DirectoryInfo(dstFsi.FullName));
                }
                else if (dstFsi is DirectoryInfo dstDir)
                {
                    _stack.Add(srcDir);
                    _stack.Add(dstDir);
                }
            }
        }

        private struct DirectoryCopier
        {
            private DirectoryMirrorer _detector;
            private List<DirectoryInfo> _stack;

            public DirectoryCopier(DirectoryMirrorer detector)
            {
                _detector = detector;
                _stack = new List<DirectoryInfo>();
            }
            
            public void CopyDirectory(DirectoryInfo srcDir, DirectoryInfo dstDir)
            {
                _stack.Clear();
                _stack.Add(dstDir);
                _stack.Add(srcDir);

                _detector.OnItemFound(new CreateDirectoryOperation(dstDir));

                if (FileSystem.ClearArchiveAttribute(srcDir.Attributes) != FileAttributes.Directory)
                    _detector.OnItemFound(new EditDirectoryOperation(dstDir, srcDir.Attributes));

                PostOperations();
            }

            private void PostOperations()
            {
                while (_stack.Count > 0)
                {
                    _detector._token.ThrowIfCancellationRequested();

                    var index = _stack.Count;
                    var srcDir = _stack[--index];
                    var dstDir = _stack[--index];

                    _stack.RemoveRange(index, 2);
                    PostCopyDirectoryOperations(srcDir, dstDir);

                    if (_stack.Count > index)
                        _stack.Reverse(index, _stack.Count - index);
                }
            }

            private void PostCopyDirectoryOperations(DirectoryInfo curSrcDir, DirectoryInfo curDstDir)
            {
                try
                {
                    foreach (var srcFsi in curSrcDir.EnumerateFileSystemInfos())
                    {
                        if (srcFsi is FileInfo srcFile)
                        {
                            var dstFile = new FileInfo(Path.Combine(curDstDir.FullName, srcFsi.Name));
                            _detector.OnItemFound(new CopyFileOperation(srcFile, dstFile));
                        }
                        else if (srcFsi is DirectoryInfo srcDir)
                        {
                            var dstDir = new DirectoryInfo(Path.Combine(curDstDir.FullName, srcFsi.Name));

                            _detector.OnItemFound(new CreateDirectoryOperation(dstDir));

                            if (FileSystem.ClearArchiveAttribute(srcDir.Attributes) != FileAttributes.Directory)
                                _detector.OnItemFound(new EditDirectoryOperation(dstDir, srcDir.Attributes));

                            _stack.Add(srcDir);
                            _stack.Add(dstDir);
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException ||
                                           ex is UnauthorizedAccessException ||
                                           ex is SecurityException)
                {
                    _detector.OnItemFound(new DirectoryError(curSrcDir, ex));
                }
            }
        }

        private struct DirectoryDeleter
        {
            private DirectoryMirrorer _detector;
            private List<DirectoryInfo> _stack;
            private List<FileSystemOperation> _operationsStack;

            public DirectoryDeleter(DirectoryMirrorer detector)
            {
                _detector = detector;
                _stack = new List<DirectoryInfo>();
                _operationsStack = new List<FileSystemOperation>();
            }

            public void DeleteDirectory(DirectoryInfo dir)
            {
                _stack.Clear();
                _operationsStack.Clear();

                _stack.Add(dir);
                _operationsStack.Add(new DestroyDirectoryOperation(dir));

                GetOperations();
                PostOperations();
            }

            private void GetOperations()
            {
                while (_stack.Count > 0)
                {
                    _detector._token.ThrowIfCancellationRequested();

                    var curDir = _stack[_stack.Count - 1];
                    _stack.RemoveAt(_stack.Count - 1);

                    var index = _operationsStack.Count;
                    GetDeleteDirectoryOperations(curDir);

                    _operationsStack.Reverse(index, _operationsStack.Count - index);
                }
            }

            private void GetDeleteDirectoryOperations(DirectoryInfo curDir)
            {
                try
                {
                    foreach (var fsi in curDir.EnumerateFileSystemInfos())
                    {
                        if (fsi is FileInfo file)
                        {
                            _operationsStack.Add(new DeleteFileOperation(file));
                        }
                        else if (fsi is DirectoryInfo dir)
                        {
                            _stack.Add(dir);
                            _operationsStack.Add(new DestroyDirectoryOperation(dir));
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException ||
                                           ex is UnauthorizedAccessException ||
                                           ex is SecurityException)
                {
                    _detector.OnItemFound(new DirectoryError(curDir, ex));
                }
            }

            private void PostOperations()
            {
                for (var i = _operationsStack.Count-1; i >= 0; i--)
                {
                    _detector._token.ThrowIfCancellationRequested();
                    _detector.OnItemFound(_operationsStack[i]);
                }

                _operationsStack.Clear();
            }
        }
    }
}
