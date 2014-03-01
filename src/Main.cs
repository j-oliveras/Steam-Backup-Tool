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

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using steamBackup.Properties;

namespace steamBackup
{
    public partial class Main : Form
    {
        string versionNum = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        bool cancelJob = false;
        bool pauseJob = false;
        int threadDone = 0;

        Task task = null;

        Job[] currJobs = new Job[4];
        Thread[] threadList = new Thread[4];

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            save(); // save Main form settings then close
        }

        // Save Main form settings
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

            // Load Main form settings
            Settings.load();
            tbxSteamDir.Text = Settings.steamDir;
            tbxBackupDir.Text = Settings.backupDir;

            lblStarted.Text = null;
            lbl0.Text = string.Format(Resources.VersionStr, versionNum);
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

        // Check to see if a steam install directory is valid
        private bool isValidSteamFolder()
        {
            if(File.Exists(tbxSteamDir.Text + "\\config\\config.vdf"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Check to see if a backup directory is valid
        private bool isValidBackupFolder()
        {
            if(File.Exists(tbxBackupDir.Text + "\\config.sbt"))
            {
                // Valid Archiver Version 2
                return true;
            }

            if(Directory.Exists(tbxBackupDir.Text + "\\common\\") && 
               File.Exists(tbxBackupDir.Text + "\\games.7z") &&
               File.Exists(tbxBackupDir.Text + "\\steamapps.7z"))
            {
                // Valid Archiver Version 1
                return true;
            }
            else
            {
                return false;
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.checkSteamRun)
            {
                MessageBox.Show(Resources.BackupSteamRunningText, Resources.SteamRunningTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (!isValidSteamFolder())
            {
                MessageBox.Show(string.Format(Resources.NotValidSteamDirectory, tbxSteamDir.Text));
            }
            else
            {
                save();

                // Open Backup User Control Window
                BackupUserCtrl backupUserCtrl = new BackupUserCtrl();
                backupUserCtrl.ShowDialog(this);

                if (backupUserCtrl.canceled)
                    return;

                // create folders if needed
                if (!Directory.Exists(tbxBackupDir.Text))
                    Directory.CreateDirectory(tbxBackupDir.Text);
                if (!Directory.Exists(tbxBackupDir.Text + "\\common"))
                    Directory.CreateDirectory(tbxBackupDir.Text + "\\common");
                if (!Directory.Exists(tbxBackupDir.Text + "\\acf"))
                    Directory.CreateDirectory(tbxBackupDir.Text + "\\acf");

                task = backupUserCtrl.getTask();
                start();
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.checkSteamRun)
            {
                MessageBox.Show(Resources.RestoreSteamRunningText, Resources.SteamRunningTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (!isValidSteamFolder())
            {
                MessageBox.Show(string.Format(Resources.NotValidSteamDirectory, tbxSteamDir.Text));
            }
            else if (!isValidBackupFolder())
            {
                MessageBox.Show(string.Format(Resources.NotValidSteamBackupDirectory, tbxBackupDir.Text));
            }
            else
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
        }

        private void start()
        {
            // set UI to starting values
            pgsBarAll.Value = 0;
            pgsBarAll.Maximum = task.jobsToDoCount;

            btnBackup.Visible = false;
            btnRestore.Visible = false;

            btnBrowseSteam.Enabled = false;
            btnFindSteam.Enabled = false;
            btnBrowseBackup.Enabled = false;
            tbxSteamDir.Enabled = false;
            tbxBackupDir.Enabled = false;

            lblStarted.Text = string.Format(Resources.ProcessingStarted, DateTime.Now.ToString("H:mm.ss dd/MM/yyyy"));
            cancelJob = false;
            pauseJob = false;
            threadDone = 0;

            btnCancel.Visible = true;
            btnPause.Visible = true;
            btnShowLog.Visible = true;

            timer.Start();

            // Launch task threads
            startThreads();
        }

        private void startThreads()
        {
            // setup the UI for each thread that is running
            if (task.threadCount >= 1)
            {
                threadList[0] = new Thread(() => doWork(0))
                {
                    Priority = ThreadPriority.Lowest,
                    Name = string.Format(Resources.JobThreadText, 1),
                    IsBackground = true
                };
                threadList[0].Start();

                lbl0.Text = string.Format(Resources.InstanceNumText, 1);
                lbl0Info.Text = Resources.WaitingText;
                lbl0SpeedEta.Text = string.Empty;
                this.Size = new Size(400, 482);
                lbl1.Text = string.Format(Resources.VersionStr, versionNum);
            }
            if (task.threadCount >= 2 && !Settings.useLzma2)
            {
                threadList[1] = new Thread(() => doWork(1))
                {
                    Priority = ThreadPriority.Lowest,
                    Name = string.Format(Resources.JobThreadText, 2),
                    IsBackground = true
                };
                threadList[1].Start();

                lbl1.Text = string.Format(Resources.InstanceNumText, 2);
                lbl1Info.Text = Resources.WaitingText;
                lbl1SpeedEta.Text = string.Empty;
                this.Size = new Size(400, 562);
                lbl2.Text = string.Format(Resources.VersionStr, versionNum);
            }
            if (task.threadCount >= 3 && !Settings.useLzma2)
            {
                threadList[2] = new Thread(() => doWork(2))
                {
                    Priority = ThreadPriority.Lowest,
                    Name = string.Format(Resources.JobThreadText, 3),
                    IsBackground = true
                };
                threadList[2].Start();

                lbl2.Text = string.Format(Resources.InstanceNumText, 3);
                lbl2Info.Text = Resources.WaitingText;
                lbl2SpeedEta.Text = string.Empty;
                this.Size = new Size(400, 642);
                lbl3.Text = string.Format(Resources.VersionStr, versionNum);
            }
            if (task.threadCount >= 4 && !Settings.useLzma2)
            {
                threadList[3] = new Thread(() => doWork(3))
                {
                    Priority = ThreadPriority.Lowest,
                    Name = string.Format(Resources.JobThreadText, 4),
                    IsBackground = true
                };
                threadList[3].Start();

                lbl3.Text = string.Format(Resources.InstanceNumText, 4);
                lbl3Info.Text = Resources.WaitingText;
                lbl3SpeedEta.Text = string.Empty;
                this.Size = new Size(400, 722);
                lbl4.Text = string.Format(Resources.VersionStr, versionNum);
            }
        }

        private void doWork(int thread)
        {
            Thread.Sleep(1000 * thread);

            ProgressBar pgsBar = null;
            Label lblJobTitle = null;
            Label lblJobFile = null;
            Label lblJobSpeedEta = null;

            switch (thread)
            {
                case 0:
                    pgsBar = pgsBar0;
                    lblJobTitle = lbl0;
                    lblJobFile = lbl0Info;
                    lblJobSpeedEta = lbl0SpeedEta;
                    break;
                case 1:
                    pgsBar = pgsBar1;
                    lblJobTitle = lbl1;
                    lblJobFile = lbl1Info;
                    lblJobSpeedEta = lbl1SpeedEta;
                    break;
                case 2:
                    pgsBar = pgsBar2;
                    lblJobTitle = lbl2;
                    lblJobFile = lbl2Info;
                    lblJobSpeedEta = lbl2SpeedEta;
                    break;
                case 3:
                    pgsBar = pgsBar3;
                    lblJobTitle = lbl3;
                    lblJobFile = lbl3Info;
                    lblJobSpeedEta = lbl3SpeedEta;
                    break;
            }

            while (task.jobsAnalysed < task.jobCount && !cancelJob)
            {
                Job job = task.getNextJob();
                if (job == null)
                    break;

                currJobs[thread] = job;
                pgsBar.Value = 0;
                pgsBarAll.Value = task.jobsDone;
                lblProgress.Text = task.progressText();
                job.status = JobStatus.WORKING;
                updateList();
                lblJobFile.Text = Resources.FindingFilesText;
                lblJobSpeedEta.Text = string.Empty;

                job.start();

                if(job.getJobType() == JobType.BACKUP)
                    copyAcfToBackup(job);
                else
                    copyAcfToRestore(job);
                
                updateList();
                lblJobFile.Text = Resources.FinishedJobText;

                if(cancelJob)
                    job.status = JobStatus.CANCELED;
                currJobs[thread] = null;
            }

            pgsBar.Value = 0;
            
            lblJobTitle.Text = string.Format(Resources.InstanceFinishedText, (thread + 1));
            lblJobFile.Text = Resources.NoJobsText;
            lblJobSpeedEta.Text = string.Empty;
            jobsFinished();
        }

        // Used to update the UI at each tick
        private void timer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                if (currJobs[i] != null)
                {
                    updateStats(i, currJobs[i]);
                }
            }
        }

        // updates the UI
        private void updateStats(int thread, Job job)
        {
            ProgressBar pgsBar = null;
            Label lblJobTitle = null;
            Label lblJobFile = null;
            Label lblJobSpeedEta = null;

            switch (thread)
            {
                case 0:
                    pgsBar = pgsBar0;
                    lblJobTitle = lbl0;
                    lblJobFile = lbl0Info;
                    lblJobSpeedEta = lbl0SpeedEta;
                    break;
                case 1:
                    pgsBar = pgsBar1;
                    lblJobTitle = lbl1;
                    lblJobFile = lbl1Info;
                    lblJobSpeedEta = lbl1SpeedEta;
                    break;
                case 2:
                    pgsBar = pgsBar2;
                    lblJobTitle = lbl2;
                    lblJobFile = lbl2Info;
                    lblJobSpeedEta = lbl2SpeedEta;
                    break;
                case 3:
                    pgsBar = pgsBar3;
                    lblJobTitle = lbl3;
                    lblJobFile = lbl3Info;
                    lblJobSpeedEta = lbl3SpeedEta;
                    break;
            }

            string name = job.name;
            if (job.name.Length >= 28)
                name = job.name.Substring(0, 25) + "...";

            lblJobTitle.Text = string.Format(Resources.InstanceProcessing, (thread + 1), job.status, name);
            pgsBar.Value = job.getPercDone();
            lblJobSpeedEta.Text = job.getSpeedEta();

            if (!string.IsNullOrEmpty(job.getCurFileStr()))
                lblJobFile.Text = job.getCurFileStr();
        }

        // Copies ACF files from the steam install to Backup
        private void copyAcfToBackup(Job job)
        {
            if (!String.IsNullOrEmpty(job.acfFiles))
            {
                string[] acfId = job.acfFiles.Split('|');

                foreach(string id in acfId)
                {
                    string src = job.acfDir + "\\appmanifest_" + id + ".acf";
                    string dst = tbxBackupDir.Text + "\\acf";

                    if (!Directory.Exists(dst))
                        Directory.CreateDirectory(dst);

                    FileInfo fi = new FileInfo(src);
                    StreamReader reader = fi.OpenText();

                    string acf = reader.ReadToEnd();
                    string gameCommonFolder = Utilities.upDirLvl(job.getSteamDir());
                    acf = acf.Replace(gameCommonFolder, "|DIRECTORY-STD|");
                    acf = acf.Replace(gameCommonFolder.ToLower(), "|DIRECTORY-LOWER|");
                    acf = acf.Replace(gameCommonFolder.ToLower().Replace("\\", "\\\\"), "|DIRECTORY-ESCSLASH-LOWER|");

                    File.WriteAllText(dst + "\\appmanifest_" + id + ".acf", acf);
                    reader.Close();
                }
            }
        }

        // Copies ACF files from the Backup to steam install
        private void copyAcfToRestore(Job job)
        {
            if (!String.IsNullOrEmpty(job.acfFiles))
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

                    string acf = reader.ReadToEnd();
                    string gameCommonFolder = job.acfDir + "common\\";
                    acf = acf.Replace("|DIRECTORY-STD|", gameCommonFolder);
                    acf = acf.Replace("|DIRECTORY-LOWER|", gameCommonFolder.ToLower());
                    acf = acf.Replace("|DIRECTORY-ESCSLASH-LOWER|", gameCommonFolder.ToLower().Replace("\\", "\\\\"));

                    File.WriteAllText(dst + "\\appmanifest_" + id + ".acf", acf);
                    reader.Close();
                }
            }
        }

        // Runs after each job is done.
        private void jobsFinished()
        {
            threadDone++;
            if ((task.threadCount == threadDone && !Settings.useLzma2) || Settings.useLzma2)
            {
                timer.Stop();
                
                btnBrowseSteam.Enabled = true;
                btnFindSteam.Enabled = true;
                btnBrowseBackup.Enabled = true;
                tbxSteamDir.Enabled = true;
                tbxBackupDir.Enabled = true;
                btnBackup.Visible = true;
                btnRestore.Visible = true;
                btnCancel.Visible = false;
                btnPause.Visible = false;
                btnPause.Text = Resources.PauseText;
                btnShowLog.Visible = false;
                this.Size = new Size(400, 402);
                lbl0.Text = string.Format(Resources.VersionStr, versionNum);


                    if (string.IsNullOrEmpty(Utilities.getErrorList()))
                    {
                        MessageBox.Show(string.Format(Resources.FinishedText, task.jobsDone, task.jobsToDoCount), Resources.FinishedTitle);
                    }
                    else
                    {
                        MessageBox.Show(string.Format(Resources.FinishedWithErrorsText, task.jobsDone, task.jobsToDoCount), Resources.FinishedWithErrorsTitle,MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                Utilities.clearErrorList();
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
            if (tbxSteamDir.Text == Resources.SteamInstallDir)
                tbxSteamDir.Text = "";
        }

        private void tbxBackupDir_Enter(object sender, EventArgs e)
        {
            if (tbxBackupDir.Text == Resources.BackupDir)
                tbxBackupDir.Text = "";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Resources.CancelQueryText, Resources.CancelQueryTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                cancelJob = true;
                pauseJob = false;
                btnPause.Visible = false;

                for (int i = 0; i < 4; i++)
                {
                    if (currJobs[i] != null)
                        currJobs[i].status = JobStatus.CANCELED;
                }
            }
            else
            {
                cancelJob = true;
                pauseJob = false;
                btnPause.Visible = false;
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (pauseJob)
            {
                pauseJob = false;
                btnPause.Text = Resources.PauseText;

                for (int i = 0; i < 4; i++)
                {
                    if (currJobs[i] != null)
                        currJobs[i].status = JobStatus.WORKING;
                }
            }
            else
            {
                pauseJob = true;
                btnPause.Text = Resources.ResumeText;

                for (int i = 0; i < 4; i++)
                {
                    if (currJobs[i] != null)
                        currJobs[i].status = JobStatus.PAUSED;
                }
            }
        }

        // Uses steam reg keys to determin why steam is installed 
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
                    MessageBox.Show(Resources.SteamFolderNotFound);
                }
            }

        }

        private void btnShowList_Click(object sender, EventArgs e)
        {
            if (Size.Width == 400)
            {
                this.Size = new Size(Size.Width + 600, Size.Height);
                listView.Size = new Size(listView.Size.Width, this.Size.Height - 50);
                btnShowLog.Text = Resources.JobListHideText;
                updateList();
            }
            else
            {
                this.Size = new Size(400, Size.Height);
                btnShowLog.Text = Resources.JobListShowText;
            }

        }

        private void updateList()
        {
            if (Size.Width != 400)
            {
                listView.Items.Clear();
                int i = 0;

                listView.BeginUpdate();
                foreach (Job job in task.list)
                {
                    i++;
                    ListViewItem listItem = listView.Items.Add(i.ToString());
                    listItem.SubItems.Add(job.name);
                    listItem.SubItems.Add("");
                    listItem.SubItems.Add(job.status.ToString());
                    listItem.SubItems.Add("");
                    listItem.SubItems.Add(job.acfFiles);

                    switch (job.status)
                    {
                        case JobStatus.PAUSED:
                        case JobStatus.WAITING:
                            listItem.ForeColor = Color.Green;
                            break;
                        case JobStatus.WORKING:
                            listItem.ForeColor = Color.BlueViolet;
                            break;
                        case JobStatus.SKIPED:
                            listItem.ForeColor = Color.DarkOrange;
                            break;
                        case JobStatus.ERROR:
                        case JobStatus.CANCELED:
                            listItem.ForeColor = Color.Red;
                            break;
                        case JobStatus.FINISHED:
                            listItem.ForeColor = Color.DarkBlue;
                            break;
                        default:
                            listItem.ForeColor = Color.Black;
                            break;
                    }
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