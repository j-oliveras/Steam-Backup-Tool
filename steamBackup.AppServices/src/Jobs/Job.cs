namespace steamBackup.AppServices
{
    using steamBackup.AppServices;
    using steamBackup.AppServices.Properties;
    using System;
    using System.Threading;
    using System.Diagnostics;

    public abstract class Job : IComparable
    {
        // General Info
        protected SevenZipWrapper m_wrapper;
        public JobType m_type { get; protected set; }
        public string m_name { get; set; }
        public JobStatus m_status;

        public byte m_percDone { get; private set; }
        public string m_curFileStr { get; private set; }
        public Stopwatch m_stopWatch { get; protected set; }

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

        public virtual void Start(Task parentTask)
        {
            m_stopWatch = new Stopwatch();
            m_stopWatch.Start();
        }

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
                m_stopWatch.Stop();
                Thread.Sleep(100);
            }
            m_stopWatch.Start();
        }

        protected void Finished(object sender, EventArgs e)
        {
            if (m_status == JobStatus.Working)
                m_status = JobStatus.Finished;

            m_stopWatch.Stop();
            m_stopWatch = null;
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

        public string GetSpeedEta()
        {
            if (m_wrapper == null) return string.Empty;

            try
            {
                UInt64 processedSize;
                UInt64 totalSize;
                lock (m_wrapper)
                {
                    totalSize = m_wrapper.m_totalSize;
                    processedSize = m_wrapper.m_processedSize;
                }

                if (totalSize <= 0)
                    totalSize = 1;
                if (processedSize <= 0)
                    processedSize = 1;

                var sizeRemaining = totalSize - processedSize;

                var processingDateTime = m_stopWatch.Elapsed;

                double bytesPerSec;

                if (m_stopWatch.Elapsed.Seconds > 0)
                    bytesPerSec = processedSize / (ulong)(m_stopWatch.Elapsed.Seconds);
                else
                    bytesPerSec = processedSize;

                var remainingSeconds = sizeRemaining / bytesPerSec;
                var remainingTime = TimeSpan.FromSeconds(remainingSeconds);

                var etaResult = string.Format(Resources.EtaFormatStr,
                    processingDateTime.ToString("hh':'mm':'ss"),
                    remainingTime.ToString("hh':'mm':'ss"));
                string speedResult;
                if (bytesPerSec < 10485760f /* 10 MB/s */)
                    speedResult = string.Format(Resources.SpeedKBFormatStr, bytesPerSec / 1024f, etaResult);
                else
                    speedResult = string.Format(Resources.SpeedMBFormatStr, bytesPerSec / 1048576f, etaResult);
                return speedResult;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public int CompareTo(object obj)
        {
            return m_steamFileSize < ((Job)obj).m_steamFileSize ? 1 : -1;
        }
    }
}
