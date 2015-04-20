namespace steamBackup.AppServices
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using steamBackup.AppServices;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.ComponentModel;

    public class BackupTask : Task
    {
        public bool m_deleteAll = false;

        public int m_compLevel;
        public bool m_useLzma2;
        public int m_lzma2Threads;

        public BackupTask()
        {
            m_taskType = TaskType.Backup;
        }

        public override void Start()
        {
            // Delete backup if the archive is not being updated (i.e all items are checked)
            if (m_deleteAll && Directory.Exists(m_backupDir))
                Directory.Delete(m_backupDir, true);

            MakeConfigFile();

            base.Start();
        }

        public int GetCompLevel()
        {
            return m_compLevel;
        }
        
        public override int RamUsage(bool useLzma2)
        {
            if (useLzma2)
            {
                float[] ramPerThread = { 1.0f, 4.5f, 16.0f, 148.0f, 292.0f, 553.0f};

                var ramMultiplier = m_threadCount;
                // if there is more than one thread and the thread count is even and the compression level is higher than 'fast'
                if (m_threadCount > 1 && m_threadCount % 2 == 1 && m_compLevel > 2)
                    ramMultiplier--;

                // times the ramPerThread with the ramMultiplier.
                return (int)(ramMultiplier * ramPerThread[m_compLevel]);
            }
            else
            {
                int[] ramPerThread = { 1, 8, 19, 192, 376, 709};

                // times the ramPerThread with the number of instances used.
                return (m_threadCount * ramPerThread[m_compLevel]);
            }
        }

        public void SetEnableUpd(bool achivedOnly)
        {
            foreach (var job in m_jobList)
            {
                if (File.Exists(job.m_backupDir))
                {
                    if (job.m_steamFileDate.CompareTo(job.m_backupFileDate) > 0)
                    {
                        EnableJob(job);
                        continue;
                    }
                }
                else
                {
                    if (!achivedOnly)
                    {
                        EnableJob(job);
                        continue;
                    }
                }

                DisableJob(job);
            }
        }

        public override void Scan(BackgroundWorker worker = null)
        {
            // Find all of the backed up items and a it to the job list
            
            //scanMisc();
            ScanCommonFolders(worker);
        }

        private void ScanCommonFolders(BackgroundWorker worker = null)
        {

            var libraries = Utilities.GetLibraries(m_steamDir);

            foreach (var lib in libraries)
            {
                var commonDir = Path.Combine(lib, SteamDirectory.Common);

                if (!Directory.Exists(commonDir)) continue;

                var acfFiles = new Dictionary<string, string>();
                BuildAcfFileList(acfFiles, lib);

                int count = 0;
                var folders = Directory.GetDirectories(commonDir);
                m_jobList.Capacity += folders.Length;
                foreach (var folder in folders)
                {
                    Job job = new BackupJob(folder, m_backupDir, lib, acfFiles);

                    m_jobList.Add(job);

                    if (worker != null)
                        worker.ReportProgress((int)((float)count / folders.Count() * 100));
                    count++;
                }
            }
        }

        private void BuildAcfFileList(Dictionary<string, string> acfFiles, string lib)
        {
            var acfFileList = Directory.GetFiles(lib, "*.acf", SearchOption.TopDirectoryOnly);

            // match text line containing "appID"   "<ID>" into named backreference "appId"
            var regExId = new Regex("\"appID\"\\s*?\"(?<appId>\\d*?)\"", RegexOptions.Multiline);

            // match text line containing "installdir"   "<directory>" into named backreference "installDir"
            var regExInstallDir = new Regex("\"installdir\"\\s*?\"(?<installDir>[-\\\\+_:\\w\\s]*?)\"", RegexOptions.Multiline);

            // match text line containing "appinstalldir"   "<directory>" into named backreference "appInstallDir"
            var regExAppInstallDir = new Regex("\"appinstalldir\"\\s*?\"(?<appInstallDir>[-\\\\+_:\\w\\s]*?)\"", RegexOptions.Multiline);

            foreach (var file in acfFileList)
            {
                if (String.IsNullOrEmpty(file)) continue;

                var dir = string.Empty;
                var appId = string.Empty;

                using (var reader = new FileInfo(file).OpenText())
                {
                    var content = reader.ReadToEnd();

                    var installDir = string.Empty;
                    var installAppDir = string.Empty;

                    var regExMatch = regExId.Match(content);
                    if (regExMatch.Success)
                    {
                        appId = regExMatch.Groups["appId"].Value;
                    }

                    regExMatch = regExInstallDir.Match(content);
                    if (regExMatch.Success)
                    {
                        installDir = regExMatch.Groups["installDir"].Value;
                    }

                    regExMatch = regExAppInstallDir.Match(content);
                    if (regExMatch.Success)
                    {
                        installAppDir = regExMatch.Groups["appInstallDir"].Value;
                    }

                    if (!string.IsNullOrEmpty(installDir))
                        dir = installDir;
                    else if (!string.IsNullOrEmpty(installAppDir))
                        dir = installAppDir;
                }

                if (!string.IsNullOrEmpty(dir))
                    dir = Path.Combine(lib, SteamDirectory.Common, dir);
                else continue;

                if (FilterAcfDir(dir))
                    dir = Utilities.GetFileSystemCasing(dir);

                if (String.IsNullOrEmpty(appId)) continue;

                if (acfFiles.ContainsKey(dir))
                    acfFiles[dir] += "|" + appId;
                else
                    acfFiles.Add(dir, appId);
            }
        }

        public bool FilterAcfDir(string acfString)
        {
            if (string.IsNullOrEmpty(acfString))
                return false;

            const string excludeString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var excludeCharList = excludeString.ToCharArray();

            return !excludeCharList.Any(excludeChar => acfString.Equals(excludeChar + ":")) && Directory.Exists(acfString);
        }

        public void MakeConfigFile()
        {
            var configDir = Path.Combine(m_backupDir, "config.sbt");
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {

                writer.Formatting = Formatting.Indented;

                writer.WriteComment("DO NOT edit this file! you might destroy the backup!");
                writer.WriteWhitespace(Environment.NewLine);
                writer.WriteStartObject();
                writer.WritePropertyName("Archiver Version");
                writer.WriteValue(m_archiveVer);
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
                            foreach (var acfId in cfgFile.AcfIds)
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
                foreach (var job in m_jobList)
                {
                    if (string.IsNullOrEmpty(job.m_acfFiles) || job.m_status != JobStatus.Waiting) continue;

                    var name = Path.GetFileName(job.m_steamDir) ?? string.Empty;

                    if (cfgFile.AcfIds != null && cfgFile.AcfIds.ContainsKey(name)) continue;

                    writer.WritePropertyName(name);
                    writer.WriteValue(job.m_acfFiles);
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            
            Directory.CreateDirectory(m_backupDir);
            File.WriteAllText(configDir, sb.ToString());
            sw.Close();
        }
    }
}
