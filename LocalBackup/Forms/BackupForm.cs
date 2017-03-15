using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LocalBackup.Controls;
using LocalBackup.Extensions;
using LocalBackup.IO;
using LocalBackup.IO.FileEqualityComparers;
using LocalBackup.IO.Operations;
namespace LocalBackup.Forms
{
    public partial class BackupForm : Form
    {
        private const int MinRefreshInterval = 500;
        private const double DisplayTimeRemainingThreshold = .1;

        private const int CreateDirectoryImageIndex = 0;
        private const int DestroyDirectoryImageIndex = 1;
        private const int CopyFileImageIndex = 2;
        private const int EditFileImageIndex = 3;
        private const int EditAttributesImageIndex = 4;
        private const int DeleteFileImageIndex = 5;
        private const int DirectoryExceptionImageIndex = 6;
        private const int FileExceptionImageIndex = 7;

        private static Color s_red = Color.FromArgb(0xFF, 0xE0, 0xE0);
        private static Color s_yellow = Color.FromArgb(0xFF, 0xFF, 0xE0);
        private static Color s_green = Color.FromArgb(0xE0, 0xFF, 0xE0);

        private List<OperationItem> _operations = new List<OperationItem>();
        private List<ListViewItem> _errors = new List<ListViewItem>();
        private BackupFormState _state;
        
        private FindChangesTask _findChangesTask;
        private PerformChangesTask _performChangesTask;

        private Task _currentTask;
        private CancellationTokenSource _cts;
        
