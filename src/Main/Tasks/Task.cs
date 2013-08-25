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

        protected void checkEnabledItems(CheckedListBox chkList)
        {

            int i = 0;
            foreach (object o in chkList.Items)
            {
                foreach (Job item in list)
                {
                    if (o.ToString().Equals(item.name))
                    {
                        if (chkList.GetItemChecked(i))
                        {
                            item.enabled = true;
                            item.status = "Waiting";
                        }
                        else
                        {
                            item.enabled = false;
                            item.status = "Skipped";
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
            foreach (Job item in list)
            {
                item.enabled = true;
            }
        }

        public void setEnableNone()
        {
            // Mark all jobs as disabled
            foreach (Job item in list)
            {
                item.enabled = false;
            }
        }
    }
}
