/*
 *-------------------------------------------------------------------------------------------------------------------------*
 * --==--This program is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.--==--
 *                           --==--http://creativecommons.org/licenses/by-nc-sa/3.0/--==--
 *-------------------------------------------------------------------------------------------------------------------------*
 */

namespace steamBackup
{
    using steamBackup.AppServices;
    using steamBackup.AppServices.Errors;
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Tasks;
    using steamBackup.Forms;
    using steamBackup.Properties;
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Forms;
    using System.Diagnostics;

    public partial class Main : Form
    {
        readonly string m_versionNum = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        bool m_working = false;

        bool m_cancelJob;
        bool m_pauseJob;
        int m_threadDone;

        Task m_task;

        readonly Job[] m_currJobs = new Job[4];
        readonly Thread[] m_threadList = new Thread[4];
        Stopwatch m_stopWatch = new Stopwatch();

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_working && MessageBox.Show(Resources.ClosingWhileRunning, "Close Application", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
                this.Activate();
            }
            else
            {
                Save(); // save Main form settings then close
            }
        }

        // Save Main form settings
        private void Save()
        {
            Settings.BackupDir = tbxBackupDir.Text;
            Settings.SteamDir = tbxSteamDir.Text;
            Settings.Save();
        }

        public Main()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            // Load Main form settings
            Settings.Load();
            tbxSteamDir.Text = Settings.SteamDir;
            tbxBackupDir.Text = Settings.BackupDir;

