using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LocalBackup.Forms;
using LocalBackup.IO;
using LocalBackup.IO.FileComparers;
using LocalBackup.IO.Operations;

namespace LocalBackup.Controller
{
    public class BackupController
    {
        private MainForm _mainForm;
        private State _state;

        private DirectoryMirrorer _mirrorer;
        private Queue<object> _queue;
        private Stopwatch _sw;

        private CancellationTokenSource _cts;
        private Task _task;

        public BackupController()
        {
            _mainForm = new MainForm();
            _mainForm.OkButtonClick += MainForm_OkButtonClick;
            _mainForm.CancelButtonClick += MainForm_CancelButtonClick;
            _mainForm.FormClosing += MainForm_FormClosing;

            _mirrorer = new DirectoryMirrorer();
            _mirrorer.OperationFound += Mirrorer_OperationFound;
            _mirrorer.Error += Mirrorer_Error;
            _queue = new Queue<object>();
            _sw = new Stopwatch();

        }

        public void Run()
        {
            Application.Run(_mainForm);
        }

        private void MainForm_OkButtonClick(object sender, EventArgs e)
        {
            switch (_state)
            {
                case State.Idle:
                    FindChanges();
                    break;
                case State.ReviewingChanges:
                    break;
                case State.Done:
                    break;
            }
        }

        private void FindChanges()
        {
            var srcDir = FindSourceDirectory();

            if (srcDir == null)
                return;

            var dstDir = FindDestinationDirectory();

            if (dstDir == null)
                return;

            var fileInfoComparer = FindFileInfoEqualityComparer(dstDir);

            if (fileInfoComparer == null)
                return;

            _state = State.FindingChanges;

            _mainForm.Text = "Local Backup - Finding changes...";
            _mainForm.UpdateHeader(false);
            _mainForm.UpdateFooter(_state);
            
            _cts = new CancellationTokenSource();
            _task = _mirrorer.RunAsync(srcDir, dstDir, fileInfoComparer);
        }

        private DirectoryInfo FindSourceDirectory()
        {
            DirectoryInfo srcDir;

            try
            {
                srcDir = new DirectoryInfo(_mainForm.SourceDirectory);
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

        private DirectoryInfo FindDestinationDirectory()
        {
            DirectoryInfo dstDir;

            try
            {
                dstDir = new DirectoryInfo(_mainForm.SourceDirectory);
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

        private IFileInfoEqualityComparer FindFileInfoEqualityComparer(DirectoryInfo dstDir)
        {
            if (!_mainForm.QuickScan)
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
                                        "Quick scan not supported",
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

        private void MainForm_CancelButtonClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Mirrorer_Error(object sender, ErrorEventArgs e)
        {
            EnqueueItem(e.GetException());
        }

        private void Mirrorer_OperationFound(object sender, FileSystemOperationEventArgs e)
        {
            EnqueueItem(e.Operation);
        }

        private void EnqueueItem(object item)
        {
            _queue.Enqueue(item);

            if (_sw.ElapsedMilliseconds <= 500)
                return;

            if (_mainForm.InvokeRequired)
                _mainForm.Invoke((Action<Queue<object>>)AddItems, _queue);
            else
                AddItems(_queue);

            _queue.Clear();
        }

        private void AddItems(Queue<object> items)
        {
            foreach (var item in items)
            {
                var lvi = (ListViewItem)null;

                switch (item)
                {
                    case FileSystemOperation op:
                        lvi = new ListViewItem(new string[] { op.Name, op.FileName, op.FilePath, string.Empty }, (int)op.Type);

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
                        
                        lvi.Tag = op;
                        break;
                    case FileException ex:
                        lvi = new ListViewItem(new string[] { "File error", ex.File.Name, ex.File.FullName, ex.Message }, 6);
                        lvi.BackColor = Colors.Red;
                        lvi.Tag = ex;
                        break;
                    case DirectoryException ex:
                        lvi = new ListViewItem(new string[] { "Directory error", ex.Directory.Name, ex.Directory.FullName, ex.Message }, 7);
                        lvi.BackColor = Colors.Red;
                        lvi.Tag = ex;
                        break;
                }
            }
        }
    }
}
