using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using LocalBackup.IO.FileComparers;
using LocalBackup.IO.Operations;

namespace LocalBackup.IO
{
    public class DirectoryMirrorer
    {
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

        public event FileSystemOperationHandler OperationFound;
        public event ErrorEventHandler Error;
        
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
            if (IsRunning)
                throw new InvalidOperationException("DirectoryMirrorer is already running.");
            
            _token = token;
            _task = Task.Run(() => InternalStart(srcDir, dstDir, fileInfoComparer));

            return _task;
        }

        protected virtual void OnOperationFound(FileSystemOperation operation)
        {
            OperationFound?.Invoke(this, new FileSystemOperationEventArgs(operation));
        }

        protected virtual void OnError(Exception ex)
        {
            Error?.Invoke(this, new ErrorEventArgs(ex));
        }

        private void InternalStart(DirectoryInfo srcDir, DirectoryInfo dstDir, IFileInfoEqualityComparer fileInfoComparer)
        {
            if (dstDir.Exists)
            {
                _stack.Clear();
                _stack.Add(dstDir);
                _stack.Add(srcDir);

                PostOperations(fileInfoComparer);

                _dstLookup.Clear();
            }
            else
            {
                _copier.CopyDirectory(srcDir, dstDir);
            }
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
                    OnOperationFound(new EditDirectoryOperation(dstDir, srcDir.Attributes));

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
                OnError(new DirectoryException(srcDir, ex));
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
                OnError(new DirectoryException(dstDir, ex));
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
                    OnOperationFound(new CopyFileOperation(srcFile, new FileInfo(Path.Combine(dstDir.FullName, srcFile.Name))));
                else if (srcFsi is DirectoryInfo srcDir)
                    _copier.CopyDirectory(srcDir, new DirectoryInfo(Path.Combine(dstDir.FullName, srcDir.Name)));
            }
        }

        private void RemoveLeftovers()
        {
            foreach (var dstFsi in _dstLookup.Values)
            {
                if (dstFsi is FileInfo fileInDst)
                    OnOperationFound(new DeleteFileOperation(fileInDst));
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
                            OnOperationFound(new EditFileOperation(srcFile, dstFile));
                    }
                    catch (FileException ex)
                    {
                        OnError(ex);
                    }
                }
                else if (dstFsi is DirectoryInfo dstDir)
                {
                    _deleter.DeleteDirectory(dstDir);
                    OnOperationFound(new CopyFileOperation(srcFile, new FileInfo(dstFsi.FullName)));
                }
            }
            else if (srcFsi is DirectoryInfo srcDir)
            {
                if (dstFsi is FileInfo dstFile)
                {
                    OnOperationFound(new DeleteFileOperation(dstFile));

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

                _detector.OnOperationFound(new CreateDirectoryOperation(dstDir));

                if (FileSystem.ClearArchiveAttribute(srcDir.Attributes) != FileAttributes.Directory)
                    _detector.OnOperationFound(new EditDirectoryOperation(dstDir, srcDir.Attributes));

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
                            _detector.OnOperationFound(new CopyFileOperation(srcFile, dstFile));
                        }
                        else if (srcFsi is DirectoryInfo srcDir)
                        {
                            var dstDir = new DirectoryInfo(Path.Combine(curDstDir.FullName, srcFsi.Name));

                            _detector.OnOperationFound(new CreateDirectoryOperation(dstDir));

                            if (FileSystem.ClearArchiveAttribute(srcDir.Attributes) != FileAttributes.Directory)
                                _detector.OnOperationFound(new EditDirectoryOperation(dstDir, srcDir.Attributes));

                            _stack.Add(srcDir);
                            _stack.Add(dstDir);
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException ||
                                           ex is UnauthorizedAccessException ||
                                           ex is SecurityException)
                {
                    _detector.OnError(new DirectoryException(curSrcDir, ex));
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
                    _detector.OnError(new DirectoryException(curDir, ex));
                }
            }

            private void PostOperations()
            {
                for (var i = _operationsStack.Count-1; i >= 0; i--)
                {
                    _detector._token.ThrowIfCancellationRequested();
                    _detector.OnOperationFound(_operationsStack[i]);
                }

                _operationsStack.Clear();
            }
        }
    }
}
