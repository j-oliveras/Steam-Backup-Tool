namespace steamBackup.AppServices.Jobs.Backup
{
    using steamBackup.AppServices.Errors;
    using steamBackup.AppServices.Properties;
    using steamBackup.AppServices.SevenZipWrapper;
    using System;
    using System.IO;
    using System.Threading;
    using System.Reflection;
    using System.Collections.Generic;

    class BackupJob : Job
    {
        private SevenZipWrapper _wrapper;
        private bool _compressionLzma2;
        private int _compLevel = 5;
        private DateTime _compStarted;
        private int _lzma2Threads;

        private BackupJob() { }

        public BackupJob(string folder, string steamDir, string backupDir, string library, Dictionary<string, string> acfFiles)
        {
            Type = JobType.Backup;

            var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
            var name = Path.GetFileName(folder) ?? string.Empty;
            Name = textInfo.ToTitleCase(name);
            SetSteamDir(folder);
            SetBackupDir(Path.Combine(backupDir, BackupDirectory.Common, name + ".7z"));
            Status = JobStatus.Waiting;
            AcfDir = library;

            if (acfFiles.ContainsKey(folder))
            {
                AcfFiles = acfFiles[folder];
                acfFiles.Remove(folder);
            }
            else
            {
                AcfFiles = "";
            }
        }

        ~BackupJob()
        {
            if (_wrapper != null)
            {
                _wrapper.Dispose(false);
                _wrapper = null;
            }
        }

        public override void Start()
        {
            _compStarted = DateTime.Now;
            var fileList = Directory.GetFiles(DirSteam, "*.*", SearchOption.AllDirectories);

            try
            {
                _wrapper = new SevenZipWrapper(DirBackup, false);

                _wrapper.Compressing += Working;
                _wrapper.FileCompressionStarted += Started;
                _wrapper.CompressionFinished += Finished;

                if (_compressionLzma2)
                {
                    _wrapper.UseLzma2Compression = true;
                    _wrapper.MultithreadingNumThreads = _lzma2Threads;
                }

                int compressionLevel;
                switch (_compLevel)
                {
                    case 2:
                        compressionLevel = 3;
                        break;
                    case 3:
                        compressionLevel = 5;
                        break;
                    case 4:
                        compressionLevel = 7;
                        break;
                    case 5:
                        compressionLevel = 9;
                        break;
                    default:
                        compressionLevel = _compLevel;
                        break;
                }

                _wrapper.CompressionLevel = compressionLevel;
                _wrapper.UseMultithreading = true;

                _wrapper.CompressFiles(Utilities.UpDirLvl(DirSteam), fileList);
                _wrapper.Dispose(false);
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
                    totalSize = 2;
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

                var etaResult = string.Format(shortStr ? Resources.EtaShortFormatStr : Resources.EtaFormatStr,
                    processingDateTime.ToString("HH:mm:ss"),
                    remainingTime.ToString("HH:mm:ss"));

                string speedResult;

                if (bytesPerSec < 10485760f /* 10 MB/s */)
                    speedResult = string.Format(shortStr ? Resources.SpeedShortKBFormatStr : Resources.SpeedKBFormatStr, bytesPerSec / 1024f, etaResult);
                else
                    speedResult = string.Format(shortStr ? Resources.SpeedShortMBFormatStr : Resources.SpeedMBFormatStr, bytesPerSec / 1048576f, etaResult);

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
            //sevenZip.TempFolderPath = dir + ".tmp";
        }

        public void SetCompression(int level)
        {
            _compLevel = level;
        }

        public void SetLzma2Compression(bool lzma2Compression)
        {
            _compressionLzma2 = lzma2Compression;
        }

        public void SetLzma2Threads(int threads)
        {
            _lzma2Threads = threads;
        }
    }
}
