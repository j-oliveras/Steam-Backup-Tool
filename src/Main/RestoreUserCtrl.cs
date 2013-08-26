using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

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
            foreach (Job item in restoreTask.list)
            {
                chkList.Items.Add(item.name, item.enabled);
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
                foreach (Job item in restoreTask.list)
                {
                    if (item.name.Equals(chkList.SelectedItem.ToString()))
                    {
                        if (string.IsNullOrEmpty(item.appId))
                        {
                            dboxLibList.Enabled = false;
                        }
                        else
                        {
                            dboxLibList.Enabled = true;
                        }

                        if (!item.name.Equals(Settings.sourceEngineGames))
                        {
                            dboxLibList.SelectedItem = Utilities.upDirLvl(item.dirSteam);
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
            foreach (Job item in restoreTask.list)
            {
                if (item.name.Equals(chkList.SelectedItem.ToString()))
                {
                    item.dirSteam = dboxLibList.SelectedItem.ToString() + "common\\";
                    item.acfDir = dboxLibList.SelectedItem.ToString();
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

                restoreTask.setup(chkList);

                this.Close();
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
            infoBox.Text = "Hover your mouse over the controls to get further information.";
        }

        private void btnStartRest_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Starts the restore procedure with the above parameters.\r\nThis will overwrite any currently installed Steam games with the back up files.";
        }

        private void btnCancelRest_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Cancels the restore procedure and navigates back to the main menu.";
        }

        private void lblThread_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "This will change how many threads are used. Doesn't use much ram and can only utilises one core per instance";
        }

        private void chkList_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Customise your selection of games to restore. Older games that utilize Valve's Source Engine share resources between each other. For this reason they cannot be separated and have to be restored up together.";
        }

        private void btnRestAll_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Click to deselect all games in the list. The selection can be modified in the check box list.";
        }

        private void btnRestNone_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Click to select all games in the list. The selection can be modified in the check box list.";
        }

        private void lblRefreshList_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Allows for some games to be restored to a alternative library.";
        }
        
    }
}
