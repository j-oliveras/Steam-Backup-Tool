using System;
using System.Windows.Forms;
using steamBackup.Properties;

namespace steamBackup
{
    public partial class RestoreUserCtrl : Form
    {
        public RestoreUserCtrl()
        {
            InitializeComponent();
        }

        public RestoreTask restoreTask = new RestoreTask();

        public bool canceled = true;

        private void RestoreUserCtrl_Load(object sender, EventArgs e)
        {
            tbarThread.Value = Settings.threadsRest;
            restoreTask.threadCount = Settings.threadsRest;
            threadText();

            btnRestAll.Enabled = false;

            restoreTask.steamDir = Settings.steamDir;
            restoreTask.backupDir = Settings.backupDir;

            restoreTask.scan();

            // use databinding instead of direct access to the control
            chkList.DataSource = restoreTask.list;
            chkList.DisplayMember = "name";

            updCheckBoxList();
            updLibDropBox();
        }

        private void RestoreUserCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.threadsRest = restoreTask.threadCount;
            Settings.save();
        }

        private void updCheckBoxList()
        {
            chkList.BeginUpdate();

            // disable ItemCheck event temporarily
            chkList.ItemCheck -= chkList_ItemCheck;
            foreach (Job job in restoreTask.list)
            {
                int index = chkList.Items.IndexOf(job);
                bool enabled = job.status == JobStatus.WAITING;
                chkList.SetItemChecked(index, enabled);
            }
            // reenable ItemCheck event
            chkList.ItemCheck += chkList_ItemCheck;

            chkList.EndUpdate();
        }

        private void updLibDropBox()
        {
            dboxLibList.Items.Clear();
            dboxLibList.Items.AddRange(Utilities.getLibraries(Settings.steamDir));
        }

        private void lblRefreshList_Click(object sender, EventArgs e)
        {
            updLibDropBox();
            dboxLibListUpd();
        }

        private void chkList_SelectedIndexChanged(object sender, EventArgs e)
        {
            dboxLibListUpd();
        }

        private void dboxLibListUpd()
        {
            if (chkList.SelectedItem == null) return;

            Job job = (Job) chkList.SelectedItem;
            if (string.IsNullOrEmpty(job.acfFiles) || job.name.Equals(Settings.sourceEngineGames))
            {
                dboxLibList.Enabled = false;
                dboxLibList.SelectedItem = Utilities.upDirLvl(job.getSteamDir());
            }
            else
            {
                dboxLibList.Enabled = true;
                dboxLibList.SelectedItem = Utilities.upDirLvl(job.getSteamDir());
            }
        }

        private void dboxLibList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (chkList.SelectedItem == null) return;

            Job job = (Job) chkList.SelectedItem;
            job.setSteamDir(dboxLibList.SelectedItem + "common\\");
            job.acfDir = dboxLibList.SelectedItem.ToString();
        }

        private void btnRestAll_Click(object sender, EventArgs e)
        {
            btnRestAll.Enabled = false;
            btnRestNone.Enabled = true;

            restoreTask.setEnableAll();
            updCheckBoxList();
        }

        private void btnRestNone_Click(object sender, EventArgs e)
        {
            btnRestNone.Enabled = false;
            btnRestAll.Enabled = true;

            restoreTask.setEnableNone();
            updCheckBoxList();
        }

        private void btnStartRest_Click(object sender, EventArgs e)
        {
            if (Utilities.isSteamRunning())
            {
                MessageBox.Show(Resources.RestoreSteamRunningText, Resources.SteamRunningTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                canceled = false;

                restoreTask.setup();

                Close();
            }
        }

        private void chkList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Job job = (Job)chkList.Items[e.Index];

            if (job != null)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    restoreTask.enableJob(job);
                }
                else
                {
                    restoreTask.disableJob(job);
                }
            }
        }

        private void btnCancelRest_Click(object sender, EventArgs e)
        {
            canceled = true;
            
            Close();
        }

        private void tbarThread_Scroll(object sender, EventArgs e)
        {
            threadText();
        }

        private void threadText() 
        {
            lblThread.Text = Resources.ThreadLblInstancesText + tbarThread.Value;
        }

        public Task getTask()
        {
            return restoreTask;
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
