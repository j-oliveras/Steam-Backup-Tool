using System;
using System.IO;
using System.Windows.Forms;
using steamBackup.Properties;

namespace steamBackup
{
    class RestoreJob : Job
    {
        private SevenZipWrapper wrapper = null;

        private DateTime compStarted;

        public RestoreJob()
        {            
            type = JobType.RESTORE;
        }

        ~RestoreJob()
        {
            if (wrapper != null)
            {
                wrapper.Dispose(true);
                wrapper = null;
            }
        }

        public override void start()
        {
            compStarted = DateTime.Now;
            try
            {
                string libPath = Path.GetDirectoryName(Application.ExecutablePath);
                if (libPath != null)
                {
                    libPath = Path.Combine(libPath, "rsc", "7z.dll");
                    wrapper = new SevenZipWrapper(libPath, dirBackup, true);
                }

                wrapper.Extracting += new EventHandler<ProgressEventArgs>(working);
                wrapper.FileExtractionStarted += new EventHandler<FileNameEventArgs>(started);
                wrapper.ExtractionFinished += new EventHandler<EventArgs>(finished);
                wrapper.DecompressFileArchive(dirSteam);
                wrapper.Dispose(true);
            }
            catch (System.Exception)
            {
                Utilities.addToErrorList(this);
            }
        }

        public override string getSpeedEta()
        {
            if (wrapper != null)
            {
                UInt64 processedSize;
                UInt64 totalSize;
                lock (wrapper)
                {
                    totalSize = wrapper.TotalSize;
                    processedSize = wrapper.ProcessedSize;
                }

                if (totalSize <= 0)
                    totalSize = 1;
                if (processedSize <= 0)
                    processedSize = 1;

                UInt64 sizeRemaining = totalSize - processedSize;

                TimeSpan processingTime = DateTime.Now.Subtract(compStarted);
                DateTime processingDateTime = new DateTime().AddSeconds(processingTime.TotalSeconds);

                double processingSeconds = processingTime.TotalSeconds;
                double bytesPerSec;

                if (processingSeconds > 0)
                    bytesPerSec = processedSize / processingTime.TotalSeconds;
                else
                    bytesPerSec = processedSize;

                double remainingSeconds = sizeRemaining / bytesPerSec;
                DateTime remainingTime = new DateTime().AddSeconds(remainingSeconds);

                string etaResult = string.Format(Resources.EtaFormatStr,
                                                 processingDateTime.ToString("HH:mm:ss"),
                                                 remainingTime.ToString("HH:mm:ss"));
                string speedResult;
                if (bytesPerSec < 10485760f /* 10 MB/s */)
                    speedResult = string.Format(Resources.SpeedKBFormatStr, bytesPerSec / 1024f, etaResult);
                else
                    speedResult = string.Format(Resources.SpeedMBFormatStr, bytesPerSec / 1048576f, etaResult);
                return speedResult;
            }
            return string.Empty;
        }

        public override void setSteamDir(string dir)
        {
            dirSteam = dir;
        }

        public override void setBackupDir(string dir)
        {
            dirBackup = dir;
        }
    }
}
