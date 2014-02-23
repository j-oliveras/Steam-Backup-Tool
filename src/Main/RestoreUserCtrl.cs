using System;
using System.Text;
using System.Windows.Forms;

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
            job.setSteamDir(dboxLibList.SelectedItem.ToString() + "common\\");
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
                MessageBox.Show("Please exit Steam before restoring. To continue, exit Steam and then click the 'Start Restore' button again. Do Not start Steam until the restore process is finished.", "Steam Is Running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                canceled = false;

                restoreTask.setup();

                this.Close();
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
            
            this.Close();
        }

        private void tbarThread_Scroll(object sender, EventArgs e)
        {
            threadText();
        }

        private void threadText() 
        {
            lblThread.Text = "Number Of Instances:\r\n" + tbarThread.Value.ToString();
        }

        public Task getTask()
        {
            return restoreTask;
        }

        private void controls_MouseLeave(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Hover your mouse over the controls to get further information.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void btnStartRest_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Starts the restore procedure with the above parameters. \b This will overwrite any currently installed Steam games with the back up files. \b0");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void btnCancelRest_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Cancels the restore procedure and navigates back to the main menu.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void lblThread_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"This will change how many threads are used. Doesn't use much ram and can only utilizes one core per instance");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void chkList_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Customise your selection of games to restore.");
            sb.Append(@" \b NOTE: \b0 Older games that utilize Valve's Source Engine share resources between each other, However Valve has patched this out. Make sure you have the latest version of these old steam games, this tool will not be able to backup or restore them otherwise.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void btnRestAll_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Click to \b deselect \b0 all games in the list. The selection can be modified in the check box list.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void btnRestNone_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Click to \b select \b0 all games in the list. The selection can be modified in the check box list.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void lblRefreshList_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Allows for some games to be restored to a alternative library.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }
        
    }
}
