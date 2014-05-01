namespace steamBackup.AppServices.Tasks
{
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Properties;
    using System.Collections.Generic;

    public abstract class Task
    {
        protected TaskType Type = TaskType.Unset;
        public TaskType GetTaskType() { return Type; }

        public int JobsToDoCount = 0;
        public int JobsToSkipCount = 0;
        public int JobsAnalysed = 0;
        public int JobsDone = 0;
        public int JobsSkiped = 0;
        public int JobCount;
        
        public List<Job> JobList = new List<Job>();

        public const int ArchiveVer = 2;
        public int CurrentArchiveVer = -1;

        public string SteamDir;
        public string BackupDir;

        public int ThreadCount = 0;

        abstract public int RamUsage(bool useLzma2);
        abstract public void Scan();
        abstract public void Setup();

        protected void SharedStart()
        {
            JobsToDoCount = 0;
            JobsToSkipCount = 0;
            JobsAnalysed = 0;
            JobsDone = 0;
            JobsSkiped = 0;
            JobCount = 0;

            foreach (var job in JobList)
            {
                if (job.Status == JobStatus.Waiting)
                    JobsToDoCount++;
                else
                    JobsToSkipCount++;
            }

            JobCount = JobsToDoCount + JobsToSkipCount;
        }

        public string ProgressText()
        {
            return string.Format(Resources.ProgressFormatStr, JobsDone, JobsToDoCount, JobsSkiped, JobsToSkipCount, JobsAnalysed, JobCount);
        }

        public Job GetNextJob()
        {
            while (JobsAnalysed < JobCount)
            {
                var job = JobList[JobsAnalysed];
                JobsAnalysed++;

                if (job.Status == JobStatus.Waiting)
                {
                    JobsDone++;
                    return job;
                }

                JobsSkiped++;
            }
            return null;
        }

        public void EnableJob(int id)
        {
            EnableJob(JobList[id]);
        }

        public void EnableJob(Job job)
        {
            job.Status = JobStatus.Waiting;
        }

        public void DisableJob(int id)
        {
            DisableJob(JobList[id]);
        }

        public void DisableJob(Job job)
        {
            job.Status = JobStatus.Skipped;
        }
        
        public void SetEnableAll()
        {
            // Mark all jobs as enable
            foreach (var job in JobList)
            {
                EnableJob(job);
            }
        }

        public void SetEnableNone()
        {
            // Mark all jobs as disabled
            foreach (var job in JobList)
            {
                DisableJob(job);
            }
        }

    }
}
