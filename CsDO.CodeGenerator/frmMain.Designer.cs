namespace CsDO.CodeGenerator
{
    partial class frmMain
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
            this.Status = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.ProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.dlgFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.lbxDatabase = new System.Windows.Forms.CheckedListBox();
            this.cbxLanguage = new System.Windows.Forms.ComboBox();
            this.cbxDriver = new System.Windows.Forms.ComboBox();
            this.lbDBServer = new System.Windows.Forms.Label();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.lblDBDriver = new System.Windows.Forms.Label();
            this.txtDBServer = new System.Windows.Forms.TextBox();
            this.lblNamespace = new System.Windows.Forms.Label();
            this.txtNamespace = new System.Windows.Forms.TextBox();
            this.txtDestinationPath = new System.Windows.Forms.TextBox();
            this.lblDestination = new System.Windows.Forms.Label();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.btnFolderSelect = new System.Windows.Forms.Button();
            this.txtDBUserName = new System.Windows.Forms.TextBox();
            this.lblDBUserName = new System.Windows.Forms.Label();
            this.txtDBPassword = new System.Windows.Forms.TextBox();
            this.lblDBPassword = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.txtDBName = new System.Windows.Forms.TextBox();
            this.lblDBName = new System.Windows.Forms.Label();
            this.txtDBPort = new System.Windows.Forms.TextBox();
            this.lblDBPort = new System.Windows.Forms.Label();
            this.Status.SuspendLayout();
            this.SuspendLayout();
            // 
            // Status
            // 
            this.Status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.ProgressBar});
            this.Status.Location = new System.Drawing.Point(0, 326);
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(437, 22);
            this.Status.TabIndex = 0;
            this.Status.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(109, 17);
            this.lblStatus.Text = "toolStripStatusLabel1";
            // 
            // ProgressBar
            // 
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(100, 16);
            // 
            // dlgFolder
            // 
            this.dlgFolder.Description = "Select Destination Folder";
            // 
            // lbxDatabase
            // 
            this.lbxDatabase.CheckOnClick = true;
            this.lbxDatabase.FormattingEnabled = true;
            this.lbxDatabase.Location = new System.Drawing.Point(206, 40);
            this.lbxDatabase.Name = "lbxDatabase";
            this.lbxDatabase.Size = new System.Drawing.Size(219, 274);
            this.lbxDatabase.TabIndex = 12;
            // 
            // cbxLanguage
            // 
            this.cbxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxLanguage.FormattingEnabled = true;
            this.cbxLanguage.Items.AddRange(new object[] {
            "C#",
            "C++",
            "VB",
            "VJ#"});
            this.cbxLanguage.Location = new System.Drawing.Point(268, 12);
            this.cbxLanguage.Name = "cbxLanguage";
            this.cbxLanguage.Size = new System.Drawing.Size(157, 21);
            this.cbxLanguage.Sorted = true;
            this.cbxLanguage.TabIndex = 11;
            // 
            // cbxDriver
            // 
            this.cbxDriver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDriver.FormattingEnabled = true;
            this.cbxDriver.Items.AddRange(new object[] {
            "OleDB",
            "PostgreSQL",
            "SQL Server"});
            this.cbxDriver.Location = new System.Drawing.Point(100, 12);
            this.cbxDriver.Name = "cbxDriver";
            this.cbxDriver.Size = new System.Drawing.Size(100, 21);
            this.cbxDriver.TabIndex = 1;
            this.cbxDriver.SelectedIndexChanged += new System.EventHandler(this.cbxDriver_SelectedIndexChanged);
            // 
            // lbDBServer
            // 
            this.lbDBServer.AutoSize = true;
            this.lbDBServer.Location = new System.Drawing.Point(7, 43);
            this.lbDBServer.Name = "lbDBServer";
            this.lbDBServer.Size = new System.Drawing.Size(56, 13);
            this.lbDBServer.TabIndex = 4;
            this.lbDBServer.Text = "DB &Server";
            // 
            // lblLanguage
            // 
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Location = new System.Drawing.Point(203, 15);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(55, 13);
            this.lblLanguage.TabIndex = 8;
            this.lblLanguage.Text = "&Language";
            // 
            // lblDBDriver
            // 
            this.lblDBDriver.AutoSize = true;
            this.lblDBDriver.Location = new System.Drawing.Point(7, 15);
            this.lblDBDriver.Name = "lblDBDriver";
            this.lblDBDriver.Size = new System.Drawing.Size(53, 13);
            this.lblDBDriver.TabIndex = 9;
            this.lblDBDriver.Text = "DB &Driver";
            // 
            // txtDBServer
            // 
            this.txtDBServer.Location = new System.Drawing.Point(100, 40);
            this.txtDBServer.Name = "txtDBServer";
            this.txtDBServer.Size = new System.Drawing.Size(100, 20);
            this.txtDBServer.TabIndex = 2;
            this.txtDBServer.TextChanged += new System.EventHandler(this.txtDBServer_TextChanged);
            // 
            // lblNamespace
            // 
            this.lblNamespace.AutoSize = true;
            this.lblNamespace.Location = new System.Drawing.Point(7, 202);
            this.lblNamespace.Name = "lblNamespace";
            this.lblNamespace.Size = new System.Drawing.Size(64, 13);
            this.lblNamespace.TabIndex = 11;
            this.lblNamespace.Text = "&Namespace";
            // 
            // txtNamespace
            // 
            this.txtNamespace.Location = new System.Drawing.Point(100, 199);
            this.txtNamespace.Name = "txtNamespace";
            this.txtNamespace.Size = new System.Drawing.Size(100, 20);
            this.txtNamespace.TabIndex = 8;
            this.txtNamespace.Text = "Application.Persist";
            // 
            // txtDestinationPath
            // 
            this.txtDestinationPath.Location = new System.Drawing.Point(100, 224);
            this.txtDestinationPath.Name = "txtDestinationPath";
            this.txtDestinationPath.Size = new System.Drawing.Size(100, 20);
            this.txtDestinationPath.TabIndex = 9;
            // 
            // lblDestination
            // 
            this.lblDestination.AutoSize = true;
            this.lblDestination.Location = new System.Drawing.Point(7, 228);
            this.lblDestination.Name = "lblDestination";
            this.lblDestination.Size = new System.Drawing.Size(92, 13);
            this.lblDestination.TabIndex = 14;
            this.lblDestination.Text = "Destination &Folder";
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.Location = new System.Drawing.Point(100, 170);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(100, 23);
            this.btnTestConnection.TabIndex = 7;
            this.btnTestConnection.Text = "&Test Connection";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
            // 
            // btnFolderSelect
            // 
            this.btnFolderSelect.Location = new System.Drawing.Point(100, 250);
            this.btnFolderSelect.Name = "btnFolderSelect";
            this.btnFolderSelect.Size = new System.Drawing.Size(100, 23);
            this.btnFolderSelect.TabIndex = 10;
            this.btnFolderSelect.Text = "Select F&older";
            this.btnFolderSelect.UseVisualStyleBackColor = true;
            this.btnFolderSelect.Click += new System.EventHandler(this.btnFolderSelect_Click);
            // 
            // txtDBUserName
            // 
            this.txtDBUserName.Location = new System.Drawing.Point(100, 118);
            this.txtDBUserName.Name = "txtDBUserName";
            this.txtDBUserName.Size = new System.Drawing.Size(100, 20);
            this.txtDBUserName.TabIndex = 5;
            this.txtDBUserName.TextChanged += new System.EventHandler(this.txtDBUserName_TextChanged);
            // 
            // lblDBUserName
            // 
            this.lblDBUserName.AutoSize = true;
            this.lblDBUserName.Location = new System.Drawing.Point(7, 121);
            this.lblDBUserName.Name = "lblDBUserName";
            this.lblDBUserName.Size = new System.Drawing.Size(78, 13);
            this.lblDBUserName.TabIndex = 17;
            this.lblDBUserName.Text = "DB &User Name";
            // 
            // txtDBPassword
            // 
            this.txtDBPassword.Location = new System.Drawing.Point(100, 144);
            this.txtDBPassword.Name = "txtDBPassword";
            this.txtDBPassword.Size = new System.Drawing.Size(100, 20);
            this.txtDBPassword.TabIndex = 6;
            this.txtDBPassword.UseSystemPasswordChar = true;
            this.txtDBPassword.TextChanged += new System.EventHandler(this.txtDBPassword_TextChanged);
            // 
            // lblDBPassword
            // 
            this.lblDBPassword.AutoSize = true;
            this.lblDBPassword.Location = new System.Drawing.Point(7, 147);
            this.lblDBPassword.Name = "lblDBPassword";
            this.lblDBPassword.Size = new System.Drawing.Size(71, 13);
            this.lblDBPassword.TabIndex = 19;
            this.lblDBPassword.Text = "DB &Password";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(10, 291);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(190, 23);
            this.btnGenerate.TabIndex = 13;
            this.btnGenerate.Text = "&Generate Code";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // txtDBName
            // 
            this.txtDBName.Location = new System.Drawing.Point(100, 66);
            this.txtDBName.Name = "txtDBName";
            this.txtDBName.Size = new System.Drawing.Size(100, 20);
            this.txtDBName.TabIndex = 3;
            this.txtDBName.TextChanged += new System.EventHandler(this.txtDBName_TextChanged);
            // 
            // lblDBName
            // 
            this.lblDBName.AutoSize = true;
            this.lblDBName.Location = new System.Drawing.Point(7, 69);
            this.lblDBName.Name = "lblDBName";
            this.lblDBName.Size = new System.Drawing.Size(53, 13);
            this.lblDBName.TabIndex = 22;
            this.lblDBName.Text = "DB N&ame";
            // 
            // txtDBPort
            // 
            this.txtDBPort.Location = new System.Drawing.Point(100, 92);
            this.txtDBPort.Name = "txtDBPort";
            this.txtDBPort.Size = new System.Drawing.Size(100, 20);
            this.txtDBPort.TabIndex = 4;
            this.txtDBPort.TextChanged += new System.EventHandler(this.txtDBPort_TextChanged);
            // 
            // lblDBPort
            // 
            this.lblDBPort.AutoSize = true;
            this.lblDBPort.Location = new System.Drawing.Point(7, 95);
            this.lblDBPort.Name = "lblDBPort";
            this.lblDBPort.Size = new System.Drawing.Size(44, 13);
            this.lblDBPort.TabIndex = 24;
            this.lblDBPort.Text = "DB P&ort";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 348);
            this.Controls.Add(this.txtDBPort);
            this.Controls.Add(this.lblDBPort);
            this.Controls.Add(this.txtDBName);
            this.Controls.Add(this.lblDBName);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.txtDBPassword);
            this.Controls.Add(this.lblDBPassword);
            this.Controls.Add(this.txtDBUserName);
            this.Controls.Add(this.lblDBUserName);
            this.Controls.Add(this.btnFolderSelect);
            this.Controls.Add(this.btnTestConnection);
            this.Controls.Add(this.lblDestination);
            this.Controls.Add(this.txtDestinationPath);
            this.Controls.Add(this.txtNamespace);
            this.Controls.Add(this.lblNamespace);
            this.Controls.Add(this.txtDBServer);
            this.Controls.Add(this.lblDBDriver);
            this.Controls.Add(this.lblLanguage);
            this.Controls.Add(this.lbDBServer);
            this.Controls.Add(this.cbxDriver);
            this.Controls.Add(this.cbxLanguage);
            this.Controls.Add(this.lbxDatabase);
            this.Controls.Add(this.Status);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CsDO Code Generator";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Status.ResumeLayout(false);
            this.Status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip Status;
        private System.Windows.Forms.FolderBrowserDialog dlgFolder;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripProgressBar ProgressBar;
        private System.Windows.Forms.CheckedListBox lbxDatabase;
        private System.Windows.Forms.ComboBox cbxLanguage;
        private System.Windows.Forms.ComboBox cbxDriver;
        private System.Windows.Forms.Label lbDBServer;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.Label lblDBDriver;
        private System.Windows.Forms.TextBox txtDBServer;
        private System.Windows.Forms.Label lblNamespace;
        private System.Windows.Forms.TextBox txtNamespace;
        private System.Windows.Forms.TextBox txtDestinationPath;
        private System.Windows.Forms.Label lblDestination;
        private System.Windows.Forms.Button btnTestConnection;
        private System.Windows.Forms.Button btnFolderSelect;
        private System.Windows.Forms.TextBox txtDBUserName;
        private System.Windows.Forms.Label lblDBUserName;
        private System.Windows.Forms.TextBox txtDBPassword;
        private System.Windows.Forms.Label lblDBPassword;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.TextBox txtDBName;
        private System.Windows.Forms.Label lblDBName;
        private System.Windows.Forms.TextBox txtDBPort;
        private System.Windows.Forms.Label lblDBPort;
    }
}

