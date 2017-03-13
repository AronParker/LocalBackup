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
            this._operationsListViewEx = new LocalBackup.Controls.ListViewEx();
            this.operationType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.operationFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.operationFilePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.operationResult = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._operationsImageList = new System.Windows.Forms.ImageList(this.components);
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._quickScanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._autoScrollToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._projectSiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._reportAnIssueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._footerPanel.SuspendLayout();
            this._menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _footerPanel
            // 
            this._footerPanel.BackColor = System.Drawing.SystemColors.Control;
            this._footerPanel.Controls.Add(this._progressBar);
            this._footerPanel.Controls.Add(this._cancelButton);
            this._footerPanel.Controls.Add(this._okButton);
            this._footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._footerPanel.Location = new System.Drawing.Point(0, 411);
            this._footerPanel.Name = "_footerPanel";
            this._footerPanel.Size = new System.Drawing.Size(784, 50);
            this._footerPanel.TabIndex = 8;
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
            this._okButton.Location = new System.Drawing.Point(526, 12);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(120, 26);
            this._okButton.TabIndex = 0;
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // _sourceLabel
            // 
            this._sourceLabel.Location = new System.Drawing.Point(12, 27);
            this._sourceLabel.Name = "_sourceLabel";
            this._sourceLabel.Size = new System.Drawing.Size(120, 23);
            this._sourceLabel.TabIndex = 1;
            this._sourceLabel.Text = "Source directory:";
            this._sourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _destinationLabel
            // 
            this._destinationLabel.Location = new System.Drawing.Point(12, 57);
            this._destinationLabel.Name = "_destinationLabel";
            this._destinationLabel.Size = new System.Drawing.Size(120, 23);
            this._destinationLabel.TabIndex = 2;
            this._destinationLabel.Text = "Destination directory:";
            this._destinationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _sourceTextBox
            // 
            this._sourceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceTextBox.Location = new System.Drawing.Point(138, 27);
            this._sourceTextBox.Name = "_sourceTextBox";
            this._sourceTextBox.Size = new System.Drawing.Size(528, 23);
            this._sourceTextBox.TabIndex = 3;
            // 
            // _destinationTextBox
            // 
            this._destinationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._destinationTextBox.Location = new System.Drawing.Point(138, 57);
            this._destinationTextBox.Name = "_destinationTextBox";
            this._destinationTextBox.Size = new System.Drawing.Size(528, 23);
            this._destinationTextBox.TabIndex = 4;
            // 
            // _sourceButton
            // 
            this._sourceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceButton.Location = new System.Drawing.Point(672, 27);
            this._sourceButton.Name = "_sourceButton";
            this._sourceButton.Size = new System.Drawing.Size(100, 23);
            this._sourceButton.TabIndex = 5;
            this._sourceButton.Text = "Browse...";
            this._sourceButton.UseVisualStyleBackColor = true;
            this._sourceButton.Click += new System.EventHandler(this.Browse_Click);
            // 
            // _destinationButton
            // 
            this._destinationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._destinationButton.Location = new System.Drawing.Point(672, 57);
            this._destinationButton.Name = "_destinationButton";
            this._destinationButton.Size = new System.Drawing.Size(100, 23);
            this._destinationButton.TabIndex = 6;
            this._destinationButton.Text = "Browse...";
            this._destinationButton.UseVisualStyleBackColor = true;
            this._destinationButton.Click += new System.EventHandler(this.Browse_Click);
            // 
            // _operationsListViewEx
            // 
            this._operationsListViewEx.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._operationsListViewEx.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.operationType,
            this.operationFileName,
            this.operationFilePath,
            this.operationResult});
            this._operationsListViewEx.FullRowSelect = true;
            this._operationsListViewEx.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._operationsListViewEx.Location = new System.Drawing.Point(12, 86);
            this._operationsListViewEx.Name = "_operationsListViewEx";
            this._operationsListViewEx.Size = new System.Drawing.Size(760, 319);
            this._operationsListViewEx.SmallImageList = this._operationsImageList;
            this._operationsListViewEx.TabIndex = 7;
            this._operationsListViewEx.UseCompatibleStateImageBehavior = false;
            this._operationsListViewEx.View = System.Windows.Forms.View.Details;
            this._operationsListViewEx.VirtualMode = true;
            this._operationsListViewEx.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.OperationsListView_RetrieveVirtualItem);
            // 
            // operationType
            // 
            this.operationType.Text = "Operation type";
            this.operationType.Width = 120;
            // 
            // operationFileName
            // 
            this.operationFileName.Text = "File name";
            this.operationFileName.Width = 150;
            // 
            // operationFilePath
            // 
            this.operationFilePath.Text = "File path";
            this.operationFilePath.Width = 286;
            // 
            // operationResult
            // 
            this.operationResult.Text = "Result";
            this.operationResult.Width = 200;
            // 
            // _operationsImageList
            // 
            this._operationsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_operationsImageList.ImageStream")));
            this._operationsImageList.TransparentColor = System.Drawing.Color.Transparent;
            this._operationsImageList.Images.SetKeyName(0, "folder_add.png");
            this._operationsImageList.Images.SetKeyName(1, "folder_delete.png");
            this._operationsImageList.Images.SetKeyName(2, "page_add.png");
            this._operationsImageList.Images.SetKeyName(3, "page_edit.png");
            this._operationsImageList.Images.SetKeyName(4, "page_gear.png");
            this._operationsImageList.Images.SetKeyName(5, "page_delete.png");
            this._operationsImageList.Images.SetKeyName(6, "folder_error.png");
            this._operationsImageList.Images.SetKeyName(7, "page_error.png");
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
            // BackupForm
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this._operationsListViewEx);
            this.Controls.Add(this._destinationButton);
            this.Controls.Add(this._sourceButton);
            this.Controls.Add(this._destinationTextBox);
            this.Controls.Add(this._sourceTextBox);
            this.Controls.Add(this._destinationLabel);
            this.Controls.Add(this._sourceLabel);
            this.Controls.Add(this._footerPanel);
            this.Controls.Add(this._menuStrip);
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
        private Controls.ListViewEx _operationsListViewEx;
        private System.Windows.Forms.ColumnHeader operationType;
        private System.Windows.Forms.ColumnHeader operationFileName;
        private System.Windows.Forms.ColumnHeader operationFilePath;
        private System.Windows.Forms.ColumnHeader operationResult;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.ImageList _operationsImageList;
        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem _optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _quickScanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _autoScrollToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _projectSiteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _reportAnIssueToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _aboutToolStripMenuItem;
    }
}