namespace steamBackup.AppServices.Jobs.Restore
{
    using steamBackup.AppServices.Errors;
    using steamBackup.AppServices.Properties;
    using steamBackup.AppServices.SevenZipWrapper;
    using System;
    using System.IO;
    using System.Threading;
    using System.Reflection;

    class RestoreJob : Job
    {
        private SevenZipWrapper m_wrapper;

        private DateTime m_compStarted;

        private RestoreJob() { }

        public RestoreJob(string fileName, string steamDir, string backupDir)
        {            
            m_type = JobType.Restore;

            var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
            var name = Path.GetFileNameWithoutExtension(fileName);
            m_name = textInfo.ToTitleCase(name);
            m_steamDir = Path.Combine(steamDir, Utilities.GetSteamAppsFolder(steamDir), SteamDirectory.Common);
            m_backupDir = Path.Combine(backupDir, BackupDirectory.Common, fileName);
            m_status = JobStatus.Waiting;
        }

        public void addAcfInfo(string ids, string acfDir)
        {
            m_acfFiles = ids;
            m_acfDir = acfDir;
        }

        ~RestoreJob()
        {
            if (m_wrapper != null)
            {
                m_wrapper.Dispose(true);
                m_wrapper = null;
            }
        }

        public override void Start()
        {
            m_compStarted = DateTime.Now;
            try
            {
                m_wrapper = new SevenZipWrapper(m_backupDir, true);

                m_wrapper.Extracting += Working;
                m_wrapper.FileExtractionStarted += Started;
                m_wrapper.ExtractionFinished += Finished;

                m_wrapper.DecompressFileArchive(m_steamDir);
                m_wrapper.Dispose(true);
            }
            catch (Exception ex)
            {
                ErrorList.Add(new ErrorItem(ex.Message, this, ex.StackTrace));
            }
        }

        public override string GetSpeedEta(bool shortStr)
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

                var processingTime = DateTime.Now.Subtract(m_compStarted);
                var processingDateTime = new DateTime().AddSeconds(processingTime.TotalSeconds);

                var processingSeconds = processingTime.TotalSeconds;
                double bytesPerSec;

                if (processingSeconds > 0)
                    bytesPerSec = processedSize / processingTime.TotalSeconds;
                else
                    bytesPerSec = processedSize;

                var remainingSeconds = sizeRemaining / bytesPerSec;
                var remainingTime = new DateTime().AddSeconds(remainingSeconds);

                var etaResult = string.Format(Resources.EtaFormatStr,
                    processingDateTime.ToString("HH:mm:ss"),
                    remainingTime.ToString("HH:mm:ss"));
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
    }
}
