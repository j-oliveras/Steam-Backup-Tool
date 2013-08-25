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
    public partial class BackupUserCtrl : Form
    {
        public BackupUserCtrl()
        {
            InitializeComponent();
        }

        public BackupTask backupTask = new BackupTask();
        
        public bool canceled = true;
        bool updCheck = false;

        private void BackupUserCtrl_Load(object sender, EventArgs e)
        {

            tbarThread.Value = Settings.threadsBup;
            backupTask.threadCount = Settings.threadsBup;
            threadText();

            tbarComp.Value = Settings.compresion;
            backupTask.compresionLevel = Settings.compresion;
            compresionText();

            ramUsage();

            backupTask.steamDir = Settings.steamDir;
            backupTask.backupDir = Settings.backupDir;

            backupTask.list.Clear();
            backupTask.scan();
            updCheckBoxList();
        }

        private void BackupUserCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.compresion = backupTask.compresionLevel;
            Settings.threadsBup = backupTask.threadCount;
            Settings.save();
        }

        private void updCheckBoxList()
        {
            chkList.BeginUpdate();
            chkList.Items.Clear();
            foreach (Job item in backupTask.list)
            {
                chkList.Items.Add(item.name, item.enabled);
            }
            chkList.EndUpdate();
        }

        private void btnBupAll_Click(object sender, EventArgs e)
        {
            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            backupTask.setEnableAll();
            updCheckBoxList();
        }

        private void btnBupNone_Click(object sender, EventArgs e)
        {
            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            backupTask.setEnableNone();
            updCheckBoxList();
        }

        private void btnBupUpd_Click(object sender, EventArgs e)
        {
            // TODO make this faster
            
            cBoxDelBup.Enabled = false;
            cBoxDelBup.Checked = false;

            if (updCheck)
            {
                foreach (Job item in backupTask.list)
                {
                    if (item.folderTime > item.archiveTime)
                        item.enabled = true;
                    else
                        item.enabled = false;
                }
                
                updCheckBoxList();
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                chkList.Items.Clear();
                foreach (Job item in backupTask.list)
                {
                    if (!item.name.Equals(Settings.sourceEngineGames))
                    {
                        item.folderTime = folderTime(item.dirSteam);
                        item.archiveTime = new DirectoryInfo(item.dirBackup).LastWriteTimeUtc;
                    }

                    if (item.folderTime > item.archiveTime)
                        item.enabled = true;
                    else
                        item.enabled = false;

                    chkList.Items.Add(item.name, item.enabled);
                    chkList.Refresh();
                }
                this.Cursor = Cursors.Arrow;
            }
            updCheck = true;
        }

        private DateTime folderTime(string folder)
        {
            DateTime newestDate = new DateTime();
            string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (string file in files)
            {
                DateTime fileDate = new FileInfo(file).LastWriteTimeUtc;

                if (fileDate.CompareTo(newestDate) > 0)
                    newestDate = new FileInfo(file).LastWriteTimeUtc;

                if(sw.ElapsedMilliseconds > 100)
                {
                    Application.DoEvents();
                    sw.Restart();
                }
            }

            return newestDate;
        }

        private void btnStartBup_Click(object sender, EventArgs e)
        {
            if (Utilities.isSteamRunning())
            {
                MessageBox.Show("Please exit Steam before backing up. To continue, exit Steam and then click the 'Backup' button again. Do Not start Steam untill the backup process is finished.", "Steam Is Running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                canceled = false;

                backupTask.setupBackup(chkList);

                this.Close();
            }
        }

        private void btnCancelBup_Click(object sender, EventArgs e)
        {
            canceled = true;
            
            this.Close();
        }

        private void tbarThread_Scroll(object sender, EventArgs e)
        {
            backupTask.threadCount = tbarThread.Value;

            threadText();

            ramUsage();
        }

        private void threadText()
        {
            lblThread.Text = "Number Of Instances:\r\n" + tbarThread.Value.ToString();
        }

        private void tbarComp_Scroll(object sender, EventArgs e)
        {
            backupTask.compresionLevel = tbarComp.Value;
            
            compresionText();

            ramUsage();
        }

        private void compresionText()
        {
            if (backupTask.compresionLevel == 6)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Ultra";
            else if (backupTask.compresionLevel == 5)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Maximum";
            else if (backupTask.compresionLevel == 4)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Normal";
            else if (backupTask.compresionLevel == 3)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Fast";
            else if (backupTask.compresionLevel == 2)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Fastest";
            else if (backupTask.compresionLevel == 1)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Copy";
            else
                lblComp.Text = "Compression Level:" + Environment.NewLine + "N/A";
        }

        private void ramUsage()
        {
            int ram = backupTask.ramUsage();

            lblRamBackup.Text = "Max Ram Usage: " + ram.ToString() + "MB";

            if (ram >= 1500)
                lblRamBackup.ForeColor = Color.Red;
            else if (ram >= 750)
                lblRamBackup.ForeColor = Color.Orange;
            else
                lblRamBackup.ForeColor = Color.Black;
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

        private void controls_MouseLeave(object sender, EventArgs e)
        {
            infoBox.Text = "Hover your mouse over the controls to get further information.";
        }

        private void btnStartBup_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Starts the backup procedure with the above parameters";
        }

        private void btnCancelBup_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Cancels the backup procedure and navigates back to the main menu.";
        }

        private void cBoxDelBup_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "This will delete EVERYTHING in the 'Backup Directory'. Make sure that there are no valuable files in there!";
        }

        private void lblComp_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "This will change how small the backup files are. Higher compression levels will use more ram, take longer but will result in far better compression.";
        }

        private void lblThread_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "This will change how many threads are used. Each instance creates two threads. Recommended to use core_count/2 for best performance. Dramatically increases ram usage when also using high compression rates.";
        }

        private void chkList_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Customise your selection of games to backup. Older games that utilize Valve's Source Engine share resources between each other. For this reason they cannot be separated and have to be backed up together.";
        }

        private void btnBupAll_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Click to select all games for backup. The selection can be modified in the check box list.";
        }

        private void btnBupNone_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Click to deselect all games for backup. The selection can be modified in the check box list.";
        }

        private void btnBupUpd_MouseHover(object sender, EventArgs e)
        {
            infoBox.Text = "Click to select all games that have been changed since the last backup. The selection can be modified in the check box list.";
        }
    }
}