        public BackupForm()
        {
            InitializeComponent();
            
            SetState(BackupFormState.Idle);
            
            _findChangesTask = new FindChangesTask(this);
            _performChangesTask = new PerformChangesTask(this);

#if DEBUG
            _sourceTextBox.Text = @"C:\Users\Aron\Desktop\1";
            _destinationTextBox.Text = @"C:\Users\Aron\Desktop\2";
#endif
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
                case BackupFormState.PerformingChanges:
                    e.Cancel = true;

                    if (MessageBox.Show("Are you sure you want to cancel?", "Confirm Cancelation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        return;

                    _cts.Cancel();
                    await _currentTask;
                    Close();
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

                    _operationsListView.VirtualListSize = 0;
                    _operations.Clear();
                    _errorsListView.VirtualListSize = 0;
                    _errors.Clear();

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
            _quickScanToolStripMenuItem.Enabled = enabled;
        }

        private void OpenLink(string link)
        {
            try
            {
                using (Process.Start(link))
                {
                }
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show("Failed to open link: " + ex.Message, "Failed to open link", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProjectSiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLink("https://github.com/AronParker/LocalBackup");
        }

        private void ReportAnIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLink("https://github.com/AronParker/LocalBackup/issues");
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Made by Aron Parker", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            e.Item = _operations[e.ItemIndex];
        }

        private void ErrorsListViewEx_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = _errors[e.ItemIndex];
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

                    if (!_findChangesTask.FindComparer(_quickScanToolStripMenuItem.Checked))
                        return;

                    _findChangesTask.Init();

                    using (_cts = new CancellationTokenSource())
                    {
                        _currentTask = _findChangesTask.Run(_cts.Token);
                        await _currentTask;
                    }
                    break;
                case BackupFormState.ReviewChanges:
                    _performChangesTask.Init();

                    using (_cts = new CancellationTokenSource())
                    {
                        _currentTask = _performChangesTask.Run(_cts.Token);
                        await _currentTask;
                    }
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
                case BackupFormState.PerformingChanges:
                    _cts.Cancel();
                    SetState(BackupFormState.Canceling);
                    break;
                case BackupFormState.ReviewChanges:
                    SetState(BackupFormState.Idle);
                    break;
            }
        }

        private class FindChangesTask
        {
            private BackupForm _backupForm;
            private QueuedDirectoryMirrorer _mirrorer;
            private DirectoryInfo _sourceDirectory;
            private DirectoryInfo _destinationDirectory;
            private IFileInfoEqualityComparer _fileInfoComparer;

            public FindChangesTask(BackupForm backupForm)
            {
                _backupForm = backupForm;
                _mirrorer = new QueuedDirectoryMirrorer();
                _mirrorer.FlushRequested += Mirrorer_FlushRequested;
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
                bool AssumeNTFS()
                {
                    return MessageBox.Show("Failed to detect destination file system. Would you like to perform a quick scan anyway on the assumption that the destination file system is NTFS?",
                                           "Unknown file system",
                                           MessageBoxButtons.YesNo,
                                           MessageBoxIcon.Error) == DialogResult.Yes;
                }

                if (quickScan)
                {
                    var fileSystem = GetDestinationFileSystem();

                    if (fileSystem == null && !AssumeNTFS())
                        return false;

                    _fileInfoComparer = CreateQuickScanComparer(fileSystem);
                }
                else
                {
                    _fileInfoComparer = new DefaultFileEqualityComparer();
                }

                return true;
            }

            public void Init()
            {
                _mirrorer.Init();
            }

            public async Task Run(CancellationToken ct)
            {
                try
                {
                    _backupForm.SetState(BackupFormState.FindingChanges);

                    await _mirrorer.RunAsync(_sourceDirectory, _destinationDirectory, _fileInfoComparer, ct);

                    if (_mirrorer.ProcessingQueue.Count > 0)
                        Flush();

                    if (_backupForm._operations.Count == 0 && _backupForm._errors.Count == 0)
                    {
                        _backupForm.SetState(BackupFormState.Idle);
                        DisplayDirectoriesEqual();
                    }
                    else
                    {
                        _backupForm.SetState(BackupFormState.ReviewChanges);
                        DisplayErrors();
                    }
                }
                catch (OperationCanceledException)
                {
                    _backupForm.Text = "Backup Utility - Canceled";
                    _backupForm.SetState(BackupFormState.Done);
                }
            }

            private static void DisplayDirectoriesEqual()
            {
                MessageBox.Show("Source and destination directory are identical.",
                                "Info",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }

            private void DisplayErrors()
            {
                var errors = _backupForm._errors.Count;

                if (errors == 0)
                    return;

                MessageBox.Show($"{errors} error(s) occured while finding changes.", $"{errors} error(s) occured", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            private string GetDestinationFileSystem()
            {
                try
                {
                    return new DriveInfo(_destinationDirectory.FullName).DriveFormat;
                }
                catch (Exception ex) when (ex is ArgumentException ||
                                           ex is IOException ||
                                           ex is UnauthorizedAccessException)
                {
                    return null;
                }
            }

            private IFileInfoEqualityComparer CreateQuickScanComparer(string fileSystem)
            {
                switch (fileSystem)
                {
                    case "FAT":
                    case "FAT32":
                    case "exFAT":
                        return new FATFileEqualityComparer();
                    case "NTFS":
                    default:
                        return new DefaultFileInfoEqualityComparer();
                }
            }

            private void Flush()
            {
                var prevOperationCount = _backupForm._operations.Count;
                var prevErrorCount = _backupForm._errors.Count;

                ProcessQueue();

                var operationsAdded = _backupForm._operations.Count > prevOperationCount;
                var errorsAdded = _backupForm._errors.Count > prevErrorCount;

                if (operationsAdded)
                    _backupForm._operationsListView.VirtualListSize = _backupForm._operations.Count;

                if (errorsAdded)
                    _backupForm._errorsListView.VirtualListSize = _backupForm._errors.Count;
                
                if (_backupForm._autoScrollToolStripMenuItem.Checked)
                    AutoScroll(operationsAdded, errorsAdded);
            }

            private void ProcessQueue()
            {
                foreach (var item in _mirrorer.ProcessingQueue)
                {
                    switch (item)
                    {
                        case FileSystemOperation op:
                            _backupForm._operations.Add(new OperationItem(op));
                            break;
                        case DirectoryException ex:
                            _backupForm._errors.Add(new ListViewItem(new string[] { "Directory error", ex.Directory.Name, ex.Directory.FullName, ex.Message }, DirectoryExceptionImageIndex)
                            {
                                Tag = ex
                            });
                            break;
                        case FileException ex:
                            _backupForm._errors.Add(new ListViewItem(new string[] { "File error", ex.File.Name, ex.File.FullName, ex.Message }, FileExceptionImageIndex)
                            {
                                Tag = ex
                            });
                            break;
                    }
                }

                _mirrorer.ProcessingQueue.Clear();
            }

            private void AutoScroll(bool operationsAdded, bool errorsAdded)
            {
                if (operationsAdded)
                {
                    var lastOperation = _backupForm._operations.Count - 1;
                    _backupForm._operationsListView.EnsureVisible(lastOperation);
                }

                if (errorsAdded)
                {
                    var lastError = _backupForm._errors.Count - 1;
                    _backupForm._errorsListView.EnsureVisible(lastError);
                }
            }

            private void Mirrorer_FlushRequested(object sender, EventArgs e)
            {
                if (_backupForm.InvokeRequired)
                    _backupForm.Invoke((MethodInvoker)Flush);
                else
                    Flush();
            }

            private class QueuedDirectoryMirrorer : DirectoryMirrorer
            {
                private DateTime _lastUpdate;

                public event EventHandler FlushRequested;

                public Queue<object> ProcessingQueue { get; } = new Queue<object>();

                public void Init()
                {
                    _lastUpdate = DateTime.MinValue;
                }

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

                    var now = DateTime.UtcNow;

                    if ((now - _lastUpdate).TotalMilliseconds >= MinRefreshInterval)
                    {
                        FlushRequested?.Invoke(this, EventArgs.Empty);

                        _lastUpdate = now;
                    }
                }
            }
        }

        private class PerformChangesTask
        {
            private BackupForm _backupForm;
            private List<ChangeResult> _queue = new List<ChangeResult>();
            private DateTime _lastUpdate;
            private long _processedWeight;
            private long _totalWeight;

            private DateTime _start;

            public PerformChangesTask(BackupForm backupForm)
            {
                _backupForm = backupForm;
            }

            public void Init()
            {
                _queue.Clear();
                _lastUpdate = DateTime.MinValue;

                _processedWeight = 0;
                _totalWeight = 0;
                
                _backupForm._operationsListView.BeginUpdate();
                foreach (var item in _backupForm._operations)
                {
                    _totalWeight += item.Operation.Weight;
                    item.MarkAsPending();
                }
                _backupForm._operationsListView.EndUpdate();
            }

            public async Task Run(CancellationToken ct)
            {
                try
                {
                    _backupForm.SetState(BackupFormState.PerformingChanges);

                    await Task.Run(() => PerformChanges(ct));

                    _backupForm.SetState(BackupFormState.Done);
                    DisplayChangesAndElapsed();
                    DisplayErrors();
                }
                catch (OperationCanceledException)
                {
                    _backupForm.Text = "Backup Utility - Canceled";
                    _backupForm.SetState(BackupFormState.Done);
                }
            }

            private void DisplayChangesAndElapsed()
            {
                var changes = _backupForm._operations.Count;
                var elapsed = DateTimeOffset.UtcNow - _start;

                _backupForm.Text = FormattableString.Invariant($"Backup Utility - {changes} change(s) performed in {elapsed.ToHumanReadableString()}");

            }

            private void DisplayErrors()
            {
                var errors = _backupForm._operations.Count(x => !x.Success);

                if (errors == 0)
                    return;

                MessageBox.Show($"{errors} error(s) occured while performing changes.", $"{errors} error(s) occured", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            private void PerformChanges(CancellationToken ct)
            {
                _start = DateTime.UtcNow;

                for (var i = 0; i < _backupForm._operations.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    ProcessOperation(i, _backupForm._operations[i].Operation);

                    var now = DateTime.UtcNow;

                    if ((now - _lastUpdate).TotalMilliseconds >= MinRefreshInterval)
                    {
                        RequestFlush();

                        _lastUpdate = now;
                    }
                }

                if (_queue.Count > 0)
                    RequestFlush();
            }

            private void ProcessOperation(int index, FileSystemOperation op)
            {
                try
                {
                    op.Perform();
                    _queue.Add(new ChangeResult(index, null));
                }
                catch (Exception ex) when (ex is ArgumentException ||
                                           ex is IOException ||
                                           ex is UnauthorizedAccessException ||
                                           ex is SecurityException)
                {
                    _queue.Add(new ChangeResult(index, ex));
                }

                _processedWeight += op.Weight;
            }

            private void RequestFlush()
            {
                if (_backupForm.InvokeRequired)
                    _backupForm.Invoke((MethodInvoker)Flush);
                else
                    Flush();
            }

            private void Flush()
            {
                Debug.Assert(_queue.Count > 0);

                UpdateItems();
                UpdateProgress();
            }

            private void UpdateItems()
            {
                _backupForm._operationsListView.BeginUpdate();
                foreach (var result in _queue)
                {
                    var item = _backupForm._operations[result.Index];

                    if (result.Exception == null)
                        item.MarkAsSuccessful();
                    else
                        item.MarkAsFailure(result.Exception);
                }
                _backupForm._operationsListView.EndUpdate();

                if (_backupForm._autoScrollToolStripMenuItem.Checked)
                {
                    var lastIndex = _queue[_queue.Count - 1].Index;
                    _backupForm._operationsListView.EnsureVisible(lastIndex);
                }

                _queue.Clear();
            }

            private void UpdateProgress()
            {
                var percentage = (double)_processedWeight / _totalWeight;

                if (percentage >= DisplayTimeRemainingThreshold)
                {
                    var elapsed = (DateTimeOffset.UtcNow - _start).Ticks;
                    var total = (long)(elapsed / percentage);
                    var remaining = new TimeSpan(total - elapsed);

                    _backupForm.Text = FormattableString.Invariant($"Backup Utility - Performing changes ({percentage:P0}, {remaining.ToHumanReadableString()} left)");
                }
                else
                {
                    _backupForm.Text = FormattableString.Invariant($"Backup Utility - Performing changes ({percentage:P0})");
                }

                _backupForm._progressBar.Value = (int)(percentage * 10000);
            }
            
            private struct ChangeResult
            {
                public int Index { get; }
                public Exception Exception { get; }

                public ChangeResult(int index, Exception ex)
                {
                    Index = index;
                    Exception = ex;
                }
            }
        }

        private class OperationItem : ListViewItem
        {
            public OperationItem(FileSystemOperation op) : base(new string[] { string.Empty, op.FileName, op.FilePath, string.Empty })
            {
                switch (op)
                {
                    case CreateDirectoryOperation _:
                        Text = "Create directory";
                        BackColor = s_green;
                        ImageIndex = CreateDirectoryImageIndex;
                        break;
                    case DestroyDirectoryOperation _:
                        Text = "Destroy directory";
                        BackColor = s_red;
                        ImageIndex = DestroyDirectoryImageIndex;
                        break;
                    case CopyFileOperation _:
                        Text = "Copy file";
                        BackColor = s_green;
                        ImageIndex = CopyFileImageIndex;
                        break;
                    case EditFileOperation _:
                        Text = "Edit file";
                        BackColor = s_yellow;
                        ImageIndex = EditFileImageIndex;
                        break;
                    case EditAttributesOperation _:
                        Text = "Edit attributes";
                        BackColor = s_yellow;
                        ImageIndex = EditAttributesImageIndex;
                        break;
                    case DeleteFileOperation _:
                        Text = "Delete file";
                        BackColor = s_red;
                        ImageIndex = DeleteFileImageIndex;
                        break;
                    default:
                        throw new NotSupportedException();
                }
                
                Operation = op;
            }

            static Random k = new Random();

            public FileSystemOperation Operation { get; }
            public bool Success { get; private set; }

            public void MarkAsPending()
            {
                BackColor = Color.FromKnownColor(KnownColor.Window);
                SubItems[3].Text = "Pending to perform...";
            }
            
            public void MarkAsSuccessful()
            {
                BackColor = s_green;
                SubItems[3].Text = "Operation completed successfully.";
                Success = true;
            }

            public void MarkAsFailure(Exception ex)
            {
                BackColor = s_red;
                SubItems[3].Text = ex.Message;

                switch (Tag)
                {
                    case CreateDirectoryOperation _:
                    case DestroyDirectoryOperation _:
                        ImageIndex = DirectoryExceptionImageIndex;
                        break;
                    case CopyFileOperation _:
                    case EditFileOperation _:
                    case DeleteFileOperation _:
                        ImageIndex = FileExceptionImageIndex;
                        break;
                    case EditAttributesOperation op:
                        if (op.FileSystemInfo is DirectoryInfo _)
                            ImageIndex = DirectoryExceptionImageIndex;
                        else if (op.FileSystemInfo is FileInfo _)
                            ImageIndex = FileExceptionImageIndex;

                        break;
                }

                Success = false;
            }
        }
    }
}
