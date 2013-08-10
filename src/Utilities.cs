using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using System.Threading;

namespace steamBackup
{

    class Utilities
    {
        public List<Item> List = new List<Item>();
        string archiveVer = "2";
        string currentArchiveVer = "1";

        // General

        public void setEnableAll()
        {
            foreach (Item item in List)
            {
                item.enabled = true;
            }
        }

        internal void setEnableNone()
        {
            foreach (Item item in List)
            {
                item.enabled = false;
            }
        }

        public void setEnableUpd()
        {
            foreach (Item item in List)
            {
                if (item.folderTime > item.archiveTime)
                    item.enabled = true;
                else
                    item.enabled = false;
            }
        }

        public void checkEnabledItems(CheckedListBox chkList)
        {

            int i = 0;
            foreach (object o in chkList.Items)
            {
                foreach (Item item in List)
                {
                    if (o.ToString().Equals(item.name))
                    {
                        if (chkList.GetItemChecked(i))
                        {
                            item.enabled = true;
                            item.status = "Waiting";
                        }
                        else
                        {
                            item.enabled = false;
                            item.status = "Skipped";
                        }
                        break;
                    }
                }

                i++;
            }
        }

        private string compresionTypeString(TrackBar tbarComp)
        {
            if (tbarComp.Value == 6)
                return "-mx9";
            else if (tbarComp.Value == 5)
                return "-mx7";
            else if (tbarComp.Value == 4)
                return "-mx5";
            else if (tbarComp.Value == 3)
                return "-mx3";
            else if (tbarComp.Value == 2)
                return "-mx1";
            else 
                return "-mx0";
        }

        public void ramUsageRestore(Label lblRamRestore, TrackBar tbarThread, TrackBar tbarComp)
        {
            int ramRestore = (tbarThread.Value + 1) * 40;
            lblRamRestore.Text = "Max Ram Usage: " + ramRestore.ToString() + "MB";
        }

