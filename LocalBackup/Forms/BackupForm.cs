using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LocalBackup.IO;
using LocalBackup.IO.FileComparers;
using LocalBackup.IO.Operations;

namespace LocalBackup.Forms
{
    public partial class BackupForm : Form
    {
        private const int MinRefreshInterval = 500;

        private static Color s_red = Color.FromArgb(0xFF, 0xE0, 0xE0);
        private static Color s_yellow = Color.FromArgb(0xFF, 0xFF, 0xE0);
        private static Color s_green = Color.FromArgb(0xE0, 0xFF, 0xE0);

        private List<ListViewItem> _items = new List<ListViewItem>();
        private BackupFormState _state;

        private FindChangesTask _findChangesTask;

        private Task _currentTask;
        
        public BackupForm()
        {
            InitializeComponent();

            _modeComboBox.SelectedIndex = 0;
            SetState(BackupFormState.Idle);

            _findChangesTask = new FindChangesTask(this);
        }

        private enum BackupFormState
        {
            Idle,
            FindingChanges,
            ReviewChanges,
            PerformingChanges,
            Canceling,
            Done,
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            switch (_state)
            {
                case BackupFormState.FindingChanges:
                    e.Cancel = true;

                    if (MessageBox.Show("Are you sure you want to cancel finding changes?", "Confirm Cancelation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        return;

                    _findChangesTask.Cancel();
                    await _currentTask;
                    Close();
                    break;
                case BackupFormState.PerformingChanges:
                    e.Cancel = true;

                    break;
                case BackupFormState.Canceling:
                    e.Cancel = true;
                    break;
                default:
                    break;
            }
        }

        private void SetState(BackupFormState state)
        {
            _state = state;

            SuspendLayout();
            switch (state)
            {
                case BackupFormState.Idle:
                    Text = "Local Backup";
                    
                    UpdateHeader(true);

                    _operationsListViewEx.VirtualListSize = 0;
                    _items.Clear();

                    _progressBar.Style = ProgressBarStyle.Continuous;
                    _progressBar.Value = 0;
                    _okButton.Enabled = true;
                    _okButton.Text = "Start";
                    _cancelButton.Enabled = true;
                    _cancelButton.Text = "Close";
                    break;
                case BackupFormState.FindingChanges:
                    Text = "Local Backup - Finding changes";

                    UpdateHeader(false);

                    _progressBar.Style = ProgressBarStyle.Marquee;
                    _okButton.Enabled = false;
                    _okButton.Text = "Finding changes...";
                    _cancelButton.Enabled = true;
                    _cancelButton.Text = "Cancel";
                    break;
                case BackupFormState.ReviewChanges:
                    Text = "Local Backup - Reviewing changes";

                    UpdateHeader(false);

                    _progressBar.Style = ProgressBarStyle.Continuous;
                    _okButton.Enabled = true;
                    _okButton.Text = "Perform changes";
                    _cancelButton.Enabled = true;
                    _cancelButton.Text = "Discard changes";
                    break;
                case BackupFormState.PerformingChanges:
                    Text = "Backup Utility - Performing changes";

                    UpdateHeader(false);

                    _progressBar.Style = ProgressBarStyle.Continuous;
                    _okButton.Enabled = false;
                    _okButton.Text = "Performing changes...";
                    _cancelButton.Enabled = true;
                    _cancelButton.Text = "Cancel";
                    break;
                case BackupFormState.Canceling:
                    Text = "Backup Utility - Canceling...";

                    UpdateHeader(false);

                    _progressBar.Style = ProgressBarStyle.Marquee;
                    _okButton.Enabled = false;
                    _okButton.Text = "Please wait...";
                    _cancelButton.Enabled = false;
                    _cancelButton.Text = "Canceling...";
                    break;
                case BackupFormState.Done:
                    UpdateHeader(false);

                    _progressBar.Style = ProgressBarStyle.Continuous;
                    _okButton.Enabled = true;
                    _okButton.Text = "Start new backup";
                    _cancelButton.Enabled = true;
                    _cancelButton.Text = "Close";
                    break;
            }
            ResumeLayout();
        }

        private void UpdateHeader(bool enabled)
        {
            _sourceLabel.Enabled = enabled;
            _sourceTextBox.Enabled = enabled;
            _sourceButton.Enabled = enabled;
            _destinationLabel.Enabled = enabled;
            _destinationTextBox.Enabled = enabled;
            _destinationButton.Enabled = enabled;
            _modeLabel.Enabled = enabled;
            _modeComboBox.Enabled = enabled;
        }
        
        private void Browse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (sender == _sourceButton)
                {
                    fbd.SelectedPath = _sourceTextBox.Text;
                    fbd.Description = "Select source directory:";
                    fbd.ShowNewFolderButton = false;
                }
                else if (sender == _destinationButton)
                {
                    fbd.SelectedPath = _destinationTextBox.Text;
                    fbd.Description = "Select destination directory:";
                    fbd.ShowNewFolderButton = true;
                }

                if (fbd.ShowDialog() != DialogResult.OK)
                    return;

                if (sender == _sourceButton)
                    _sourceTextBox.Text = fbd.SelectedPath;
                else if (sender == _destinationButton)
                    _destinationTextBox.Text = fbd.SelectedPath;
            }
        }

