namespace steamBackup.Forms
{
    using steamBackup.AppServices;
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Tasks;
    using steamBackup.AppServices.Tasks.Backup;
    using steamBackup.Properties;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class BackupUserCtrl : Form
    {
        public BackupUserCtrl()
        {
            InitializeComponent();
        }

        public BackupTask BupTask = new BackupTask();
        
        public bool Canceled = true;

        private static readonly string[] CompressionStrings = { "Copy", "Fastest", "Fast", "Normal", "Maximum", "Ultra", "N/A" };

        private void BackupUserCtrl_Load(object sender, EventArgs e)
        {

            BupTask.SteamDir = Settings.SteamDir;
            BupTask.BackupDir = Settings.BackupDir;

            BupTask.JobList.Clear();
            BupTask.Scan();

            // use databinding instead of direct access to the control
            chkList.DataSource = BupTask.JobList;
            chkList.DisplayMember = "name";

            UpdCheckBoxList();

            cBoxUnlockThreads.Checked = Settings.Lzma2UnlockThreads;
            
            if (Settings.UseLzma2)
            {
                if (cBoxUnlockThreads.Checked)
                    tbarThread.Maximum = 8;
                else
                    tbarThread.Maximum = Math.Min(8, Environment.ProcessorCount);
                tbarThread.Value = Settings.Lzma2Threads;
                BupTask.ThreadCount = Settings.Lzma2Threads;
                cBoxUnlockThreads.Visible = true;
            }
            else
            {
                tbarThread.Maximum = 4;
                tbarThread.Value = Settings.ThreadsBup;
                BupTask.ThreadCount = Settings.ThreadsBup;
                cBoxUnlockThreads.Visible = false;
            }

            cBoxLzma2.Checked = Settings.UseLzma2;

            ThreadText();

            tbarComp.Value = Settings.Compression;
            BupTask.SetCompLevel(Settings.Compression);
            CompresionText();

            RamUsage();
        }

        private void BackupUserCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Compression = BupTask.GetCompLevel();
            Settings.UseLzma2 = cBoxLzma2.Checked;

            Settings.Lzma2UnlockThreads = cBoxUnlockThreads.Checked;

            if (Settings.UseLzma2)
                Settings.Lzma2Threads = tbarThread.Value;
            else
                Settings.ThreadsBup = tbarThread.Value;
            Settings.Save();
        }

        private void UpdCheckBoxList()
        {
            chkList.BeginUpdate();

            // disable ItemCheck event temporarily
            chkList.ItemCheck -= chkList_ItemCheck;
            foreach (var item in BupTask.JobList)
            {
                var index = chkList.Items.IndexOf(item);
                var enabled = item.Status == JobStatus.Waiting;
                chkList.SetItemChecked(index, enabled);
            }
            // reenable ItemCheck event
            chkList.ItemCheck += chkList_ItemCheck;

            chkList.EndUpdate();
        }

        private void btnBupAll_Click(object sender, EventArgs e)
        {
            DisableButtons(false);

            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            BupTask.SetEnableAll();
            UpdCheckBoxList();

            DisableButtons(true);
        }

        private void btnBupNone_Click(object sender, EventArgs e)
        {
            DisableButtons(false);

            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            BupTask.SetEnableNone();
            UpdCheckBoxList();

            DisableButtons(true);
        }

        private void btnUpdBup_Click(object sender, EventArgs e)
        {
            DisableButtons(false);
  
            cBoxDelBup.Enabled = false;
            cBoxDelBup.Checked = false;

            Cursor = Cursors.WaitCursor;
            BupTask.SetEnableUpd(true);
            UpdCheckBoxList();
            Cursor = Cursors.Arrow;

            DisableButtons(true);
        }

        private void btnUpdLib_Click(object sender, EventArgs e)
        {
            DisableButtons(false);

            cBoxDelBup.Enabled = false;
            cBoxDelBup.Checked = false;

            Cursor = Cursors.WaitCursor;
            BupTask.SetEnableUpd(false);
            UpdCheckBoxList();
            Cursor = Cursors.Arrow;

            DisableButtons(true);
        }

        private void DisableButtons(bool disableBool)
        {
            btnBupAll.Enabled = disableBool;
            btnBupNone.Enabled = disableBool;
            btnUpdBup.Enabled = disableBool;
            btnUpdLib.Enabled = disableBool;
            chkList.Enabled = disableBool;
        }

