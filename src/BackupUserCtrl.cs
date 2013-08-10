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

        Utilities utilities = new Utilities();
        string steamDir;
        string backupDir;
        public bool canceled = true;

        public int threads = 1;
        public List<Item> List;

        bool updCheck = false;

        private void BackupUserCtrl_Load(object sender, EventArgs e)
        {

            steamDir = Settings.steamDir;
            backupDir = Settings.backupDir;

            tbarThread.Value = Settings.threadsBup;
            threadText();
            tbarComp.Value = Settings.compresion;
            compresionText();
            ramUsage();

            utilities.List.Clear();
            utilities.scanMisc(steamDir, backupDir);
            utilities.scanSteamAcf(steamDir, backupDir);
            utilities.scanSteamLostCommonFolders(steamDir, backupDir);
            popCheckBoxList();
        }

        private void BackupUserCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.compresion = tbarComp.Value;
            Settings.threadsBup = tbarThread.Value;
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

        private void btnBupAll_Click(object sender, EventArgs e)
        {
            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            utilities.setEnableAll();
            popCheckBoxList();
        }

        private void btnBupNone_Click(object sender, EventArgs e)
        {
            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            utilities.setEnableNone();
            popCheckBoxList();
        }

        private void btnBupUpd_Click(object sender, EventArgs e)
        {
            cBoxDelBup.Enabled = false;
            cBoxDelBup.Checked = false;

            if (updCheck)
            {
                foreach (Item item in utilities.List)
                {
                    if (item.folderTime > item.archiveTime)
                        item.enabled = true;
                    else
                        item.enabled = false;
                }
                
                popCheckBoxList();
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                chkList.Items.Clear();
                foreach (Item item in utilities.List)
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
            Process[] pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.checkSteamRun)
            {
                MessageBox.Show("Please exit Steam before backing up. To continue, exit Steam and then click the 'Backup' button again. Do Not start Steam untill the backup process is finished.", "Steam Is Running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                canceled = false;

                utilities.setupBackup(chkList, tbarComp, steamDir, backupDir, cBoxDelBup.Checked);

                threads = tbarThread.Value;
                List = utilities.List;

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
            threadText();
            
            ramUsage();
        }

        private void threadText() 
        {
            lblThread.Text = "Number Of Instances:\r\n" + tbarThread.Value.ToString();
        }

        private void tbarComp_Scroll(object sender, EventArgs e)
        {
            compresionText();

            ramUsage();
        }

        private void compresionText()
        {
            if (tbarComp.Value == 6)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Ultra";
            else if (tbarComp.Value == 5)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Maximum";
            else if (tbarComp.Value == 4)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Normal";
            else if (tbarComp.Value == 3)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Fast";
            else if (tbarComp.Value == 2)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Fastest";
            else if (tbarComp.Value == 1)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Copy";
            else
                lblComp.Text = "Compression Level:" + Environment.NewLine + "N/A";
        }

        public void ramUsage()
        {
            int ramPerThread = 0;

            if (tbarComp.Value == 6)
                ramPerThread = 709;
            else if (tbarComp.Value == 5)
                ramPerThread = 376;
            else if (tbarComp.Value == 4)
                ramPerThread = 192;
            else if (tbarComp.Value == 3)
                ramPerThread = 19;
            else if (tbarComp.Value == 2)
                ramPerThread = 6;
            else if (tbarComp.Value == 1)
                ramPerThread = 1;

            int ramBackup = (tbarThread.Value + 1) * ramPerThread;
            lblRamBackup.Text = "Max Ram Usage: " + ramBackup.ToString() + "MB";

            if (ramBackup >= 1500)
                lblRamBackup.ForeColor = Color.Red;
            else if (ramBackup >= 750)
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
