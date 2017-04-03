using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LocalBackup.IO;
using LocalBackup.IO.FileEqualityComparers;
using LocalBackup.IO.Operations;
using LocalBackup.Localizations;
using static System.FormattableString;
using static LocalBackup.NativeMethods;

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
        private bool _closeAfterCancellation = false;

        private FindChangesTask _findChangesTask;
        private PerformChangesTask _performChangesTask;
        private BackupFormState _state;

        private CancellationTokenSource _cts;
        
        public BackupForm()
        {
            _findChangesTask = new FindChangesTask(this);
            _performChangesTask = new PerformChangesTask(this);

            InitializeComponent();            
            SetState(BackupFormState.Idle);
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            switch (_state)
            {
                case BackupFormState.FindingChanges:
                case BackupFormState.PerformingChanges:
                    e.Cancel = true;
                    
                    if (MessageBox.Show("Are you sure you want to cancel?", "Confirm Cancelation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        return;

                    _cts.Cancel();
                    SetState(BackupFormState.Canceling);
                    _closeAfterCancellation = true;
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

                    _operationsLabel.Text = "No changes detected";
                    _operationsListView.VirtualListSize = 0;
                    _operations.Clear();
                    _errorsLabel.Text = "No errors occured";
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

        private void TextBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
        }
        
        private void TextBox_DragDrop(object sender, DragEventArgs e)
        {
            var folders = (string[])e.Data.GetData(DataFormats.FileDrop);
            var folder = folders.SingleOrDefault(x => Directory.Exists(x));
            
            if (folder != null)
                ((TextBox)sender).Text = folder;
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFolderDialog();

            if (sender == _sourceButton)
            {
                ofd.Title = "Select source directory";
                ofd.SelectedPath = _sourceTextBox.Text;
            }
            else if (sender == _destinationButton)
            {
                ofd.Title = "Select destination directory";
                ofd.SelectedPath = _destinationTextBox.Text;
            }

            if (!ofd.Show(this))
                return;
            
            if (sender == _sourceButton)
                _sourceTextBox.Text = ofd.SelectedPath;
            else if (sender == _destinationButton)
                _destinationTextBox.Text = ofd.SelectedPath;
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
                    if (!_findChangesTask.Init(_sourceTextBox.Text, _destinationTextBox.Text, _quickScanToolStripMenuItem.Checked))
                        return;

                    using (_cts = new CancellationTokenSource())
                        await _findChangesTask.RunAsync(_cts.Token);
                    
                    if (_closeAfterCancellation)
                        Close();
                    break;
                case BackupFormState.ReviewChanges:
                    _performChangesTask.Init();

                    using (_cts = new CancellationTokenSource())
                        await _performChangesTask.RunAsync(_cts.Token);
                    
                    if (_closeAfterCancellation)
                        Close();
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
            private DirectoryMirrorerEx _mirrorer;
            private DirectoryInfo _sourceDirectory;
            private DirectoryInfo _destinationDirectory;
            private IFileInfoEqualityComparer _fileInfoComparer;

            public FindChangesTask(BackupForm backupForm)
            {
                _backupForm = backupForm;
                _mirrorer = new DirectoryMirrorerEx(this);
            }

            public bool Init(string sourceDirectory, string destinationDirectory, bool quickScan)
            {
                if (!SetSourceDirectory(sourceDirectory))
                    return false;

                if (!SetDestinationDirectory(destinationDirectory))
                    return false;

                if (SourceAndDestinationDirectoriesEqual())
                    return false;

                if (!SetComparer(quickScan))
                    return false;

                _mirrorer.Init();
                return true;
            }

            public async Task RunAsync(CancellationToken ct)
            {
                try
                {
                    _backupForm.SetState(BackupFormState.FindingChanges);

                    await _mirrorer.RunAsync(_sourceDirectory, _destinationDirectory, _fileInfoComparer, ct);
                    Update();

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

            private bool SetSourceDirectory(string sourceDirectory)
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

            private bool SetDestinationDirectory(string destinationDirectory)
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

            private bool SourceAndDestinationDirectoriesEqual()
            {
                if (string.Equals(_sourceDirectory.FullName, _destinationDirectory.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("The source directory and destination directory refer to the same location.",
                                    "Source and destination directories equal",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return true;
                }
                
                return false;
            }

            private bool SetComparer(bool quickScan)
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
                    _fileInfoComparer = new FileEqualityComparer();
                }

                return true;
            }

            private void DisplayErrors()
            {
                var errors = _backupForm._errors.Count;

                if (errors == 0)
                    return;

                var localizedErrors = Localization.GetPlural(errors, "error");

                MessageBox.Show($"{localizedErrors} occured while finding changes.", $"{localizedErrors} occured", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        return new FileInfoEqualityComparer();
                }
            }

            private void Update()
            {
                var operationsAdded = _mirrorer.PendingOperations.Count > 0;
                var errorsAdded = _mirrorer.PendingErrors.Count > 0;

                if (operationsAdded)
                    FlushOperations();

                if (errorsAdded)
                    FlushErrors();
            }

            private void FlushOperations()
            {
                _backupForm._operations.AddRange(_mirrorer.PendingOperations);
                _mirrorer.PendingOperations.Clear();
                _backupForm._operationsListView.VirtualListSize = _backupForm._operations.Count;
                _backupForm._operationsLabel.Text = $"{Localization.GetPlural(_backupForm._operations.Count, "change")} detected";

                if (_backupForm._autoScrollToolStripMenuItem.Checked)
                {
                    var lastOperation = _backupForm._operations.Count - 1;
                    _backupForm._operationsListView.EnsureVisible(lastOperation);
                }
            }

            private void FlushErrors()
            {
                _backupForm._errors.AddRange(_mirrorer.PendingErrors);
                _mirrorer.PendingErrors.Clear();
                _backupForm._errorsListView.VirtualListSize = _backupForm._errors.Count;
                _backupForm._errorsLabel.Text = $"{Localization.GetPlural(_backupForm._errors.Count, "error")} occured";

                if (_backupForm._autoScrollToolStripMenuItem.Checked)
                {
                    var lastError = _backupForm._errors.Count - 1;
                    _backupForm._errorsListView.EnsureVisible(lastError);
                }
            }

            private class DirectoryMirrorerEx : DirectoryMirrorer
            {
                private FindChangesTask _findChangesTask;
                private DateTime _lastUpdate;

                public DirectoryMirrorerEx(FindChangesTask findChangesTask)
                {
                    _findChangesTask = findChangesTask;
                }

                public List<OperationItem> PendingOperations { get; } = new List<OperationItem>();
                public List<ListViewItem> PendingErrors { get; } = new List<ListViewItem>();

                public void Init()
                {
                    _lastUpdate = DateTime.MinValue;
                    PendingOperations.Clear();
                    PendingErrors.Clear();
                }

                protected override void OnOperationFound(FileSystemOperation operation)
                {
                    PendingOperations.Add(new OperationItem(operation));

                    if (ShouldUpdate())
                        RequestUpdate();
                }

                protected override void OnError(FileSystemInfoException ex)
                {
                    var isDir = ex.FileSystemInfo is DirectoryInfo;
                    var lvi = new ListViewItem(new[] { isDir ? "Directory error" : "File error", ex.FileSystemInfo.Name, ex.FileSystemInfo.FullName, ex.Message }, isDir ? DirectoryExceptionImageIndex : FileExceptionImageIndex)
                    {
                        Tag = ex
                    };

                    PendingErrors.Add(lvi);

                    if (ShouldUpdate())
                        RequestUpdate();
                }

                private bool ShouldUpdate()
                {
                    var now = DateTime.UtcNow;

                    if ((now - _lastUpdate).TotalMilliseconds >= MinRefreshInterval)
                    {
                        _lastUpdate = now;
                        return true;
                    }

                    return false;
                }

                private void RequestUpdate()
                {
                    if (_findChangesTask._backupForm.InvokeRequired)
                        _findChangesTask._backupForm.Invoke((MethodInvoker)_findChangesTask.Update);
                    else
                        _findChangesTask.Update();
                }
            }
        }

        private class PerformChangesTask
        {
            private BackupForm _backupForm;
            private ChangesPerformer _performer;

            public PerformChangesTask(BackupForm backupForm)
            {
                _backupForm = backupForm;
                _performer = new ChangesPerformer(this);
            }

            public void Init()
            {
                var totalWeight = 0L;

                _backupForm._operationsListView.BeginUpdate();
                foreach (var item in _backupForm._operations)
                {
                    totalWeight += GetOperationWeight(item.Operation);
                    item.MarkAsPending();
                }
                _backupForm._operationsListView.EndUpdate();

                _performer.Init(totalWeight);
            }

            public async Task RunAsync(CancellationToken ct)
            {
                try
                {
                    _backupForm.SetState(BackupFormState.PerformingChanges);

                    await Task.Run(() => _performer.PerformChanges(ct));

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

            private static long GetOperationWeight(FileSystemOperation op)
            {
                switch (op)
                {
                    case CreateDirectoryOperation _:
                    case DestroyDirectoryOperation _:
                    case EditAttributesOperation _:
                    case DeleteFileOperation _:
                        return 1;
                    case CopyFileOperation copy:
                        return 1 + copy.SourceFile.Length / 8192;
                    case EditFileOperation edit:
                        return 1 + edit.SourceFile.Length / 8192;
                    default:
                        throw new NotSupportedException();
                }
            }

            private void DisplayChangesAndElapsed()
            {
                var changes = _backupForm._operations.Count;
                var elapsed = DateTime.UtcNow - _performer.Start;

                _backupForm.Text = $"Backup Utility - {Localization.GetPlural(changes, "change")} performed in {Localization.GetHumanReadableTimeSpan(elapsed)}";

            }

            private void DisplayErrors()
            {
                var errors = _backupForm._operations.Count(x => !x.Success);

                if (errors == 0)
                    return;

                var localizedErrors = Localization.GetPlural(errors, "error");

                MessageBox.Show($"{localizedErrors} occured while performing changes.", $"{localizedErrors} occured", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            private void Update()
            {
                Debug.Assert(_performer.ProcessingQueue.Count > 0);

                FlushItems();
                UpdateProgress();
            }

            private void FlushItems()
            {
                _backupForm._operationsListView.BeginUpdate();
                foreach (var result in _performer.ProcessingQueue)
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
                    var lastIndex = _performer.ProcessingQueue.Last().Index;
                    _backupForm._operationsListView.EnsureVisible(lastIndex);
                }

                _performer.ProcessingQueue.Clear();
            }

            private void UpdateProgress()
            {
                var percentage = (double)_performer.ProcessedWeight / _performer.TotalWeight;

                if (percentage >= DisplayTimeRemainingThreshold)
                {
                    var elapsed = (DateTime.UtcNow - _performer.Start).Ticks;
                    var total = (long)(elapsed / percentage);
                    var remaining = new TimeSpan(total - elapsed);

                    _backupForm.Text = Invariant($"Backup Utility - Performing changes ({percentage:P0}, {Localization.GetHumanReadableTimeSpan(remaining)} left)");
                }
                else
                {
                    _backupForm.Text = Invariant($"Backup Utility - Performing changes ({percentage:P0})");
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

            private class ChangesPerformer
            {
                private PerformChangesTask _performChangesTask;
                private DateTime _lastUpdate;

                public ChangesPerformer(PerformChangesTask performChangesTask)
                {
                    _performChangesTask = performChangesTask;
                }

                public DateTime Start { get; private set; }
                public long ProcessedWeight { get; private set; }
                public long TotalWeight { get; private set; }
                public List<ChangeResult> ProcessingQueue { get; } = new List<ChangeResult>();

                public void Init(long totalWeight)
                {
                    _lastUpdate = DateTime.MinValue;
                    ProcessedWeight = 0;
                    TotalWeight = totalWeight;
                    ProcessingQueue.Clear();
                }

                public void PerformChanges(CancellationToken ct)
                {
                    Start = DateTime.UtcNow;

                    var operations = _performChangesTask._backupForm._operations;

                    for (var i = 0; i < operations.Count; i++)
                    {
                        ct.ThrowIfCancellationRequested();

                        ProcessOperation(i, operations[i].Operation);

                        if (ShouldUpdate())
                            RequestUpdate();
                    }

                    if (ProcessingQueue.Count > 0)
                        RequestUpdate();
                }

                private void ProcessOperation(int index, FileSystemOperation op)
                {
                    try
                    {
                        op.Perform();
                        ProcessingQueue.Add(new ChangeResult(index, null));
                    }
                    catch (Exception ex) when (ex is ArgumentException ||
                                               ex is IOException ||
                                               ex is UnauthorizedAccessException ||
                                               ex is SecurityException)
                    {
                        ProcessingQueue.Add(new ChangeResult(index, ex));
                    }

                    ProcessedWeight += GetOperationWeight(op);
                }

                private bool ShouldUpdate()
                {
                    var now = DateTime.UtcNow;

                    if ((now - _lastUpdate).TotalMilliseconds >= MinRefreshInterval)
                    {
                        _lastUpdate = now;
                        return true;
                    }

                    return false;
                }

                private void RequestUpdate()
                {
                    if (_performChangesTask._backupForm.InvokeRequired)
                        _performChangesTask._backupForm.Invoke((MethodInvoker)_performChangesTask.Update);
                    else
                        _performChangesTask.Update();
                }
            }
        }

        private class OperationItem : ListViewItem
        {
            public OperationItem(FileSystemOperation op) : base(new[] { string.Empty, op.FileName, op.FilePath, string.Empty })
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

        private class OpenFolderDialog
        {
            private IFileOpenDialog _dialog;

            public OpenFolderDialog()
            {
                _dialog = new IFileOpenDialog();
                _dialog.SetOptions(FOS.PICKFOLDERS | FOS.FORCEFILESYSTEM | FOS.PATHMUSTEXIST | FOS.FILEMUSTEXIST);
            }

            public string Title
            {
                set => _dialog.SetTitle(value);
            }

            public string SelectedPath
            {
                get
                {
                    _dialog.GetFolder(out var si);
                    si.GetDisplayName(SIGDN.FILESYSPATH, out var folder);
                    return folder;
                }
                set
                {
                    if (!Directory.Exists(value))
                        return;

                    var iid = typeof(IShellItem).GUID;
                    var hr = SHCreateItemFromParsingName(value, IntPtr.Zero, ref iid, out var folder);

                    if (hr >= 0)
                        _dialog.SetFolder(folder);
                }
            }

            public bool Show()
            {
                return Show(null);
            }

            public bool Show(IWin32Window owner)
            {
                return _dialog.Show(owner?.Handle ?? IntPtr.Zero) >= 0;
            }
        }
    }
}
