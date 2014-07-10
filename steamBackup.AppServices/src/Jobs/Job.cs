namespace steamBackup.AppServices.Jobs
{
    using steamBackup.AppServices.Errors;
    using steamBackup.AppServices.Properties;
    using System;
    using System.Threading;

    public abstract class Job
    {
        // General Info
        public JobType m_type { get; protected set; }
        public string m_name { get; set; }
        public JobStatus m_status;

        public byte m_percDone { get; private set; }
        public string m_curFileStr { get; private set; }

        // Steam Info
        public string m_steamDir;
        public DateTime m_steamFileDate;
        public long m_steamFileSize;

        // Backup Info
        public string m_backupDir;
        public DateTime m_backupFileDate;
        public long m_backupFileSize;

        // ACF Info
        public string m_acfFiles { get; protected set; }
        public string m_acfDir { get; set; }

        abstract public void Start();

        protected void Working(object sender, ProgressEventArgs e)
        {
            m_percDone = e.PercentDone;
        }

        protected void Started(object sender, FileNameEventArgs e)
        {
            m_curFileStr = e.FileName;

            if (m_status == JobStatus.Canceled)
            {
                e.Cancel = true;
                ErrorList.Add(new ErrorItem(Resources.JobCanceledUser, this));
            }

            while (m_status == JobStatus.Paused)
            {
                Thread.Sleep(100);
            }
        }

        protected void Finished(object sender, EventArgs e)
        {
            if (m_status == JobStatus.Working)
                m_status = JobStatus.Finished;
        }

        public override string ToString()
        {
            var str = "";
            
            str += "name = " + m_name + Environment.NewLine;
            str += "acfFiles = " + m_acfFiles + Environment.NewLine;
            str += "dirSteam = " + m_steamDir + Environment.NewLine;
            str += "dirBackup = " + m_backupDir + Environment.NewLine;
            str += "status = " + m_status;

            return str;
        }

        public abstract string GetSpeedEta(bool shortStr);
    }
}
