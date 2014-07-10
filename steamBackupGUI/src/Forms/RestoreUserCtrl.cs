namespace steamBackup.Forms
{
    using System.IO;
    using steamBackup.AppServices;
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Tasks;
    using steamBackup.AppServices.Tasks.Restore;
    using steamBackup.Properties;
    using System;
    using System.Windows.Forms;

    public partial class RestoreUserCtrl : Form
    {
        public RestoreUserCtrl()
        {
            InitializeComponent();
        }

        public RestoreTask m_task = new RestoreTask();

        public bool m_canceled = true;

        private void RestoreUserCtrl_Load(object sender, EventArgs e)
        {
            tbarThread.Value = Settings.ThreadsRest;
            m_task.m_threadCount = Settings.ThreadsRest;
            ThreadText();

            btnRestAll.Enabled = false;

            m_task.m_steamDir = Settings.SteamDir;
            m_task.m_backupDir = Settings.BackupDir;

            m_task.Scan();

            // use databinding instead of direct access to the control
            chkList.DataSource = m_task.JobList;
            chkList.DisplayMember = "name";

            UpdCheckBoxList();
            UpdLibDropBox();
        }

        private void RestoreUserCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.ThreadsRest = m_task.m_threadCount;
            Settings.Save();
        }

        private void UpdCheckBoxList()
        {
            chkList.BeginUpdate();

            // disable ItemCheck event temporarily
            chkList.ItemCheck -= chkList_ItemCheck;
            foreach (var job in m_task.JobList)
            {
                var index = chkList.Items.IndexOf(job);
                var enabled = job.m_status == JobStatus.Waiting;
                chkList.SetItemChecked(index, enabled);
            }
            // reenable ItemCheck event
            chkList.ItemCheck += chkList_ItemCheck;

            chkList.ClearSelected();

            chkList.EndUpdate();
        }

        private void UpdLibDropBox()
        {
            dboxLibList.Items.Clear();

            var libraries = Utilities.GetLibraries(Settings.SteamDir);
            foreach (var library in libraries)
            {
                dboxLibList.Items.Add(library);
            }
        }

        private void lblRefreshList_Click(object sender, EventArgs e)
        {
            UpdLibDropBox();
            DboxLibListUpd();
        }

        private void chkList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DboxLibListUpd();
        }

        private void DboxLibListUpd()
        {
            if (chkList.SelectedItem == null)
            {
                dboxLibList.Enabled = false;
                return;
            }

            var job = (Job) chkList.SelectedItem;
            if (string.IsNullOrEmpty(job.m_acfFiles))
            {
                dboxLibList.Enabled = false;
                dboxLibList.SelectedItem = Utilities.UpDirLvl(job.m_steamDir);
            }
            else
            {
                dboxLibList.Enabled = true;
                dboxLibList.SelectedItem = Utilities.UpDirLvl(job.m_steamDir);
            }
        }

        private void dboxLibList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (chkList.SelectedItem == null) return;

            var job = (Job) chkList.SelectedItem;

            var selectedPath = dboxLibList.SelectedItem as string;
            if (selectedPath == null) return;

            job.m_steamDir = Path.Combine(selectedPath, SteamDirectory.Common);
            job.m_acfDir = dboxLibList.SelectedItem.ToString();
        }

        private void btnRestAll_Click(object sender, EventArgs e)
        {
            btnRestAll.Enabled = false;
            btnRestNone.Enabled = true;

            m_task.SetEnableAll();
            UpdCheckBoxList();
        }

        private void btnRestNone_Click(object sender, EventArgs e)
        {
            btnRestNone.Enabled = false;
            btnRestAll.Enabled = true;

            m_task.SetEnableNone();
            UpdCheckBoxList();
        }

        private void btnStartRest_Click(object sender, EventArgs e)
        {
            if (Utilities.IsSteamRunning())
            {
                MessageBox.Show(Resources.RestoreSteamRunningText, Resources.SteamRunningTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                m_canceled = false;

                m_task.Setup();

                Close();
            }
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

        private void btnCancelRest_Click(object sender, EventArgs e)
        {
            m_canceled = true;
            
            Close();
        }

        private void tbarThread_Scroll(object sender, EventArgs e)
        {
            ThreadText();
        }

        private void ThreadText() 
        {
            lblThread.Text = Resources.ThreadLblInstancesText + tbarThread.Value;
        }

        public Task GetTask()
        {
            return m_task;
        }

        private void controls_MouseLeave(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.ControlsDefaultTooltip;
        }

        private void btnStartRest_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.RestoreStartButtonTooltip;
        }

        private void btnCancelRest_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.RestoreCancelButtonTooltip;
        }

        private void lblThread_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.RestoreThreadsTooltip;
        }

        private void chkList_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.RestoreCheckListTooltip;
        }

        private void btnRestAll_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.RestoreAllButtonTooltip;
        }

        private void btnRestNone_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.RestoreNoneButtonTooltip;
        }

        private void lblRefreshList_MouseHover(object sender, EventArgs e)
        {
            infoBox.Rtf = Resources.RestoreRefreshListLabelTooltip;
        }
        
    }
}