        private void OperationsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = _items[e.ItemIndex];
        }

        private async void OkButton_Click(object sender, EventArgs e)
        {
            switch (_state)
            {
                case BackupFormState.Idle:
                    if (!_findChangesTask.SetSourceDirectory(_sourceTextBox.Text))
                        return;

                    if (!_findChangesTask.SetDestinationDirectory(_destinationTextBox.Text))
                        return;

                    if (!_findChangesTask.FindComparer(_modeComboBox.SelectedIndex == 0))
                        return;

                    _currentTask = _findChangesTask.Run();
                    await _currentTask;
                    break;
                case BackupFormState.ReviewChanges:
                    break;
                case BackupFormState.Done:
                    SetState(BackupFormState.Idle);
                    break;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            switch (_state)
            {
                case BackupFormState.Idle:
                case BackupFormState.Done:
                    Close();
                    break;
                case BackupFormState.FindingChanges:
                    _findChangesTask.Cancel();
                    break;
                case BackupFormState.ReviewChanges:
                    SetState(BackupFormState.Idle);
                    break;
                case BackupFormState.PerformingChanges:
                    break;
                case BackupFormState.Canceling:
                    break;
                default:
                    break;
            }
        }

        private struct FindChangesTask
        {
            private BackupForm _backupForm;
            private BufferedDirectoryMirrorer _mirrorer;
            private DirectoryInfo _sourceDirectory;
            private DirectoryInfo _destinationDirectory;
            private IFileInfoEqualityComparer _fileInfoComparer;
            private CancellationTokenSource _cts;

            public FindChangesTask(BackupForm backupForm) : this()
            {
                _backupForm = backupForm;
                _mirrorer = new BufferedDirectoryMirrorer();

                _mirrorer.QueueFlushRequested += Mirrorer_QueueFlushRequested;
            }


            public bool SetSourceDirectory(string sourceDirectory)
            {
                try
                {
                    _sourceDirectory = new DirectoryInfo(sourceDirectory);
                }
                catch (Exception ex) when (ex is ArgumentException ||
                                           ex is PathTooLongException ||
                                           ex is SecurityException)

                {
                    MessageBox.Show("The source directory you specified is invalid: " + ex.Message,
                                    "Source directory invalid",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return false;
                }

                if (!_sourceDirectory.Exists)
                {
                    MessageBox.Show("The source directory you specified does not exist.",
                                    "Source directory not found",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }

            public bool SetDestinationDirectory(string destinationDirectory)
            {
                try
                {
                    _destinationDirectory = new DirectoryInfo(destinationDirectory);
                    return true;
                }
                catch (Exception ex) when (ex is ArgumentException ||
                                           ex is PathTooLongException ||
                                           ex is SecurityException)

                {
                    MessageBox.Show("The destination directory you specified is invalid: " + ex.Message,
                                    "Destination directory invalid",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return false;
                }
            }

            public bool FindComparer(bool quickScan)
            {
                if (!quickScan)
                {
                    _fileInfoComparer = new FileComparer();
                    return true;
                }

                var fileSystem = GetDestinationFileSystem();

                if (fileSystem == null)
                {
                    MessageBox.Show("Failed to detect destination file system.",
                                    "Unknown file system",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return false;
                }

                _fileInfoComparer = CreateQuickScanComparer(fileSystem);

                if (_fileInfoComparer == null)
                {
                    MessageBox.Show("Destination directory uses a file system where quick scan is not supported.",
                                    "Unsupported file system",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }

            public async Task Run()
            {
                using (_cts = new CancellationTokenSource())
                {
                    try
                    {
                        _backupForm.SetState(BackupFormState.FindingChanges);

                        await _mirrorer.RunAsync(_sourceDirectory, _destinationDirectory, _fileInfoComparer, _cts.Token);

                        if (_mirrorer.ProcessingQueue.Count > 0)
                            FlushQueue();
                        
                        _backupForm.SetState(BackupFormState.ReviewChanges);
                    }
                    catch (OperationCanceledException)
                    {
                        _backupForm.Text = "Backup Utility - Canceled";
                        _backupForm.SetState(BackupFormState.Done);
                    }
                }
            }

            public void Cancel()
            {
                _cts.Cancel();
                _backupForm.SetState(BackupFormState.Canceling);
            }

            private string GetDestinationFileSystem()
            {
                try
                {
                    return new DriveInfo(_destinationDirectory.FullName).DriveFormat;
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

            private IFileInfoEqualityComparer CreateQuickScanComparer(string fileSystem)
            {
                switch (fileSystem)
                {
                    case "FAT32":
                    case "exFAT":
                        return new FATFileComparer();
                    case "NTFS":
                        return new NTFSFileComparer();
                    default:
                        return null;
                }
            }

            private void FlushQueue()
            {
                Debug.Assert(_mirrorer.ProcessingQueue.Count > 0);

                foreach (var item in _mirrorer.ProcessingQueue)
                {
                    ListViewItem lvi;

                    switch (item)
                    {
                        case FileSystemOperation op:
                            lvi = new ListViewItem(new string[] { op.Name, op.FileName, op.FilePath, string.Empty });

                            switch (op.Type)
                            {
                                case FileSystemOperationType.CreateDirectory:
                                case FileSystemOperationType.CopyFile:
                                    lvi.BackColor = s_green;
                                    break;
                                case FileSystemOperationType.EditDirectory:
                                case FileSystemOperationType.EditFile:
                                    lvi.BackColor = s_yellow;
                                    break;
                                case FileSystemOperationType.DestroyDirectory:
                                case FileSystemOperationType.DeleteFile:
                                    lvi.BackColor = s_red;
                                    break;
                            }

                            lvi.ImageIndex = (int)op.Type;
                            lvi.Tag = op;
                            break;
                        case FileException ex:
                            lvi = new ListViewItem(new string[] { "File error", ex.File.Name, ex.File.FullName, ex.Message });
                            lvi.BackColor = s_red;
                            lvi.ImageIndex = 6;
                            lvi.Tag = ex;
                            break;
                        case DirectoryException ex:
                            lvi = new ListViewItem(new string[] { "Directory error", ex.Directory.Name, ex.Directory.FullName, ex.Message });
                            lvi.BackColor = s_red;
                            lvi.ImageIndex = 7;
                            lvi.Tag = ex;
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    _backupForm._items.Add(lvi);
                }

                _mirrorer.ProcessingQueue.Clear();
                _backupForm._operationsListViewEx.VirtualListSize = _backupForm._items.Count;
            }

            private void Mirrorer_QueueFlushRequested(object sender, EventArgs e)
            {
                if (_backupForm.InvokeRequired)
                    _backupForm.Invoke((MethodInvoker)FlushQueue);
                else
                    FlushQueue();
            }

            private class BufferedDirectoryMirrorer : DirectoryMirrorer
            {
                private DateTimeOffset _lastUpdate = DateTimeOffset.MinValue;

                public event EventHandler QueueFlushRequested;

                public Queue<object> ProcessingQueue { get; } = new Queue<object>();

                protected override void OnOperationFound(FileSystemOperation operation)
                {
                    EnqueueItem(operation);
                }

                protected override void OnError(Exception ex)
                {
                    EnqueueItem(ex);
                }

                private void EnqueueItem(object item)
                {
                    ProcessingQueue.Enqueue(item);

                    var now = DateTimeOffset.UtcNow;

                    if ((now - _lastUpdate).TotalMilliseconds >= MinRefreshInterval)
                    {
                        QueueFlushRequested?.Invoke(this, EventArgs.Empty);

                        _lastUpdate = now;
                    }
                }
            }
        }
    }
}
