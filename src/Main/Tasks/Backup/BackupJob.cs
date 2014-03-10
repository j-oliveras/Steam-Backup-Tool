using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using steamBackup.Properties;

namespace steamBackup
{
    class BackupJob : Job
    {
        private SevenZipWrapper wrapper = null;
        private bool compressionLzma2 = false;
        private int compLevel = 5;
        private DateTime compStarted;

        public BackupJob()
        {
            type = JobType.BACKUP;
        }

        ~BackupJob()
        {
            if (wrapper != null)
            {
                wrapper.Dispose(false);
                wrapper = null;
            }
        }

        public override void start()
        {
            compStarted = DateTime.Now;
            string[] fileList;
            
            if (name.Equals(Settings.sourceEngineGames))
            {                
                List<string> fileListBuilder = new List<string>();
                
                fileListBuilder.AddRange(Directory.GetFiles(dirSteam, "*.gcf", SearchOption.TopDirectoryOnly));
                fileListBuilder.AddRange(Directory.GetFiles(dirSteam, "*.ncf", SearchOption.TopDirectoryOnly));

                string[] folderList = Directory.GetDirectories(dirSteam, "*", SearchOption.TopDirectoryOnly);
                foreach (string folder in folderList)
                {
                    if (!folder.Contains(@"\" + Utilities.getSteamAppsFolder(dirSteam) + @"\common") &&
                        !folder.Contains(@"\" + Utilities.getSteamAppsFolder(dirSteam) + @"\downloading") &&
                        !folder.Contains(@"\" + Utilities.getSteamAppsFolder(dirSteam) + @"\temp"))
                    {
                        fileListBuilder.AddRange(Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories));
                    }
                }

                fileList = fileListBuilder.ToArray();
            }
            else
            {
                fileList = Directory.GetFiles(dirSteam, "*.*", SearchOption.AllDirectories);
            }

            try
            {
                string libPath = Path.GetDirectoryName(Application.ExecutablePath);
                if (libPath != null)
                {
                    libPath = Path.Combine(libPath, "rsc", "7z.dll");
                    wrapper = new SevenZipWrapper(libPath, dirBackup, false);
                }
                wrapper.Compressing += new EventHandler<ProgressEventArgs>(working);
                wrapper.FileCompressionStarted += new EventHandler<FileNameEventArgs>(started);
                wrapper.CompressionFinished += new EventHandler<EventArgs>(finished);
                if (compressionLzma2)
                {
                    wrapper.UseLzma2Compression = true;
                    wrapper.MultithreadingNumThreads = Settings.lzma2Threads;
                }
                int compressionLevel;
                switch (compLevel)
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
                        compressionLevel = compLevel;
                        break;
                }
                wrapper.CompressionLevel = compressionLevel;
                wrapper.UseMultithreading = true;
                
                wrapper.CompressFiles(Utilities.upDirLvl(dirSteam), fileList);
                wrapper.Dispose(false);
            }
            catch (System.Exception ex)
            {
                ErrorList.add(new ErrorItem(ex.Message, this, ex.StackTrace));
            }
        }

        public override string getSpeedEta()
        {
            if (wrapper != null)
            {
                try
                {
                    UInt64 processedSize;
                    UInt64 totalSize;
                    lock (wrapper)
                    {
                        totalSize = wrapper.TotalSize;
                        processedSize = wrapper.ProcessedSize;
                    }

                    if (totalSize <= 0)
                        totalSize = 2;
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
                catch (Exception)
                {

                }
                
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
            //sevenZip.TempFolderPath = dir + ".tmp";
        }

        public void setCompression(int level)
        {
            compLevel = level;
        }

        public void setLzma2Compression(bool lzma2Compression)
        {
            compressionLzma2 = lzma2Compression;
        }

    }
}
