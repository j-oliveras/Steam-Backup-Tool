using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace steamBackup
{
    public abstract class Task
    {
        public List<Job> list = new List<Job>();

        public const int archiveVer = 2;
        public int currentArchiveVer = -1;

        public string steamDir;
        public string backupDir;

        public int threadCount = 0;

        abstract public int ramUsage();
        abstract public void scan();
        abstract public void setup(CheckedListBox chkList);

        public void enableJob(int id){enableJob(list[id]);}
        public void enableJob(Job job)
        {
            job.enabled = true;
            job.status = "Waiting";
        }

        public void disableJob(int id){disableJob(list[id]);}
        public void disableJob(Job job)
        {
            job.enabled = false;
            job.status = "Skipped";
        }

        protected void checkEnabledItems(CheckedListBox chkList)
        {

            int i = 0;
            foreach (object o in chkList.Items)
            {
                foreach (Job job in list)
                {
                    if (o.ToString().Equals(job.name))
                    {
                        if (chkList.GetItemChecked(i))
                        {
                            enableJob(job);
                        }
                        else
                        {
                            disableJob(job);
                        }
                        break;
                    }
                }

                i++;
            }
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
