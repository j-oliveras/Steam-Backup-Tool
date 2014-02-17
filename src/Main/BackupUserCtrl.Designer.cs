namespace steamBackup
{
    partial class BackupUserCtrl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackupUserCtrl));
            this.chkList = new System.Windows.Forms.CheckedListBox();
            this.btnBupAll = new System.Windows.Forms.Button();
            this.btnUpdBup = new System.Windows.Forms.Button();
            this.tbarComp = new System.Windows.Forms.TrackBar();
            this.lblComp = new System.Windows.Forms.Label();
            this.lblRamBackup = new System.Windows.Forms.Label();
            this.lblThread = new System.Windows.Forms.Label();
            this.tbarThread = new System.Windows.Forms.TrackBar();
            this.cBoxDelBup = new System.Windows.Forms.CheckBox();
            this.btnCancelBup = new System.Windows.Forms.Button();
            this.btnStartBup = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.infoBox = new System.Windows.Forms.RichTextBox();
            this.tbarThreadLbl = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btnBupNone = new System.Windows.Forms.Button();
            this.btnUpdLib = new System.Windows.Forms.Button();
            this.cBoxLzma2 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.tbarComp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbarThread)).BeginInit();
            this.SuspendLayout();
            // 
            // chkList
            // 
            this.chkList.FormattingEnabled = true;
            this.chkList.Location = new System.Drawing.Point(12, 106);
            this.chkList.Name = "chkList";
            this.chkList.Size = new System.Drawing.Size(260, 454);
            this.chkList.Sorted = true;
            this.chkList.TabIndex = 0;
            this.chkList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chkList_ItemCheck);
            this.chkList.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.chkList.MouseHover += new System.EventHandler(this.chkList_MouseHover);
            // 
            // btnBupAll
            // 
            this.btnBupAll.AccessibleDescription = "";
            this.btnBupAll.Location = new System.Drawing.Point(12, 38);
            this.btnBupAll.Name = "btnBupAll";
            this.btnBupAll.Size = new System.Drawing.Size(120, 25);
            this.btnBupAll.TabIndex = 1;
            this.btnBupAll.Text = "Backup All";
            this.btnBupAll.UseVisualStyleBackColor = true;
            this.btnBupAll.Click += new System.EventHandler(this.btnBupAll_Click);
            this.btnBupAll.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.btnBupAll.MouseHover += new System.EventHandler(this.btnBupAll_MouseHover);
            // 
            // btnUpdBup
            // 
            this.btnUpdBup.Location = new System.Drawing.Point(152, 38);
            this.btnUpdBup.Name = "btnUpdBup";
            this.btnUpdBup.Size = new System.Drawing.Size(120, 25);
            this.btnUpdBup.TabIndex = 2;
            this.btnUpdBup.Text = "Update Backup";
            this.btnUpdBup.UseVisualStyleBackColor = true;
            this.btnUpdBup.Click += new System.EventHandler(this.btnUpdBup_Click);
            this.btnUpdBup.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.btnUpdBup.MouseHover += new System.EventHandler(this.btnUpdBup_MouseHover);
            // 
            // tbarComp
            // 
            this.tbarComp.LargeChange = 1;
            this.tbarComp.Location = new System.Drawing.Point(308, 190);
            this.tbarComp.Maximum = 5;
            this.tbarComp.Name = "tbarComp";
            this.tbarComp.Size = new System.Drawing.Size(120, 45);
            this.tbarComp.TabIndex = 34;
            this.tbarComp.Scroll += new System.EventHandler(this.tbarComp_Scroll);
            this.tbarComp.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.tbarComp.MouseHover += new System.EventHandler(this.lblComp_MouseHover);
            // 
            // lblComp
            // 
            this.lblComp.AutoSize = true;
            this.lblComp.Location = new System.Drawing.Point(317, 223);
            this.lblComp.Name = "lblComp";
            this.lblComp.Size = new System.Drawing.Size(99, 26);
            this.lblComp.TabIndex = 35;
            this.lblComp.Text = "Compression Level:\r\nN/A";
            this.lblComp.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.lblComp.MouseHover += new System.EventHandler(this.lblComp_MouseHover);
            // 
            // lblRamBackup
            // 
            this.lblRamBackup.AutoSize = true;
            this.lblRamBackup.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRamBackup.Location = new System.Drawing.Point(317, 274);
            this.lblRamBackup.Name = "lblRamBackup";
            this.lblRamBackup.Size = new System.Drawing.Size(176, 16);
            this.lblRamBackup.TabIndex = 39;
            this.lblRamBackup.Text = "Max Ram Usage: 700MB";
            // 
            // lblThread
            // 
            this.lblThread.AutoSize = true;
            this.lblThread.Location = new System.Drawing.Point(318, 91);
            this.lblThread.Name = "lblThread";
            this.lblThread.Size = new System.Drawing.Size(110, 26);
            this.lblThread.TabIndex = 37;
            this.lblThread.Text = "Number Of Instances:\r\nN/A";
            this.lblThread.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.lblThread.MouseHover += new System.EventHandler(this.lblThread_MouseHover);
            // 
            // tbarThread
            // 
            this.tbarThread.LargeChange = 1;
            this.tbarThread.Location = new System.Drawing.Point(309, 58);
            this.tbarThread.Maximum = 4;
            this.tbarThread.Minimum = 1;
            this.tbarThread.Name = "tbarThread";
            this.tbarThread.Size = new System.Drawing.Size(120, 45);
            this.tbarThread.TabIndex = 36;
            this.tbarThread.Value = 1;
            this.tbarThread.Scroll += new System.EventHandler(this.tbarThread_Scroll);
            this.tbarThread.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.tbarThread.MouseHover += new System.EventHandler(this.lblThread_MouseHover);
            // 
            // cBoxDelBup
            // 
            this.cBoxDelBup.AutoSize = true;
            this.cBoxDelBup.Location = new System.Drawing.Point(312, 366);
            this.cBoxDelBup.Name = "cBoxDelBup";
            this.cBoxDelBup.Size = new System.Drawing.Size(206, 17);
            this.cBoxDelBup.TabIndex = 41;
            this.cBoxDelBup.Text = "Delete all backup files before starting?";
            this.cBoxDelBup.UseVisualStyleBackColor = true;
            this.cBoxDelBup.CheckedChanged += new System.EventHandler(this.cBoxDelBup_CheckedChanged);
            this.cBoxDelBup.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.cBoxDelBup.MouseHover += new System.EventHandler(this.cBoxDelBup_MouseHover);
            // 
            // btnCancelBup
            // 
            this.btnCancelBup.Location = new System.Drawing.Point(303, 507);
            this.btnCancelBup.Name = "btnCancelBup";
            this.btnCancelBup.Size = new System.Drawing.Size(125, 55);
            this.btnCancelBup.TabIndex = 4;
            this.btnCancelBup.Text = "Cancel Backup";
            this.btnCancelBup.UseVisualStyleBackColor = true;
            this.btnCancelBup.Click += new System.EventHandler(this.btnCancelBup_Click);
            this.btnCancelBup.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.btnCancelBup.MouseHover += new System.EventHandler(this.btnCancelBup_MouseHover);
            // 
            // btnStartBup
            // 
            this.btnStartBup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartBup.Location = new System.Drawing.Point(434, 507);
            this.btnStartBup.Name = "btnStartBup";
            this.btnStartBup.Size = new System.Drawing.Size(125, 55);
            this.btnStartBup.TabIndex = 3;
            this.btnStartBup.Text = "Start Backup";
            this.btnStartBup.UseVisualStyleBackColor = true;
            this.btnStartBup.Click += new System.EventHandler(this.btnStartBup_Click);
            this.btnStartBup.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.btnStartBup.MouseHover += new System.EventHandler(this.btnStartBup_MouseHover);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.MediumBlue;
            this.label1.Location = new System.Drawing.Point(12, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 16);
            this.label1.TabIndex = 42;
            this.label1.Text = "Step 1:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(214, 13);
            this.label2.TabIndex = 43;
            this.label2.Text = "Choose what sort of backup you wish to do.";
            // 
            // infoBox
            // 
            this.infoBox.BackColor = System.Drawing.Color.White;
            this.infoBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.infoBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.infoBox.Location = new System.Drawing.Point(12, 579);
            this.infoBox.Name = "infoBox";
            this.infoBox.ReadOnly = true;
            this.infoBox.Size = new System.Drawing.Size(566, 55);
            this.infoBox.TabIndex = 44;
            this.infoBox.Text = "Hover your mouse over the controls to get further information.";
            // 
            // tbarThreadLbl
            // 
            this.tbarThreadLbl.AutoSize = true;
            this.tbarThreadLbl.Location = new System.Drawing.Point(306, 22);
            this.tbarThreadLbl.Name = "tbarThreadLbl";
            this.tbarThreadLbl.Size = new System.Drawing.Size(269, 26);
            this.tbarThreadLbl.TabIndex = 46;
            this.tbarThreadLbl.Text = "Choose the number of instances to run.\r\nRecommended: One instance for every two C" +
                "PU cores.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.MediumBlue;
            this.label5.Location = new System.Drawing.Point(306, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 16);
            this.label5.TabIndex = 45;
            this.label5.Text = "Step 2:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(306, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(275, 26);
            this.label4.TabIndex = 48;
            this.label4.Text = "Choose the level of compression  to use.\r\nNote: Use a lower compression level for" +
                " older computers.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.MediumBlue;
            this.label6.Location = new System.Drawing.Point(306, 138);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 16);
            this.label6.TabIndex = 47;
            this.label6.Text = "Step 3:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(306, 326);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(173, 26);
            this.label7.TabIndex = 50;
            this.label7.Text = "Delete files in the backup directory.\r\nNote: This cannot be undone.";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.MediumBlue;
            this.label8.Location = new System.Drawing.Point(306, 310);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 16);
            this.label8.TabIndex = 49;
            this.label8.Text = "Step 4:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(306, 417);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(187, 13);
            this.label9.TabIndex = 52;
            this.label9.Text = "Start or cancel the backup procedure!";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.MediumBlue;
            this.label10.Location = new System.Drawing.Point(306, 401);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(56, 16);
            this.label10.TabIndex = 51;
            this.label10.Text = "Step 5:";
            // 
            // btnBupNone
            // 
            this.btnBupNone.AccessibleDescription = "";
            this.btnBupNone.Location = new System.Drawing.Point(12, 69);
            this.btnBupNone.Name = "btnBupNone";
            this.btnBupNone.Size = new System.Drawing.Size(120, 25);
            this.btnBupNone.TabIndex = 1;
            this.btnBupNone.Text = "Backup None";
            this.btnBupNone.UseVisualStyleBackColor = true;
            this.btnBupNone.Click += new System.EventHandler(this.btnBupNone_Click);
            this.btnBupNone.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.btnBupNone.MouseHover += new System.EventHandler(this.btnBupNone_MouseHover);
            // 
            // btnUpdLib
            // 
            this.btnUpdLib.Location = new System.Drawing.Point(152, 69);
            this.btnUpdLib.Name = "btnUpdLib";
            this.btnUpdLib.Size = new System.Drawing.Size(120, 25);
            this.btnUpdLib.TabIndex = 53;
            this.btnUpdLib.Text = "Update Library";
            this.btnUpdLib.UseVisualStyleBackColor = true;
            this.btnUpdLib.Click += new System.EventHandler(this.btnUpdLib_Click);
            this.btnUpdLib.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.btnUpdLib.MouseHover += new System.EventHandler(this.btnUpdLib_MouseHover);
            // 
            // cBoxLzma2
            // 
            this.cBoxLzma2.AutoSize = true;
            this.cBoxLzma2.Location = new System.Drawing.Point(438, 190);
            this.cBoxLzma2.Name = "cBoxLzma2";
            this.cBoxLzma2.Size = new System.Drawing.Size(123, 17);
            this.cBoxLzma2.TabIndex = 54;
            this.cBoxLzma2.Text = "LZMA2 compression";
            this.cBoxLzma2.UseVisualStyleBackColor = true;
            this.cBoxLzma2.CheckStateChanged += new System.EventHandler(this.cBoxLzma2_CheckStateChanged);
            this.cBoxLzma2.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.cBoxLzma2.MouseHover += new System.EventHandler(this.cBoxLzma2_MouseHover);
            // 
            // BackupUserCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 646);
            this.Controls.Add(this.cBoxLzma2);
            this.Controls.Add(this.btnUpdLib);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbarThreadLbl);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.infoBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cBoxDelBup);
            this.Controls.Add(this.lblRamBackup);
            this.Controls.Add(this.lblThread);
            this.Controls.Add(this.tbarThread);
            this.Controls.Add(this.lblComp);
            this.Controls.Add(this.btnCancelBup);
            this.Controls.Add(this.btnStartBup);
            this.Controls.Add(this.btnUpdBup);
            this.Controls.Add(this.btnBupNone);
            this.Controls.Add(this.btnBupAll);
            this.Controls.Add(this.chkList);
            this.Controls.Add(this.tbarComp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "BackupUserCtrl";
            this.Text = "Steam Backup Wizard";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BackupUserCtrl_FormClosing);
            this.Load += new System.EventHandler(this.BackupUserCtrl_Load);
            this.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            ((System.ComponentModel.ISupportInitialize)(this.tbarComp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbarThread)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBupAll;
        private System.Windows.Forms.Button btnUpdBup;
        private System.Windows.Forms.TrackBar tbarComp;
        private System.Windows.Forms.Label lblComp;
        private System.Windows.Forms.Label lblRamBackup;
        private System.Windows.Forms.Label lblThread;
        private System.Windows.Forms.TrackBar tbarThread;
        private System.Windows.Forms.CheckBox cBoxDelBup;
        private System.Windows.Forms.Button btnCancelBup;
        private System.Windows.Forms.Button btnStartBup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox infoBox;
        private System.Windows.Forms.Label tbarThreadLbl;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnBupNone;
        private System.Windows.Forms.CheckedListBox chkList;
        private System.Windows.Forms.Button btnUpdLib;
        private System.Windows.Forms.CheckBox cBoxLzma2;

    }
}