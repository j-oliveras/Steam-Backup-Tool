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
            base.Start();

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
    }
}
