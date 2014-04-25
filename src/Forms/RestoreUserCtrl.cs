namespace steamBackup.Forms
{
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

        public RestoreTask RstTask = new RestoreTask();

        public bool Canceled = true;

        private void RestoreUserCtrl_Load(object sender, EventArgs e)
        {
            tbarThread.Value = Settings.ThreadsRest;
            RstTask.ThreadCount = Settings.ThreadsRest;
            ThreadText();

            btnRestAll.Enabled = false;

            RstTask.SteamDir = Settings.SteamDir;
            RstTask.BackupDir = Settings.BackupDir;

            RstTask.Scan();

            // use databinding instead of direct access to the control
            chkList.DataSource = RstTask.JobList;
            chkList.DisplayMember = "name";

            UpdCheckBoxList();
            UpdLibDropBox();
        }

        private void RestoreUserCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.ThreadsRest = RstTask.ThreadCount;
            Settings.Save();
        }

        private void UpdCheckBoxList()
        {
            chkList.BeginUpdate();

            // disable ItemCheck event temporarily
            chkList.ItemCheck -= chkList_ItemCheck;
            foreach (Job job in RstTask.JobList)
            {
                int index = chkList.Items.IndexOf(job);
                bool enabled = job.Status == JobStatus.Waiting;
                chkList.SetItemChecked(index, enabled);
            }
            // reenable ItemCheck event
            chkList.ItemCheck += chkList_ItemCheck;

            chkList.EndUpdate();
        }

        private void UpdLibDropBox()
        {
            dboxLibList.Items.Clear();
            dboxLibList.Items.AddRange(Utilities.GetLibraries(Settings.SteamDir));
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
            if (chkList.SelectedItem == null) return;

            var job = (Job) chkList.SelectedItem;
            if (string.IsNullOrEmpty(job.AcfFiles) || job.Name.Equals(Settings.SourceEngineGames))
            {
                dboxLibList.Enabled = false;
                dboxLibList.SelectedItem = Utilities.UpDirLvl(job.GetSteamDir());
            }
            else
            {
                dboxLibList.Enabled = true;
                dboxLibList.SelectedItem = Utilities.UpDirLvl(job.GetSteamDir());
            }
        }

        private void dboxLibList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (chkList.SelectedItem == null) return;

            var job = (Job) chkList.SelectedItem;
            job.SetSteamDir(dboxLibList.SelectedItem + "common\\");
            job.AcfDir = dboxLibList.SelectedItem.ToString();
        }

        private void btnRestAll_Click(object sender, EventArgs e)
        {
            btnRestAll.Enabled = false;
            btnRestNone.Enabled = true;

            RstTask.SetEnableAll();
            UpdCheckBoxList();
        }

        private void btnRestNone_Click(object sender, EventArgs e)
        {
            btnRestNone.Enabled = false;
            btnRestAll.Enabled = true;

            RstTask.SetEnableNone();
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
                Canceled = false;

                RstTask.Setup();

                Close();
            }
        }

        private void chkList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var job = (Job)chkList.Items[e.Index];

            if (job != null)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    RstTask.EnableJob(job);
                }
                else
                {
                    RstTask.DisableJob(job);
                }
            }
        }

        private void btnCancelRest_Click(object sender, EventArgs e)
        {
            Canceled = true;
            
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
            return RstTask;
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
