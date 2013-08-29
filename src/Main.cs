/*-------------------------------------------------------------------------------------------------------------------------*
 * --==--This program is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.--==--
 *                           --==--http://creativecommons.org/licenses/by-nc-sa/3.0/--==--
 *-------------------------------------------------------------------------------------------------------------------------*
 *Coding by:
 *  ____  ____  _  _       ____  __  __     ____
 * ( ___)(_  _)( \/ )     (  _ \(  )(  )___(_   )
 *  )__)  _)(_  )  (       )(_) ))(__)((___)/ /_
 * (__)  (____)(_/\_) AND (____/(______)   (____)
 *        FiX                        Du-z
 *    
 * aka James Warner and Brian Duhs
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Globalization;
using Newtonsoft.Json;
using System.Reflection;
using SevenZip;


namespace steamBackup
{
    public partial class Main : Form
    {
        string versionNum = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        bool cancelJob = false;
        bool pauseJob = false;
        int threadDone = 0;

        string errorList = null;

        Task task = null;

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            save();

            Process[] processes = Process.GetProcessesByName("7za_cmd");
            foreach (Process process in processes)
            {
                process.Kill();
            }
            processes = null;
        }

        private void save()
        {
            Settings.backupDir = tbxBackupDir.Text;
            Settings.steamDir = tbxSteamDir.Text;
            Settings.save();
        }

        public Main()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            Settings.load();
            tbxSteamDir.Text = Settings.steamDir;
            tbxBackupDir.Text = Settings.backupDir;

            lblStarted.Text = null;
            lbl0.Text = "Version: " + versionNum;

            SevenZipExtractor.SetLibraryPath(Directory.GetCurrentDirectory() + @"\rsc\7z.dll");
        }

        private void btnBrowseSteam_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fdlg = new FolderBrowserDialog();
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                tbxSteamDir.Text = fdlg.SelectedPath;
            }
        }

        private void btnBrowseBackup_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fdlg = new FolderBrowserDialog();
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                tbxBackupDir.Text = fdlg.SelectedPath;
            }

        }

        private void btnBackup_Click(object sender, EventArgs e)
        {

            Process[] pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.checkSteamRun)
            {
                MessageBox.Show("Please exit Steam before backing up. To continue, exit Steam and then click the 'Backup' button again. Do Not start Steam until the backup process is finished.", "Steam Is Running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (!Directory.Exists(tbxSteamDir.Text.ToString() + "\\steamapps\\common\\") && !Directory.Exists(tbxSteamDir.Text.ToString() + "\\steam\\games\\"))
            {
                MessageBox.Show("'" + tbxSteamDir.Text.ToString() + "' is not a valid Steam installation directory");
            }
            else
            {
                

                save();

                // Open Backup User Control Window
                BackupUserCtrl backupUserCtrl = new BackupUserCtrl();
                backupUserCtrl.ShowDialog(this);

                if (backupUserCtrl.canceled)
                    return;

                if (!Directory.Exists(tbxBackupDir.Text.ToString()))
                    Directory.CreateDirectory(tbxBackupDir.Text.ToString());
                if (!Directory.Exists(tbxBackupDir.Text.ToString() + "\\common"))
                    Directory.CreateDirectory(tbxBackupDir.Text.ToString() + "\\common");
                if (!Directory.Exists(tbxBackupDir.Text.ToString() + "\\acf"))
                    Directory.CreateDirectory(tbxBackupDir.Text.ToString() + "\\acf");

                task = backupUserCtrl.getTask();
                start();

            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.checkSteamRun)
            {
                MessageBox.Show("Please exit Steam before restoring. To continue, exit Steam and then click the 'Restore' button again. Do Not start Steam until the restore process is finished.", "Steam Is Running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if(
                File.Exists(tbxBackupDir.Text.ToString() + "\\config.sbt") ||// Valid Archiver Version 2
                Directory.Exists(tbxBackupDir.Text.ToString() + "\\common\\") &&// Valid Archiver Version 1
                File.Exists(tbxBackupDir.Text.ToString() + "\\games.7z") &&
                File.Exists(tbxBackupDir.Text.ToString() + "\\steamapps.7z")
                )
            {
                save();

                // Open Backup User Control Window
                RestoreUserCtrl restoreUserCtrl = new RestoreUserCtrl();
                restoreUserCtrl.ShowDialog(this);

                if (restoreUserCtrl.canceled)
                    return;

                task = restoreUserCtrl.getTask();
                start();

            }
            else
            {
                // Invalid Archiver Version
                MessageBox.Show("'" + tbxBackupDir.Text.ToString() + "' is not a valid Steam Backup folder");
                return;
            }
            pname = null;
        }

        private void start()
        {
            
            errorList = null;

            pgsBarAll.Value = 0;
            pgsBarAll.Maximum = task.jobsToDoCount;

            btnBackup.Visible = false;
            btnRestore.Visible = false;

            btnBrowseSteam.Enabled = false;
            btnFindSteam.Enabled = false;
            btnBrowseBackup.Enabled = false;
            tbxSteamDir.Enabled = false;
            tbxBackupDir.Enabled = false;

            lblStarted.Text = "Started: " + DateTime.Now.ToString("H:mm.ss dd/MM/yyyy");
            cancelJob = false;
            pauseJob = false;
            threadDone = 0;

            btnCancel.Visible = true;
            btnPause.Visible = true;
            btnShowLog.Visible = true;

            startThreads();
        }

        private void startThreads()
        {
            if (task.threadCount >= 1)
            {
                thread0.RunWorkerAsync();
                lbl0.Text = "Instance 1:- ";
                lbl0Info.Text = "Waiting...";
                lbl1.Text = "Version: " + versionNum;
                this.Size = new Size(400, 482);
            }
            if (task.threadCount >= 2)
            {
                thread1.RunWorkerAsync();
                lbl1.Text = "Instance 2:- ";
                lbl1Info.Text = "Waiting...";
                lbl2.Text = "Version: " + versionNum;
                this.Size = new Size(400, 562);
            }
            if (task.threadCount >= 3)
            {
                thread2.RunWorkerAsync();
                lbl2.Text = "Instance 3:- ";
                lbl2Info.Text = "Waiting...";
                lbl3.Text = "Version: " + versionNum;
                this.Size = new Size(400, 642);
            }
            if (task.threadCount >= 4)
            {
                thread3.RunWorkerAsync();
                lbl3.Text = "Instance 4:- ";
                lbl3Info.Text = "Waiting...";
                lbl4.Text = "Version: " + versionNum;
                this.Size = new Size(400, 722);
            }
        }

        private void btnCancelBackup_Click(object sender, EventArgs e)
        {
            btnBrowseSteam.Enabled = true;
            btnFindSteam.Enabled = true;
            btnBrowseBackup.Enabled = true;
            tbxSteamDir.Enabled = true;
            tbxBackupDir.Enabled = true;
            btnBackup.Visible = true;
            btnRestore.Visible = true;
            btnShowLog.Visible = false;
        }

        private void thread0_DoWork(object sender, DoWorkEventArgs e)
        {
            doWork(0);
        }

        private void thread1_DoWork(object sender, DoWorkEventArgs e)
        {
            doWork(1);
        }

        private void thread2_DoWork(object sender, DoWorkEventArgs e)
        {
            doWork(2);
        }

        private void thread3_DoWork(object sender, DoWorkEventArgs e)
        {
            doWork(3);
        }

        private void doWork(int thread)
        {
            Thread.Sleep(1000 * thread);

            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

            ProgressBar pgsBar = null;
            Label lblJobTitle = null;
            Label lblJobFile = null;
            if (thread == 0)
            {
                pgsBar = pgsBar0;
                lblJobTitle = lbl0;
                lblJobFile = lbl0Info;
            }
            else if (thread == 1)
            {
                pgsBar = pgsBar1;
                lblJobTitle = lbl1;
                lblJobFile = lbl1Info;
            }
            else if (thread == 2)
            {
                pgsBar = pgsBar2;
                lblJobTitle = lbl2;
                lblJobFile = lbl2Info;
            }
            else if (thread == 3)
            {
                pgsBar = pgsBar3;
                lblJobTitle = lbl3;
                lblJobFile = lbl3Info;
            }

            while (task.jobsAnalysed < task.jobCount && !cancelJob)
            {
                Job job = task.getNextJob();
                if (job == null)
                    break;

                pgsBar.Value = 0;
                pgsBarAll.Value = task.jobsDone;
                lblProgress.Text = task.progressText();
                job.status = JobStatus.WORKING;
                updateList();

                job.start(pgsBar);

                lblJobFile.Text = "Finding Files...";

                while (job.status == JobStatus.WORKING || job.status == JobStatus.PAUSED)
                {
                    if (cancelJob)
                        job.status = JobStatus.CANCELED;
                    else if(pauseJob)
                        job.status = JobStatus.PAUSED;
                    else
                        job.status = JobStatus.WORKING;

                    string name = job.name;
                    if (job.name.Length >= 28)
                        name = job.name.Substring(0, 25) + "...";

                    lblJobTitle.Text = "Instance " + (thread + 1) + ":- (" + job.status.ToString() + ") " + name;

                    if (!string.IsNullOrEmpty(job.getCurFileStr()))
                    {
                        lblJobFile.Text = job.getCurFileStr();
                    }
                    
                    Thread.Sleep(1000);
                }

                if (job.status == JobStatus.FINISHED)
                {
                    if(job.getJobType() == JobType.BACKUP)
                        copyAcfToBackup(job);
                    else
                        copyAcfToRestore(job);
                }
                
                updateList();
                lblJobFile.Text = "Finished Job...";
            }

            pgsBar.Value = 0;
            
            lblJobTitle.Text = "Instance " + (thread + 1) + ":- Finished";
            lblJobFile.Text = "No Jobs Remaining...";
            jobsFinished();
        }

        private void addToErrorList(Job job, int exitCode)
        {
            // TODO redo this
            
            if (string.IsNullOrEmpty(errorList))
            {
                errorList += "Listed below are the errors for the backup or restore finished " + DateTime.Now.ToString("dd/MM/yyyy H:mm.ss") + Environment.NewLine + Environment.NewLine;
                errorList += "Please try running the backup process again making sure that there are no programs accessing the files being backed up (e.g. Steam)." + Environment.NewLine + Environment.NewLine;
                errorList += "To check the integrity of this backup: navigate to the backup location -> Select all files in the 'common' folder -> right click -> 7zip -> Test archive. You should do the same for 'Source Games.7z' also.";
            }

            errorList += Environment.NewLine + Environment.NewLine + "//////////////////// ERROR CODE: " + exitCode + " \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\" + Environment.NewLine + Environment.NewLine;

            if (exitCode == 1)
                errorList += "Warning (Non fatal error(s)). For example, one or more files were locked by some other application, so they were not compressed.";
            else if (exitCode == 2)
                errorList += "Fatal error";
            else if (exitCode == 7)
                errorList += "Command line error";
            else if (exitCode == 8)
                errorList += "Not enough memory for operation";
            else if (exitCode == 255)
                errorList += "User stopped the process";
            else
                errorList += "Unknown Error";

            errorList += Environment.NewLine + job.toString();
        }

        private void copyAcfToBackup(Job job)
        {
            string[] acfId = job.acfFiles.Split('|');

            foreach(string id in acfId)
            {
                string src = job.acfDir + "\\appmanifest_" + id + ".acf";
                string dst = tbxBackupDir.Text + "\\acf";

                if(!Directory.Exists(dst))
                    Directory.CreateDirectory(dst);

                FileInfo fi = new FileInfo(src);
                StreamReader reader = fi.OpenText();

                string acf = reader.ReadToEnd().ToString();
                string gameCommonFolder = Utilities.upDirLvl(job.getSteamDir());
                acf = acf.Replace(gameCommonFolder, "|DIRECTORY-STD|");
                acf = acf.Replace(gameCommonFolder.ToLower(), "|DIRECTORY-LOWER|");
                acf = acf.Replace(gameCommonFolder.ToLower().Replace("\\", "\\\\"), "|DIRECTORY-ESCSLASH-LOWER|");

                File.WriteAllText(dst + "\\appmanifest_" + id + ".acf", acf);
                reader.Close();
            }
        }

        private void copyAcfToRestore(Job job)
        {
            string[] acfId = job.acfFiles.Split('|');

            foreach (string id in acfId)
            {
                string src = tbxBackupDir.Text + "\\acf\\appmanifest_" + id + ".acf";
                string dst = job.acfDir;

                if (!Directory.Exists(dst))
                    Directory.CreateDirectory(dst);

                FileInfo fi = new FileInfo(src);
                StreamReader reader = fi.OpenText();

                string acf = reader.ReadToEnd().ToString();
                string gameCommonFolder = job.acfDir + "common\\";
                acf = acf.Replace("|DIRECTORY-STD|", gameCommonFolder);
                acf = acf.Replace("|DIRECTORY-LOWER|", gameCommonFolder.ToLower());
                acf = acf.Replace("|DIRECTORY-ESCSLASH-LOWER|", gameCommonFolder.ToLower().Replace("\\", "\\\\"));

                File.WriteAllText(dst + "\\appmanifest_" + id + ".acf", acf);
                reader.Close();
            }
        }

        private void jobsFinished()
        {
            threadDone++;
            if (task.threadCount == threadDone)
            {
                
                btnBrowseSteam.Enabled = true;
                btnFindSteam.Enabled = true;
                btnBrowseBackup.Enabled = true;
                tbxSteamDir.Enabled = true;
                tbxBackupDir.Enabled = true;
                btnBackup.Visible = true;
                btnRestore.Visible = true;
                btnCancel.Visible = false;
                btnPause.Visible = false;
                btnPause.Text = "Pause";
                btnShowLog.Visible = false;
                lbl0.Text = "Version: " + versionNum;
                this.Size = new Size(400, 402);


                    if (string.IsNullOrEmpty(errorList))
                    {
                        MessageBox.Show("Jobs finished with " + task.jobsDone + " of " + task.jobsToDoCount + " completed!" + Environment.NewLine + Environment.NewLine +
                            "Steam Backup Tool finished without finding any errors.", "Finished Successfully");
                    }
                    else
                    {

                        File.WriteAllText(tbxBackupDir.Text + "\\Error Log.txt", errorList);
                        MessageBox.Show("WARNING!" + Environment.NewLine + Environment.NewLine +
                            "Jobs finished with " + task.jobsDone + " of " + task.jobsToDoCount + " completed!" + Environment.NewLine + Environment.NewLine +
                            "However, Steam Backup Tool has encountered error, It is recommended that you look at the 'Error Log.txt' file in the backup directory for a full list of errors.", "Errors Found",MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                lblProgress.Text = task.progressText();
                task = null;
            }
            else
            {
                lblProgress.Text = task.progressText();
            }
        }

        private void tbxSteamDir_Enter(object sender, EventArgs e)
        {
            if (tbxSteamDir.Text == "Steam Install Directory")
                tbxSteamDir.Text = "";
        }

        private void tbxBackupDir_Enter(object sender, EventArgs e)
        {
            if (tbxBackupDir.Text == "Backup Directory")
                tbxBackupDir.Text = "";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("Do you want to cancel immediately? This could corrupt the games that are currently being worked on." + Environment.NewLine + Environment.NewLine +
                "Click 'Yes' to stop immediately, or 'No' to cancel once the current jobs have been finished.", "Cancel immediately?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)

            cancelJob = true;
            pauseJob = false;
            btnPause.Visible = false;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (pauseJob)
            {
                pauseJob = false;
                btnPause.Text = "Pause";
            }
            else
            {
                pauseJob = true;
                btnPause.Text = "Resume";
            }
        }

        private void btnFindSteam_Click(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam", false);


            try
            {
                tbxSteamDir.Text = Utilities.getFileSystemCasing((string)key.GetValue("SteamPath"));
            }
            catch (NullReferenceException)
            {
                key = Registry.LocalMachine.OpenSubKey("Software\\Valve\\Steam", false);
                
                try
                {
                    tbxSteamDir.Text = (string)key.GetValue("InstallPath");
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("Sorry, But I could not find where steam has been installed." + Environment.NewLine + Environment.NewLine + "Please browse for it manually.");
                }
            }

        }

        private void btnShowList_Click(object sender, EventArgs e)
        {
            if (Size.Width == 400)
            {
                btnShowLog.Text = "Hide Job List";
                this.Size = new Size(Size.Width + 600, Size.Height);
                listView.Size = new Size(listView.Size.Width, this.Size.Height - 50);
                updateList();
            }
            else
            {
                btnShowLog.Text = "Show Job List";
                this.Size = new Size(400, Size.Height);
            }

        }

        private void updateList()
        {
            if (Size.Width != 400)
            {
                listView.Items.Clear();
                ListViewItem listItem = new ListViewItem();
                int i = 0;

                listView.BeginUpdate();
                foreach (Job job in task.list)
                {
                    i++;
                    listItem = listView.Items.Add(i.ToString());
                    listItem.SubItems.Add(job.name);
                    listItem.SubItems.Add("");
                    listItem.SubItems.Add(job.status.ToString());
                    listItem.SubItems.Add("");
                    listItem.SubItems.Add(job.acfFiles);

                    if (job.status == JobStatus.WAITING || job.status == JobStatus.PAUSED)
                        listItem.ForeColor = Color.Green;
                    else if (job.status == JobStatus.WORKING)
                        listItem.ForeColor = Color.BlueViolet;
                    else if (job.status == JobStatus.SKIPED)
                        listItem.ForeColor = Color.DarkOrange;
                    else if (job.status == JobStatus.CANCELED || job.status == JobStatus.ERROR)
                        listItem.ForeColor = Color.Red;
                    else if (job.status == JobStatus.FINISHED)
                        listItem.ForeColor = Color.DarkBlue;
                    else
                        listItem.ForeColor = Color.Black;
                }
                listView.EndUpdate();
            }
        }

        private void title_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog();
        }

    }
}