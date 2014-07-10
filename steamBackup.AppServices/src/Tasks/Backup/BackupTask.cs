namespace steamBackup.AppServices.Tasks.Backup
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using steamBackup.AppServices;
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Jobs.Backup;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;

    public class BackupTask : Task
    {
        public bool DeleteAll = false;

        protected int CompLevel;
        protected bool UseLzma2Compression;
        protected int Lzma2Threads;

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

                var ramMultiplier = ThreadCount;
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

            foreach (var bJob in JobList.Cast<BackupJob>())
            {
                bJob.SetCompression(compressionLevel);
            }
        }

        public void SetCompMethod(bool useLzma2)
        {
            UseLzma2Compression = useLzma2;

            foreach (var bJob in JobList.Cast<BackupJob>())
            {
                bJob.SetLzma2Compression(UseLzma2Compression);
            }
        }

        public void SetLzma2Threads(int threads)
        {
            Lzma2Threads = threads;

            foreach (var bJob in JobList.Cast<BackupJob>())
            {
                bJob.SetLzma2Threads(threads);
            }
        }

        public void SetEnableUpd(bool achivedOnly)
        {
            foreach (var job in JobList)
            {
                if (File.Exists(job.GetBackupDir()))
                {
                    var achiveDate = new FileInfo(job.GetBackupDir()).LastWriteTimeUtc;
                    var fileList = Directory.GetFiles(job.GetSteamDir(), "*.*", SearchOption.AllDirectories);

                    if (
                        fileList.Select(file => new FileInfo(file).LastWriteTimeUtc)
                            .Any(fileDate => fileDate.CompareTo(achiveDate) > 0))
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

        private void ScanCommonFolders()
        {

            var libraries = Utilities.GetLibraries(SteamDir);

            foreach (var lib in libraries)
            {
                var commonDir = Path.Combine(lib, SteamDirectory.Common);

                if (!Directory.Exists(commonDir)) continue;

                var acfFiles = new Dictionary<string, string>();
                BuildAcfFileList(acfFiles, lib);

                var folders = Directory.GetDirectories(commonDir);
                foreach (var folder in folders)
                {
                    Job job = new BackupJob(folder, SteamDir, BackupDir, lib, acfFiles);

                    JobList.Add(job);
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
            var configDir = Path.Combine(BackupDir, "config.sbt");
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
                foreach (var job in JobList)
                {
                    if (string.IsNullOrEmpty(job.AcfFiles) || job.Status != JobStatus.Waiting) continue;

                    var name = Path.GetFileName(job.GetSteamDir()) ?? string.Empty;

                    if (cfgFile.AcfIds != null && cfgFile.AcfIds.ContainsKey(name)) continue;

                    writer.WritePropertyName(name);
                    writer.WriteValue(job.AcfFiles);
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
