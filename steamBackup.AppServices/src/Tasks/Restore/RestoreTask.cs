namespace steamBackup.AppServices.Tasks.Restore
{
    using Newtonsoft.Json;
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Jobs.Restore;
    using System;
    using System.IO;
    using System.Threading;
    using System.Reflection;

    public class RestoreTask : Task
    {
        public RestoreTask()
        {
            Type = TaskType.Restore;
        }
        
        public override int RamUsage(bool useLzma2)
        {
            return (useLzma2 ? 1 : ThreadCount) * 40;
        }

        public override void Scan()
        {
            // Find all of the backed up items and a it to the job list

            var files = Directory.GetFiles(Path.Combine(BackupDir, BackupDirectory.Common), "*.7z");
            foreach (var file in files)
            {
                Job job = new RestoreJob(Path.GetFileName(file), SteamDir, BackupDir);

                JobList.Add(job);
            }


            var configDir = Path.Combine(BackupDir, "config.sbt");
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
                            CurrentArchiveVer = cfgFile.ArchiverVersion;
                            foreach (var acfId in cfgFile.AcfIds)
                            {
                                var name = acfId.Key;
                                var ids = acfId.Value;

                                var foundJob = JobList.Find(job => job.Name.Equals(name));

                                if (foundJob == null) continue;

                                foundJob.AcfFiles = ids;
                                foundJob.AcfDir = Path.Combine(SteamDir, Utilities.GetSteamAppsFolder(SteamDir));
                            }
                        }
                    }
                }
            }
        }

        public override void Setup()
        {
            if (CurrentArchiveVer == 1)
                AddMiscItems();

            SharedStart();
        }

        private void AddMiscItems()
        {
            // for legacy backups (version 1)

            Job job = new RestoreJob("steamapps", SteamDir, BackupDir);

            JobList.Insert(0, job);
        }
    }
}
