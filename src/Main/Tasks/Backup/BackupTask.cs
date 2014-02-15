using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using SevenZip;

namespace steamBackup
{
    public class BackupTask : Task
    {
        public bool deleteAll = false;

        protected CompressionLevel compLevel;
        protected CompressionMethod compMethod;
        public CompressionLevel getCompLevel(){return compLevel;}

        public BackupTask()
        {
            type = TaskType.BACKUP;
        }
        
        public override int ramUsage(bool useLzma2)
        {
            int ramPerThread = 0;
            bool modifiedSevenZip = Utilities.getSevenZipRelease() > 64;

            int cpuCount;
            if (modifiedSevenZip && useLzma2)
                cpuCount = threadCount;
            else 
                cpuCount = Environment.ProcessorCount;

            int cpuCountEven = cpuCount;

            if (cpuCount > 1 && cpuCount%2 != 0)
                cpuCountEven--;

            switch ((int)compLevel)
            {
                case 5:
                    if (!useLzma2 || (useLzma2 && cpuCountEven == 2))
                        ramPerThread = 709;
                    else
                    {
                        ramPerThread = (int) Math.Ceiling(cpuCountEven*553f);
                    }
                    break;
                case 4:
                    if (!useLzma2)
                        ramPerThread = 376;
                    else
                    {
                        ramPerThread = (int)Math.Ceiling(cpuCountEven * 292f);
                    }
                    break;
                case 3:
                    if (!useLzma2)
                        ramPerThread = 192;
                    else
                    {
                        ramPerThread = (int) Math.Ceiling(cpuCountEven*148f);
                    }
                    break;
                case 2: 
                    if (!useLzma2)
                        ramPerThread = 19;
                    else
                    {
                        ramPerThread = (int) Math.Ceiling(cpuCount*16f);
                    }
                    break;
                case 1:
                    if (!useLzma2)
                        ramPerThread = 6;
                    else
                    {
                        ramPerThread = (int) Math.Ceiling(cpuCount*4.66f);
                    }
                    break;
                case 0:
                    ramPerThread = 1;
                    break;
                default:
                    return -1;
            }

            return (useLzma2 ? 1 : (threadCount)) * ramPerThread;
        }

        internal void setCompLevel(CompressionLevel compressionLevel)
        {
            compLevel = compressionLevel;

            foreach (Job job in list)
            {
                BackupJob bJob = (BackupJob)job;

                bJob.setCompression(compressionLevel);
            }
        }

        internal void setCompMethod(CompressionMethod compressionMethod)
        {
            compMethod = compressionMethod;

            foreach (Job job in list)
            {
                BackupJob bJob = (BackupJob) job;
                bJob.setCompressionMethod(compMethod);
            }
        }

        public void setEnableUpd(CheckedListBox chkList, bool achivedOnly)
        {
            chkList.Items.Clear();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            foreach (Job job in list)
            {  
                
                
                if (job.name.Equals(Settings.sourceEngineGames))
                {
                    enableJob(job);

                    bool enabled = false;
                    if (job.status == JobStatus.WAITING)
                        enabled = true;

                    chkList.Items.Add(job.name, enabled);
                    chkList.Refresh();
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
                        chkList.Items.Add(job.name, true);
                    }
                    else
                    {
                        disableJob(job);
                        chkList.Items.Add(job.name, false);
                    }

                    chkList.Refresh();
                }
            }
        }

        public override void scan()
        {
            // Find all of the backed up items and a it to the job list
            
            scanMisc();
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

        private void scanMisc()
        {
            // Add misc backup

            Job job = new BackupJob();

            job.name = Settings.sourceEngineGames;
            job.setSteamDir(steamDir + "\\steamapps\\");
            job.setBackupDir(backupDir + "\\Source Games.7z");
            job.status = JobStatus.WAITING;

            list.Add(job);
        }

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
                // Add already archived apps back into the list if they are not being backed up again (so we don't get any orphaned acf files).
                if (File.Exists(configDir))
                {
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteComment("From older backups");
                    StreamReader streamReader = new StreamReader(configDir);
                    JsonTextReader reader = new JsonTextReader(new StringReader(streamReader.ReadToEnd()));

                    while (reader.Read())
                    {
                        if (reader.Value != null)
                        {
                            if (reader.TokenType.ToString() == "PropertyName" && reader.Value.ToString() == "ACF IDs")
                            {
                                reader.Read();
                                do
                                {
                                    while (reader.TokenType.ToString() != "PropertyName")
                                    {
                                        if (reader.TokenType.ToString() == "EndObject")
                                            goto Finish;
                                        reader.Read();
                                    }

                                    writer.WritePropertyName(reader.Value.ToString());
                                    reader.Read();
                                    writer.WriteValue(reader.Value.ToString());
                                    reader.Read();
                                } while (reader.TokenType.ToString() != "EndObject");
                            }
                        }
                    }
                Finish:
                    streamReader.Close();
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

                        if (!sb.ToString().Contains(name))
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
