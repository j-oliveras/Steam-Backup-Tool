namespace steamBackup.AppServices.Jobs.Restore
{
    using steamBackup.AppServices.Errors;
    using steamBackup.AppServices.Properties;
    using steamBackup.AppServices.SevenZipWrapper;
    using System;
    using System.IO;
    using System.Reflection;

    class RestoreJob : Job
    {
        private SevenZipWrapper _wrapper;

        private DateTime _compStarted;

        public RestoreJob()
        {            
            Type = JobType.Restore;
        }

        ~RestoreJob()
        {
            if (_wrapper != null)
            {
                _wrapper.Dispose(true);
                _wrapper = null;
            }
        }

        public override void Start()
        {
            _compStarted = DateTime.Now;
            try
            {
                _wrapper = new SevenZipWrapper(DirBackup, true);

                _wrapper.Extracting += Working;
                _wrapper.FileExtractionStarted += Started;
                _wrapper.ExtractionFinished += Finished;

                _wrapper.DecompressFileArchive(DirSteam);
                _wrapper.Dispose(true);
            }
            catch (Exception ex)
            {
                ErrorList.Add(new ErrorItem(ex.Message, this, ex.StackTrace));
            }
        }

        public override string GetSpeedEta(bool shortStr)
        {
            if (_wrapper == null) return string.Empty;

            try
            {
                UInt64 processedSize;
                UInt64 totalSize;
                lock (_wrapper)
                {
                    totalSize = _wrapper.TotalSize;
                    processedSize = _wrapper.ProcessedSize;
                }

                if (totalSize <= 0)
                    totalSize = 1;
                if (processedSize <= 0)
                    processedSize = 1;

                var sizeRemaining = totalSize - processedSize;

                var processingTime = DateTime.Now.Subtract(_compStarted);
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

        public override void SetSteamDir(string dir)
        {
            DirSteam = dir;
        }

        public override void SetBackupDir(string dir)
        {
            DirBackup = dir;
        }
    }
}
