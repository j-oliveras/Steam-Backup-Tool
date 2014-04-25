namespace steamBackup.AppServices.Tasks.Backup
{
    using System.Linq;
    using Newtonsoft.Json;
    using steamBackup.AppServices;
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Jobs.Backup;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;

    public class BackupTask : Task
    {
        public bool DeleteAll = false;

        protected int CompLevel;
        protected bool UseLzma2Compression;

        public int GetCompLevel()
        {
            return CompLevel;
        }

        public BackupTask()
        {
            Type = TaskType.Backup;
        }
        
        public override int RamUsage(bool useLzma2)
        {
            if (useLzma2)
            {
                float[] ramPerThread = { 1.0f, 4.5f, 16.0f, 148.0f, 292.0f, 553.0f};

                int ramMultiplier = ThreadCount;
                // if there is more than one thread and the thread count is even and the compression level is higher than 'fast'
                if (ThreadCount > 1 && ThreadCount % 2 == 1 && CompLevel > 2)
                    ramMultiplier--;

                // times the ramPerThread with the ramMultiplier.
                return (int)(ramMultiplier * ramPerThread[CompLevel]);
            }
            else
            {
                int[] ramPerThread = { 1, 8, 19, 192, 376, 709};

                // times the ramPerThread with the number of instances used.
                return (ThreadCount * ramPerThread[CompLevel]);
            }
        }

        public void SetCompLevel(int compressionLevel)
        {
            CompLevel = compressionLevel;

            foreach (Job job in JobList)
            {
                var bJob = (BackupJob)job;

                bJob.SetCompression(compressionLevel);
            }
        }

        public void SetCompMethod(bool useLzma2)
        {
            UseLzma2Compression = useLzma2;

            foreach (Job job in JobList)
            {
                var bJob = (BackupJob) job;
                bJob.SetLzma2Compression(UseLzma2Compression);
            }
        }

        public void SetEnableUpd(bool achivedOnly)
        {
            foreach (Job job in JobList)
            {  
                if (job.Name.Equals(Settings.SourceEngineGames))
                {
                    EnableJob(job);
                }
                else
                {
                    bool isNewer = false;

                    if (File.Exists(job.GetBackupDir()))
                    {
                        DateTime achiveDate = new FileInfo(job.GetBackupDir()).LastWriteTimeUtc;
                        string[] fileList = Directory.GetFiles(job.GetSteamDir(), "*.*", SearchOption.AllDirectories);

                        if (fileList.Select(file => new FileInfo(file).LastWriteTimeUtc).Any(fileDate => fileDate.CompareTo(achiveDate) > 0))
                        {
                            isNewer = true;
                        }
                    }
                    else
                    {
                        if (!achivedOnly)
                            isNewer = true;
                    }

                    if (isNewer)
                    {
                        EnableJob(job);
                    }
                    else
                    {
                        DisableJob(job);
                    }
                }
            }
        }

        public override void Scan()
        {
            // Find all of the backed up items and a it to the job list
            
            //scanMisc();
            ScanCommonFolders();
        }

        public override void Setup()
        {
            // Delete backup if the archive is not being updated (i.e all items are checked)
            if (DeleteAll && Directory.Exists(BackupDir))
                Directory.Delete(BackupDir, true);

            MakeConfigFile();

            SharedStart();
        }

        //private void scanMisc()
        //{
        //    // Add misc backup

        //    Job job = new BackupJob();

        //    job.name = Settings.sourceEngineGames;
        //    job.setSteamDir(steamDir + "\\" + Utilities.getSteamAppsFolder(steamDir) + "\\");
        //    job.setBackupDir(backupDir + "\\Source Games.7z");
        //    job.status = JobStatus.WAITING;

        //    list.Add(job);
        //}

        private void ScanCommonFolders()
        {

            string[] libraries = Utilities.GetLibraries(SteamDir);

            foreach (string lib in libraries)
            {
                if (Directory.Exists(lib + "common\\"))
                {
                    var acfFiles = new Dictionary<string, string>();
                    BuildAcfFileList(acfFiles, lib);

                
                    string[] folders = Directory.GetDirectories(lib + "common\\");
                    foreach (string folder in folders)
                    {
                        
                        string[] splits = folder.Split('\\');
                        string name = splits[splits.Length - 1];

                        TextInfo textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;

                        Job job = new BackupJob();

                        job.Name = textInfo.ToTitleCase(name);
                        job.SetSteamDir(folder);
                        job.SetBackupDir(BackupDir + "\\common\\" + name + ".7z");
                        job.Status = JobStatus.Waiting;
                        job.AcfDir = lib;

                        if (acfFiles.ContainsKey(folder))
                        {
                            job.AcfFiles = acfFiles[folder];
                            acfFiles.Remove(folder);
                        }
                        else
                        {
                            job.AcfFiles = "";
                        }

                        JobList.Add(job);
                    }
                }
            }
        }

        private void BuildAcfFileList(Dictionary<string, string> acfFiles, string lib)
        {
            string[] acfFileList = Directory.GetFiles(lib, "*.acf", SearchOption.TopDirectoryOnly);

            foreach (string file in acfFileList)
            {
                
                if (!String.IsNullOrEmpty(file))
                {
                    string dir = "";
                    string appId = "";

                    var fi = new FileInfo(file);
                    StreamReader reader = fi.OpenText();

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        string[] lineData = line.Split(new[] { "		" }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < lineData.Length; i++)
                        {
                            string data = lineData[i].Trim('\"');

                            if (data.Equals("appID"))
                            {
                                i++;
                                appId = lineData[i].Trim('\"');
                            }
                            else if (data.Equals("installdir"))
                            {
                                i++;
                                string str = lineData[i].Trim('\"').Replace("\\\\", "\\");
                                if (!Path.IsPathRooted(str))
                                    str = Path.Combine(lib, "common", str);
                                if (FilterAcfDir(str))
                                    dir = Utilities.GetFileSystemCasing(str);
                            }
                            else if (data.Equals("appinstalldir"))
                            {
                                i++;
                                string str = lineData[i].Trim('\"');
                                if (!Path.IsPathRooted(str))
                                    str = Path.Combine(lib, "common", str);
                                if (FilterAcfDir(str))
                                    dir = Utilities.GetFileSystemCasing(str);
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(dir) && !String.IsNullOrEmpty(appId))
                    {
                        if (acfFiles.ContainsKey(dir))
                            acfFiles[dir] += "|" + appId;
                        else
                            acfFiles.Add(dir, appId);
                    }
                }
            }
        }

        public bool FilterAcfDir(string acfString)
        {
            if (acfString.Equals(""))
                return false;

            const string excludeString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            char[] excludeCharList = excludeString.ToCharArray();

            if (excludeCharList.Any(excludeChar => acfString.Equals(excludeChar + ":")))
            {
                return false;
            }

            if (!Directory.Exists(acfString))
                return false;

            return true;
        }

        public void MakeConfigFile()
        {
            string configDir = BackupDir + "\\config.sbt";
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {

                writer.Formatting = Formatting.Indented;

                writer.WriteComment("DO NOT edit this file! you might destroy the backup!");
                writer.WriteWhitespace(Environment.NewLine);
                writer.WriteStartObject();
                writer.WritePropertyName("Archiver Version");
                writer.WriteValue(ArchiveVer);
                writer.WritePropertyName("ACF IDs");
                writer.WriteStartObject();

                var cfgFile = new ConfigFile();
                // Add already archived apps back into the list if they are not being backed up again (so we don't get any orphaned acf files).
                if (File.Exists(configDir))
                {
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteComment("From older backups");

                    using (var streamReader = new StreamReader(configDir))
                    {
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
                            foreach (KeyValuePair<string, string> acfId in cfgFile.AcfIds)
                            {
                                writer.WritePropertyName(acfId.Key);
                                writer.WriteValue(acfId.Value);
                            }
                        }
                    }
                }

                // Add new apps to the list
                writer.WriteWhitespace(Environment.NewLine);
                writer.WriteComment("From latest backup");
                foreach (Job job in JobList)
                {
                    if (!string.IsNullOrEmpty(job.AcfFiles) && job.Status == JobStatus.Waiting)
                    {
                        string[] nameSplit = job.GetSteamDir().Split('\\');
                        string name = nameSplit[nameSplit.Length - 1];

                        if (cfgFile.AcfIds == null || !cfgFile.AcfIds.ContainsKey(name))
                        {
                            writer.WritePropertyName(name);
                            writer.WriteValue(job.AcfFiles);
                        }
                    }
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            
            Directory.CreateDirectory(BackupDir);
            File.WriteAllText(configDir, sb.ToString());
            sw.Close();
        }
    }
}
