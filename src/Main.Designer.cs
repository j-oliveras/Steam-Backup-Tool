namespace steamBackup
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.thread1 = new System.ComponentModel.BackgroundWorker();
            this.thread2 = new System.ComponentModel.BackgroundWorker();
            this.tbxSteamDir = new System.Windows.Forms.TextBox();
            this.btnBrowseSteam = new System.Windows.Forms.Button();
            this.btnBrowseBackup = new System.Windows.Forms.Button();
            this.tbxBackupDir = new System.Windows.Forms.TextBox();
            this.lbl0 = new System.Windows.Forms.Label();
            this.thread0 = new System.ComponentModel.BackgroundWorker();
            this.thread3 = new System.ComponentModel.BackgroundWorker();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.lblStarted = new System.Windows.Forms.Label();
            this.tBoxInst0Out = new System.Windows.Forms.RichTextBox();
            this.tBoxInst1Out = new System.Windows.Forms.RichTextBox();
            this.lbl1 = new System.Windows.Forms.Label();
            this.tBoxInst2Out = new System.Windows.Forms.RichTextBox();
            this.lbl2 = new System.Windows.Forms.Label();
            this.tBoxInst3Out = new System.Windows.Forms.RichTextBox();
            this.lbl3 = new System.Windows.Forms.Label();
            this.lbl4 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnFindSteam = new System.Windows.Forms.Button();
            this.btnBackup = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnShowLog = new System.Windows.Forms.Button();
            this.listView = new System.Windows.Forms.ListView();
            this.cHeadId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CHeadName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cHeadProg = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cHeadStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cHeadArg = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cHeadAcfId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnPause = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label1.Font = new System.Drawing.Font("Courier New", 24F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(357, 36);
            this.label1.TabIndex = 0;
            this.label1.Text = "Steam Backup Tool!";
            this.label1.Click += new System.EventHandler(this.title_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label2.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(69, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(259, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Made for the Overclock.net Community";
            this.label2.Click += new System.EventHandler(this.title_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label3.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(96, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(189, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = " by [OCN]FiX and [OCN]Du-z";
            this.label3.Click += new System.EventHandler(this.title_Click);
            // 
            // thread1
            // 
            this.thread1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.thread1_DoWork);
            // 
            // thread2
            // 
            this.thread2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.thread2_DoWork);
            // 
            // tbxSteamDir
            // 
            this.tbxSteamDir.Location = new System.Drawing.Point(26, 113);
            this.tbxSteamDir.Name = "tbxSteamDir";
            this.tbxSteamDir.Size = new System.Drawing.Size(247, 20);
            this.tbxSteamDir.TabIndex = 4;
            this.tbxSteamDir.Enter += new System.EventHandler(this.tbxSteamDir_Enter);
            // 
            // btnBrowseSteam
            // 
            this.btnBrowseSteam.Location = new System.Drawing.Point(279, 112);
            this.btnBrowseSteam.Name = "btnBrowseSteam";
            this.btnBrowseSteam.Size = new System.Drawing.Size(54, 20);
            this.btnBrowseSteam.TabIndex = 5;
            this.btnBrowseSteam.Text = "Browse";
            this.btnBrowseSteam.UseVisualStyleBackColor = true;
            this.btnBrowseSteam.Click += new System.EventHandler(this.btnBrowseSteam_Click);
            // 
            // btnBrowseBackup
            // 
            this.btnBrowseBackup.Location = new System.Drawing.Point(279, 148);
            this.btnBrowseBackup.Name = "btnBrowseBackup";
            this.btnBrowseBackup.Size = new System.Drawing.Size(54, 20);
            this.btnBrowseBackup.TabIndex = 7;
            this.btnBrowseBackup.Text = "Browse";
            this.btnBrowseBackup.UseVisualStyleBackColor = true;
            this.btnBrowseBackup.Click += new System.EventHandler(this.btnBrowseBackup_Click);
            // 
            // tbxBackupDir
            // 
            this.tbxBackupDir.Location = new System.Drawing.Point(26, 149);
            this.tbxBackupDir.Name = "tbxBackupDir";
            this.tbxBackupDir.Size = new System.Drawing.Size(247, 20);
            this.tbxBackupDir.TabIndex = 6;
            this.tbxBackupDir.Enter += new System.EventHandler(this.tbxBackupDir_Enter);
            // 
            // lbl0
            // 
            this.lbl0.AutoSize = true;
            this.lbl0.Location = new System.Drawing.Point(12, 356);
            this.lbl0.Name = "lbl0";
            this.lbl0.Size = new System.Drawing.Size(23, 13);
            this.lbl0.TabIndex = 13;
            this.lbl0.Text = "lbl0";
            // 
            // thread0
            // 
            this.thread0.DoWork += new System.ComponentModel.DoWorkEventHandler(this.thread0_DoWork);
            // 
            // thread3
            // 
            this.thread3.DoWork += new System.ComponentModel.DoWorkEventHandler(this.thread3_DoWork);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(15, 279);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(363, 23);
            this.progressBar.TabIndex = 15;
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(23, 305);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(102, 39);
            this.lblProgress.TabIndex = 20;
            this.lblProgress.Text = "Jobs started: 0 of 0\nJobs skipped: 0 of 0\nJobs total: 0 of 0";
            // 
            // lblStarted
            // 
            this.lblStarted.AutoSize = true;
            this.lblStarted.Location = new System.Drawing.Point(23, 263);
            this.lblStarted.Name = "lblStarted";
            this.lblStarted.Size = new System.Drawing.Size(51, 13);
            this.lblStarted.TabIndex = 21;
            this.lblStarted.Text = "lblStarted";
            // 
            // tBoxInst0Out
            // 
            this.tBoxInst0Out.BackColor = System.Drawing.Color.White;
            this.tBoxInst0Out.DetectUrls = false;
            this.tBoxInst0Out.Location = new System.Drawing.Point(12, 372);
            this.tBoxInst0Out.Name = "tBoxInst0Out";
            this.tBoxInst0Out.ReadOnly = true;
            this.tBoxInst0Out.Size = new System.Drawing.Size(365, 61);
            this.tBoxInst0Out.TabIndex = 22;
            this.tBoxInst0Out.Text = "";
            this.tBoxInst0Out.WordWrap = false;
            // 
            // tBoxInst1Out
            // 
            this.tBoxInst1Out.BackColor = System.Drawing.Color.White;
            this.tBoxInst1Out.DetectUrls = false;
            this.tBoxInst1Out.Location = new System.Drawing.Point(12, 452);
            this.tBoxInst1Out.Name = "tBoxInst1Out";
            this.tBoxInst1Out.ReadOnly = true;
            this.tBoxInst1Out.Size = new System.Drawing.Size(365, 61);
            this.tBoxInst1Out.TabIndex = 24;
            this.tBoxInst1Out.Text = "";
            this.tBoxInst1Out.WordWrap = false;
            // 
            // lbl1
            // 
            this.lbl1.AutoSize = true;
            this.lbl1.Location = new System.Drawing.Point(12, 436);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(23, 13);
            this.lbl1.TabIndex = 23;
            this.lbl1.Text = "lbl1";
            // 
            // tBoxInst2Out
            // 
            this.tBoxInst2Out.BackColor = System.Drawing.Color.White;
            this.tBoxInst2Out.DetectUrls = false;
            this.tBoxInst2Out.Location = new System.Drawing.Point(12, 532);
            this.tBoxInst2Out.Name = "tBoxInst2Out";
            this.tBoxInst2Out.ReadOnly = true;
            this.tBoxInst2Out.Size = new System.Drawing.Size(365, 61);
            this.tBoxInst2Out.TabIndex = 26;
            this.tBoxInst2Out.Text = "";
            this.tBoxInst2Out.WordWrap = false;
            // 
            // lbl2
            // 
            this.lbl2.AutoSize = true;
            this.lbl2.Location = new System.Drawing.Point(12, 516);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(23, 13);
            this.lbl2.TabIndex = 25;
            this.lbl2.Text = "lbl2";
            // 
            // tBoxInst3Out
            // 
            this.tBoxInst3Out.BackColor = System.Drawing.Color.White;
            this.tBoxInst3Out.DetectUrls = false;
            this.tBoxInst3Out.Location = new System.Drawing.Point(12, 612);
            this.tBoxInst3Out.Name = "tBoxInst3Out";
            this.tBoxInst3Out.ReadOnly = true;
            this.tBoxInst3Out.Size = new System.Drawing.Size(365, 61);
            this.tBoxInst3Out.TabIndex = 28;
            this.tBoxInst3Out.Text = "";
            this.tBoxInst3Out.WordWrap = false;
            // 
            // lbl3
            // 
            this.lbl3.AutoSize = true;
            this.lbl3.Location = new System.Drawing.Point(12, 596);
            this.lbl3.Name = "lbl3";
            this.lbl3.Size = new System.Drawing.Size(23, 13);
            this.lbl3.TabIndex = 27;
            this.lbl3.Text = "lbl3";
            // 
            // lbl4
            // 
            this.lbl4.AutoSize = true;
            this.lbl4.Location = new System.Drawing.Point(12, 676);
            this.lbl4.Name = "lbl4";
            this.lbl4.Size = new System.Drawing.Size(23, 13);
            this.lbl4.TabIndex = 29;
            this.lbl4.Text = "lbl4";
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(146, 200);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(96, 45);
            this.btnCancel.TabIndex = 30;
            this.btnCancel.Text = "CANCEL";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Visible = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnFindSteam
            // 
            this.btnFindSteam.Location = new System.Drawing.Point(339, 112);
            this.btnFindSteam.Name = "btnFindSteam";
            this.btnFindSteam.Size = new System.Drawing.Size(39, 21);
            this.btnFindSteam.TabIndex = 1;
            this.btnFindSteam.Text = "Find";
            this.btnFindSteam.UseVisualStyleBackColor = true;
            this.btnFindSteam.Click += new System.EventHandler(this.btnFindSteam_Click);
            // 
            // btnBackup
            // 
            this.btnBackup.Location = new System.Drawing.Point(26, 200);
            this.btnBackup.Name = "btnBackup";
            this.btnBackup.Size = new System.Drawing.Size(96, 45);
            this.btnBackup.TabIndex = 34;
            this.btnBackup.Text = "Backup";
            this.btnBackup.UseVisualStyleBackColor = true;
            this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);
            // 
            // btnRestore
            // 
            this.btnRestore.Location = new System.Drawing.Point(146, 200);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(96, 45);
            this.btnRestore.TabIndex = 38;
            this.btnRestore.Text = "Restore";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // btnShowLog
            // 
            this.btnShowLog.Location = new System.Drawing.Point(279, 200);
            this.btnShowLog.Name = "btnShowLog";
            this.btnShowLog.Size = new System.Drawing.Size(98, 45);
            this.btnShowLog.TabIndex = 39;
            this.btnShowLog.Text = "Show Job List";
            this.btnShowLog.UseVisualStyleBackColor = true;
            this.btnShowLog.Visible = false;
            this.btnShowLog.Click += new System.EventHandler(this.btnShowList_Click);
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cHeadId,
            this.CHeadName,
            this.cHeadProg,
            this.cHeadStatus,
            this.cHeadArg,
            this.cHeadAcfId});
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.Location = new System.Drawing.Point(410, 12);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(568, 346);
            this.listView.TabIndex = 40;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // cHeadId
            // 
            this.cHeadId.Text = "Job #";
            this.cHeadId.Width = 43;
            // 
            // CHeadName
            // 
            this.CHeadName.Text = "Name";
            this.CHeadName.Width = 150;
            // 
            // cHeadProg
            // 
            this.cHeadProg.Text = "Program";
            this.cHeadProg.Width = 75;
            // 
            // cHeadStatus
            // 
            this.cHeadStatus.Text = "Status";
            this.cHeadStatus.Width = 55;
            // 
            // cHeadArg
            // 
            this.cHeadArg.Text = "Argument";
            this.cHeadArg.Width = 1250;
            // 
            // cHeadAcfId
            // 
            this.cHeadAcfId.Text = "ACF ID";
            this.cHeadAcfId.Width = 150;
            // 
            // btnPause
            // 
            this.btnPause.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPause.Location = new System.Drawing.Point(26, 200);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(96, 45);
            this.btnPause.TabIndex = 41;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Visible = false;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(390, 370);
            this.Controls.Add(this.listView);
            this.Controls.Add(this.btnShowLog);
            this.Controls.Add(this.btnFindSteam);
            this.Controls.Add(this.lbl4);
            this.Controls.Add(this.tBoxInst3Out);
            this.Controls.Add(this.lbl3);
            this.Controls.Add(this.tBoxInst2Out);
            this.Controls.Add(this.lbl2);
            this.Controls.Add(this.tBoxInst1Out);
            this.Controls.Add(this.lbl1);
            this.Controls.Add(this.tBoxInst0Out);
            this.Controls.Add(this.lblStarted);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lbl0);
            this.Controls.Add(this.btnBrowseBackup);
            this.Controls.Add(this.tbxBackupDir);
            this.Controls.Add(this.btnBrowseSteam);
            this.Controls.Add(this.tbxSteamDir);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnBackup);
            this.Controls.Add(this.btnRestore);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnCancel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "Steam Backup Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.main_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.ComponentModel.BackgroundWorker thread1;
        private System.ComponentModel.BackgroundWorker thread2;
        private System.Windows.Forms.Button btnBrowseSteam;
        private System.Windows.Forms.Button btnBrowseBackup;
        private System.Windows.Forms.Label lbl0;
        private System.ComponentModel.BackgroundWorker thread0;
        private System.ComponentModel.BackgroundWorker thread3;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Label lblStarted;
        private System.Windows.Forms.RichTextBox tBoxInst0Out;
        private System.Windows.Forms.RichTextBox tBoxInst1Out;
        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.RichTextBox tBoxInst2Out;
        private System.Windows.Forms.Label lbl2;
        private System.Windows.Forms.RichTextBox tBoxInst3Out;
        private System.Windows.Forms.Label lbl3;
        private System.Windows.Forms.Label lbl4;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnFindSteam;
        private System.Windows.Forms.Button btnBackup;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnShowLog;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader cHeadProg;
        private System.Windows.Forms.ColumnHeader cHeadArg;
        private System.Windows.Forms.ColumnHeader cHeadStatus;
        private System.Windows.Forms.ColumnHeader cHeadId;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.ColumnHeader CHeadName;
        private System.Windows.Forms.TextBox tbxSteamDir;
        private System.Windows.Forms.TextBox tbxBackupDir;
        private System.Windows.Forms.ColumnHeader cHeadAcfId;
    }
}

