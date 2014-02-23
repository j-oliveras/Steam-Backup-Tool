using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace steamBackup
{
    static class Utilities
    {
        // General
        public static string getFileSystemCasing(string path)
        {
            if (Path.IsPathRooted(path))
            {
                path = path.TrimEnd(Path.DirectorySeparatorChar); // if you type c:\foo\ instead of c:\foo
                try
                {
                    string name = Path.GetFileName(path);
                    if (name == "") return path.ToUpper() + Path.DirectorySeparatorChar; // root reached

                    string parent = Path.GetDirectoryName(path); // retrieving parent of element to be corrected

                    parent = getFileSystemCasing(parent); //to get correct casing on the entire string, and not only on the last element

                    DirectoryInfo diParent = new DirectoryInfo(parent);
                    FileSystemInfo[] fsiChildren = diParent.GetFileSystemInfos(name);
                    FileSystemInfo fsiChild = fsiChildren.First();
                    return fsiChild.FullName; // coming from GetFileSystemImfos() this has the correct case
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message); throw new ArgumentException("Invalid path");
                }
            }
            else throw new ArgumentException("Absolute path needed, not relative");
        }

        public static bool folderExists(string dir, string folder)
        {
            string[] folderList = Directory.GetDirectories(dir);

            foreach(string folderToTest in folderList)
            {
                if (folderToTest.Equals(dir + @"\" + folder))
                    return true;
            }

            return false;
        }

        public static string getSteamAppsFolder(string steamDir)
        {
            if (folderExists(steamDir , "steamapps"))
                return "steamapps";
            else if (folderExists(steamDir, "SteamApps"))
                return "SteamApps";
            else
                return "ERROR";
        }

        public static string[] getLibraries(string steamDir)
        {
            string libraryLocation = steamDir + "\\" + getSteamAppsFolder(steamDir) + "\\";

            FileInfo fi = new FileInfo(steamDir + "\\config\\config.vdf");
            StreamReader reader = fi.OpenText();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                string[] lineData = line.Split(new string[] { "		" }, StringSplitOptions.RemoveEmptyEntries);

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

        public static string upDirLvl(string dir)
        {
            string[] splits = dir.TrimEnd('\\').Split('\\');
            string rdir = "";

            for (int i = 0; i < splits.Length - 1; i++)
            {
                rdir += splits[i] + "\\";
            }

            return rdir;
        }

        public static bool isSteamRunning()
        {
            Process[] pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.checkSteamRun)
                return true;

            return false;
        }

        static private string errorList;
        static public void clearErrorList(){errorList = "";}
        static public string getErrorList() { return errorList; }
        
        static public void addToErrorList(Job job)
        {
            // TODO redo this

            if (string.IsNullOrEmpty(errorList))
            {
                errorList += "Listed below are the errors for a backup or restore." + Environment.NewLine + Environment.NewLine;
                errorList += "Please try running the backup process again making sure that there are no programs accessing the files being backed up (e.g. Steam)." + Environment.NewLine + Environment.NewLine;
                errorList += "To check the integrity of this backup: navigate to the backup location -> Select all files in the 'common' folder -> right click -> 7zip -> Test archive. You should do the same for 'Source Games.7z' also.";
            }

            errorList += Environment.NewLine + Environment.NewLine + @"//////////////////// Error Time: " + DateTime.Now.ToString("dd/MM/yyyy H:mm.ss") + @" \\\\\\\\\\\\\\\\\\\\" + Environment.NewLine + Environment.NewLine;

            errorList += Environment.NewLine + job.toString();

            File.WriteAllText(Settings.backupDir + "\\Error Log.txt", errorList);
        }

        static public bool TryOpenUrl(string p_url)
        {
            // try use default browser [registry: HKEY_CURRENT_USER\Software\Classes\http\shell\open\command]
            try
            {
                string keyValue = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Classes\http\shell\open\command", "", null) as string;
                if (string.IsNullOrEmpty(keyValue) == false)
                {
                    string browserPath = keyValue.Replace("%1", p_url);
                    System.Diagnostics.Process.Start(browserPath);
                    return true;
                }
            }
            catch { }

            // try open browser as default command
            try
            {
                System.Diagnostics.Process.Start(p_url); //browserPath, argUrl);
                return true;
            }
            catch { }

            // try open through 'explorer.exe'
            try
            {
                string browserPath = GetWindowsPath("explorer.exe");
                string argUrl = "\"" + p_url + "\"";

                System.Diagnostics.Process.Start(browserPath, argUrl);
                return true;
            }
            catch { }

            // return false, all failed
            return false;
        }

        static public string GetWindowsPath(string p_fileName)
        {
            string path = null;
            string sysdir;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (i == 0)
                    {
                        path = Environment.GetEnvironmentVariable("SystemRoot");
                    }
                    else if (i == 1)
                    {
                        path = Environment.GetEnvironmentVariable("windir");
                    }
                    else if (i == 2)
                    {
                        sysdir = Environment.GetFolderPath(Environment.SpecialFolder.System);
                        path = System.IO.Directory.GetParent(sysdir).FullName;
                    }

                    if (path != null)
                    {
                        path = System.IO.Path.Combine(path, p_fileName);
                        if (System.IO.File.Exists(path) == true)
                        {
                            return path;
                        }
                    }
                }
                catch { }
            }

            // not found
            return null;
        }

    }
}
