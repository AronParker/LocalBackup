namespace LocalBackup.Forms
{
    partial class BackupForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackupForm));
            this._footerPanel = new System.Windows.Forms.Panel();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._sourceLabel = new System.Windows.Forms.Label();
            this._destinationLabel = new System.Windows.Forms.Label();
            this._sourceTextBox = new System.Windows.Forms.TextBox();
            this._destinationTextBox = new System.Windows.Forms.TextBox();
            this._sourceButton = new System.Windows.Forms.Button();
            this._destinationButton = new System.Windows.Forms.Button();
            this._operationsListView = new LocalBackup.Controls.ExplorerListView();
            this._operationTypeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._operationNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._operationPathColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._operationResultColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._imageList = new System.Windows.Forms.ImageList(this.components);
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._quickScanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._autoScrollToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._projectSiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._reportAnIssueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._errorsListView = new LocalBackup.Controls.ExplorerListView();
            this._errorType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._errorName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._errorPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._errorMessage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._directoriesLabel = new System.Windows.Forms.Label();
            this._operationsLabel = new System.Windows.Forms.Label();
            this._errorsLabel = new System.Windows.Forms.Label();
            this._footerPanel.SuspendLayout();
            this._menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _footerPanel
            // 
            this._footerPanel.BackColor = System.Drawing.SystemColors.Control;
            this._footerPanel.CausesValidation = false;
            this._footerPanel.Controls.Add(this._progressBar);
            this._footerPanel.Controls.Add(this._cancelButton);
            this._footerPanel.Controls.Add(this._okButton);
            this._footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._footerPanel.Location = new System.Drawing.Point(0, 497);
            this._footerPanel.Name = "_footerPanel";
            this._footerPanel.Size = new System.Drawing.Size(784, 50);
            this._footerPanel.TabIndex = 12;
            // 
            // _progressBar
            // 
            this._progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._progressBar.Location = new System.Drawing.Point(12, 12);
            this._progressBar.Maximum = 10000;
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(508, 26);
            this._progressBar.TabIndex = 2;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.CausesValidation = false;
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(652, 12);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(120, 26);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.CausesValidation = false;
            this._okButton.Location = new System.Drawing.Point(526, 12);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(120, 26);
            this._okButton.TabIndex = 0;
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // _sourceLabel
            // 
            this._sourceLabel.CausesValidation = false;
            this._sourceLabel.Location = new System.Drawing.Point(12, 57);
            this._sourceLabel.Name = "_sourceLabel";
            this._sourceLabel.Size = new System.Drawing.Size(120, 23);
            this._sourceLabel.TabIndex = 2;
            this._sourceLabel.Text = "Source directory:";
            this._sourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _destinationLabel
            // 
            this._destinationLabel.CausesValidation = false;
            this._destinationLabel.Location = new System.Drawing.Point(12, 87);
            this._destinationLabel.Name = "_destinationLabel";
            this._destinationLabel.Size = new System.Drawing.Size(120, 23);
            this._destinationLabel.TabIndex = 3;
            this._destinationLabel.Text = "Destination directory:";
            this._destinationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _sourceTextBox
            // 
            this._sourceTextBox.AllowDrop = true;
            this._sourceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceTextBox.CausesValidation = false;
            this._sourceTextBox.Location = new System.Drawing.Point(138, 57);
            this._sourceTextBox.Name = "_sourceTextBox";
            this._sourceTextBox.Size = new System.Drawing.Size(528, 23);
            this._sourceTextBox.TabIndex = 4;
            this._sourceTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBox_DragDrop);
            this._sourceTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBox_DragEnter);
            // 
            // _destinationTextBox
            // 
            this._destinationTextBox.AllowDrop = true;
            this._destinationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._destinationTextBox.CausesValidation = false;
            this._destinationTextBox.Location = new System.Drawing.Point(138, 87);
            this._destinationTextBox.Name = "_destinationTextBox";
            this._destinationTextBox.Size = new System.Drawing.Size(528, 23);
            this._destinationTextBox.TabIndex = 5;
            this._destinationTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBox_DragDrop);
            this._destinationTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBox_DragEnter);
            // 
            // _sourceButton
            // 
            this._sourceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceButton.CausesValidation = false;
            this._sourceButton.Location = new System.Drawing.Point(672, 57);
            this._sourceButton.Name = "_sourceButton";
            this._sourceButton.Size = new System.Drawing.Size(100, 23);
            this._sourceButton.TabIndex = 6;
            this._sourceButton.Text = "Browse...";
            this._sourceButton.UseVisualStyleBackColor = true;
            this._sourceButton.Click += new System.EventHandler(this.Browse_Click);
            // 
            // _destinationButton
            // 
            this._destinationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._destinationButton.CausesValidation = false;
            this._destinationButton.Location = new System.Drawing.Point(672, 87);
            this._destinationButton.Name = "_destinationButton";
            this._destinationButton.Size = new System.Drawing.Size(100, 23);
            this._destinationButton.TabIndex = 7;
            this._destinationButton.Text = "Browse...";
            this._destinationButton.UseVisualStyleBackColor = true;
            this._destinationButton.Click += new System.EventHandler(this.Browse_Click);
            // 
            // _operationsListView
            // 
            this._operationsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._operationsListView.CausesValidation = false;
            this._operationsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._operationTypeColumnHeader,
            this._operationNameColumnHeader,
            this._operationPathColumnHeader,
            this._operationResultColumnHeader});
            this._operationsListView.FullRowSelect = true;
            this._operationsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._operationsListView.Location = new System.Drawing.Point(12, 146);
            this._operationsListView.Name = "_operationsListView";
            this._operationsListView.Size = new System.Drawing.Size(760, 209);
            this._operationsListView.SmallImageList = this._imageList;
            this._operationsListView.TabIndex = 9;
            this._operationsListView.UseCompatibleStateImageBehavior = false;
            this._operationsListView.View = System.Windows.Forms.View.Details;
            this._operationsListView.VirtualMode = true;
            this._operationsListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.OperationsListView_RetrieveVirtualItem);
            // 
            // _operationTypeColumnHeader
            // 
            this._operationTypeColumnHeader.Text = "Type";
            this._operationTypeColumnHeader.Width = 120;
            // 
            // _operationNameColumnHeader
            // 
            this._operationNameColumnHeader.Text = "Name";
            this._operationNameColumnHeader.Width = 150;
            // 
            // _operationPathColumnHeader
            // 
            this._operationPathColumnHeader.Text = "Path";
            this._operationPathColumnHeader.Width = 286;
            // 
            // _operationResultColumnHeader
            // 
            this._operationResultColumnHeader.Text = "Result";
            this._operationResultColumnHeader.Width = 200;
            // 
            // _imageList
            // 
            this._imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_imageList.ImageStream")));
            this._imageList.TransparentColor = System.Drawing.Color.Transparent;
            this._imageList.Images.SetKeyName(0, "folder_add.png");
            this._imageList.Images.SetKeyName(1, "folder_delete.png");
            this._imageList.Images.SetKeyName(2, "page_add.png");
            this._imageList.Images.SetKeyName(3, "page_edit.png");
            this._imageList.Images.SetKeyName(4, "page_gear.png");
            this._imageList.Images.SetKeyName(5, "page_delete.png");
            this._imageList.Images.SetKeyName(6, "folder_error.png");
            this._imageList.Images.SetKeyName(7, "page_error.png");
            // 
            // _menuStrip
            // 
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._optionsToolStripMenuItem,
            this._helpToolStripMenuItem});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(784, 24);
            this._menuStrip.TabIndex = 0;
            this._menuStrip.Text = "menuStrip1";
            // 
            // _optionsToolStripMenuItem
            // 
            this._optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._quickScanToolStripMenuItem,
            this._autoScrollToolStripMenuItem});
            this._optionsToolStripMenuItem.Name = "_optionsToolStripMenuItem";
            this._optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this._optionsToolStripMenuItem.Text = "Options";
            // 
            // _quickScanToolStripMenuItem
            // 
            this._quickScanToolStripMenuItem.Checked = true;
            this._quickScanToolStripMenuItem.CheckOnClick = true;
            this._quickScanToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this._quickScanToolStripMenuItem.Name = "_quickScanToolStripMenuItem";
            this._quickScanToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this._quickScanToolStripMenuItem.Text = "Quick scan";
            // 
            // _autoScrollToolStripMenuItem
            // 
            this._autoScrollToolStripMenuItem.Checked = true;
            this._autoScrollToolStripMenuItem.CheckOnClick = true;
            this._autoScrollToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this._autoScrollToolStripMenuItem.Name = "_autoScrollToolStripMenuItem";
            this._autoScrollToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this._autoScrollToolStripMenuItem.Text = "Auto Scroll";
            // 
            // _helpToolStripMenuItem
            // 
            this._helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._projectSiteToolStripMenuItem,
            this._reportAnIssueToolStripMenuItem,
            this._toolStripSeparator1,
            this._aboutToolStripMenuItem});
            this._helpToolStripMenuItem.Name = "_helpToolStripMenuItem";
            this._helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this._helpToolStripMenuItem.Text = "Help";
            // 
            // _projectSiteToolStripMenuItem
            // 
            this._projectSiteToolStripMenuItem.Name = "_projectSiteToolStripMenuItem";
            this._projectSiteToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._projectSiteToolStripMenuItem.Text = "Project site";
            this._projectSiteToolStripMenuItem.Click += new System.EventHandler(this.ProjectSiteToolStripMenuItem_Click);
            // 
            // _reportAnIssueToolStripMenuItem
            // 
            this._reportAnIssueToolStripMenuItem.Name = "_reportAnIssueToolStripMenuItem";
            this._reportAnIssueToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._reportAnIssueToolStripMenuItem.Text = "Report an issue";
            this._reportAnIssueToolStripMenuItem.Click += new System.EventHandler(this.ReportAnIssueToolStripMenuItem_Click);
            // 
            // _toolStripSeparator1
            // 
            this._toolStripSeparator1.Name = "_toolStripSeparator1";
            this._toolStripSeparator1.Size = new System.Drawing.Size(151, 6);
            // 
            // _aboutToolStripMenuItem
            // 
            this._aboutToolStripMenuItem.Name = "_aboutToolStripMenuItem";
            this._aboutToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._aboutToolStripMenuItem.Text = "About...";
            this._aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // _errorsListView
            // 
            this._errorsListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._errorsListView.CausesValidation = false;
            this._errorsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._errorType,
            this._errorName,
            this._errorPath,
            this._errorMessage});
            this._errorsListView.FullRowSelect = true;
            this._errorsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._errorsListView.Location = new System.Drawing.Point(12, 391);
            this._errorsListView.Name = "_errorsListView";
            this._errorsListView.Size = new System.Drawing.Size(760, 100);
            this._errorsListView.SmallImageList = this._imageList;
            this._errorsListView.TabIndex = 11;
            this._errorsListView.UseCompatibleStateImageBehavior = false;
            this._errorsListView.View = System.Windows.Forms.View.Details;
            this._errorsListView.VirtualMode = true;
            this._errorsListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.ErrorsListViewEx_RetrieveVirtualItem);
            // 
            // _errorType
            // 
            this._errorType.Text = "Type";
            this._errorType.Width = 120;
            // 
            // _errorName
            // 
            this._errorName.Text = "Name";
            this._errorName.Width = 150;
            // 
            // _errorPath
            // 
            this._errorPath.Text = "Path";
            this._errorPath.Width = 286;
            // 
            // _errorMessage
            // 
            this._errorMessage.Text = "Message";
            this._errorMessage.Width = 200;
            // 
            // _directoriesLabel
            // 
            this._directoriesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._directoriesLabel.CausesValidation = false;
            this._directoriesLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
            this._directoriesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this._directoriesLabel.Location = new System.Drawing.Point(12, 24);
            this._directoriesLabel.Name = "_directoriesLabel";
            this._directoriesLabel.Size = new System.Drawing.Size(760, 30);
            this._directoriesLabel.TabIndex = 1;
            this._directoriesLabel.Text = "Directories";
            this._directoriesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _operationsLabel
            // 
            this._operationsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._operationsLabel.CausesValidation = false;
            this._operationsLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
            this._operationsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this._operationsLabel.Location = new System.Drawing.Point(12, 113);
            this._operationsLabel.Name = "_operationsLabel";
            this._operationsLabel.Size = new System.Drawing.Size(760, 30);
            this._operationsLabel.TabIndex = 8;
            this._operationsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _errorsLabel
            // 
            this._errorsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._errorsLabel.CausesValidation = false;
            this._errorsLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
            this._errorsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this._errorsLabel.Location = new System.Drawing.Point(12, 358);
            this._errorsLabel.Name = "_errorsLabel";
            this._errorsLabel.Size = new System.Drawing.Size(760, 30);
            this._errorsLabel.TabIndex = 10;
            this._errorsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BackupForm
            // 
            this.AcceptButton = this._okButton;
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this._cancelButton;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(784, 547);
            this.Controls.Add(this._menuStrip);
            this.Controls.Add(this._directoriesLabel);
            this.Controls.Add(this._sourceLabel);
            this.Controls.Add(this._destinationLabel);
            this.Controls.Add(this._sourceTextBox);
            this.Controls.Add(this._destinationTextBox);
            this.Controls.Add(this._sourceButton);
            this.Controls.Add(this._destinationButton);
            this.Controls.Add(this._operationsLabel);
            this.Controls.Add(this._operationsListView);
            this.Controls.Add(this._errorsLabel);
            this.Controls.Add(this._errorsListView);
            this.Controls.Add(this._footerPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this._menuStrip;
            this.Name = "BackupForm";
            this._footerPanel.ResumeLayout(false);
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel _footerPanel;
        private System.Windows.Forms.Label _sourceLabel;
        private System.Windows.Forms.Label _destinationLabel;
        private System.Windows.Forms.TextBox _sourceTextBox;
        private System.Windows.Forms.TextBox _destinationTextBox;
        private System.Windows.Forms.Button _sourceButton;
        private System.Windows.Forms.Button _destinationButton;
        private Controls.ExplorerListView _operationsListView;
        private System.Windows.Forms.ColumnHeader _operationTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader _operationNameColumnHeader;
        private System.Windows.Forms.ColumnHeader _operationPathColumnHeader;
        private System.Windows.Forms.ColumnHeader _operationResultColumnHeader;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.ImageList _imageList;
        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem _optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _quickScanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _autoScrollToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _projectSiteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _reportAnIssueToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _aboutToolStripMenuItem;
        private Controls.ExplorerListView _errorsListView;
        private System.Windows.Forms.ColumnHeader _errorType;
        private System.Windows.Forms.ColumnHeader _errorName;
        private System.Windows.Forms.ColumnHeader _errorPath;
        private System.Windows.Forms.ColumnHeader _errorMessage;
        private System.Windows.Forms.Label _directoriesLabel;
        private System.Windows.Forms.Label _operationsLabel;
        private System.Windows.Forms.Label _errorsLabel;
    }
}