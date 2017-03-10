using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LocalBackup.Controller;
using LocalBackup.IO;
using LocalBackup.IO.Operations;

namespace LocalBackup.Forms
{
    public partial class MainForm : Form
    {
        private IReadOnlyList<ListViewItem> _dataSource;

        public MainForm()
        {
            InitializeComponent();

            _modeComboBox.SelectedIndex = 0;
        }

        public event EventHandler OkButtonClick
        {
            add => _okButton.Click += value;
            remove => _okButton.Click -= value;
        }

        public event EventHandler CancelButtonClick
        {
            add => _cancelButton.Click += value;
            remove => _cancelButton.Click -= value;
        }

        public string SourceDirectory
        {
            get => _sourceTextBox.Text;
            set => _sourceTextBox.Text = value;
        }

        public string DestinationDirectory
        {
            get => _destinationTextBox.Text;
            set => _destinationTextBox.Text = value;
        }

        public bool QuickScan
        {
            get => _modeComboBox.SelectedIndex == 0;
            set => _modeComboBox.SelectedIndex = value ? 0 : 1;
        }

        public bool ScrollToEnd
        {
            get => _autoScrollCheckBox.Checked;
            set => _autoScrollCheckBox.Checked = value;
        }

        public int Progress
        {
            get => _progressBar.Value;
            set => _progressBar.Value = value;
        }

        public IReadOnlyList<ListViewItem> DataSource
        {
            get => _dataSource;
            set
            {
                _dataSource = value;
                RefreshDataSource();
            }
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
            e.Item = DataSource[e.ItemIndex];
        }

        public void RefreshDataSource()
        {
            if (_dataSource == null)
                _operationsListViewEx.VirtualListSize = 0;
            else if (_operationsListViewEx.VirtualListSize != _dataSource.Count)
                _operationsListViewEx.VirtualListSize = _dataSource.Count;
            else
                _operationsListViewEx.Refresh();
        }

        public void UpdateHeader(bool enabled)
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

        public void UpdateFooter(State state)
        {
            switch (state)
            {
                case State.Idle:
                    _progressBar.Style = ProgressBarStyle.Continuous;
                    _okButton.Enabled = true;
                    _okButton.Text = "Start";
                    _cancelButton.Enabled = true;
                    _cancelButton.Text = "Close";
                    break;
                case State.FindingChanges:
                    _progressBar.Style = ProgressBarStyle.Marquee;
                    _okButton.Enabled = false;
                    _okButton.Text = "Finding changes...";
                    _cancelButton.Enabled = true;
                    _cancelButton.Text = "Cancel";
                    break;
                case State.ReviewingChanges:
                    _progressBar.Style = ProgressBarStyle.Continuous;
                    _okButton.Enabled = true;
                    _okButton.Text = "Perform changes";
                    _cancelButton.Enabled = true;
                    _cancelButton.Text = "Discard changes";
                    break;
                case State.PerformingChanges:
                    _progressBar.Style = ProgressBarStyle.Continuous;
                    _okButton.Enabled = false;
                    _okButton.Text = "Performing changes...";
                    _cancelButton.Enabled = true;
                    _cancelButton.Text = "Cancel";
                    break;
                case State.Done:
                    _progressBar.Style = ProgressBarStyle.Continuous;
                    _okButton.Enabled = true;
                    _okButton.Text = "Start new backup";
                    _cancelButton.Enabled = true;
                    _cancelButton.Text = "Close";
                    break;
                case State.Canceling:
                    _progressBar.Style = ProgressBarStyle.Marquee;
                    _okButton.Enabled = false;
                    _okButton.Text = "Please wait...";
                    _cancelButton.Enabled = false;
                    _cancelButton.Text = "Canceling...";
                    break;
            }
        }
    }
}
