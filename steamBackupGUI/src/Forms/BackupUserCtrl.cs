namespace steamBackup.Forms
{
    using steamBackup.AppServices;
    using steamBackup.Properties;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class BackupUserCtrl : Form
    {
        public BackupUserCtrl()
        {
            InitializeComponent();
        }

        public BackupTask m_task = new BackupTask();
        
        public bool m_canceled = true;

        private static readonly string[] m_compressionStrings = { "Copy", "Fastest", "Fast", "Normal", "Maximum", "Ultra", "N/A" };

        private void BackupUserCtrl_Load(object sender, EventArgs e)
        {
            m_task.m_steamDir = Settings.SteamDir;
            m_task.m_backupDir = Settings.BackupDir;

            m_task.m_jobList.Clear();

            Cursor = Cursors.WaitCursor;
            EnableControl(false);

            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (o, args) => m_task.Scan(worker);
            worker.ProgressChanged += (o, args) =>
            {
                workingProgBar.Value = args.ProgressPercentage;
                workingLbl.Text = "Working: " + m_task.m_jobList.Count + "/" + m_task.m_jobList.Capacity + " Apps Analysed";
            };
            worker.RunWorkerCompleted += (o, args) =>
            {
                m_task.m_jobList.Sort();
                chkList.DataSource = m_task.m_jobList;
                chkList.DisplayMember = "m_name";

                UpdCheckBoxList();

                cBoxUnlockThreads.Checked = Settings.Lzma2UnlockThreads;
                if (Settings.UseLzma2)
                {
                    tbarThread.Maximum = cBoxUnlockThreads.Checked ? 8 : Math.Min(8, Environment.ProcessorCount);

                    tbarThread.Value = Settings.Lzma2Threads;
                    m_task.m_threadCount = Settings.Lzma2Threads;
                    cBoxUnlockThreads.Visible = true;
                }
                else
                {
                    tbarThread.Maximum = 4;
                    tbarThread.Value = Settings.ThreadsBup;
                    m_task.m_threadCount = Settings.ThreadsBup;
                    cBoxUnlockThreads.Visible = false;
                }

                cBoxLzma2.Checked = Settings.UseLzma2;

                ThreadText();

                tbarComp.Value = Settings.Compression;
                m_task.m_compLevel = Settings.Compression;
                CompresionText();

                RamUsage();

                Cursor = Cursors.Arrow;
                EnableControl(true);
            };
            worker.RunWorkerAsync();

            
        }

        private void BackupUserCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Compression = m_task.GetCompLevel();
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
            foreach (var item in m_task.m_jobList)
            {
                var index = chkList.Items.IndexOf(item);
                var enabled = item.m_status == JobStatus.Waiting;
                chkList.SetItemChecked(index, enabled);
            }
            // reenable ItemCheck event
            chkList.ItemCheck += chkList_ItemCheck;

            chkList.EndUpdate();
        }

        private void btnBupAll_Click(object sender, EventArgs e)
        {
            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            m_task.SetEnableAll();
            UpdCheckBoxList();
        }

        private void btnBupNone_Click(object sender, EventArgs e)
        {
            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            m_task.SetEnableNone();
            UpdCheckBoxList();
        }

        private void btnUpdBup_Click(object sender, EventArgs e)
        {
            cBoxDelBup.Enabled = false;
            cBoxDelBup.Checked = false;

            m_task.SetEnableUpd(true);
            UpdCheckBoxList();
        }

        private void btnUpdLib_Click(object sender, EventArgs e)
        {
            cBoxDelBup.Enabled = false;
            cBoxDelBup.Checked = false;

            m_task.SetEnableUpd(false);
            UpdCheckBoxList();
        }

        private void EnableControl(bool enabled)
        {
            btnBupAll.Enabled = enabled;
            btnBupNone.Enabled = enabled;
            btnUpdBup.Enabled = enabled;
            btnUpdLib.Enabled = enabled;
            chkList.Enabled = enabled;
            btnStartBup.Enabled = enabled;

            cBoxUnlockThreads.Enabled = enabled;

            workingLbl.Visible = !enabled;
            workingProgBar.Visible = !enabled;
        }

        private void btnStartBup_Click(object sender, EventArgs e)
        {
            foreach(Job job in m_task.m_jobList)
            {
                if (job.m_status == JobStatus.Waiting && job.m_steamFileSize < job.m_backupFileSize / 2)
                {
                    if(MessageBox.Show(string.Format(Resources.PotentiallyDeletedInstall, job.m_name), "Warning", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        m_task.DisableJob(job);
                    }
                }
            }
            
            if (Utilities.IsSteamRunning())
            {
                MessageBox.Show(Resources.BackupSteamRunningText, Resources.SteamRunningTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                m_canceled = false;

                m_task.Start();

                Close();
            }
        }

        private void btnCancelBup_Click(object sender, EventArgs e)
        {
            m_canceled = true;
            
            Close();
        }

        private void tbarThread_Scroll(object sender, EventArgs e)
        {
            m_task.m_threadCount = tbarThread.Value;

            if (cBoxLzma2.Checked)
                m_task.m_lzma2Threads = tbarThread.Value;

            ThreadText();

            int ram = RamUsage();

            if (!Environment.Is64BitProcess && ram > 2750)
            {
                tbarThread.Value--;
                tbarThread_Scroll(sender, e); // just incase it was moved two or more spots with only one call
            }
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
            m_task.m_compLevel = tbarComp.Value;
            
            CompresionText();

            int ram = RamUsage();

            if(!Environment.Is64BitProcess && ram > 2750)
            {
                tbarComp.Value--;
                tbarComp_Scroll(sender, e); // just incase it was moved two or more spots with only one call
            }
        }

        private void CompresionText()
        {
            var compLevel = m_task.GetCompLevel();

            if (compLevel <= 5 && compLevel >= 0)
                lblComp.Text = Resources.CompressionLevelText + m_compressionStrings[compLevel];
            else
                lblComp.Text = Resources.CompressionLevelText + m_compressionStrings[6];
        }

        private int RamUsage()
        {
            int ram = m_task.RamUsage(cBoxLzma2.Checked);

            lblRamBackup.Text = string.Format(Resources.MaxRamUsageText, ram);

            if (ram >= 1500)
                lblRamBackup.ForeColor = Color.Red;
            else if (ram >= 750)
                lblRamBackup.ForeColor = Color.Orange;
            else
                lblRamBackup.ForeColor = Color.Black;

            return ram;
        }

        private void chkList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var job = (Job)chkList.Items[e.Index];
            if (job == null) return;

            if (e.NewValue == CheckState.Checked)
            {
                m_task.EnableJob(job);
            }
            else
            {
                m_task.DisableJob(job);
            }
        }

        private void cBoxDelBup_CheckedChanged(object sender, EventArgs e)
        {
            cBoxDelBup.ForeColor = cBoxDelBup.Checked ? Color.Red : Color.Black;

            m_task.m_deleteAll = cBoxDelBup.Checked;
        }

        public Task GetTask()
        {
            return m_task;
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
            infoBox.Rtf = cBoxLzma2.Checked ? Resources.ThreadsLzma2Tooltip : Resources.ThreadsLzmaTooltip;
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
                m_task.m_useLzma2 = true;

                tbarThread.Maximum = cBoxUnlockThreads.Checked ? 8 : Math.Min(8, Environment.ProcessorCount);

                var numThreads = Math.Min(tbarThread.Maximum, tbarThread.Value);
                tbarThread.Value = numThreads;
                m_task.m_threadCount = numThreads;

                m_task.m_lzma2Threads = numThreads;

                tbarThreadLbl.Text = Resources.ThreadsCountTooltip;

                cBoxUnlockThreads.Visible = true;
            }
            else
            {
                m_task.m_useLzma2 = false;

                tbarThread.Maximum = 4;

                var numThreads = Math.Min(tbarThread.Maximum, tbarThread.Value);
                tbarThread.Value = numThreads;
                m_task.m_threadCount = numThreads;

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

                var numThreads = Math.Min(tbarThread.Maximum, m_task.m_threadCount);
                tbarThread.Value = numThreads;
                m_task.m_threadCount = numThreads;

                m_task.m_lzma2Threads = numThreads;

                ThreadText();
                RamUsage();
            }
        }
    }
}
