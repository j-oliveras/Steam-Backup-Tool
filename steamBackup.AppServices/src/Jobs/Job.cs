namespace steamBackup.AppServices.Jobs
{
    using steamBackup.AppServices.Errors;
    using steamBackup.AppServices.Properties;
    using System;
    using System.Threading;

    public abstract class Job
    {        
        protected JobType Type = JobType.Unset;
        public JobType GetJobType() { return Type; }

        public string AcfFiles;
        public string AcfDir;
        public string Name { get; set; }
        public JobStatus Status;

        protected string DirSteam;
        abstract public void SetSteamDir(string dir);
        public string GetSteamDir() { return DirSteam; }

        protected string DirBackup;
        abstract public void SetBackupDir(string dir);
        public string GetBackupDir() { return DirBackup; }


        abstract public void Start();

        protected byte PercDone;
        public byte GetPercDone() { return PercDone; }
        private string _curFileStr;
        public string GetCurFileStr() { return _curFileStr; }

        protected void Working(object sender, ProgressEventArgs e)
        {
            PercDone = e.PercentDone;
        }

        protected void Started(object sender, FileNameEventArgs e)
        {
            _curFileStr = e.FileName;

            if (Status == JobStatus.Canceled)
            {
                e.Cancel = true;
                ErrorList.Add(new ErrorItem(Resources.JobCanceledUser, this));
            }

            while (Status == JobStatus.Paused)
            {
                Thread.Sleep(100);
            }
        }

        protected void Finished(object sender, EventArgs e)
        {
            if (Status == JobStatus.Working)
                Status = JobStatus.Finished;
        }

        public override string ToString()
        {
            string str = "";
            
            str += "name = " + Name + Environment.NewLine;
            str += "acfFiles = " + AcfFiles + Environment.NewLine;
            str += "dirSteam = " + DirSteam + Environment.NewLine;
            str += "dirBackup = " + DirBackup + Environment.NewLine;
            str += "status = " + Status;

            return str;
        }

        public abstract string GetSpeedEta();
    }
}
