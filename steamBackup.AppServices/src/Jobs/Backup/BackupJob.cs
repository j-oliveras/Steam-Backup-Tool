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
        private bool m_compIsLzma2;
        private int m_compLevel = 5;
        private int m_lzma2Threads;

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

        public override void Start()
        {
            base.Start();

            var fileList = Directory.GetFiles(m_steamDir, "*.*", SearchOption.AllDirectories);

            try
            {
                m_wrapper = new SevenZipWrapper(m_backupDir, false);

                m_wrapper.Compressing += Working;
                m_wrapper.FileCompressionStarted += Started;
                m_wrapper.CompressionFinished += Finished;

                if (m_compIsLzma2)
                {
                    m_wrapper.UseLzma2Compression = true;
                    m_wrapper.MultithreadingNumThreads = m_lzma2Threads;
                }

                int compressionLevel;
                switch (m_compLevel)
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
                        compressionLevel = m_compLevel;
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

        public void SetCompression(int level)
        {
            m_compLevel = level;
        }

        public void SetLzma2Compression(bool lzma2Compression)
        {
            m_compIsLzma2 = lzma2Compression;
        }

        public void SetLzma2Threads(int threads)
        {
            m_lzma2Threads = threads;
        }
    }
}
