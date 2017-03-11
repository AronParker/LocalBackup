using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LocalBackup.IO;
using LocalBackup.IO.FileComparers;
using LocalBackup.IO.Operations;
using LocalBackup.View;

namespace LocalBackup.Controller
{
    public class BackupController
    {
        private List<ListViewItem> _items;
        private BackupFormState _state;
        private BackupForm _view;

        private BufferedDirectoryMirrorer _mirrorer;

        private CancellationTokenSource _cts;
        private Task _task;

        public BackupController()
        {
            _items = new List<ListViewItem>();
            _state = BackupFormState.Idle;

            _view = new BackupForm();
            _view.OkButtonClick += View_OkButtonClick;
            _view.CancelButtonClick += View_CancelButtonClick;
            _view.FormClosing += View_FormClosing;
            _view.DataSource = _items;

            _mirrorer = new BufferedDirectoryMirrorer();
            _mirrorer.QueueFlushRequested += Mirrorer_QueueFlushRequested;


        }

        private void Mirrorer_QueueFlushRequested(object sender, EventArgs e)
        {
            FlushQueue();
        }

        public void Run()
        {
            Application.Run(_view);
        }

        public async Task FindChanges()
        {
            var findChangesHelper = new FindChangesHelper();

            if (!findChangesHelper.SetSourceDirectory(_view.SourceDirectory))
                return;

            if (!findChangesHelper.SetDestinationDirectory(_view.DestinationDirectory))
                return;

            if (!findChangesHelper.FindComparer(_view.QuickScan))
                return;

            _state = BackupFormState.FindingChanges;
            _view.Text = "Local Backup - Finding changes...";
            _view.ApplyState(BackupFormState.FindingChanges);

            using (_cts = new CancellationTokenSource())
            {
                try
                {
                    _task = _mirrorer.RunAsync(findChangesHelper.SourceDirectory,
                                               findChangesHelper.DestinationDirectory,
                                               findChangesHelper.FileInfoComparer,
                                               _cts.Token);

                    await _task;

                    if (_mirrorer.ProcessingQueue.Count > 0)
                        FlushQueue();

                    _state = BackupFormState.ReviewingChanges;
                    _view.Text = "Local Backup - Reviewing changes...";
                    _view.ApplyState(BackupFormState.ReviewingChanges);
                }
                catch (OperationCanceledException)
                {
                    _state = BackupFormState.Done;
                    _view.Text = "Backup Utility - Canceled";
                    _view.ApplyState(BackupFormState.Done);
                }
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
                                lvi.BackColor = Colors.Green;
                                break;
                            case FileSystemOperationType.EditDirectory:
                            case FileSystemOperationType.EditFile:
                                lvi.BackColor = Colors.Yellow;
                                break;
                            case FileSystemOperationType.DestroyDirectory:
                            case FileSystemOperationType.DeleteFile:
                                lvi.BackColor = Colors.Red;
                                break;
                        }

                        lvi.ImageIndex = (int)op.Type;
                        lvi.Tag = op;
                        break;
                    case FileException ex:
                        lvi = new ListViewItem(new string[] { "File error", ex.File.Name, ex.File.FullName, ex.Message });
                        lvi.BackColor = Colors.Red;
                        lvi.ImageIndex = 6;
                        lvi.Tag = ex;
                        break;
                    case DirectoryException ex:
                        lvi = new ListViewItem(new string[] { "Directory error", ex.Directory.Name, ex.Directory.FullName, ex.Message });
                        lvi.BackColor = Colors.Red;
                        lvi.ImageIndex = 7;
                        lvi.Tag = ex;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                _items.Add(lvi);
            }

            _mirrorer.ProcessingQueue.Clear();
            _view.RefreshDataSource();
        }

        private void Model_QueueFlushRequested(object sender, EventArgs e)
        {
            if (_view.InvokeRequired)
                _view.Invoke((MethodInvoker)FlushQueue);
            else
                FlushQueue();
        }

        private async void View_OkButtonClick(object sender, EventArgs e)
        {
            switch (_state)
            {
                case BackupFormState.Idle:
                    await FindChanges();
                    break;
                case BackupFormState.ReviewingChanges:
                    break;
                case BackupFormState.Done:
                    break;
            }
        }
        
        private void View_CancelButtonClick(object sender, EventArgs e)
        {

        }

        private void View_FormClosing(object sender, FormClosingEventArgs e)
        {
            //throw new NotImplementedException();

            /*
protected override void OnFormClosing(FormClosingEventArgs e)
{
    switch (_state)
    {
        case State.FindingChanges:
        case State.PerformingChanges:
            if (MessageBox.Show("Are you sure you want to cancel?", "Confirm Cancelation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                break;

            CancelGUI();
            await _task;
            Close();
            goto case State.Canceling;
        case State.Canceling:
            e.Cancel = true;
            break;
    }

    base.OnFormClosing(e);
}
*/
        }

        private struct FindChangesHelper
        {
            public DirectoryInfo SourceDirectory { get; private set; }
            public DirectoryInfo DestinationDirectory { get; private set; }
            public IFileInfoEqualityComparer FileInfoComparer { get; private set; }

            public bool SetSourceDirectory(string sourceDirectory)
            {
                try
                {
                    SourceDirectory = new DirectoryInfo(sourceDirectory);
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
                
                if (!SourceDirectory.Exists)
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
                    DestinationDirectory = new DirectoryInfo(destinationDirectory);
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
                if (quickScan)
                {
                    FileInfoComparer = new FileComparer();
                    return true;
                }

                var fileSystem = GetDestinationFileSystem();

                if (fileSystem == null)
                    return false;

                FileInfoComparer = CreateQuickScanComparere(fileSystem);

                if (FileInfoComparer == null)
                {
                    MessageBox.Show("Destination directory uses a file system where quick scan is not supported.",
                                    "Unsupported file system",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }

            private string GetDestinationFileSystem()
            {
                try
                {
                    return new DriveInfo(DestinationDirectory.FullName).DriveFormat;
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

            private IFileInfoEqualityComparer CreateQuickScanComparere(string fileSystem)
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
        }
    }
}