        private string GetFileSystemCasing(string path)
        {
            if (Path.IsPathRooted(path))
            {
                path = path.TrimEnd(Path.DirectorySeparatorChar); // if you type c:\foo\ instead of c:\foo
                try
                {
                    string name = Path.GetFileName(path);
                    if (name == "") return path.ToUpper() + Path.DirectorySeparatorChar; // root reached

                    string parent = Path.GetDirectoryName(path); // retrieving parent of element to be corrected

                    parent = GetFileSystemCasing(parent); //to get correct casing on the entire string, and not only on the last element

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

        public string[] getLibraries(string steamDir)
        {
            string libraryLocation = steamDir + "\\steamapps\\";

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

        public string upDirLvl(string dir)
        {
            string[] splits = dir.TrimEnd('\\').Split('\\');
            string rdir = "";

            for (int i = 0; i < splits.Length - 1; i++)
            {
                rdir += splits[i] + "\\";
            }

            return rdir;
        }

        // Backup

        public void scanMisc(string steamDir, string backupDir)
        {
            // Add misc backup

            Item item = new Item();

            item.name = Settings.sourceEngineGames;
            item.dirSteam = steamDir + "\\steamapps\\";
            item.dirBackup = backupDir + "\\Source Games.7z";
            item.enabled = true;

            item.program = "7za_cmd.exe";
            item.status = "Waiting";

            item.folderTime = DateTime.UtcNow;
            item.archiveTime = new DirectoryInfo(item.dirBackup).LastWriteTimeUtc;

            if (File.Exists(item.dirBackup))
                item.alreadyArchived = true;
            else
                item.alreadyArchived = false;

            List.Add(item);
        }

        public void scanSteamAcf(string steamDir, string backupDir)
        {
            // appId,dependentOn|appId,dependentOn
            string dependentAppList = null;

            string[] libraries = getLibraries(steamDir);

            foreach (string lib in libraries)
            {
                string[] files = Directory.GetFiles(lib, "*.acf", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    Item item = new Item();
                    bool dependsOnApps = false;

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
                                item.appId = lineData[i].Trim('\"');
                            }
                            else if (data.Equals("name"))
                            {
                                i++;
                                item.name = lineData[i].Trim('\"');
                            }
                            else if (data.Equals("installdir"))
                            {
                                i++;
                                string dir = lineData[i].Trim('\"').Replace("\\\\", "\\");
                                if (filterAcfDir(dir))
                                    item.dirSteam = GetFileSystemCasing(dir);
                            }
                            else if (data.Equals("appinstalldir"))
                            {
                                i++;
                                string dir = lineData[i].Trim('\"');
                                if (filterAcfDir(dir))
                                    item.dirSteam = GetFileSystemCasing(dir);
                            }
                            else if (data.Equals("DependsOnApps"))
                            {
                                i++;
                                string dependantId = lineData[i].Trim('\"');
                                
                                dependsOnApps = true;

                                if (!string.IsNullOrEmpty(dependentAppList))
                                    dependentAppList += "|";
                                dependentAppList += item.appId + "," + dependantId;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(item.name) && !string.IsNullOrEmpty(item.dirSteam) && !dependsOnApps)
                    {
                        string[] splits = item.dirSteam.Split('\\');
                        string name = splits[splits.Length - 1];

                        item.dirBackup = backupDir + "\\common\\" + name + ".7z";
                        item.enabled = true;

                        item.program = "7za_cmd.exe";
                        item.status = "Waiting";
                        item.acfDir = lib;

                        if (File.Exists(item.dirBackup))
                            item.alreadyArchived = true;
                        else
                            item.alreadyArchived = false;

                        List.Add(item);
                    }
                }
            }

            if (!string.IsNullOrEmpty(dependentAppList))
            {
                string[] dependentApps = dependentAppList.Split('|');
                foreach (string dependentApp in dependentApps)
                {
                    string[] dependent = dependentApp.Split(',');
                    bool found = false;
                    foreach (Item item in List)
                    {
                        if (!string.IsNullOrEmpty(item.appId))
                        {
                            string[] appIdList = item.appId.Split('|');

                            foreach (string idToCheck in appIdList)
                            {
                                if (idToCheck.Equals(dependent[1]))
                                {
                                    item.appId += "|" + dependent[0];
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!found)
                    {

                        foreach (string lib in libraries)
                        {
                            string file = lib + "\\appmanifest_" + dependent[0] + ".acf";
                            if (File.Exists(file))
                            {
                                Item item = new Item();

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

                                        if (data.Equals("name"))
                                        {
                                            i++;
                                            item.name = lineData[i].Trim('\"');
                                        }
                                        else if (data.Equals("installdir"))
                                        {
                                            i++;
                                            string dir = lineData[i].Trim('\"').Replace("\\\\", "\\");
                                            if (filterAcfDir(dir))
                                                item.dirSteam = GetFileSystemCasing(dir);
                                        }
                                        else if (data.Equals("appinstalldir"))
                                        {
                                            i++;
                                            string dir = lineData[i].Trim('\"');
                                            if (filterAcfDir(dir))
                                                item.dirSteam = GetFileSystemCasing(dir);
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(item.name) && !string.IsNullOrEmpty(item.dirSteam))
                                {
                                    string[] splits = item.dirSteam.Split('\\');
                                    string name = splits[splits.Length - 1];

                                    item.dirBackup = backupDir + "\\common\\" + name + ".7z";
                                    item.appId = dependent[1] + "|" + dependent[0];
                                    item.enabled = true;

                                    item.program = "7za_cmd.exe";
                                    item.status = "Waiting";
                                    item.acfDir = lib;

                                    if (File.Exists(item.dirBackup))
                                        item.alreadyArchived = true;
                                    else
                                        item.alreadyArchived = false;

                                    List.Add(item);
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        public void scanSteamLostCommonFolders(string steamDir, string backupDir)
        {

            string[] libraries = getLibraries(steamDir);

            foreach (string lib in libraries)
            {

                if (Directory.Exists(lib + "common\\"))
                {
                    string[] folders = Directory.GetDirectories(lib + "common\\");
                    foreach (string folder in folders)
                    {

                        bool isNew = true;

                        foreach (Item itemSearch in List)
                        {
                            string listDir = itemSearch.dirSteam.ToLower();
                            string folderDir = folder.ToLower();

                            if (listDir.Equals(folderDir))
                            {
                                isNew = false;
                                break;
                            }
                        }


                        if (isNew)
                        {
                            string[] splits = folder.Split('\\');
                            string name = splits[splits.Length - 1];

                            TextInfo textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;

                            Item item = new Item();

                            item.name = textInfo.ToTitleCase(name);
                            item.dirSteam = folder;
                            item.dirBackup = backupDir + "\\common\\" + name + ".7z";
                            item.enabled = true;

                            item.program = "7za_cmd.exe";
                            item.status = "Waiting";

                            if (File.Exists(item.dirBackup))
                                item.alreadyArchived = true;
                            else
                                item.alreadyArchived = false;

                            List.Add(item);
                        }
                    }
                }
            }
        }

        private bool filterAcfDir(string acfString)
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

        internal void setupBackup(CheckedListBox chkList, TrackBar tbarComp, string steamDir, string backupDir, bool delBackup)
        {
            checkEnabledItems(chkList);
            setBupArgument(tbarComp, backupDir);

            // Delete backup if the achive is not being updated (i.e all items are checked)
            if (delBackup && Directory.Exists(backupDir))
                Directory.Delete(backupDir, true);

            makeConfigFile(backupDir);
        }

        private void setBupArgument(TrackBar tbarComp, string backupDir)
        {
            string compType = compresionTypeString(tbarComp);

            foreach (Item item in List)
            {
                if (item.name.Equals(Settings.sourceEngineGames))
                {
                    item.argument = "a \"" + item.dirBackup + "\" \"" + item.dirSteam + "\" " + compresionTypeString(tbarComp) + " -w\"" + backupDir + "\\\" -t7z -aoa -xr!*.acf -xr!common -xr!temp -xr!downloading";
                }
                else
                {
                    string[] splits = item.dirSteam.Split('\\');
                    string name = splits[splits.Length - 1];

                    item.argument = "a \"" + item.dirBackup + "\" \"" + item.dirSteam + "\" " + compType + " -w\"" + backupDir + "\\common\\\" -t7z -aoa";
                }
            }
        }

        private void makeConfigFile(string backupDir)
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
                // Add alread archived apps back into the list if they are not being backed up again (so we dont get any orphaned acf files).
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
                foreach (Item item in List)
                {
                    if (!string.IsNullOrEmpty(item.appId) && item.enabled)
                    {
                        string[] nameSplit = item.dirSteam.Split('\\');
                        string name = nameSplit[nameSplit.Length - 1];

                        if (!sb.ToString().Contains(name))
                        {
                            writer.WritePropertyName(name);
                            writer.WriteValue(item.appId);
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

        // Restore

        public void scanBackup(string steamDir, string backupDir)
        {
            
            string[] files = Directory.GetFiles(backupDir + "\\common\\", "*.7z");
            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                
                Item item = new Item();


                item.name = name;
                item.dirSteam = steamDir + "\\steamapps\\common\\";
                item.dirBackup = backupDir + "\\common\\" + item.name + ".7z";
                item.enabled = true;

                item.program = "7za_cmd.exe";
                item.status = "Waiting";

                List.Add(item);
            }


            string configDir = backupDir + "\\config.sbt";
            if (File.Exists(configDir))
            {
                StreamReader streamReader = new StreamReader(configDir);
                JsonTextReader reader = new JsonTextReader(new StringReader(streamReader.ReadToEnd()));

                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        if (reader.TokenType.ToString() == "PropertyName" && reader.Value.ToString() == "Archiver Version")
                        {
                            reader.Read();
                            currentArchiveVer = reader.Value.ToString();
                        }
                        else if (reader.TokenType.ToString() == "PropertyName" && reader.Value.ToString() == "ACF IDs")
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
                                
                                string name = reader.Value.ToString();
                                reader.Read();
                                string acfId = reader.Value.ToString();
                                reader.Read();

                                foreach (Item item in List)
                                {
                                    if (item.name.Equals(name))
                                    {
                                        item.appId = acfId;
                                        item.acfDir = steamDir + "\\steamapps\\";

                                        break;
                                    }
                                }

                                
                            } while (reader.TokenType.ToString() != "EndObject");
                        }
                    }
                }
            Finish:
                streamReader.Close();
            }

            TextInfo textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
            foreach (Item item in List)
                item.name = textInfo.ToTitleCase(item.name);

            // if we are using v2 of the archiver add 'Source Games.7z'
            if (currentArchiveVer.Equals("2") && File.Exists(backupDir + "\\Source Games.7z"))
            {
                Item item = new Item();

                item.dirSteam = steamDir + "\\";
                item.dirBackup = backupDir + "\\Source Games.7z";
                item.name = Settings.sourceEngineGames;
                item.enabled = true;

                item.program = "7za_cmd.exe";

                List.Insert(0, item);
            }
        }

        internal void setupRestore(CheckedListBox chkList, string steamDir, string backupDir)
        {
            checkEnabledItems(chkList);
            setRestArgument(backupDir);
            if (currentArchiveVer.Equals("1"))
            {
                addMiscRestItems(steamDir, backupDir);
            }
        }

        private void addMiscRestItems(string steamDir, string backupDir)
        {
            Item item = new Item();

            item.dirSteam = backupDir + "\\";
            item.name = "steamapps";
            item.enabled = true;

            item.program = "7za_cmd.exe";
            item.argument = "x \"" + item.dirSteam + "\\steamapps.7z\" -o\"" + steamDir + "\\\" -aoa";

            List.Insert(0, item);

        }

        private void setRestArgument(string backupDir)
        {
            foreach (Item item in List)
            {
                if (item.name.Equals(Settings.sourceEngineGames))
                {
                    item.argument = "x \"" + item.dirBackup + "\" -o\"" + item.dirSteam + "\" -aoa";
                }
                else
                {
                    item.argument = "x \"" + item.dirBackup + "\" -o\"" + item.dirSteam + "\" -aoa";
                }
            }
        }
    }  
}