        private void btnStartBup_Click(object sender, EventArgs e)
        {
            if (Utilities.IsSteamRunning())
            {
                MessageBox.Show(Resources.BackupSteamRunningText, Resources.SteamRunningTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                Canceled = false;

                BupTask.Setup();

                Close();
            }
        }

        private void btnCancelBup_Click(object sender, EventArgs e)
        {
            Canceled = true;
            
            Close();
        }

        private void tbarThread_Scroll(object sender, EventArgs e)
        {
            BupTask.ThreadCount = tbarThread.Value;

            ThreadText();

            RamUsage();
        }

        private void ThreadText()
        {
            if (cBoxLzma2.Checked)
                lblThread.Text = Resources.ThreadLblThreadsText + tbarThread.Value;
            else
                lblThread.Text = Resources.ThreadLblInstancesText + tbarThread.Value;
        }

        private void tbarComp_Scroll(object sender, EventArgs e)
        {
            BupTask.SetCompLevel(tbarComp.Value);
            
            CompresionText();

            RamUsage();
        }

        private void CompresionText()
        {
            var compLevel = BupTask.GetCompLevel();

            if (compLevel <= 5 && compLevel >= 0)
                lblComp.Text = Resources.CompressionLevelText + CompressionStrings[compLevel];
            else
                lblComp.Text = Resources.CompressionLevelText + CompressionStrings[6];
        }

        private void RamUsage()
        {
            var ram = BupTask.RamUsage(cBoxLzma2.Checked);

            lblRamBackup.Text = string.Format(Resources.MaxRamUsageText, ram);

            if (ram >= 1500)
                lblRamBackup.ForeColor = Color.Red;
            else if (ram >= 750)
                lblRamBackup.ForeColor = Color.Orange;
            else
                lblRamBackup.ForeColor = Color.Black;
        }

        private void chkList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var job = (Job)chkList.Items[e.Index];
            if (job == null) return;

            if (e.NewValue == CheckState.Checked)
            {
                BupTask.EnableJob(job);
            }
            else
            {
                BupTask.DisableJob(job);
            }
        }

        private void cBoxDelBup_CheckedChanged(object sender, EventArgs e)
        {
            if (cBoxDelBup.Checked)
                cBoxDelBup.ForeColor = Color.Red;
            else
                cBoxDelBup.ForeColor = Color.Black;

            BupTask.DeleteAll = cBoxDelBup.Checked;
        }

        public Task GetTask()
        {
            return BupTask;
        }

        #region Info text handling

        private void controls_MouseLeave(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.ControlsDefaultTooltip;
        }

        private void btnStartBup_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.BackupStartButtonTooltip;
        }

        private void btnCancelBup_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.BackupCancelButtonTooltip;
        }

        private void cBoxDelBup_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.DelBackupCheckBoxTooltip;
        }

        private void lblComp_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.CompressionTooltip;
        }

        private void lblThread_MouseHover(object sender, EventArgs e)
        {
            if (cBoxLzma2.Checked) 
                infoBox.Rtf = Resources.ThreadsLzma2Tooltip;
            else 
                infoBox.Rtf = Resources.ThreadsLzmaTooltip;
        }

        private void chkList_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.BackupCheckListTooltip;
        }

        private void btnBupAll_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.BackupAllButtonTooltip;
        }

        private void btnBupNone_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.BackupNoneButtonTooltip;
        }

        private void btnUpdBup_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.UpdateBackupButtonTooltip;
        }

        private void btnUpdLib_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.UpdateLibButtonTooltip;
        }

        private void cBoxLzma2_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.Lzma2CheckboxTooltip;
        }

        private void cBoxUnlockThreads_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.ThreadsLzma2UnlockTooltip;
        }

        #endregion
        
        private void cBoxLzma2_CheckStateChanged(object sender, EventArgs e)
        {
            if (cBoxLzma2.Checked)
            {
                BupTask.SetCompMethod(true);

                if (cBoxUnlockThreads.Checked)
                    tbarThread.Maximum = 8;
                else
                    tbarThread.Maximum = Math.Min(8, Environment.ProcessorCount);

                var numThreads = Math.Min(tbarThread.Maximum, tbarThread.Value);
                tbarThread.Value = numThreads;
                BupTask.ThreadCount = numThreads;

                tbarThreadLbl.Text = Resources.ThreadsCountTooltip;

                cBoxUnlockThreads.Visible = true;
            }
            else
            {
                BupTask.SetCompMethod(false);

                tbarThread.Maximum = 4;

                var numThreads = Math.Min(tbarThread.Maximum, tbarThread.Value);
                tbarThread.Value = numThreads;
                BupTask.ThreadCount = numThreads;

                tbarThreadLbl.Text = Resources.InstancesCountTooltip;

                cBoxUnlockThreads.Visible = false;
            }

            ThreadText();
            RamUsage();
        }

        private void cBoxUnlockThreads_CheckedChanged(object sender, EventArgs e)
        {
            if (cBoxUnlockThreads.Checked)
            {
                tbarThread.Maximum = 8;
            }
            else
            {
                tbarThread.Maximum = Math.Min(8, Environment.ProcessorCount);

                var numThreads = Math.Min(tbarThread.Maximum, BupTask.ThreadCount);
                tbarThread.Value = numThreads;
                BupTask.ThreadCount = numThreads;

                ThreadText();
                RamUsage();
            }
        }
    }
}
