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
        private Dictionary<string, FileSystemInfo> _dict = new Dictionary<string, FileSystemInfo>(StringComparer.OrdinalIgnoreCase);
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
            
            while (_stack.Count > 0)
            {
                _token.ThrowIfCancellationRequested();

                var index = _stack.Count;

                curSrcDir = _stack[--index];
                curDstDir = _stack[--index];

                _stack.RemoveRange(index, 2);
                
                if (FileSystem.ClearArchiveAttribute(curSrcDir.Attributes) != FileSystem.ClearArchiveAttribute(curDstDir.Attributes))
                    OnItemFound(new EditDirectoryOperation(curDstDir, curSrcDir.Attributes));

                _dict.Clear();
                try
                {
                    foreach (var dstFsi in curDstDir.EnumerateFileSystemInfos())
                        _dict.Add(dstFsi.Name, dstFsi);

                    try
                    {
                        foreach (var srcFsi in curSrcDir.EnumerateFileSystemInfos())
                        {
                            if (_dict.TryGetValue(srcFsi.Name, out var dstFsi))
                            {
                                _dict.Remove(srcFsi.Name);
                                
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
                            else
                            {
                                if (srcFsi is FileInfo srcFile)
                                    OnItemFound(new CopyFileOperation(srcFile, new FileInfo(Path.Combine(curDstDir.FullName, srcFile.Name))));
                                else if (srcFsi is DirectoryInfo srcDir)
                                    _copier.CopyDirectory(srcDir, new DirectoryInfo(Path.Combine(curDstDir.FullName, srcDir.Name)));
                            }
                        }
                    }
                    catch (Exception ex) when (ex is IOException ||
                                               ex is UnauthorizedAccessException ||
                                               ex is SecurityException)
                    {
                        OnItemFound(new DirectoryError(curSrcDir, ex));
                    }
                }
                catch (Exception ex) when (ex is IOException ||
                                           ex is UnauthorizedAccessException ||
                                           ex is SecurityException)
                {
                    OnItemFound(new DirectoryError(curDstDir, ex));
                }

                if (_stack.Count > index)
                    _stack.Reverse(index, _stack.Count - index);

                foreach (var dstFsi in _dict.Values)
                {
                    if (dstFsi is FileInfo fileInDst)
                        OnItemFound(new DeleteFileOperation(fileInDst));
                    else if (dstFsi is DirectoryInfo dirInDst)
                        _deleter.DeleteDirectory(dirInDst);
                }
            }
        }

        private struct DirectoryCopier
        {
            private DirectoryMirrorer _detector;
            private List<DirectoryInfo> _srcDirStack;
            private List<DirectoryInfo> _dstDirStack;

            public DirectoryCopier(DirectoryMirrorer detector)
            {
                _detector = detector;
                _srcDirStack = new List<DirectoryInfo>();
                _dstDirStack = new List<DirectoryInfo>();
            }
            
            public void CopyDirectory(DirectoryInfo curSrcDir, DirectoryInfo curDstDir)
            {
                _srcDirStack.Clear();
                _dstDirStack.Clear();

                _srcDirStack.Add(curSrcDir);
                _dstDirStack.Add(curDstDir);

                _detector.OnItemFound(new CreateDirectoryOperation(curDstDir));
                
                if (FileSystem.ClearArchiveAttribute(curSrcDir.Attributes) != FileAttributes.Directory)
                    _detector.OnItemFound(new EditDirectoryOperation(curDstDir, curSrcDir.Attributes));
                
                while (_srcDirStack.Count > 0)
                {
                    _detector._token.ThrowIfCancellationRequested();

                    var index = _srcDirStack.Count - 1;

                    curSrcDir = _srcDirStack[index];
                    curDstDir = _dstDirStack[index];

                    _srcDirStack.RemoveAt(index);
                    _dstDirStack.RemoveAt(index);

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

                                _srcDirStack.Add(srcDir);
                                _dstDirStack.Add(dstDir);
                            }
                        }
                    }
                    catch (Exception ex) when (ex is IOException ||
                                               ex is UnauthorizedAccessException ||
                                               ex is SecurityException)
                    {
                        _detector.OnItemFound(new DirectoryError(curSrcDir, ex));
                    }
                    
                    if (_srcDirStack.Count > index)
                    {
                        _srcDirStack.Reverse(index, _srcDirStack.Count - index);
                        _dstDirStack.Reverse(index, _dstDirStack.Count - index);
                    }
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

            public void DeleteDirectory(DirectoryInfo curDir)
            {
                _stack.Clear();
                _operationsStack.Clear();

                _stack.Add(curDir);
                _operationsStack.Add(new DestroyDirectoryOperation(curDir));

                while (_stack.Count > 0)
                {
                    _detector._token.ThrowIfCancellationRequested();

                    var dirIndex = _stack.Count - 1;

                    curDir = _stack[dirIndex];
                    _stack.RemoveAt(dirIndex);

                    var opIndex = _operationsStack.Count;

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

                    _operationsStack.Reverse(opIndex, _operationsStack.Count - opIndex);
                }

                var index = _operationsStack.Count;

                while (--index >= 0)
                {
                    _detector._token.ThrowIfCancellationRequested();

                    _detector.OnItemFound(_operationsStack[index]);
                }

                _operationsStack.Clear();
            }
        }
    }
}
