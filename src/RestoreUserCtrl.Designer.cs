namespace steamBackup
{
    partial class RestoreUserCtrl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RestoreUserCtrl));
            this.chkList = new System.Windows.Forms.CheckedListBox();
            this.btnRestAll = new System.Windows.Forms.Button();
            this.lblThread = new System.Windows.Forms.Label();
            this.tbarThread = new System.Windows.Forms.TrackBar();
            this.btnCancelRest = new System.Windows.Forms.Button();
            this.btnStartRest = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.infoBox = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btnRestNone = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.dboxLibList = new System.Windows.Forms.ComboBox();
            this.lblRefreshList = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tbarThread)).BeginInit();
            this.SuspendLayout();
            // 
            // chkList
            // 
            this.chkList.FormattingEnabled = true;
            this.chkList.Location = new System.Drawing.Point(12, 108);
            this.chkList.Name = "chkList";
            this.chkList.Size = new System.Drawing.Size(260, 454);
            this.chkList.Sorted = true;
            this.chkList.TabIndex = 0;
            this.chkList.SelectedIndexChanged += new System.EventHandler(this.chkList_SelectedIndexChanged);
            this.chkList.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.chkList.MouseHover += new System.EventHandler(this.chkList_MouseHover);
            // 
            // btnRestAll
            // 
            this.btnRestAll.AccessibleDescription = "";
            this.btnRestAll.Location = new System.Drawing.Point(12, 59);
            this.btnRestAll.Name = "btnRestAll";
            this.btnRestAll.Size = new System.Drawing.Size(125, 35);
            this.btnRestAll.TabIndex = 1;
            this.btnRestAll.Text = "Restore All";
            this.btnRestAll.UseVisualStyleBackColor = true;
            this.btnRestAll.Click += new System.EventHandler(this.btnRestAll_Click);
            this.btnRestAll.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.btnRestAll.MouseHover += new System.EventHandler(this.btnRestAll_MouseHover);
            // 
            // lblThread
            // 
            this.lblThread.AutoSize = true;
            this.lblThread.Location = new System.Drawing.Point(318, 369);
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
            this.tbarThread.Location = new System.Drawing.Point(309, 336);
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
            // btnCancelRest
            // 
            this.btnCancelRest.Location = new System.Drawing.Point(303, 507);
            this.btnCancelRest.Name = "btnCancelRest";
            this.btnCancelRest.Size = new System.Drawing.Size(125, 55);
            this.btnCancelRest.TabIndex = 4;
            this.btnCancelRest.Text = "Cancel Restore";
            this.btnCancelRest.UseVisualStyleBackColor = true;
            this.btnCancelRest.Click += new System.EventHandler(this.btnCancelRest_Click);
            this.btnCancelRest.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.btnCancelRest.MouseHover += new System.EventHandler(this.btnCancelRest_MouseHover);
            // 
            // btnStartRest
            // 
            this.btnStartRest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartRest.Location = new System.Drawing.Point(434, 507);
            this.btnStartRest.Name = "btnStartRest";
            this.btnStartRest.Size = new System.Drawing.Size(125, 55);
            this.btnStartRest.TabIndex = 3;
            this.btnStartRest.Text = "Start Restore";
            this.btnStartRest.UseVisualStyleBackColor = true;
            this.btnStartRest.Click += new System.EventHandler(this.btnStartRest_Click);
            this.btnStartRest.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.btnStartRest.MouseHover += new System.EventHandler(this.btnStartRest_MouseHover);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.MediumBlue;
            this.label1.Location = new System.Drawing.Point(12, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 16);
            this.label1.TabIndex = 42;
            this.label1.Text = "Step 1:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(210, 13);
            this.label2.TabIndex = 43;
            this.label2.Text = "Choose what sort of restore you wish to do.";
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(306, 300);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(265, 26);
            this.label3.TabIndex = 46;
            this.label3.Text = "Choose the number of instances to run.\r\nRecommended: One instance for every one C" +
    "PU core.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.MediumBlue;
            this.label5.Location = new System.Drawing.Point(306, 284);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 16);
            this.label5.TabIndex = 45;
            this.label5.Text = "Step 3:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(306, 456);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(183, 13);
            this.label9.TabIndex = 52;
            this.label9.Text = "Start or cancel the restore procedure!";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.MediumBlue;
            this.label10.Location = new System.Drawing.Point(306, 440);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(56, 16);
            this.label10.TabIndex = 51;
            this.label10.Text = "Step 4:";
            // 
            // btnRestNone
            // 
            this.btnRestNone.Location = new System.Drawing.Point(147, 60);
            this.btnRestNone.Name = "btnRestNone";
            this.btnRestNone.Size = new System.Drawing.Size(125, 35);
            this.btnRestNone.TabIndex = 2;
            this.btnRestNone.Text = "Restore None";
            this.btnRestNone.UseVisualStyleBackColor = true;
            this.btnRestNone.Click += new System.EventHandler(this.btnRestNone_Click);
            this.btnRestNone.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.btnRestNone.MouseHover += new System.EventHandler(this.btnRestNone_MouseHover);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(306, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(282, 130);
            this.label4.TabIndex = 54;
            this.label4.Text = resources.GetString("label4.Text");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.MediumBlue;
            this.label6.Location = new System.Drawing.Point(306, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 16);
            this.label6.TabIndex = 53;
            this.label6.Text = "Step 2:";
            // 
            // dboxLibList
            // 
            this.dboxLibList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dboxLibList.Enabled = false;
            this.dboxLibList.FormattingEnabled = true;
            this.dboxLibList.Location = new System.Drawing.Point(309, 208);
            this.dboxLibList.Name = "dboxLibList";
            this.dboxLibList.Size = new System.Drawing.Size(250, 21);
            this.dboxLibList.TabIndex = 55;
            this.dboxLibList.SelectedValueChanged += new System.EventHandler(this.dboxLibList_SelectedValueChanged);
            this.dboxLibList.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.dboxLibList.MouseHover += new System.EventHandler(this.lblRefreshList_MouseHover);
            // 
            // lblRefreshList
            // 
            this.lblRefreshList.AutoSize = true;
            this.lblRefreshList.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblRefreshList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRefreshList.ForeColor = System.Drawing.Color.DarkRed;
            this.lblRefreshList.Location = new System.Drawing.Point(309, 179);
            this.lblRefreshList.Name = "lblRefreshList";
            this.lblRefreshList.Size = new System.Drawing.Size(84, 15);
            this.lblRefreshList.TabIndex = 56;
            this.lblRefreshList.Text = "Refresh List";
            this.lblRefreshList.Click += new System.EventHandler(this.lblRefreshList_Click);
            this.lblRefreshList.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            this.lblRefreshList.MouseHover += new System.EventHandler(this.lblRefreshList_MouseHover);
            // 
            // RestoreUserCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 646);
            this.Controls.Add(this.lblRefreshList);
            this.Controls.Add(this.dboxLibList);
            this.Controls.Add(this.lblThread);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.infoBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancelRest);
            this.Controls.Add(this.btnStartRest);
            this.Controls.Add(this.btnRestNone);
            this.Controls.Add(this.btnRestAll);
            this.Controls.Add(this.chkList);
            this.Controls.Add(this.tbarThread);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "RestoreUserCtrl";
            this.Text = "Steam Restore Wizard";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RestoreUserCtrl_FormClosing);
            this.Load += new System.EventHandler(this.RestoreUserCtrl_Load);
            this.MouseLeave += new System.EventHandler(this.controls_MouseLeave);
            ((System.ComponentModel.ISupportInitialize)(this.tbarThread)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox chkList;
        private System.Windows.Forms.Button btnRestAll;
        private System.Windows.Forms.Label lblThread;
        private System.Windows.Forms.TrackBar tbarThread;
        private System.Windows.Forms.Button btnCancelRest;
        private System.Windows.Forms.Button btnStartRest;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox infoBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnRestNone;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox dboxLibList;
        private System.Windows.Forms.Label lblRefreshList;

    }
}