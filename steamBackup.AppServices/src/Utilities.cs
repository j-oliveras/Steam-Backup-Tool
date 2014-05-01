﻿namespace steamBackup.AppServices
{
    using System.Security;
    using Microsoft.Win32;
    using Properties;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using steamBackup.AppServices.Jobs;

    public static class Utilities
    {
        // General
        public static string GetFileSystemCasing(string path)
        {
            if (!Path.IsPathRooted(path)) throw new ArgumentException(Resources.AbsolutePathExceptionText);

            path = path.TrimEnd(Path.DirectorySeparatorChar); // if you type c:\foo\ instead of c:\foo
            try
            {
                var name = Path.GetFileName(path);
                if (String.IsNullOrEmpty(name)) return path.ToUpper() + Path.DirectorySeparatorChar; // root reached

                var parent = Path.GetDirectoryName(path); // retrieving parent of element to be corrected
                parent = GetFileSystemCasing(parent); //to get correct casing on the entire string, and not only on the last element

                return new DirectoryInfo(parent).GetFileSystemInfos(name).First().FullName; // coming from GetFileSystemImfos() this has the correct case
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message); 
                throw new ArgumentException(Resources.InvalidPathExceptionText);
            }
        }

        public static bool FolderExists(string dir, string folder)
        {
            // This does a case sensitive check (Other solutions are non-case sensitive)
            var folderList = Directory.GetDirectories(dir);

            return folderList.Any(folderToTest => folderToTest.Equals(Path.Combine(dir, folder)));
        }

        public static string GetSteamAppsFolder(string steamDir)
        {
            if (FolderExists(steamDir , "steamapps"))
                return "steamapps";

            return FolderExists(steamDir, "SteamApps") ? "SteamApps" : "ERROR";
        }

        public static string[] GetLibraries(string steamDir)
        {
            var libraryLocation = Path.Combine(steamDir, GetSteamAppsFolder(steamDir));

            var fi = new FileInfo(Path.Combine(steamDir, SteamDirectory.Common, "config.vdf"));
            var reader = fi.OpenText();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                var lineData = line.Split(new[] { "		" }, StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < lineData.Length; i++)
                {
                    var data = lineData[i].Trim('\"');

                    if (!data.Contains("BaseInstallFolder_")) continue;

                    i++;
                    var dir = lineData[i].Trim('\"').Replace("\\\\", "\\") + "\\SteamApps\\";
                    if (!String.IsNullOrEmpty(dir) && Directory.Exists(dir))
                        libraryLocation += "|" + dir;
                }
            }

            return libraryLocation.Split('|');
        }

        public static string UpDirLvl(string dir)
        {
            return Path.GetDirectoryName(dir);
        }

        public static bool IsSteamRunning()
        {
            var pname = Process.GetProcessesByName("Steam");
            return pname.Length != 0 && Settings.CheckSteamRun;
        }

        /// <summary>
        /// Copy .acf files from steam to backup directory
        /// </summary>
        /// <param name="job">Processed job</param>
        /// <param name="backupDir">Backup directory</param>
        public static void CopyAcfToBackup(Job job, string backupDir)
        {
            if (String.IsNullOrEmpty(job.AcfFiles)) return;

            var acfId = job.AcfFiles.Split('|');

            foreach(var id in acfId)
            {
                var src = Path.Combine(job.AcfDir, "appmanifest_" + id + ".acf");
                var dst = Path.Combine(backupDir, BackupDirectory.Acf);

                if (!Directory.Exists(dst))
                    Directory.CreateDirectory(dst);

                var fi = new FileInfo(src);
                var reader = fi.OpenText();

                var acf = reader.ReadToEnd();
                var gameCommonFolder = UpDirLvl(job.GetSteamDir());
                acf = acf.Replace(gameCommonFolder, "|DIRECTORY-STD|");
                acf = acf.Replace(gameCommonFolder.ToLower(), "|DIRECTORY-LOWER|");
                acf = acf.Replace(gameCommonFolder.ToLower().Replace("\\", "\\\\"), "|DIRECTORY-ESCSLASH-LOWER|");

                File.WriteAllText(Path.Combine(dst, "appmanifest_" + id + ".acf"), acf);
                reader.Close();
            }
        }

        /// <summary>
        /// Copies .acf files from the Backup to steam install
        /// </summary>
        /// <param name="job">Processed job</param>
        /// <param name="backupDir">Backup directory</param>
        public static void CopyAcfToRestore(Job job, string backupDir)
        {
            if (String.IsNullOrEmpty(job.AcfFiles)) return;

            var acfId = job.AcfFiles.Split('|');

            foreach (var id in acfId)
            {
                var src = Path.Combine(backupDir, BackupDirectory.Acf, "appmanifest_" + id + ".acf");
                var dst = job.AcfDir;

                if (!Directory.Exists(dst))
                    Directory.CreateDirectory(dst);

                var fi = new FileInfo(src);
                var reader = fi.OpenText();

                var acf = reader.ReadToEnd();
                var gameCommonFolder = Path.Combine(job.AcfDir, SteamDirectory.Common);

                acf = acf.Replace("|DIRECTORY-STD|", gameCommonFolder);
                acf = acf.Replace("|DIRECTORY-LOWER|", gameCommonFolder.ToLower());
                acf = acf.Replace("|DIRECTORY-ESCSLASH-LOWER|", gameCommonFolder.ToLower().Replace("\\", "\\\\"));

                File.WriteAllText(Path.Combine(dst, "appmanifest_" + id + ".acf"), acf);
                reader.Close();
            }
        }

        /// <summary>
        /// Check to see if a steam install directory is valid
        /// </summary>
        /// <param name="steamDir">Steam directory</param>
        /// <returns></returns>
        public static bool IsValidSteamFolder(string steamDir)
        {
            return File.Exists(Path.Combine(steamDir, SteamDirectory.Config, "config.vdf"));
        }

        /// <summary>
        /// Check to see if a backup directory is valid
        /// </summary>
        /// <param name="backupDir">Backup directory</param>
        /// <returns></returns>
        public static bool IsValidBackupFolder(string backupDir)
        {
            if(File.Exists(Path.Combine(backupDir, "config.sbt")))
            {
                // Valid Archiver Version 2
                return true;
            }

            return Directory.Exists(Path.Combine(backupDir, BackupDirectory.Common)) && 
                   File.Exists(Path.Combine(backupDir, "games.7z")) &&
                   File.Exists(Path.Combine(backupDir, "steamapps.7z"));
        }

        /// <summary>
        /// Create Backup folders if needed
        /// </summary>
        /// <param name="backupDir">Target Backup directory</param>
        public static void SetupBackupDirectory(string backupDir)
        {
            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);

            if (!Directory.Exists(Path.Combine(backupDir, BackupDirectory.Common)))
                Directory.CreateDirectory(Path.Combine(backupDir, BackupDirectory.Common));

            if (!Directory.Exists(Path.Combine(backupDir, BackupDirectory.Acf)))
                Directory.CreateDirectory(Path.Combine(backupDir, BackupDirectory.Acf));
        }

        /// <summary>
        /// Get Steam directory from registry
        /// </summary>
        /// <returns></returns>
        public static string GetSteamDirectory()
        {
            const string keyStr = @"Software\Valve\Steam";
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(keyStr, false);
                if (key != null)
                    return GetFileSystemCasing((string)key.GetValue("SteamPath"));
                
                key = Registry.LocalMachine.OpenSubKey(keyStr, false);
                if (key != null)
                    return (string)key.GetValue("InstallPath");
            }
            catch (NullReferenceException)
            {
                throw new Exception(Resources.SteamFolderNotFound);
             }
            catch (SecurityException)
            {
                throw new Exception(Resources.SteamFolderNotFound);
            }

            return string.Empty;
        }
    }
}