            lblStarted.Text = null;
            lbl0.Text = string.Format(Resources.VersionStr, m_versionNum);
        }

        private void btnBrowseSteam_Click(object sender, EventArgs e)
        {
            var fdlg = new FolderBrowserDialog();
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                tbxSteamDir.Text = fdlg.SelectedPath;
            }
        }

        private void btnBrowseBackup_Click(object sender, EventArgs e)
        {
            var fdlg = new FolderBrowserDialog();
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                tbxBackupDir.Text = fdlg.SelectedPath;
            }

        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            if (Utilities.IsSteamRunning())
            {
                MessageBox.Show(Resources.BackupSteamRunningText, Resources.SteamRunningTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!Utilities.IsValidSteamFolder(tbxSteamDir.Text))
            {
                MessageBox.Show(string.Format(Resources.NotValidSteamDirectory, tbxSteamDir.Text));
                return;
            }

            Save();

            // Open Backup User Control Window
            var backupUserCtrl = new BackupUserCtrl();
            backupUserCtrl.ShowDialog(this);

            if (backupUserCtrl.m_canceled)
                return;

            // create folders if needed
            Utilities.SetupBackupDirectory(tbxBackupDir.Text);

            m_task = backupUserCtrl.GetTask();
            Start();
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            if (Utilities.IsSteamRunning())
            {
                MessageBox.Show(Resources.RestoreSteamRunningText, Resources.SteamRunningTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            
            if (!Utilities.IsValidSteamFolder(tbxSteamDir.Text))
            {
                MessageBox.Show(string.Format(Resources.NotValidSteamDirectory, tbxSteamDir.Text));
                return;
            }

            if (!Utilities.IsValidBackupFolder(tbxBackupDir.Text))
            {
                MessageBox.Show(string.Format(Resources.NotValidSteamBackupDirectory, tbxBackupDir.Text));
                return;
            }

            Save();

            // Open Backup User Control Window
            var restoreUserCtrl = new RestoreUserCtrl();
            restoreUserCtrl.ShowDialog(this);

            if (restoreUserCtrl.m_canceled)
                return;

            m_task = restoreUserCtrl.GetTask();
            Start();
        }

        private void Start()
        {
            m_working = true;
            
            // set UI to starting values
            pgsBarAll.Value = 0;
            pgsBarAll.Maximum = m_task.m_jobsToDoCount;

            btnBackup.Visible = false;
            btnRestore.Visible = false;

            btnBrowseSteam.Enabled = false;
            btnFindSteam.Enabled = false;
            btnBrowseBackup.Enabled = false;
            tbxSteamDir.Enabled = false;
            tbxBackupDir.Enabled = false;

            lblStarted.Text = string.Format(Resources.ProcessingStarted, DateTime.Now.ToString("H:mm.ss dd/MM/yyyy"));
            m_cancelJob = false;
            m_pauseJob = false;
            m_threadDone = 0;

            btnCancel.Visible = true;
            btnPause.Visible = true;
            btnShowLog.Visible = true;
            btnUpdWiz.Visible = false;

            timer.Start();
            m_stopWatch.Start();

            // Launch task threads
            StartThreads();
        }

        private void StartThreads()
        {
            // setup the UI for each thread that is running
            if (m_task.m_threadCount >= 1)
            {
                m_threadList[0] = new Thread(() => DoWork(0))
                {
                    Priority = ThreadPriority.Lowest,
                    Name = string.Format(Resources.JobThreadText, 1),
                    IsBackground = true
                };
                m_threadList[0].Start();

                lbl0.Text = string.Format(Resources.InstanceNumText, 1);
                lbl0Info.Text = Resources.WaitingText;
                lbl0SpeedEta.Text = string.Empty;
                this.Size = new Size(400, 482);
                lbl1.Text = string.Format(Resources.VersionStr, m_versionNum);
            }
            if (m_task.m_threadCount >= 2 && !Settings.UseLzma2)
            {
                m_threadList[1] = new Thread(() => DoWork(1))
                {
                    Priority = ThreadPriority.Lowest,
                    Name = string.Format(Resources.JobThreadText, 2),
                    IsBackground = true
                };
                m_threadList[1].Start();

                lbl1.Text = string.Format(Resources.InstanceNumText, 2);
                lbl1Info.Text = Resources.WaitingText;
                lbl1SpeedEta.Text = string.Empty;
                this.Size = new Size(400, 562);
                lbl2.Text = string.Format(Resources.VersionStr, m_versionNum);
            }
            if (m_task.m_threadCount >= 3 && !Settings.UseLzma2)
            {
                m_threadList[2] = new Thread(() => DoWork(2))
                {
                    Priority = ThreadPriority.Lowest,
                    Name = string.Format(Resources.JobThreadText, 3),
                    IsBackground = true
                };
                m_threadList[2].Start();

                lbl2.Text = string.Format(Resources.InstanceNumText, 3);
                lbl2Info.Text = Resources.WaitingText;
                lbl2SpeedEta.Text = string.Empty;
                this.Size = new Size(400, 642);
                lbl3.Text = string.Format(Resources.VersionStr, m_versionNum);
            }

            if (m_task.m_threadCount < 4 || Settings.UseLzma2) return;

            m_threadList[3] = new Thread(() => DoWork(3))
            {
                Priority = ThreadPriority.Lowest,
                Name = string.Format(Resources.JobThreadText, 4),
                IsBackground = true
            };
            m_threadList[3].Start();

            lbl3.Text = string.Format(Resources.InstanceNumText, 4);
            lbl3Info.Text = Resources.WaitingText;
            lbl3SpeedEta.Text = string.Empty;
            this.Size = new Size(400, 722);
            lbl4.Text = string.Format(Resources.VersionStr, m_versionNum);
        }

        private void DoWork(int thread)
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

            while (m_task.m_jobsAnalysed < m_task.m_jobCount && !m_cancelJob)
            {
                var job = m_task.GetNextJob();
                if (job == null)
                    break;

                m_currJobs[thread] = job;
                if (pgsBar != null) 
                    pgsBar.Value = 0;
                pgsBarAll.Value = m_task.m_jobsDone;
                lblProgress.Text = m_task.ProgressText();
                job.m_status = JobStatus.Working;
                UpdateList();

                if (lblJobFile != null)
                    lblJobFile.Text = Resources.FindingFilesText;

                if (lblJobSpeedEta != null) 
                    lblJobSpeedEta.Text = string.Empty;

                job.Start();

                if(job.m_type == JobType.Backup)
                    Utilities.CopyAcfToBackup(job, tbxBackupDir.Text);
                else
                    Utilities.CopyAcfToRestore(job, tbxBackupDir.Text);
                
                UpdateList();

                if (lblJobFile != null) 
                    lblJobFile.Text = Resources.FinishedJobText;

                if(m_cancelJob)
                    job.m_status = JobStatus.Canceled;
                m_currJobs[thread] = null;
            }

            if (pgsBar != null) 
                pgsBar.Value = 0;

            if (lblJobTitle != null) 
                lblJobTitle.Text = string.Format(Resources.InstanceFinishedText, (thread + 1));

            if (lblJobFile != null) 
                lblJobFile.Text = Resources.NoJobsText;

            if (lblJobSpeedEta != null) 
                lblJobSpeedEta.Text = string.Empty;

            JobsFinished();
        }

        // Used to update the UI at each tick
        private void timer_Tick(object sender, EventArgs e)
        {
            for (var i = 0; i < 4; i++)
            {
                if (m_currJobs[i] != null)
                {
                    UpdateStats(i, m_currJobs[i]);
                }
            }
        }

        // updates the UI
        private void UpdateStats(int thread, Job job)
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

            var name = job.m_name;
            if (job.m_name.Length >= 28)
                name = job.m_name.Substring(0, 25) + "...";

            if (lblJobTitle != null)
                lblJobTitle.Text = string.Format(Resources.InstanceProcessing, (thread + 1), job.m_status, name);

            if (pgsBar != null)
                pgsBar.Value = job.m_percDone;

            if (lblJobSpeedEta != null) 
                lblJobSpeedEta.Text = job.GetSpeedEta(false);

            if (string.IsNullOrEmpty(job.m_curFileStr)) return;

            if (lblJobFile != null)
                lblJobFile.Text = job.m_curFileStr;
        }

        // Runs after each job is done.
        private void JobsFinished()
        {
            m_threadDone++;
            if ((m_task.m_threadCount == m_threadDone && !Settings.UseLzma2) || Settings.UseLzma2)
            {
                timer.Stop();
                m_stopWatch.Stop();

                m_working = false;
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
                btnUpdWiz.Visible = true;
                this.Size = new Size(400, 402);
                lbl0.Text = string.Format(Resources.VersionStr, m_versionNum);

                TimeSpan ts = m_stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                if (ErrorList.HasErrors())
                {
                    string str = string.Format(Resources.FinishedWithErrorsText, m_task.m_jobsDone, m_task.m_jobsToDoCount, elapsedTime);
                    MessageBox.Show(str, Resources.FinishedWithErrorsTitle,MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    string str = string.Format(Resources.FinishedText, m_task.m_jobsDone, m_task.m_jobsToDoCount, elapsedTime);
                    MessageBox.Show(str, Resources.FinishedTitle);
                }

                ErrorList.Clear();
                lblProgress.Text = m_task.ProgressText();
                m_task = null;
            }
            else
            {
                lblProgress.Text = m_task.ProgressText();
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
                m_cancelJob = true;
                m_pauseJob = false;
                btnPause.Visible = false;

                for (var i = 0; i < 4; i++)
                {
                    if (m_currJobs[i] != null)
                        m_currJobs[i].m_status = JobStatus.Canceled;
                }
            }
            else
            {
                m_cancelJob = true;
                m_pauseJob = false;
                btnPause.Visible = false;
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (m_pauseJob)
            {
                m_pauseJob = false;
                btnPause.Text = Resources.PauseText;

                for (var i = 0; i < 4; i++)
                {
                    if (m_currJobs[i] != null)
                        m_currJobs[i].m_status = JobStatus.Working;
                }
            }
            else
            {
                m_pauseJob = true;
                btnPause.Text = Resources.ResumeText;

                for (var i = 0; i < 4; i++)
                {
                    if (m_currJobs[i] != null)
                        m_currJobs[i].m_status = JobStatus.Paused;
                }
            }
        }

        // Uses steam reg keys to determine where steam is installed 
        private void btnFindSteam_Click(object sender, EventArgs e)
        {
            try
            {
                string dir = Utilities.GetSteamDirectory();

                if (String.IsNullOrEmpty(dir))
                    MessageBox.Show(Resources.SteamDirNotFound, Resources.SteamDirNotFoundTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);                    
                else
                    tbxSteamDir.Text = Utilities.GetSteamDirectory();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnShowList_Click(object sender, EventArgs e)
        {
            if (Size.Width == 400)
            {
                this.Size = new Size(Size.Width + 600, Size.Height);
                listView.Size = new Size(listView.Size.Width, this.Size.Height - 50);
                btnShowLog.Text = Resources.JobListHideText;
                UpdateList();
            }
            else
            {
                this.Size = new Size(400, Size.Height);
                btnShowLog.Text = Resources.JobListShowText;
            }
        }

        private void UpdateList()
        {
            if (Size.Width == 400) return;

            listView.Items.Clear();
            var i = 0;

            listView.BeginUpdate();
            foreach (var job in m_task.JobList)
            {
                i++;
                var listItem = listView.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                listItem.SubItems.Add(job.m_name);
                listItem.SubItems.Add("");
                listItem.SubItems.Add(job.m_status.ToString());
                listItem.SubItems.Add("");
                listItem.SubItems.Add(job.m_acfFiles);

                switch (job.m_status)
                {
                    case JobStatus.Paused:
                        listItem.ForeColor = Color.Blue;
                        break;
                    case JobStatus.Waiting:
                        listItem.ForeColor = Color.Green;
                        break;
                    case JobStatus.Working:
                        listItem.ForeColor = Color.BlueViolet;
                        break;
                    case JobStatus.Skipped:
                        listItem.ForeColor = Color.DarkOrange;
                        break;
                    case JobStatus.Error:
                        listItem.ForeColor = Color.Red;
                        break;
                    case JobStatus.Canceled:
                        listItem.ForeColor = Color.Orange;
                        break;
                    case JobStatus.Finished:
                        listItem.ForeColor = Color.DarkBlue;
                        break;
                    default:
                        listItem.ForeColor = Color.Black;
                        break;
                }
            }
            listView.EndUpdate();
        }

        private void title_Click(object sender, EventArgs e)
        {
            var about = new AboutBox();
            about.ShowDialog();
        }

        private void btnUpdWiz_Click(object sender, EventArgs e)
        {
            var updater = new UpdateWizard();
            updater.ShowDialog();
        }
    }
}