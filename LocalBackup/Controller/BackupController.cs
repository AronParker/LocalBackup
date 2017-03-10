using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LocalBackup.Forms;

namespace LocalBackup.Controller
{
    public class BackupController
    {
        private MainForm _mainForm;
        private State _state;

        public BackupController()
        {
            _mainForm = new MainForm();
            _mainForm.OkButtonClick += MainForm_OkButtonClick;
            _mainForm.CancelButtonClick += MainForm_CancelButtonClick;
            _mainForm.FormClosing += MainForm_FormClosing;
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

        private void MainForm_CancelButtonClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            Application.Run(_mainForm);
        }
    }
}
