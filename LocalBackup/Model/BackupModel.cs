using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LocalBackup.IO.FileComparers;

namespace LocalBackup.Model
{
    public class BackupModel
    {
        private string _title;
        private BackupFormState _state;
        private BufferedDirectoryMirrorer _mirrorer = new BufferedDirectoryMirrorer();

        private CancellationTokenSource _cts;
        private Task _task;

        public BackupModel()
        {
            _mirrorer = new BufferedDirectoryMirrorer();
            _mirrorer.QueueFlushRequested += Mirrorer_QueueFlushRequested;
        }

        private void Mirrorer_QueueFlushRequested(object sender, EventArgs e)
        {
            QueueFlushRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TitleChanged;
        public event EventHandler StateChanged;
        public event EventHandler QueueFlushRequested;

        public List<ListViewItem> Items { get; } = new List<ListViewItem>();
        public Queue<object> ProcessingQueue => _mirrorer.ProcessingQueue;

        public string Title
        {
            get => _title;
            private set
            {
                _title = value;
                TitleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public BackupFormState State
        {
            get => _state;
            private set
            {
                _state = value;
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task FindChanges(string srcPath, string dstPath, bool quickScan)
        {
            var srcDir = FindSourceDirectory(srcPath);

            if (srcDir == null)
                return;

            var dstDir = FindDestinationDirectory(dstPath);

            if (dstDir == null)
                return;

            var fileInfoComparer = FindFileInfoEqualityComparer(quickScan, dstDir);

            if (fileInfoComparer == null)
                return;

            Title = "Local Backup - Finding changes...";
            State = BackupFormState.FindingChanges;

            using (_cts = new CancellationTokenSource())
            {
                try
                {
                    _task = _mirrorer.RunAsync(srcDir, dstDir, fileInfoComparer, _cts.Token);

                    await _task;

                    if (ProcessingQueue.Count > 0)
                        QueueFlushRequested?.Invoke(this, EventArgs.Empty);

                    Title = "Local Backup - Reviewing changes...";
                    State = BackupFormState.ReviewingChanges;
                }
                catch (OperationCanceledException)
                {
                    Title = "Backup Utility - Canceled";
                    State = BackupFormState.Done;
                }
            }
        }

        private DirectoryInfo FindSourceDirectory(string srcPath)
        {
            DirectoryInfo srcDir;

            try
            {
                srcDir = new DirectoryInfo(srcPath);
                srcDir.Refresh();
            }
            catch (Exception ex) when (ex is ArgumentException ||
                                       ex is NotSupportedException ||
                                       ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)

            {
                MessageBox.Show("The source directory you specified is invalid: " + ex.Message,
                                "Source directory invalid",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return null;
            }

            if (!srcDir.Exists)
            {
                MessageBox.Show("The source directory you specified does not exist.",
                                "Source directory not found",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return null;
            }

            return srcDir;
        }

        private DirectoryInfo FindDestinationDirectory(string dstPath)
        {
            DirectoryInfo dstDir;

            try
            {
                dstDir = new DirectoryInfo(dstPath);
                dstDir.Refresh();
            }
            catch (Exception ex) when (ex is ArgumentException ||
                                       ex is NotSupportedException ||
                                       ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException)

            {
                MessageBox.Show("The destination directory you specified is invalid: " + ex.Message,
                                "Destination directory invalid",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return null;
            }

            if (!dstDir.Exists)
            {
                if (MessageBox.Show("The destination directory you specified does not exist. Would you like to create it?",
                                    "Destination directory not found",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return null;
                }

                try
                {
                    dstDir.Create();
                    dstDir.Refresh(); // fill file attributes so no exception can be triggered later on
                }
                catch (Exception ex) when (ex is IOException ||
                                           ex is UnauthorizedAccessException ||
                                           ex is SecurityException)
                {
                    MessageBox.Show("Failed to create destination directory: " + ex.Message,
                                    "Failed to create destination directory",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return null;
                }
            }

            return dstDir;
        }

        private IFileInfoEqualityComparer FindFileInfoEqualityComparer(bool quickScan, DirectoryInfo dstDir)
        {
            if (!quickScan)
                return new FileComparer();

            try
            {
                var driveFormat = new DriveInfo(dstDir.FullName).DriveFormat;

                switch (driveFormat)
                {
                    case "FAT32":
                    case "exFAT":
                        return new FATFileComparer();
                    case "NTFS":
                        return new NTFSFileComparer();
                    default:
                        MessageBox.Show("Destination directory uses a file system where quick scan is not supported.",
                                        "Unsupported file system",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                        return null;
                }
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException)
            {
                MessageBox.Show("Failed to detect destination directory file system.",
                                "Failed to detect file system",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
