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

        Main mainForm = new Main();
        Utilities utilities = new Utilities();
        string steamDir;
        string backupDir;
        public bool canceled = true;

        public int threads = 1;
        public List<Item> List;

        private void RestoreUserCtrl_Load(object sender, EventArgs e)
        {
            steamDir = Settings.steamDir;
            backupDir = Settings.backupDir;

            tbarThread.Value = Settings.threadsRest;
            threadText();

            btnRestAll.Enabled = false;

            utilities.scanBackup(steamDir, backupDir);
            popCheckBoxList();
            popLibDropBox();
        }

        private void RestoreUserCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.threadsRest = tbarThread.Value;
            Settings.save();
        }

        private void popCheckBoxList()
        {
            chkList.BeginUpdate();
            chkList.Items.Clear();
            foreach (Item item in utilities.List)
            {
                chkList.Items.Add(item.name, item.enabled);
            }
            chkList.EndUpdate();
        }

        private void popLibDropBox()
        {
            dboxLibList.Items.Clear();
            dboxLibList.Items.AddRange(utilities.getLibraries(steamDir));
        }

        private void lblRefreshList_Click(object sender, EventArgs e)
        {
                popLibDropBox();
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
                foreach (Item item in utilities.List)
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
                            dboxLibList.SelectedItem = utilities.upDirLvl(item.dirSteam);
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
            foreach (Item item in utilities.List)
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

            utilities.setEnableAll();
            popCheckBoxList();
        }

        private void btnRestNone_Click(object sender, EventArgs e)
        {
            btnRestNone.Enabled = false;
            btnRestAll.Enabled = true;

            utilities.setEnableNone();
            popCheckBoxList();
        }

        private void btnStartRest_Click(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.checkSteamRun)
            {
                MessageBox.Show("Please exit Steam before restoring. To continue, exit Steam and then click the 'Start Restore' button again. Do Not start Steam untill the restore process is finished.", "Steam Is Running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                canceled = false;

                utilities.setupRestore(chkList, steamDir, backupDir);

                threads = tbarThread.Value;
                List = utilities.List;

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
