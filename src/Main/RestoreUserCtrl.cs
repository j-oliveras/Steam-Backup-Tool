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
            chkList.Items.Clear();
            foreach (Job job in restoreTask.list)
            {
                bool enabled = false;
                if (job.status == JobStatus.WAITING)
                    enabled = true;

                chkList.Items.Add(job.name, enabled);
            }
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

            if (chkList.SelectedItem != null)
            {
                foreach (Job job in restoreTask.list)
                {
                    if (job.name.Equals(chkList.SelectedItem.ToString()))
                    {
                        if (string.IsNullOrEmpty(job.acfFiles))
                        {
                            dboxLibList.Enabled = false;
                        }
                        else
                        {
                            dboxLibList.Enabled = true;
                        }

                        if (!job.name.Equals(Settings.sourceEngineGames))
                        {
                            dboxLibList.SelectedItem = Utilities.upDirLvl(job.getSteamDir());
                        }
                        else
                        {
                            dboxLibList.SelectedItem = null;
                        }
                    }
                }
            }
        }

        private void dboxLibList_SelectedValueChanged(object sender, EventArgs e)
        {
            foreach (Job job in restoreTask.list)
            {
                if (job.name.Equals(chkList.SelectedItem.ToString()))
                {
                    job.setSteamDir(dboxLibList.SelectedItem.ToString() + "common\\");
                    job.acfDir = dboxLibList.SelectedItem.ToString();
                }
            }
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
            foreach (Job job in restoreTask.list)
            {
                CheckedListBox chkList = (CheckedListBox)sender;

                if (chkList.Items[e.Index].ToString().Equals(job.name))
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        restoreTask.enableJob(job);
                    }
                    else
                    {
                        restoreTask.disableJob(job);
                    }
                    break;
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
            sb.Append(@"This will change how many threads are used. Doesn't use much ram and can only utilises one core per instance");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void chkList_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Customise your selection of games to restore. Older games that utilize Valve's Source Engine share resources between each other. For this reason they cannot be separated and have to be restored up together.");
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
