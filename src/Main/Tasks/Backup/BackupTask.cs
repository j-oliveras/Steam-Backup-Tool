using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using System.Diagnostics;

namespace steamBackup
{
    public class BackupTask : Task
    {
        public bool deleteAll = false;

        protected int compLevel;
        protected bool useLzma2Compression;
        public int getCompLevel(){return compLevel;}

        public BackupTask()
        {
            type = TaskType.BACKUP;
        }
        
        public override int ramUsage(bool useLzma2)
        {
            if (useLzma2)
            {
                float[] ramPerThread = { 1.0f, 4.5f, 16.0f, 148.0f, 292.0f, 553.0f};

                int ramMultiplier = threadCount;
                // if there is more than one thread and the thread count is even and the compression level is higher than 'fast'
                if (threadCount > 1 && threadCount % 2 == 1 && (int)compLevel > 2)
                    ramMultiplier--;

                // times the ramPerThread with the ramMultiplier.
                return (int)(ramMultiplier * ramPerThread[(int)compLevel]);
            }
            else
            {
                int[] ramPerThread = { 1, 8, 19, 192, 376, 709};

                // times the ramPerThread with the number of instances used.
                return (int)(threadCount * ramPerThread[(int)compLevel]);
            }
        }

        internal void setCompLevel(int compressionLevel)
        {
            compLevel = compressionLevel;

            foreach (Job job in list)
            {
                BackupJob bJob = (BackupJob)job;

                bJob.setCompression(compressionLevel);
            }
        }

        internal void setCompMethod(bool useLzma2)
        {
            useLzma2Compression = useLzma2;

            foreach (Job job in list)
            {
                BackupJob bJob = (BackupJob) job;
                bJob.setLzma2Compression(useLzma2Compression);
            }
        }

        public void setEnableUpd(bool achivedOnly)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            foreach (Job job in list)
            {  
                
                
                if (job.name.Equals(Settings.sourceEngineGames))
                {
                    enableJob(job);
                }
                else
                {
                    bool isNewer = false;

                    if (File.Exists(job.getBackupDir()))
                    {
                        DateTime achiveDate = new FileInfo(job.getBackupDir()).LastWriteTimeUtc;
                        string[] fileList = Directory.GetFiles(job.getSteamDir(), "*.*", SearchOption.AllDirectories);

                        foreach (string file in fileList)
                        {
                            DateTime fileDate = new FileInfo(file).LastWriteTimeUtc;

                            if (fileDate.CompareTo(achiveDate) > 0)
                            {
                                isNewer = true;

                                break;
                            }

                            if (sw.ElapsedMilliseconds > 100)
                            {
                                Application.DoEvents();
                                sw.Restart();
                            }
                        }
                    }
                    else
                    {
                        if (achivedOnly)
                            isNewer = false;
                        else
                            isNewer = true;
                    }

                    if (isNewer)
                    {
                        enableJob(job);
                    }
                    else
                    {
                        disableJob(job);
                    }
                }
            }
        }

        public override void scan()
        {
            // Find all of the backed up items and a it to the job list
            
            //scanMisc();
            scanCommonFolders();
        }

        public override void setup()
        {
            // Delete backup if the archive is not being updated (i.e all items are checked)
            if (deleteAll && Directory.Exists(backupDir))
                Directory.Delete(backupDir, true);

            makeConfigFile();

            sharedStart();
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

        private void scanCommonFolders()
        {

            string[] libraries = Utilities.getLibraries(steamDir);

            foreach (string lib in libraries)
            {
                if (Directory.Exists(lib + "common\\"))
                {
                    Dictionary<string, string> acfFiles = new Dictionary<string, string>();
                    buildAcfFileList(acfFiles, lib);

                
                    string[] folders = Directory.GetDirectories(lib + "common\\");
                    foreach (string folder in folders)
                    {
                        
                        string[] splits = folder.Split('\\');
                        string name = splits[splits.Length - 1];

                        TextInfo textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;

                        Job job = new BackupJob();

                        job.name = textInfo.ToTitleCase(name);
                        job.setSteamDir(folder);
                        job.setBackupDir(backupDir + "\\common\\" + name + ".7z");
                        job.status = JobStatus.WAITING;
                        job.acfDir = lib;

                        if (acfFiles.ContainsKey(folder))
                        {
                            job.acfFiles = acfFiles[folder];
                            acfFiles.Remove(folder);
                        }
                        else
                        {
                            job.acfFiles = "";
                        }

                        list.Add(job);
                    }
                }
            }
        }

        private void buildAcfFileList(Dictionary<string, string> acfFiles, string lib)
        {
            string[] acfFileList = Directory.GetFiles(lib, "*.acf", SearchOption.TopDirectoryOnly);

            foreach (string file in acfFileList)
            {
                
                if (!String.IsNullOrEmpty(file))
                {
                    string dir = "";
                    string appId = "";

                    FileInfo fi = new FileInfo(file);
                    StreamReader reader = fi.OpenText();

                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        string[] lineData = line.Split(new string[] { "		" }, StringSplitOptions.RemoveEmptyEntries);

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
                                if (filterAcfDir(str))
                                    dir = Utilities.getFileSystemCasing(str);
                            }
                            else if (data.Equals("appinstalldir"))
                            {
                                i++;
                                string str = lineData[i].Trim('\"');
                                if (!Path.IsPathRooted(str))
                                    str = Path.Combine(lib, "common", str);
                                if (filterAcfDir(str))
                                    dir = Utilities.getFileSystemCasing(str);
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

        public bool filterAcfDir(string acfString)
        {
            if (acfString.Equals(""))
                return false;

            string excludeString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            char[] excludeCharList = excludeString.ToCharArray();
            foreach (char excludeChar in excludeCharList)
            {
                if (acfString.Equals(excludeChar + ":"))
                {
                    return false;
                }
            }

            if (!Directory.Exists(acfString))
                return false;

            return true;
        }

        public void makeConfigFile()
        {
            string configDir = backupDir + "\\config.sbt";
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {

                writer.Formatting = Formatting.Indented;

                writer.WriteComment("DO NOT edit this file! you might destroy the backup!");
                writer.WriteWhitespace(Environment.NewLine);
                writer.WriteStartObject();
                writer.WritePropertyName("Archiver Version");
                writer.WriteValue(archiveVer);
                writer.WritePropertyName("ACF IDs");
                writer.WriteStartObject();

                ConfigFile cfgFile = new ConfigFile();
                // Add already archived apps back into the list if they are not being backed up again (so we don't get any orphaned acf files).
                if (File.Exists(configDir))
                {
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteComment("From older backups");

                    using (StreamReader streamReader = new StreamReader(configDir))
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
                foreach (Job job in list)
                {
                    if (!string.IsNullOrEmpty(job.acfFiles) && job.status == JobStatus.WAITING)
                    {
                        string[] nameSplit = job.getSteamDir().Split('\\');
                        string name = nameSplit[nameSplit.Length - 1];

                        if (cfgFile.AcfIds == null || !cfgFile.AcfIds.ContainsKey(name))
                        {
                            writer.WritePropertyName(name);
                            writer.WriteValue(job.acfFiles);
                        }
                    }
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            
            Directory.CreateDirectory(backupDir);
            File.WriteAllText(configDir, sb.ToString());
            sw.Close();
        }
    }
}
