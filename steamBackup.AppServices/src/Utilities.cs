namespace steamBackup.AppServices
{
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
                if (name == "") return path.ToUpper() + Path.DirectorySeparatorChar; // root reached

                var parent = Path.GetDirectoryName(path); // retrieving parent of element to be corrected

                parent = GetFileSystemCasing(parent); //to get correct casing on the entire string, and not only on the last element

                var diParent = new DirectoryInfo(parent);
                var fsiChildren = diParent.GetFileSystemInfos(name);
                var fsiChild = fsiChildren.First();
                return fsiChild.FullName; // coming from GetFileSystemImfos() this has the correct case
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

            return folderList.Any(folderToTest => folderToTest.Equals(dir + @"\" + folder));
        }

        public static string GetSteamAppsFolder(string steamDir)
        {
            if (FolderExists(steamDir , "steamapps"))
                return "steamapps";

            if (FolderExists(steamDir, "SteamApps"))
                return "SteamApps";

            return "ERROR";
        }

        public static string[] GetLibraries(string steamDir)
        {
            var libraryLocation = steamDir + "\\" + GetSteamAppsFolder(steamDir) + "\\";

            var fi = new FileInfo(steamDir + "\\config\\config.vdf");
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
            var splits = dir.TrimEnd('\\').Split('\\');
            var rdir = "";

            for (var i = 0; i < splits.Length - 1; i++)
            {
                rdir += splits[i] + "\\";
            }

            return rdir;
        }

        public static bool IsSteamRunning()
        {
            var pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.CheckSteamRun)
                return true;

            return false;
        }

        public static void CopyAcfToBackup(Job job, string backupDir)
        {
            if (String.IsNullOrEmpty(job.AcfFiles)) return;

            var acfId = job.AcfFiles.Split('|');

            foreach(var id in acfId)
            {
                var src = job.AcfDir + "\\appmanifest_" + id + ".acf";
                var dst = backupDir + "\\acf";

                if (!Directory.Exists(dst))
                    Directory.CreateDirectory(dst);

                var fi = new FileInfo(src);
                var reader = fi.OpenText();

                var acf = reader.ReadToEnd();
                var gameCommonFolder = UpDirLvl(job.GetSteamDir());
                acf = acf.Replace(gameCommonFolder, "|DIRECTORY-STD|");
                acf = acf.Replace(gameCommonFolder.ToLower(), "|DIRECTORY-LOWER|");
                acf = acf.Replace(gameCommonFolder.ToLower().Replace("\\", "\\\\"), "|DIRECTORY-ESCSLASH-LOWER|");

                File.WriteAllText(dst + "\\appmanifest_" + id + ".acf", acf);
                reader.Close();
            }
        }
    }
}
