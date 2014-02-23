using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace steamBackup
{
    public class RestoreTask : Task
    {
        public RestoreTask()
        {
            type = TaskType.RESTORE;
        }
        
        public override int ramUsage(bool useLzma2)
        {
            return (useLzma2 ? 1 : threadCount) * 40;
        }

        public override void scan()
        {
            // Find all of the backed up items and a it to the job list

            string[] files = Directory.GetFiles(backupDir + "\\common\\", "*.7z");
            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);

                Job job = new RestoreJob();


                job.name = name;
                job.setSteamDir(steamDir + "\\" + Utilities.getSteamAppsFolder(steamDir) + "\\common\\");
                job.setBackupDir(backupDir + "\\common\\" + job.name + ".7z");
                job.status = JobStatus.WAITING;

                list.Add(job);
            }


            string configDir = backupDir + "\\config.sbt";
            if (File.Exists(configDir))
            {
                using (StreamReader streamReader = new StreamReader(configDir))
                {
                    ConfigFile cfgFile = new ConfigFile();

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
                        if (!string.IsNullOrEmpty(cfgFile.ArchiverVersion))
                            currentArchiveVer = Convert.ToInt32(cfgFile.ArchiverVersion);

                        foreach (KeyValuePair<string, string> acfId in cfgFile.AcfIds)
                        {
                            string name = acfId.Key;
                            string Ids = acfId.Value;

                            Job foundJob = list.Find(job => job.name.Equals(name));

                            if (foundJob != null)
                            {
                                foundJob.acfFiles = Ids;
                                foundJob.acfDir = steamDir + "\\" + Utilities.getSteamAppsFolder(steamDir) + "\\";
                            }
                        }
                    }
                }
            }

            TextInfo textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
            foreach (Job job in list)
                job.name = textInfo.ToTitleCase(job.name);

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

        public override void setup()
        {
            if (currentArchiveVer == 1)
                addMiscItems();

            sharedStart();
        }

        private void addMiscItems()
        {
            // for legacy backups (version 1)

            Job job = new RestoreJob();

            job.setBackupDir(backupDir + "\\");
            job.name = "steamapps";
            job.status = JobStatus.WAITING;

            list.Insert(0, job);

        }
    }
}
