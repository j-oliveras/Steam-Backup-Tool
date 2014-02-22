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

        public static string[] getLibraries(string steamDir)
        {
            string libraryLocation = "";
            if(Directory.Exists(steamDir + "\\steamapps\\"))
                libraryLocation = steamDir + "\\steamapps\\";
            if (Directory.Exists(steamDir + "\\SteamApps\\"))
                libraryLocation = steamDir + "\\SteamApps\\";

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

    }
}
