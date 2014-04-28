namespace steamBackup.AppServices.Jobs.Backup
{
    using steamBackup.AppServices.Errors;
    using steamBackup.AppServices.Properties;
    using steamBackup.AppServices.SevenZipWrapper;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    class BackupJob : Job
    {
        private SevenZipWrapper _wrapper;
        private bool _compressionLzma2;
        private int _compLevel = 5;
        private DateTime _compStarted;

        public BackupJob()
        {
            Type = JobType.Backup;
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
            string[] fileList;
            
            if (Name.Equals(Settings.SourceEngineGames))
            {                
                var fileListBuilder = new List<string>();
                
                fileListBuilder.AddRange(Directory.GetFiles(DirSteam, "*.gcf", SearchOption.TopDirectoryOnly));
                fileListBuilder.AddRange(Directory.GetFiles(DirSteam, "*.ncf", SearchOption.TopDirectoryOnly));

                string[] folderList = Directory.GetDirectories(DirSteam, "*", SearchOption.TopDirectoryOnly);
                foreach (string folder in folderList)
                {
                    if (!folder.Contains(@"\" + Utilities.GetSteamAppsFolder(DirSteam) + @"\common") &&
                        !folder.Contains(@"\" + Utilities.GetSteamAppsFolder(DirSteam) + @"\downloading") &&
                        !folder.Contains(@"\" + Utilities.GetSteamAppsFolder(DirSteam) + @"\temp"))
                    {
                        fileListBuilder.AddRange(Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories));
                    }
                }

                fileList = fileListBuilder.ToArray();
            }
            else
            {
                fileList = Directory.GetFiles(DirSteam, "*.*", SearchOption.AllDirectories);
            }

            try
            {
                string libPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                if (libPath != null)
                {
                    libPath = Path.Combine(libPath, "rsc", "7z.dll");
                    _wrapper = new SevenZipWrapper(libPath, DirBackup, false);
                }
                _wrapper.Compressing += Working;
                _wrapper.FileCompressionStarted += Started;
                _wrapper.CompressionFinished += Finished;
                if (_compressionLzma2)
                {
                    _wrapper.UseLzma2Compression = true;
                    _wrapper.MultithreadingNumThreads = Settings.Lzma2Threads;
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

                UInt64 sizeRemaining = totalSize - processedSize;

                TimeSpan processingTime = DateTime.Now.Subtract(_compStarted);
                DateTime processingDateTime = new DateTime().AddSeconds(processingTime.TotalSeconds);

                double processingSeconds = processingTime.TotalSeconds;
                double bytesPerSec;

                if (processingSeconds > 0)
                    bytesPerSec = processedSize / processingTime.TotalSeconds;
                else
                    bytesPerSec = processedSize;

                double remainingSeconds = sizeRemaining / bytesPerSec;
                DateTime remainingTime = new DateTime().AddSeconds(remainingSeconds);

                string etaResult = string.Format(shortStr ? Resources.EtaShortFormatStr : Resources.EtaFormatStr,
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

    }
}
