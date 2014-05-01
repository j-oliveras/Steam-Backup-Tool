namespace steamBackup.AppServices.Tasks.Restore
{
    using Newtonsoft.Json;
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Jobs.Restore;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;

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

            var files = Directory.GetFiles(BackupDir + "\\common\\", "*.7z");
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);

                Job job = new RestoreJob();


                job.Name = name;
                job.SetSteamDir(SteamDir + "\\" + Utilities.GetSteamAppsFolder(SteamDir) + "\\common\\");
                job.SetBackupDir(BackupDir + "\\common\\" + job.Name + ".7z");
                job.Status = JobStatus.Waiting;

                JobList.Add(job);
            }


            var configDir = BackupDir + "\\config.sbt";
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
                                foundJob.AcfDir = SteamDir + "\\" + Utilities.GetSteamAppsFolder(SteamDir) + "\\";
                            }
                        }
                    }
                }
            }

            var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
            foreach (var job in JobList)
                job.Name = textInfo.ToTitleCase(job.Name);

            //// if we are using v2 of the archiver add 'Source Games.7z'
            //if (currentArchiveVer == 2 && File.Exists(backupDir + "\\Source Games.7z"))
            //{
            //    Job job = new RestoreJob();

            //    job.setSteamDir(steamDir + "\\");
            //    job.setBackupDir(backupDir + "\\Source Games.7z");
            //    job.name = Settings.sourceEngineGames;
            //    job.status = JobStatus.WAITING;

            //    list.Insert(0, job);
            //}
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

            Job job = new RestoreJob();

            job.SetBackupDir(BackupDir + "\\");
            job.Name = "steamapps";
            job.Status = JobStatus.Waiting;

            JobList.Insert(0, job);

        }
    }
}
