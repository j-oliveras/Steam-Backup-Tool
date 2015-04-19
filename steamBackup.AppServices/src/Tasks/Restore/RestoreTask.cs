namespace steamBackup.AppServices.Tasks.Restore
{
    using Newtonsoft.Json;
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Jobs.Restore;
    using System;
    using System.IO;
    using System.Threading;
    using System.Reflection;
    using System.ComponentModel;

    public class RestoreTask : Task
    {
        public RestoreTask()
        {
            m_taskType = TaskType.Restore;
        }

        public override void Start()
        {
            if (m_currentArchiveVer == 1)
                AddMiscItems();

            base.Start();
        }
        
        public override int RamUsage(bool useLzma2)
        {
            return (useLzma2 ? 1 : m_threadCount) * 40;
        }

        public override void Scan(BackgroundWorker worker = null)
        {
            // Find all of the backed up items and a it to the job list

            var files = Directory.GetFiles(Path.Combine(m_backupDir, BackupDirectory.Common), "*.7z");
            foreach (var file in files)
            {
                Job job = new RestoreJob(Path.GetFileName(file), m_steamDir, m_backupDir);

                m_jobList.Add(job);
            }


            var configDir = Path.Combine(m_backupDir, "config.sbt");
            if (File.Exists(configDir))
            {
                using (var streamReader = new StreamReader(configDir))
                {
                    ConfigFile cfgFile = null;

                    try
                    {
                        cfgFile = JsonConvert.DeserializeObject<ConfigFile>(streamReader.ReadToEnd());
                    }
                    catch (Exception)
                    {
                        cfgFile = new ConfigFile();
                    }
                    finally
                    {
                        if (cfgFile != null)
                        {
                            m_currentArchiveVer = cfgFile.ArchiverVersion;
                            foreach (var acfId in cfgFile.AcfIds)
                            {
                                var name = acfId.Key;
                                var ids = acfId.Value;

                                var foundJob = (RestoreJob)m_jobList.Find(job => job.m_name.Equals(name));

                                if (foundJob == null) continue;

                                foundJob.addAcfInfo(ids, Path.Combine(m_steamDir, Utilities.GetSteamAppsFolder(m_steamDir)));
                            }
                        }
                    }
                }
            }
        }

        private void AddMiscItems()
        {
            // for legacy backups (version 1)

            Job job = new RestoreJob("steamapps", m_steamDir, m_backupDir);

            m_jobList.Insert(0, job);
        }
    }
}
