using System;
using System.Diagnostics;
using System.Windows.Forms;
using LocalBackup.View;
using LocalBackup.IO;
using LocalBackup.IO.Operations;
using LocalBackup.Model;

namespace LocalBackup.Controller
{
    public class BackupController
    {
        private BackupModel _model;
        private BackupForm _view;
        
        public BackupController()
        {
            _model = new BackupModel();
            _model.TitleChanged += Model_TitleChanged;
            _model.StateChanged += Model_StateChanged;
            _model.QueueFlushRequested += Model_QueueFlushRequested;

            _view = new BackupForm();
            _view.OkButtonClick += View_OkButtonClick;
            _view.CancelButtonClick += View_CancelButtonClick;
            _view.FormClosing += View_FormClosing;
            _view.DataSource = _model.Items;
        }

        public void Run()
        {
            Application.Run(_view);
        }

        private void Model_TitleChanged(object sender, EventArgs e)
        {
            _view.Text = _model.Title;
        }

        private void Model_StateChanged(object sender, EventArgs e)
        {
            _view.SetState(_model.State);
        }

        private void Model_QueueFlushRequested(object sender, EventArgs e)
        {
            if (_view.InvokeRequired)
                _view.Invoke((MethodInvoker)FlushQueue);
            else
                FlushQueue();
        }

        private void FlushQueue()
        {
            Debug.Assert(_model.ProcessingQueue.Count > 0);

            foreach (var item in _model.ProcessingQueue)
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

                _model.Items.Add(lvi);
            }

            _model.ProcessingQueue.Clear();
            _view.RefreshDataSource();
        }

        private async void View_OkButtonClick(object sender, EventArgs e)
        {
            switch (_model.State)
            {
                case BackupFormState.Idle:
                    await _model.FindChanges(_view.SourceDirectory,_view.DestinationDirectory, _view.QuickScan);
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
    }
}
