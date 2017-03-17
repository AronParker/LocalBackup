using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using LocalBackup.IO.FileEqualityComparers;
using LocalBackup.IO.Operations;

namespace LocalBackup.IO
{
    public class DirectoryMirrorer
    {
        private List<DirectoryInfo> _stack = new List<DirectoryInfo>();
        private Dictionary<string, FileSystemInfo> _srcLookup = new Dictionary<string, FileSystemInfo>(StringComparer.OrdinalIgnoreCase);
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
        public event FileSystemInfoErrorEventHandler Error;
        
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

        protected virtual void OnError(FileSystemInfoException ex)
        {
            Error?.Invoke(this, new FileSystemInfoErrorEventArgs(ex));
        }

        private static bool ContainsSpecialDirectoryAttributes(FileAttributes attributes)
        {
            return (attributes & ~(FileAttributes.Directory | FileAttributes.Archive | FileAttributes.ReparsePoint)) != 0;
        }

        private static bool AttributesEqual(FileAttributes attributes1, FileAttributes attributes2)
        {
            return ((attributes1 ^ attributes2) & ~(FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Normal | FileAttributes.ReparsePoint)) == 0;
        }

        private void InternalStart(DirectoryInfo srcDir, DirectoryInfo dstDir, IFileInfoEqualityComparer fileInfoComparer)
        {
            if (dstDir.Exists)
            {
                _stack.Clear();
                _stack.Add(dstDir);
                _stack.Add(srcDir);

                GetOperations(fileInfoComparer);
            }
            else
            {
                _copier.CopyDirectory(srcDir, dstDir);
            }
        }

        private void GetOperations(IFileInfoEqualityComparer fileInfoComparer)
        {
            while (_stack.Count > 0)
            {
                _token.ThrowIfCancellationRequested();

                var index = _stack.Count;
                var srcDir = _stack[--index];
                var dstDir = _stack[--index];

                _stack.RemoveRange(index, 2);

                if (!AttributesEqual(srcDir.Attributes, dstDir.Attributes))
                    OnOperationFound(new EditAttributesOperation(dstDir, srcDir.Attributes));

                GetDirectoryChanges(srcDir, dstDir, fileInfoComparer);

                if (_stack.Count > index)
                    _stack.Reverse(index, _stack.Count - index);
            }
        }

        private void GetDirectoryChanges(DirectoryInfo srcDir, DirectoryInfo dstDir, IFileInfoEqualityComparer fileInfoComparer)
        {
            BuildSourceLookup(srcDir);

            try
            {
                foreach (var dstFsi in dstDir.EnumerateFileSystemInfos())
                    ProcessDestinationFileSystemInfo(dstFsi, fileInfoComparer);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)
            {
                OnError(new FileSystemInfoException(dstDir, ex));
            }

            CopyRemainingFileSystemInfosTo(dstDir);
        }

        private void BuildSourceLookup(DirectoryInfo srcDir)
        {
            _srcLookup.Clear();

            try
            {
                foreach (var srcFsi in srcDir.EnumerateFileSystemInfos())
                    _srcLookup.Add(srcFsi.Name, srcFsi);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)
            {
                OnError(new FileSystemInfoException(srcDir, ex));
            }
        }

        private void ProcessDestinationFileSystemInfo(FileSystemInfo dstFsi, IFileInfoEqualityComparer fileInfoComparer)
        {
            var fsiExistsInSrc = _srcLookup.TryGetValue(dstFsi.Name, out var srcFsi);

            if (fsiExistsInSrc)
            {
                _srcLookup.Remove(dstFsi.Name);
                CompareFileSystenInfo(srcFsi, dstFsi, fileInfoComparer);
            }
            else
            {
                if (dstFsi is FileInfo dstFile)
                    OnOperationFound(new DeleteFileOperation(dstFile));
                else if (dstFsi is DirectoryInfo dstDir)
                    _deleter.DeleteDirectory(dstDir);
            }
        }

        private void CopyRemainingFileSystemInfosTo(DirectoryInfo dstDir)
        {                
            foreach (var dstFsi in _srcLookup.Values)
            {
                if (dstFsi is FileInfo srcFile)
                    OnOperationFound(new CopyFileOperation(srcFile, new FileInfo(Path.Combine(dstDir.FullName, srcFile.Name))));
                else if (dstFsi is DirectoryInfo srcDir)
                    _copier.CopyDirectory(srcDir, new DirectoryInfo(Path.Combine(dstDir.FullName, srcDir.Name)));
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
                        else if (!AttributesEqual(srcFile.Attributes, dstFile.Attributes))
                            OnOperationFound(new EditAttributesOperation(dstFile, srcFile.Attributes));
                    }
                    catch (FileSystemInfoException ex)
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

                if (ContainsSpecialDirectoryAttributes(srcDir.Attributes))
                    _detector.OnOperationFound(new EditAttributesOperation(dstDir, srcDir.Attributes));

                GetOperations();
            }

            private void GetOperations()
            {
                while (_stack.Count > 0)
                {
                    _detector._token.ThrowIfCancellationRequested();

                    var index = _stack.Count;
                    var srcDir = _stack[--index];
                    var dstDir = _stack[--index];

                    _stack.RemoveRange(index, 2);
                    GetCopyDirectoryOperations(srcDir, dstDir);

                    if (_stack.Count > index)
                        _stack.Reverse(index, _stack.Count - index);
                }
            }

            private void GetCopyDirectoryOperations(DirectoryInfo curSrcDir, DirectoryInfo curDstDir)
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

                            if (ContainsSpecialDirectoryAttributes(srcDir.Attributes))
                                _detector.OnOperationFound(new EditAttributesOperation(dstDir, srcDir.Attributes));

                            _stack.Add(srcDir);
                            _stack.Add(dstDir);
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException ||
                                           ex is UnauthorizedAccessException ||
                                           ex is SecurityException)
                {
                    _detector.OnError(new FileSystemInfoException(curSrcDir, ex));
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
                    _detector.OnError(new FileSystemInfoException(curDir, ex));
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
