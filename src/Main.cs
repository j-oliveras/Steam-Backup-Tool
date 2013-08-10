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


namespace steamBackup
{
    public partial class Main : Form
    {
        string versionNum = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        int jobsToDoCount = 0;
        int jobsToSkipCount = 0;
        int jobsAnalysed = 0;
        int jobsDone = 0;
        int jobsSkiped = 0;
        int jobCount;
        int[] PID = new int[4];
        bool cancelJob = false;
        bool pauseJob = false;
        int threadDone = 0;
        bool updatingLog;
        bool isBackup;
        string errorList = null;

        List<Item> List;
        int numThreads;

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
                MessageBox.Show("Please exit Steam before backing up. To continue, exit Steam and then click the 'Backup' button again. Do Not start Steam untill the backup process is finished.", "Steam Is Running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                if (!Directory.Exists(tbxSteamDir.Text.ToString() + "\\steamapps\\common\\") && !Directory.Exists(tbxSteamDir.Text.ToString() + "\\steam\\games\\"))
                {
                    MessageBox.Show("'" + tbxSteamDir.Text.ToString() + "' is not a valid Steam installation directory");
                    return;
                }

                save();

                // Open Backup User Control Window
                BackupUserCtrl backupUserCtrl = new BackupUserCtrl();
                backupUserCtrl.ShowDialog(this);

                if (backupUserCtrl.canceled)
                    return;

                startBackup(backupUserCtrl.List, backupUserCtrl.threads);

            }
            pname = null;
        }

        public void startBackup(List<Item> BackupList, int NumThreads)
        {
            jobsToDoCount = 0;
            jobsToSkipCount = 0;
            jobsAnalysed = 0;
            jobsDone = 0;
            jobsSkiped = 0;
            jobCount = 0;
            errorList = null;

            List = BackupList;
            numThreads = NumThreads;

            foreach (Item item in List)
            {
                if(item.enabled)
                    jobsToDoCount++;
                else
                    jobsToSkipCount++;
            }

            progressBar.Value = 0;
            progressBar.Maximum = jobsToDoCount;
            jobCount = jobsToDoCount + jobsToSkipCount;

            if (!Directory.Exists(tbxBackupDir.Text.ToString()))
            {
                Directory.CreateDirectory(tbxBackupDir.Text.ToString());
                while (!Directory.Exists(tbxBackupDir.Text.ToString()))
                    Thread.Sleep(10);
            }

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

            isBackup = true;
            startThreads();
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.checkSteamRun)
            {
                MessageBox.Show("Please exit Steam before restoring. To continue, exit Steam and then click the 'Restore' button again. Do Not start Steam untill the restore process is finished.", "Steam Is Running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                if (File.Exists(tbxBackupDir.Text.ToString() + "\\config.sbt"))
                {
                    // Valid Archiver Version 2
                }
                else if (Directory.Exists(tbxBackupDir.Text.ToString() + "\\common\\") &&
                    File.Exists(tbxBackupDir.Text.ToString() + "\\games.7z") &&
                    File.Exists(tbxBackupDir.Text.ToString() + "\\steamapps.7z"))
                {
                    // Valid Archiver Version 1
                }
                else
                {
                    // Invalid Archiver Version
                    MessageBox.Show("'" + tbxBackupDir.Text.ToString() + "' is not a valid Steam Backup folder");
                    return;
                }

                save();

                // Open Backup User Control Window
                RestoreUserCtrl restoreUserCtrl = new RestoreUserCtrl();
                restoreUserCtrl.ShowDialog(this);

                if (restoreUserCtrl.canceled)
                    return;

                startRestore(restoreUserCtrl.List, restoreUserCtrl.threads);

            }
            pname = null;
        }

