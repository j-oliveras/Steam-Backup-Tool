namespace steamBackup.AppServices
{
    using Properties;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public static class Utilities
    {
        // General
        public static string GetFileSystemCasing(string path)
        {
            if (!Path.IsPathRooted(path)) throw new ArgumentException(Resources.AbsolutePathExceptionText);

            path = path.TrimEnd(Path.DirectorySeparatorChar); // if you type c:\foo\ instead of c:\foo
            try
            {
                string name = Path.GetFileName(path);
                if (name == "") return path.ToUpper() + Path.DirectorySeparatorChar; // root reached

                string parent = Path.GetDirectoryName(path); // retrieving parent of element to be corrected

                parent = GetFileSystemCasing(parent); //to get correct casing on the entire string, and not only on the last element

                var diParent = new DirectoryInfo(parent);
                FileSystemInfo[] fsiChildren = diParent.GetFileSystemInfos(name);
                FileSystemInfo fsiChild = fsiChildren.First();
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
            string[] folderList = Directory.GetDirectories(dir);

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
            string libraryLocation = steamDir + "\\" + GetSteamAppsFolder(steamDir) + "\\";

            var fi = new FileInfo(steamDir + "\\config\\config.vdf");
            StreamReader reader = fi.OpenText();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                string[] lineData = line.Split(new[] { "		" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lineData.Length; i++)
                {
                    string data = lineData[i].Trim('\"');

                    if (data.Contains("BaseInstallFolder_"))
                    {
                        i++;
                        string dir = lineData[i].Trim('\"').Replace("\\\\", "\\") + "\\SteamApps\\";
                        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                            libraryLocation += "|" + dir;
                    }
                }
            }

            return libraryLocation.Split('|');
        }

        public static string UpDirLvl(string dir)
        {
            string[] splits = dir.TrimEnd('\\').Split('\\');
            string rdir = "";

            for (int i = 0; i < splits.Length - 1; i++)
            {
                rdir += splits[i] + "\\";
            }

            return rdir;
        }

        public static bool IsSteamRunning()
        {
            Process[] pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.CheckSteamRun)
                return true;

            return false;
        }
    }
}
