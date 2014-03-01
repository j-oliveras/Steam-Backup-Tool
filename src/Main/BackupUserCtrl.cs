using System;
using System.Drawing;
using System.Windows.Forms;
using steamBackup.Properties;

namespace steamBackup
{
    public partial class BackupUserCtrl : Form
    {
        public BackupUserCtrl()
        {
            InitializeComponent();
        }

        public BackupTask backupTask = new BackupTask();
        
        public bool canceled = true;

        private static readonly string[] compressionStrings = { "Copy", "Fastest", "Fast", "Normal", "Maximum", "Ultra", "N/A" };

        private void BackupUserCtrl_Load(object sender, EventArgs e)
        {

            backupTask.steamDir = Settings.steamDir;
            backupTask.backupDir = Settings.backupDir;

            backupTask.list.Clear();
            backupTask.scan();

            // use databinding instead of direct access to the control
            chkList.DataSource = backupTask.list;
            chkList.DisplayMember = "name";

            updCheckBoxList();

            if (Settings.useLzma2)
            {
                tbarThread.Maximum = Environment.ProcessorCount;
                tbarThread.Value = Settings.lzma2Threads;
                backupTask.threadCount = Settings.lzma2Threads;
            }
            else
            {
                tbarThread.Maximum = 4;
                tbarThread.Value = Settings.threadsBup;
                backupTask.threadCount = Settings.threadsBup;
            }

            cBoxLzma2.Checked = Settings.useLzma2;

            threadText();

            tbarComp.Value = Settings.compresion;
            backupTask.setCompLevel(Settings.compresion);
            compresionText();

            ramUsage();
        }

        private void BackupUserCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.compresion = backupTask.getCompLevel();
            Settings.useLzma2 = cBoxLzma2.Checked;

            if (Settings.useLzma2)
                Settings.lzma2Threads = tbarThread.Value;
            else
                Settings.threadsBup = tbarThread.Value;
            Settings.save();
        }

        private void updCheckBoxList()
        {
            chkList.BeginUpdate();

            // disable ItemCheck event temporarily
            chkList.ItemCheck -= chkList_ItemCheck;
            foreach (Job item in backupTask.list)
            {
                int index = chkList.Items.IndexOf(item);
                bool enabled = item.status == JobStatus.WAITING;
                chkList.SetItemChecked(index, enabled);
            }
            // reenable ItemCheck event
            chkList.ItemCheck += chkList_ItemCheck;

            chkList.EndUpdate();
        }

        private void btnBupAll_Click(object sender, EventArgs e)
        {
            disableButtons(false);

            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            backupTask.setEnableAll();
            updCheckBoxList();

            disableButtons(true);
        }

        private void btnBupNone_Click(object sender, EventArgs e)
        {
            disableButtons(false);

            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            backupTask.setEnableNone();
            updCheckBoxList();

            disableButtons(true);
        }

        private void btnUpdBup_Click(object sender, EventArgs e)
        {
            disableButtons(false);
  
            cBoxDelBup.Enabled = false;
            cBoxDelBup.Checked = false;

            Cursor = Cursors.WaitCursor;
            backupTask.setEnableUpd(true);
            updCheckBoxList();
            Cursor = Cursors.Arrow;

            disableButtons(true);
        }

        private void btnUpdLib_Click(object sender, EventArgs e)
        {
            disableButtons(false);

            cBoxDelBup.Enabled = false;
            cBoxDelBup.Checked = false;

            Cursor = Cursors.WaitCursor;
            backupTask.setEnableUpd(false);
            updCheckBoxList();
            Cursor = Cursors.Arrow;

            disableButtons(true);
        }

        private void disableButtons(bool disableBool)
        {
            btnBupAll.Enabled = disableBool;
            btnBupNone.Enabled = disableBool;
            btnUpdBup.Enabled = disableBool;
            btnUpdLib.Enabled = disableBool;
            chkList.Enabled = disableBool;
        }

        private void btnStartBup_Click(object sender, EventArgs e)
        {
            if (Utilities.isSteamRunning())
            {
                MessageBox.Show(Resources.BackupSteamRunningText, Resources.SteamRunningTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                canceled = false;

                backupTask.setup();

                Close();
            }
        }

        private void btnCancelBup_Click(object sender, EventArgs e)
        {
            canceled = true;
            
            Close();
        }

        private void tbarThread_Scroll(object sender, EventArgs e)
        {
            backupTask.threadCount = tbarThread.Value;

            threadText();

            ramUsage();
        }

        private void threadText()
        {
            if (cBoxLzma2.Checked)
                lblThread.Text = Resources.ThreadLblThreadsText + tbarThread.Value;
            else
                lblThread.Text = Resources.ThreadLblInstancesText + tbarThread.Value;
        }

        private void tbarComp_Scroll(object sender, EventArgs e)
        {
            backupTask.setCompLevel(tbarComp.Value);
            
            compresionText();

            ramUsage();
        }

        private void compresionText()
        {
            int compLevel = backupTask.getCompLevel();

            if (compLevel <= 5 && compLevel >= 0)
                lblComp.Text = Resources.CompressionLevelText + compressionStrings[compLevel];
            else
                lblComp.Text = Resources.CompressionLevelText + compressionStrings[6];
        }

        private void ramUsage()
        {
            int ram = backupTask.ramUsage(cBoxLzma2.Checked);

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
            Job job = (Job)chkList.Items[e.Index];
            if (job == null) return;

            if (e.NewValue == CheckState.Checked)
            {
                backupTask.enableJob(job);
            }
            else
            {
                backupTask.disableJob(job);
            }
        }

        private void cBoxDelBup_CheckedChanged(object sender, EventArgs e)
        {
            if (cBoxDelBup.Checked)
                cBoxDelBup.ForeColor = Color.Red;
            else
                cBoxDelBup.ForeColor = Color.Black;

            backupTask.deleteAll = cBoxDelBup.Checked;
        }

        public Task getTask()
        {
            return backupTask;
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

        #endregion
        
        private void cBoxLzma2_CheckStateChanged(object sender, EventArgs e)
        {
            if (cBoxLzma2.Checked)
            {
                backupTask.setCompMethod(true);

                tbarThread.Maximum = Environment.ProcessorCount;
                tbarThread.Value = Settings.lzma2Threads;
                backupTask.threadCount = Settings.lzma2Threads;
                threadText();

                tbarThreadLbl.Text = Resources.ThreadsCountTooltip;
            }
            else
            {
                backupTask.setCompMethod(false);

                tbarThread.Maximum = 4;
                tbarThread.Value = Settings.threadsBup;
                backupTask.threadCount = Settings.threadsBup;
                threadText();

                tbarThreadLbl.Text = Resources.InstancesCountTooltip;
            }

            ramUsage();
        }
    }
}
