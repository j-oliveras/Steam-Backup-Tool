using System;
using System.Collections.Generic;

namespace steamBackup
{
    public enum TaskType
    {
        UNSET = -1,
        BACKUP,
        RESTORE
    }
    
    public abstract class Task
    {
        protected TaskType type = TaskType.UNSET;
        public TaskType getTaskType() { return type; }

        public int jobsToDoCount = 0;
        public int jobsToSkipCount = 0;
        public int jobsAnalysed = 0;
        public int jobsDone = 0;
        public int jobsSkiped = 0;
        public int jobCount;
        
        public List<Job> list = new List<Job>();

        public const int archiveVer = 2;
        public int currentArchiveVer = -1;

        public string steamDir;
        public string backupDir;

        public int threadCount = 0;

        abstract public int ramUsage(bool useLzma2);
        abstract public void scan();
        abstract public void setup();

        protected void sharedStart()
        {
            jobsToDoCount = 0;
            jobsToSkipCount = 0;
            jobsAnalysed = 0;
            jobsDone = 0;
            jobsSkiped = 0;
            jobCount = 0;

            foreach (Job job in list)
            {
                if (job.status == JobStatus.WAITING)
                    jobsToDoCount++;
                else
                    jobsToSkipCount++;
            }

            jobCount = jobsToDoCount + jobsToSkipCount;
        }

        public string progressText()
        {
            return "Jobs started: " + jobsDone + " of " + jobsToDoCount + Environment.NewLine +
                    "Jobs skipped: " + jobsSkiped + " of " + jobsToSkipCount + Environment.NewLine +
                    "Jobs total: " + jobsAnalysed + " of " + jobCount;
        }

        public Job getNextJob()
        {
            Job job = null;
            while (jobsAnalysed < jobCount)
            {
                job = list[jobsAnalysed];
                jobsAnalysed++;

                if (job.status == JobStatus.WAITING)
                {
                    jobsDone++;
                    return job;
                }
                else
                {
                    jobsSkiped++;
                }
            }
            return null;
        }
        
        public void enableJob(int id){enableJob(list[id]);}
        public void enableJob(Job job)
        {
            job.status = JobStatus.WAITING;
        }

        public void disableJob(int id){disableJob(list[id]);}
        public void disableJob(Job job)
        {
            job.status = JobStatus.SKIPED;
        }
        
        public void setEnableAll()
        {
            // Mark all jobs as enable
            foreach (Job job in list)
            {
                enableJob(job);
            }
        }

        public void setEnableNone()
        {
            // Mark all jobs as disabled
            foreach (Job job in list)
            {
                disableJob(job);
            }
        }

    }
}
