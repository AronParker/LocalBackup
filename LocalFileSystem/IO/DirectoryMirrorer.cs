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
    public class ChangesDetector
    {
        private const int BufferSize = 512 * 1024;

        private List<DirectoryInfo> _stack = new List<DirectoryInfo>();
        private Dictionary<string, FileSystemInfo> _dict = new Dictionary<string, FileSystemInfo>(StringComparer.OrdinalIgnoreCase);
        private DirectoryCopier _copier;
        private DirectoryDeleter _deleter;
        
        private CancellationToken _token;
        private Task _task;
        
        public ChangesDetector()
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

        private void InternalStart(DirectoryInfo srcDir, DirectoryInfo dstDir, IFileInfoEqualityComparer fileInfoComparer)
        {
            _stack.Clear();
            _stack.Add(dstDir);
            _stack.Add(srcDir);

            int index;

            while ((index = _stack.Count) > 0)
            {
                _token.ThrowIfCancellationRequested();

                srcDir = _stack[--index];
                dstDir = _stack[--index];

                _stack.RemoveRange(index, 2);
                
                if (FileSystem.ClearArchiveAttribute(srcDir.Attributes) != FileSystem.ClearArchiveAttribute(dstDir.Attributes))
                    OnItemFound(new EditDirectoryOperation(dstDir, srcDir.Attributes));

                _dict.Clear();
                try
                {
                    foreach (var fsiInDst in dstDir.EnumerateFileSystemInfos())
                        _dict.Add(fsiInDst.Name, fsiInDst);

                    try
                    {
                        foreach (var fsiInSrc in srcDir.EnumerateFileSystemInfos())
                        {
                            if (_dict.TryGetValue(fsiInSrc.Name, out var fsiInDst))
                            {
                                _dict.Remove(fsiInSrc.Name);
                                
                                if (fsiInSrc is FileInfo fileInSrc)
                                {
                                    if (fsiInDst is FileInfo fileInDst)
                                    {
                                        try
                                        {
                                            if (!fileInfoComparer.Equals(fileInSrc, fileInDst))
                                                OnItemFound(new EditFileOperation(fileInSrc, fileInDst));
                                        }
                                        catch (FileException ex)
                                        {
                                            OnItemFound(new FileError(ex.File, ex.InnerException));
                                        }
                                    }
                                    else if (fsiInDst is DirectoryInfo dirInDst)
                                    {
                                        _deleter.DeleteDirectory(dirInDst);
                                        OnItemFound(new CopyFileOperation(fileInSrc, new FileInfo(fsiInDst.FullName)));
                                    }
                                }
                                else if (fsiInSrc is DirectoryInfo dirInSrc)
                                {
                                    if (fsiInDst is FileInfo fileInDst)
                                    {
                                        OnItemFound(new DeleteFileOperation(fileInDst));

                                        _copier.CopyDirectory(dirInSrc, new DirectoryInfo(fsiInDst.FullName));
                                    }
                                    else if (fsiInDst is DirectoryInfo dirInDst)
                                    {                                        
                                        _stack.Add(dirInSrc);
                                        _stack.Add(dirInDst);
                                    }
                                }
                            }
                            else
                            {
                                if (fsiInSrc is FileInfo fileInSrc)
                                    OnItemFound(new CopyFileOperation(fileInSrc, new FileInfo(Path.Combine(dstDir.FullName, fileInSrc.Name))));
                                else if (fsiInSrc is DirectoryInfo dirInSrc)
                                    _copier.CopyDirectory(dirInSrc, new DirectoryInfo(Path.Combine(dstDir.FullName, dirInSrc.Name)));
                            }
                        }
                    }
                    catch (Exception ex) when (ex is IOException ||
                                               ex is UnauthorizedAccessException ||
                                               ex is SecurityException)
                    {
                        OnItemFound(new DirectoryError(srcDir, ex));
                    }
                }
                catch (Exception ex) when (ex is IOException ||
                                           ex is UnauthorizedAccessException ||
                                           ex is SecurityException)
                {
                    OnItemFound(new DirectoryError(dstDir, ex));
                }

                if (_stack.Count > index)
                    _stack.Reverse(index, _stack.Count - index);

                foreach (var fsiInDst in _dict.Values)
                {
                    if (fsiInDst is FileInfo fileInDst)
                        OnItemFound(new DeleteFileOperation(fileInDst));
                    else if (fsiInDst is DirectoryInfo dirInDst)
                        _deleter.DeleteDirectory(dirInDst);
                }
            }
        }

        private struct DirectoryCopier
        {
            private ChangesDetector _detector;
            private List<DirectoryInfo> _srcDirStack;
            private List<DirectoryInfo> _dstDirStack;

            public DirectoryCopier(ChangesDetector detector)
            {
                _detector = detector;
                _srcDirStack = new List<DirectoryInfo>();
                _dstDirStack = new List<DirectoryInfo>();
            }

            public void CopyDirectory(DirectoryInfo srcDir, DirectoryInfo dstDir)
            {
                _srcDirStack.Clear();
                _dstDirStack.Clear();

                _srcDirStack.Add(srcDir);
                _dstDirStack.Add(dstDir);

                _detector.OnItemFound(new CreateDirectoryOperation(dstDir));
                
                if (FileSystem.ClearArchiveAttribute(srcDir.Attributes) != FileAttributes.Directory)
                    _detector.OnItemFound(new EditDirectoryOperation(dstDir, srcDir.Attributes));
                
                while (_srcDirStack.Count > 0)
                {
                    _detector._token.ThrowIfCancellationRequested();

                    var index = _srcDirStack.Count - 1;

                    srcDir = _srcDirStack[index];
                    dstDir = _dstDirStack[index];

                    _srcDirStack.RemoveAt(index);
                    _dstDirStack.RemoveAt(index);

                    try
                    {
                        foreach (var fsiInSrc in srcDir.EnumerateFileSystemInfos())
                        {
                            if (fsiInSrc is FileInfo fileInSrc)
                            {
                                var fileInDst = new FileInfo(Path.Combine(dstDir.FullName, fsiInSrc.Name));
                                _detector.OnItemFound(new CopyFileOperation(fileInSrc, fileInDst));
                            }
                            else if (fsiInSrc is DirectoryInfo dirInSrc)
                            {
                                var dirInDst = new DirectoryInfo(Path.Combine(dstDir.FullName, fsiInSrc.Name));

                                _detector.OnItemFound(new CreateDirectoryOperation(dirInDst));

                                if (FileSystem.ClearArchiveAttribute(dirInSrc.Attributes) != FileAttributes.Directory)
                                    _detector.OnItemFound(new EditDirectoryOperation(dirInDst, dirInSrc.Attributes));

                                _srcDirStack.Add(dirInSrc);
                                _dstDirStack.Add(dirInDst);
                            }
                        }
                    }
                    catch (Exception ex) when (ex is IOException ||
                                               ex is UnauthorizedAccessException ||
                                               ex is SecurityException)
                    {
                        _detector.OnItemFound(new DirectoryError(srcDir, ex));
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
            private ChangesDetector _detector;
            private List<DirectoryInfo> _srcDirStack;
            private List<FileSystemOperation> _opStack;

            public DirectoryDeleter(ChangesDetector detector)
            {
                _detector = detector;
                _srcDirStack = new List<DirectoryInfo>();
                _opStack = new List<FileSystemOperation>();
            }

            public void DeleteDirectory(DirectoryInfo srcDir)
            {
                _srcDirStack.Clear();
                _opStack.Clear();

                _srcDirStack.Add(srcDir);
                _opStack.Add(new DestroyDirectoryOperation(srcDir));

                while (_srcDirStack.Count > 0)
                {
                    _detector._token.ThrowIfCancellationRequested();

                    var srcDirIndex = _srcDirStack.Count - 1;

                    srcDir = _srcDirStack[srcDirIndex];
                    _srcDirStack.RemoveAt(srcDirIndex);

                    var opIndex = _opStack.Count;

                    try
                    {
                        foreach (var fsiInSrc in srcDir.EnumerateFileSystemInfos())
                        {
                            if (fsiInSrc is FileInfo fileInSrc)
                            {
                                _opStack.Add(new DeleteFileOperation(fileInSrc));
                            }
                            else if (fsiInSrc is DirectoryInfo dirInSrc)
                            {
                                _srcDirStack.Add(dirInSrc);
                                _opStack.Add(new DestroyDirectoryOperation(dirInSrc));
                            }
                        }
                    }
                    catch (Exception ex) when (ex is IOException ||
                                               ex is UnauthorizedAccessException ||
                                               ex is SecurityException)
                    {
                        _detector.OnItemFound(new DirectoryError(srcDir, ex));
                    }

                    _opStack.Reverse(opIndex, _opStack.Count - opIndex);
                }

                var index = _opStack.Count;

                while (--index >= 0)
                {
                    _detector._token.ThrowIfCancellationRequested();

                    _detector.OnItemFound(_opStack[index]);
                }

                _opStack.Clear();
            }
        }
    }
}
