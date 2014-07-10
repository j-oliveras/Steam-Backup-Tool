namespace steamBackup.AppServices.Tasks
{
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Properties;
    using System.Collections.Generic;
    using System.ComponentModel;

    public abstract class Task
    {
        protected TaskType Type = TaskType.Unset;
        public TaskType GetTaskType() { return Type; }

        public int m_jobsToDoCount = 0;
        public int m_jobsToSkipCount = 0;
        public int m_jobsAnalysed = 0;
        public int m_jobsDone = 0;
        public int m_jobsSkiped = 0;
        public int m_jobCount;
        
        public List<Job> JobList = new List<Job>();

        public const int m_archiveVer = 2;
        public int m_currentArchiveVer = -1;

        public string m_steamDir;
        public string m_backupDir;

        public int m_threadCount = 0;

        abstract public int RamUsage(bool useLzma2);
        abstract public void Scan(BackgroundWorker worker);
        abstract public void Setup();

        protected void SharedStart()
        {
            m_jobsToDoCount = 0;
            m_jobsToSkipCount = 0;
            m_jobsAnalysed = 0;
            m_jobsDone = 0;
            m_jobsSkiped = 0;
            m_jobCount = 0;

            foreach (var job in JobList)
            {
                if (job.m_status == JobStatus.Waiting)
                    m_jobsToDoCount++;
                else
                    m_jobsToSkipCount++;
            }

            m_jobCount = m_jobsToDoCount + m_jobsToSkipCount;
        }

        public string ProgressText()
        {
            return string.Format(Resources.ProgressFormatStr, m_jobsDone, m_jobsToDoCount, m_jobsSkiped, m_jobsToSkipCount, m_jobsAnalysed, m_jobCount);
        }

        public Job GetNextJob()
        {
            while (m_jobsAnalysed < m_jobCount)
            {
                var job = JobList[m_jobsAnalysed];
                m_jobsAnalysed++;

                if (job.m_status == JobStatus.Waiting)
                {
                    m_jobsDone++;
                    return job;
                }

                m_jobsSkiped++;
            }
            return null;
        }

        public void EnableJob(int id)
        {
            EnableJob(JobList[id]);
        }

        public void EnableJob(Job job)
        {
            job.m_status = JobStatus.Waiting;
        }

        public void DisableJob(int id)
        {
            DisableJob(JobList[id]);
        }

        public void DisableJob(Job job)
        {
            job.m_status = JobStatus.Skipped;
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
