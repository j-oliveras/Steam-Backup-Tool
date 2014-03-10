using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using steamBackup.Properties;

namespace steamBackup
{
    class ErrorItem
    {
        private Job mJob = null;
        private DateTime mTime;
        private string mErrorString = null;
        private string mStackTrace = null;

        private ErrorItem()
        {
        }
        public ErrorItem(string errorString, Job job = null, string stackTrace = null)
        {
            mJob = job;
            mTime = DateTime.Now;
            mErrorString = errorString;
            mStackTrace = stackTrace;
        }

        public string toString()
        {
            string str = string.Format(Resources.JobErrorTime, mTime.ToString("dd/MM/yyyy H:mm.ss")) + Environment.NewLine;

            if (mJob != null)
                str += string.Format(Resources.JobErrorDetails, mJob.toString()) + Environment.NewLine;

            if (mErrorString != null)
                str += string.Format(Resources.JobErrorMsg, mErrorString) + Environment.NewLine;

            if (mStackTrace != null)
                str += string.Format(Resources.JobErrorStack, mStackTrace) + Environment.NewLine;

            return str;
        }
    }
    
    static class ErrorList
    {
        static private List<ErrorItem> mList = new List<ErrorItem>();

        static public void clear()
        {
            mList.Clear();
        }

        static public void add(ErrorItem item)
        {
            mList.Add(item);

            toFile();
        }

        static public bool hasErrors()
        {
            if (mList.Count == 0)
                return false;
            return true;
        }

        static public string toString()
        {
            string str = Resources.ErrorListHeader;

            foreach(ErrorItem error in mList)
            {
                str += error.toString();
            }

            return str; 
        }

        static public void toFile()
        {
            ToFile(Settings.backupDir + "\\Error Log.txt");
        }

        static public void ToFile(string dir)
        {
            File.WriteAllText(dir, toString());
        }

    }
}
