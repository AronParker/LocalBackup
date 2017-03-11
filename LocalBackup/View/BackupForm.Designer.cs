namespace LocalBackup.View
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
            this._modeLabel = new System.Windows.Forms.Label();
            this._sourceTextBox = new System.Windows.Forms.TextBox();
            this._destinationTextBox = new System.Windows.Forms.TextBox();
            this._modeComboBox = new System.Windows.Forms.ComboBox();
            this._sourceButton = new System.Windows.Forms.Button();
            this._destinationButton = new System.Windows.Forms.Button();
            this._autoScrollCheckBox = new System.Windows.Forms.CheckBox();
            this._operationsListViewEx = new LocalBackup.Controls.ListViewEx();
            this.operationType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.operationFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.operationFilePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.operationResult = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._operationsImageList = new System.Windows.Forms.ImageList(this.components);
            this._footerPanel.SuspendLayout();
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
            this._footerPanel.TabIndex = 10;
            // 
            // _progressBar
            // 
            this._progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._progressBar.Location = new System.Drawing.Point(12, 12);
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
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(526, 12);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(120, 26);
            this._okButton.TabIndex = 0;
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _sourceLabel
            // 
            this._sourceLabel.Location = new System.Drawing.Point(12, 12);
            this._sourceLabel.Name = "_sourceLabel";
            this._sourceLabel.Size = new System.Drawing.Size(120, 23);
            this._sourceLabel.TabIndex = 0;
            this._sourceLabel.Text = "Source directory:";
            this._sourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _destinationLabel
            // 
            this._destinationLabel.Location = new System.Drawing.Point(12, 42);
            this._destinationLabel.Name = "_destinationLabel";
            this._destinationLabel.Size = new System.Drawing.Size(120, 23);
            this._destinationLabel.TabIndex = 1;
            this._destinationLabel.Text = "Destination directory:";
            this._destinationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _modeLabel
            // 
            this._modeLabel.Location = new System.Drawing.Point(12, 71);
            this._modeLabel.Name = "_modeLabel";
            this._modeLabel.Size = new System.Drawing.Size(120, 23);
            this._modeLabel.TabIndex = 2;
            this._modeLabel.Text = "Backup mode:";
            this._modeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _sourceTextBox
            // 
            this._sourceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceTextBox.Location = new System.Drawing.Point(138, 12);
            this._sourceTextBox.Name = "_sourceTextBox";
            this._sourceTextBox.Size = new System.Drawing.Size(528, 23);
            this._sourceTextBox.TabIndex = 3;
            this._sourceTextBox.Text = "C:\\Users\\Aron\\Desktop\\1";
            // 
            // _destinationTextBox
            // 
            this._destinationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._destinationTextBox.Location = new System.Drawing.Point(138, 42);
            this._destinationTextBox.Name = "_destinationTextBox";
            this._destinationTextBox.Size = new System.Drawing.Size(528, 23);
            this._destinationTextBox.TabIndex = 4;
            this._destinationTextBox.Text = "C:\\Users\\Aron\\Desktop\\2";
            // 
            // _modeComboBox
            // 
            this._modeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._modeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._modeComboBox.FormattingEnabled = true;
            this._modeComboBox.Items.AddRange(new object[] {
            "Quick scan",
            "Full scan"});
            this._modeComboBox.Location = new System.Drawing.Point(138, 71);
            this._modeComboBox.Name = "_modeComboBox";
            this._modeComboBox.Size = new System.Drawing.Size(528, 23);
            this._modeComboBox.TabIndex = 5;
            // 
            // _sourceButton
            // 
            this._sourceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceButton.Location = new System.Drawing.Point(672, 12);
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
            this._destinationButton.Location = new System.Drawing.Point(672, 42);
            this._destinationButton.Name = "_destinationButton";
            this._destinationButton.Size = new System.Drawing.Size(100, 23);
            this._destinationButton.TabIndex = 7;
            this._destinationButton.Text = "Browse...";
            this._destinationButton.UseVisualStyleBackColor = true;
            this._destinationButton.Click += new System.EventHandler(this.Browse_Click);
            // 
            // _autoScrollCheckBox
            // 
            this._autoScrollCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._autoScrollCheckBox.Location = new System.Drawing.Point(672, 71);
            this._autoScrollCheckBox.Name = "_autoScrollCheckBox";
            this._autoScrollCheckBox.Size = new System.Drawing.Size(100, 23);
            this._autoScrollCheckBox.TabIndex = 8;
            this._autoScrollCheckBox.Text = "Auto scroll";
            this._autoScrollCheckBox.UseVisualStyleBackColor = true;
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
            this._operationsListViewEx.Location = new System.Drawing.Point(12, 100);
            this._operationsListViewEx.Name = "_operationsListViewEx";
            this._operationsListViewEx.Size = new System.Drawing.Size(760, 305);
            this._operationsListViewEx.SmallImageList = this._operationsImageList;
            this._operationsListViewEx.TabIndex = 9;
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
            this._operationsImageList.Images.SetKeyName(1, "folder_edit.png");
            this._operationsImageList.Images.SetKeyName(2, "folder_delete.png");
            this._operationsImageList.Images.SetKeyName(3, "page_add.png");
            this._operationsImageList.Images.SetKeyName(4, "page_edit.png");
            this._operationsImageList.Images.SetKeyName(5, "page_delete.png");
            this._operationsImageList.Images.SetKeyName(6, "folder_error.png");
            this._operationsImageList.Images.SetKeyName(7, "page_error.png");
            // 
            // MainForm
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this._operationsListViewEx);
            this.Controls.Add(this._autoScrollCheckBox);
            this.Controls.Add(this._destinationButton);
            this.Controls.Add(this._sourceButton);
            this.Controls.Add(this._modeComboBox);
            this.Controls.Add(this._destinationTextBox);
            this.Controls.Add(this._sourceTextBox);
            this.Controls.Add(this._modeLabel);
            this.Controls.Add(this._destinationLabel);
            this.Controls.Add(this._sourceLabel);
            this.Controls.Add(this._footerPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Local Backup";
            this._footerPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel _footerPanel;
        private System.Windows.Forms.Label _sourceLabel;
        private System.Windows.Forms.Label _destinationLabel;
        private System.Windows.Forms.Label _modeLabel;
        private System.Windows.Forms.TextBox _sourceTextBox;
        private System.Windows.Forms.TextBox _destinationTextBox;
        private System.Windows.Forms.ComboBox _modeComboBox;
        private System.Windows.Forms.Button _sourceButton;
        private System.Windows.Forms.Button _destinationButton;
        private System.Windows.Forms.CheckBox _autoScrollCheckBox;
        private Controls.ListViewEx _operationsListViewEx;
        private System.Windows.Forms.ColumnHeader operationType;
        private System.Windows.Forms.ColumnHeader operationFileName;
        private System.Windows.Forms.ColumnHeader operationFilePath;
        private System.Windows.Forms.ColumnHeader operationResult;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.ImageList _operationsImageList;
    }
}