        private void startRestore(List<Item> BackupList, int NumThreads)
        {

            jobsToDoCount = 0;
            jobsToSkipCount = 0;
            jobsAnalysed = 0;
            jobsDone = 0;
            jobsSkiped = 0;
            jobCount = 0;
            errorList = null;

            List = BackupList;
            numThreads = NumThreads;

            foreach (Item item in List)
            {
                if (item.enabled)
                    jobsToDoCount++;
                else
                    jobsToSkipCount++;
            }

            progressBar.Value = 0;
            progressBar.Maximum = jobsToDoCount;
            jobCount = jobsToDoCount + jobsToSkipCount;

            btnBackup.Visible = false;
            btnRestore.Visible = false;

            btnBrowseSteam.Enabled = false;
            btnFindSteam.Enabled = false;
            btnBrowseBackup.Enabled = false;
            tbxSteamDir.Enabled = false;
            tbxBackupDir.Enabled = false;

            lblStarted.Text = "Started: " + DateTime.Now.ToString("dd/MM/yyyy H:mm.ss");
            cancelJob = false;
            pauseJob = false;
            threadDone = 0;

            btnCancel.Visible = true;
            btnPause.Visible = true;
            btnShowLog.Visible = true;

            isBackup = false;
            startThreads();
        }

        private void startThreads()
        {
            if (numThreads >= 1)
            {
                thread0.RunWorkerAsync();
                lbl0.Text = "Instance 1: ";
                lbl1.Text = "Version: " + versionNum;
                this.Size = new Size(400, 482);
            }
            if (numThreads >= 2)
            {
                thread1.RunWorkerAsync();
                lbl1.Text = "Instance 2: ";
                lbl2.Text = "Version: " + versionNum;
                this.Size = new Size(400, 562);
            }
            if (numThreads >= 3)
            {
                thread2.RunWorkerAsync();
                lbl2.Text = "Instance 3: ";
                lbl3.Text = "Version: " + versionNum;
                this.Size = new Size(400, 642);
            }
            if (numThreads >= 4)
            {
                thread3.RunWorkerAsync();
                lbl3.Text = "Instance 4: ";
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

        StringBuilder[] InstOut = { new StringBuilder(10000), new StringBuilder(1000), new StringBuilder(10000), new StringBuilder(10000) };

        private void thread0_DoWork(object sender, DoWorkEventArgs e)
        {
            doWork(0);
        }

        void thread0GetRedirect(object sender, DataReceivedEventArgs e)
        {
            InstOut[0].Insert(0, e.Data + Environment.NewLine);
            InstOut[0].Length = 5000;
        }

        private void thread1_DoWork(object sender, DoWorkEventArgs e)
        {
            doWork(1);
        }

        void thread1GetRedirect(object sender, DataReceivedEventArgs e)
        {
            InstOut[1].Insert(0, e.Data + Environment.NewLine);
            InstOut[1].Length = 5000;
        }

        private void thread2_DoWork(object sender, DoWorkEventArgs e)
        {
            doWork(2);
        }

        void thread2GetRedirect(object sender, DataReceivedEventArgs e)
        {
            InstOut[2].Insert(0, e.Data + Environment.NewLine);
            InstOut[2].Length = 5000;
        }

        private void thread3_DoWork(object sender, DoWorkEventArgs e)
        {
            doWork(3);
        }

        void thread3GetRedirect(object sender, DataReceivedEventArgs e)
        {
            InstOut[3].Insert(0, e.Data + Environment.NewLine);
            InstOut[3].Length = 5000;
        }

        private Item getJob()
        {
            Item item = null;
            while (jobsAnalysed < jobCount)
            {
                item = List[jobsAnalysed];
                jobsAnalysed++;

                if (item.enabled)
                {
                    jobsDone++;
                    return item;
                }
                else
                {
                    jobsSkiped++;
                }
            }
            return null;
        }

        private void doWork(int thread)
        {
            
            while (jobsAnalysed < jobCount)
            {
                Thread.Sleep(100 * thread);
                Item job = getJob();

                if (job == null)
                    break;
                checkPause(job, thread);
                if (cancelJob)
                    break;
                InstOut[thread].Clear();
                InstOut[thread].Insert(0, "Starting next job: " + jobsDone + " out of " + jobsToDoCount + " jobs started." + Environment.NewLine + "'" + job.program + ":" + job.argument + "'" + Environment.NewLine);
                progressBar.Value = jobsDone;
                lblProgress.Text = "Jobs started: " + jobsDone + " of " + jobsToDoCount + Environment.NewLine +
                    "Jobs skipped: " + jobsToSkipCount + " of " + jobsToSkipCount + Environment.NewLine +
                    "Jobs total: " + jobsAnalysed + " of " + jobCount;
                job.status = "Working";
                updateList();
                Process theProcess = new Process();
                theProcess.StartInfo.FileName = job.program;
                theProcess.StartInfo.Arguments = job.argument;
                theProcess.StartInfo.RedirectStandardOutput = true;
                theProcess.StartInfo.RedirectStandardError = true;
                theProcess.EnableRaisingEvents = true;
                theProcess.StartInfo.CreateNoWindow = true;
                theProcess.StartInfo.UseShellExecute = false;
                if (thread == 0)
                {
                    theProcess.ErrorDataReceived += thread0GetRedirect;
                    theProcess.OutputDataReceived += thread0GetRedirect;
                }
                if (thread == 1)
                {
                    theProcess.ErrorDataReceived += thread1GetRedirect;
                    theProcess.OutputDataReceived += thread1GetRedirect;
                }
                if (thread == 2)
                {
                    theProcess.ErrorDataReceived += thread2GetRedirect;
                    theProcess.OutputDataReceived += thread2GetRedirect;
                }
                if (thread == 3)
                {
                    theProcess.ErrorDataReceived += thread3GetRedirect;
                    theProcess.OutputDataReceived += thread3GetRedirect;
                }
                theProcess.Start();
                theProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
                PID[thread] = theProcess.Id;
                theProcess.BeginErrorReadLine();
                theProcess.BeginOutputReadLine();
                while (!theProcess.HasExited)
                {
                    monitorProcess(theProcess, thread);
                }
                theProcess.WaitForExit();
                int exitCode = theProcess.ExitCode;
                theProcess.Dispose();
                if (exitCode == 0)
                {
                    if (!string.IsNullOrEmpty(job.acfDir))
                    {
                        if (isBackup)
                            copyAcfToBackup(job);
                        else
                            copyAcfToRestore(job);
                    }
                    job.status = "Done";
                }
                else
                {
                    addToErrorList(job, exitCode);
                    job.status = "Error";
                }
                updateList();
                InstOut[thread].Insert(0, "Job Finished, Finding next job" + Environment.NewLine);
            }
            InstOut[thread].Clear();
            InstOut[thread].Insert(0, "Instance " + (thread + 1) + " has finished with " + (jobsAnalysed - jobsAnalysed).ToString() + " jobs to go.");
            updTextBox(thread);
            jobsFinished();
        }

        private void addToErrorList(Item job, int exitCode)
        {
            if (string.IsNullOrEmpty(errorList))
            {
                errorList += "Listed below are the errors for the backup or restore finished " + DateTime.Now.ToString("dd/MM/yyyy H:mm.ss") + Environment.NewLine + Environment.NewLine;
                errorList += "Please try running the backup process again making sure that there are no programs accessing the files being backed up (i.e. Steam)." + Environment.NewLine + Environment.NewLine;
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
                errorList += "Unkown Error";

            errorList += Environment.NewLine + job.toString();
        }

        private void monitorProcess(Process theProcess, int thread)
        {
            if (cancelJob)
                theProcess.Kill();

            updTextBox(thread);
            updNameLabel(thread, theProcess);

            Thread.Sleep(1000);
        }

        private void updTextBox(int thread)
        {
            RichTextBox tb;
            if (thread == 3)
            {
                tb = tBoxInst3Out;
            }
            else if (thread == 2)
            {
                tb = tBoxInst2Out;
            }
            else if (thread == 1)
            {
                tb = tBoxInst1Out;
            }
            else
            {
                tb = tBoxInst0Out;
            }

            try
            {
                string output = InstOut[thread].ToString();

                if (!tb.Text.Equals(output))
                    tb.Text = output;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                InstOut[thread].Clear();
            }
            catch (NullReferenceException)
            {
            }
        }

        private void updNameLabel(int thread, Process theProcess)
        {

            string status;
            if (theProcess.Responding)
                status = "Running";
            else
                status = "Not Responding";

            if (thread == 3)
            {
                lbl3.Text = "Instance 4: " + status;
            }
            else if (thread == 2)
            {
                lbl2.Text = "Instance 3: " + status;
            }
            else if (thread == 1)
            {
                lbl1.Text = "Instance 2: " + status;
            }
            else
            {
                lbl0.Text = "Instance 1: " + status;
            }
        }

        private void copyAcfToBackup(Item job)
        {
            string[] acfId = job.appId.Split('|');

            foreach(string id in acfId)
            {
                string src = job.acfDir + "\\appmanifest_" + id + ".acf";
                string dst = tbxBackupDir.Text + "\\acf";

                if(!Directory.Exists(dst))
                    Directory.CreateDirectory(dst);

                FileInfo fi = new FileInfo(src);
                StreamReader reader = fi.OpenText();

                string acf = reader.ReadToEnd().ToString();
                string gameCommonFolder = upDirLvl(job.dirSteam);
                acf = acf.Replace(gameCommonFolder, "|DIRECTORY-STD|");
                acf = acf.Replace(gameCommonFolder.ToLower(), "|DIRECTORY-LOWER|");
                acf = acf.Replace(gameCommonFolder.ToLower().Replace("\\", "\\\\"), "|DIRECTORY-ESCSLASH-LOWER|");

                File.WriteAllText(dst + "\\appmanifest_" + id + ".acf", acf);
                reader.Close();
            }
        }

        private void copyAcfToRestore(Item job)
        {
            string[] acfId = job.appId.Split('|');

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

        private string upDirLvl(string dir)
        {
            string[] splits = dir.TrimEnd('\\').Split('\\');
            string rdir = "";

            for (int i = 0; i < splits.Length - 1; i++)
            {
                rdir += splits[i] + "\\"; 
            }

            return rdir;
        }

        private void jobsFinished()
        {
            threadDone++;
            if (numThreads == threadDone)
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
                        MessageBox.Show("Jobs finished with " + jobsDone + " of " + jobsToDoCount + " completed!" + Environment.NewLine + Environment.NewLine +
                            "Steam Backup Tool finished without finding any errors.", "Finished Successfully");
                    }
                    else
                    {

                        File.WriteAllText(tbxBackupDir.Text + "\\Error Log.txt", errorList);
                        MessageBox.Show("WARNING!" + Environment.NewLine + Environment.NewLine +
                            "Jobs finished with " + jobsDone + " of " + jobsToDoCount + " completed!" + Environment.NewLine + Environment.NewLine +
                            "However, Steam Backup Tool has encountered error, It is recommended that you look at the 'Error Log.txt' file in the backup directory for a full list of errors.", "Errors Found",MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

            }
            
            lblProgress.Text = "Jobs started: " + jobsDone + " of " + jobsToDoCount + Environment.NewLine +
                    "Jobs skipped: " + jobsToSkipCount + " of " + jobsToSkipCount + Environment.NewLine +
                    "Jobs total: " + jobsAnalysed + " of " + jobCount;
        }

        private void checkPause(Item item, int thread)
        {
            if (pauseJob)
            {
                InstOut[thread].Insert(0, "Instance Paused");
                item.status = "Paused";
                updateList();
                while (pauseJob)
                {
                    Thread.Sleep(50);
                }
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
                MessageBox.Show("Pausing when the current jobs are finished.");
            }
        }

        private void btnFindSteam_Click(object sender, EventArgs e)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Valve\\Steam", false);

            try
            {
                tbxSteamDir.Text = (string)key.GetValue("InstallPath");
            }
            catch (NullReferenceException)
            {
                tbxSteamDir.Text = "";
                MessageBox.Show("Sorry, But I could not find where steam has been installed." + Environment.NewLine + Environment.NewLine + "Please browse for it manually.");
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
            if (!updatingLog && Size.Width != 400)
            {
                updatingLog = true;
                listView.Items.Clear();
                ListViewItem listItem = new ListViewItem();
                int i = 0;

                listView.BeginUpdate();
                foreach (Item item in List)
                {
                    i++;
                    listItem = listView.Items.Add(i.ToString());
                    listItem.SubItems.Add(item.name);
                    listItem.SubItems.Add(item.program);
                    listItem.SubItems.Add(item.status);
                    listItem.SubItems.Add(item.argument);
                    listItem.SubItems.Add(item.appId);

                    if (item.status.Equals("Working") || item.status.Equals("Paused"))
                        listItem.ForeColor = Color.Green;
                    else if (item.status.Equals("Skipped"))
                        listItem.ForeColor = Color.DarkOrange;
                    else if (item.status.Equals("Error"))
                        listItem.ForeColor = Color.Red;
                    else if (item.status.Equals("Done"))
                        listItem.ForeColor = Color.DarkBlue;
                    else
                        listItem.ForeColor = Color.Black;
                }
                listView.EndUpdate();
                updatingLog = false;
            }
        }

        private void title_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog();
        }

    }
}