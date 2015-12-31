namespace steamBackup.AppServices
{
    using steamBackup.AppServices;
    using steamBackup.AppServices.Properties;
    using System;
    using System.IO;
    using System.Threading;
    using System.Reflection;
    using System.Collections.Generic;

    class BackupJob : Job
    {
        private BackupJob() { }

        public BackupJob(string steamDir, string backupDir, string library, Dictionary<string, string> acfFiles)
        {
            m_type = JobType.Backup;

            var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
            var name = Path.GetFileName(steamDir) ?? string.Empty;
            m_name = textInfo.ToTitleCase(name);

            m_steamDir = steamDir;

            var dirInfo = new  DirectoryInfo(m_steamDir);
            foreach (FileInfo fileInfo in dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories))
            {
                m_steamFileSize += fileInfo.Length;
                if(fileInfo.LastWriteTimeUtc.CompareTo(m_steamFileDate) > 0)
                    m_steamFileDate = fileInfo.LastWriteTimeUtc;
            }

            m_backupDir = Path.Combine(backupDir, BackupDirectory.Common, name + ".7z");

            FileInfo backupFileInfo = new FileInfo(m_backupDir);
            m_backupFileDate = backupFileInfo.LastWriteTimeUtc;
            m_backupFileSize = backupFileInfo.Exists ? backupFileInfo.Length : 0;

            var driveLetter = Path.GetPathRoot(steamDir).Substring(0, 1);
            if (driveLetter != "C")
            {
                m_name += " (" + driveLetter + ")";
            }
            m_name += " Size: " + BytesToString(m_steamFileSize);

            m_status = JobStatus.Waiting;
            m_acfDir = library;
            m_acfFiles = GetAcfFiles(acfFiles);
        }

        ~BackupJob()
        {
            if (m_wrapper != null)
            {
                m_wrapper.Dispose(false);
                m_wrapper = null;
            }
        }

        public override void Start(Task parentTask)
        {
            base.Start(parentTask);
            BackupTask parentBackupTask = (BackupTask)parentTask;

            var fileList = Directory.GetFiles(m_steamDir, "*.*", SearchOption.AllDirectories);

            try
            {
                m_wrapper = new SevenZipWrapper(m_backupDir, false);

                m_wrapper.Compressing += Working;
                m_wrapper.FileCompressionStarted += Started;
                m_wrapper.CompressionFinished += Finished;

                if (parentBackupTask.m_useLzma2)
                {
                    m_wrapper.UseLzma2Compression = true;
                    m_wrapper.MultithreadingNumThreads = parentBackupTask.m_lzma2Threads;
                }

                int compressionLevel;
                switch (parentBackupTask.m_compLevel)
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
                        compressionLevel = parentBackupTask.m_compLevel;
                        break;
                }

                m_wrapper.CompressionLevel = compressionLevel;
                m_wrapper.UseMultithreading = true;

                m_wrapper.CompressFiles(Utilities.UpDirLvl(m_steamDir), fileList);
                m_wrapper.Dispose(false);
            }
            catch (Exception ex)
            {
                ErrorList.Add(new ErrorItem(ex.Message, this, ex.StackTrace));
            }
        }

        private string GetAcfFiles(Dictionary<string, string> acfFiles)
        {
            string acfStr = null;

            if (acfFiles.ContainsKey(m_steamDir))
            {
                acfStr = acfFiles[m_steamDir];
                acfFiles.Remove(m_steamDir);
            }
            else
            {
                acfStr = "";
            }

            return acfStr;
        }

        private string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
    }
}
