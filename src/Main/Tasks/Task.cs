using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
        
        public List<Job> list = new List<Job>();

        public const int archiveVer = 2;
        public int currentArchiveVer = -1;

        public string steamDir;
        public string backupDir;

        public int threadCount = 0;

        abstract public int ramUsage();
        abstract public void scan();
        abstract public void setup();

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
