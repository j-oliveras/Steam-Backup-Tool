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
        public int compresionLevel = 0;

        public bool deleteAll = false;
        
        public override int ramUsage()
        {
            int ramPerThread = 0;

            if (compresionLevel == 6)
                ramPerThread = 709;
            else if (compresionLevel == 5)
                ramPerThread = 376;
            else if (compresionLevel == 4)
                ramPerThread = 192;
            else if (compresionLevel == 3)
                ramPerThread = 19;
            else if (compresionLevel == 2)
                ramPerThread = 6;
            else if (compresionLevel == 1)
                ramPerThread = 1;
            else
                return -1;

            return (threadCount) * ramPerThread;
        }

        public string compresionLevelString(int val)
        {

            if (val == 6)
                return "-mx9";
            else if (val == 5)
                return "-mx7";
            else if (val == 4)
                return "-mx5";
            else if (val == 3)
                return "-mx3";
            else if (val == 2)
                return "-mx1";
            else
                return "-mx0";
        }

        public void setEnableUpd(CheckedListBox chkList)
        {
            chkList.Items.Clear();
            
            foreach (Job job in list)
            {  
                if (job.name.Equals(Settings.sourceEngineGames))
                {
                    enableJob(job);

                    chkList.Items.Add(job.name, job.enabled);
                    chkList.Refresh();
                }
                else
                {
                    DateTime achiveDate = new FileInfo(job.dirBackup).LastWriteTimeUtc;
                    string[] fileList = Directory.GetFiles(job.dirSteam, "*.*", SearchOption.AllDirectories);

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    bool isNewer = false;

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

                    if (isNewer)
                        enableJob(job);
                    else
                        disableJob(job);
                    chkList.Items.Add(job.name, job.enabled);
                    chkList.Refresh();
                }
            } 
        }

        public override void scan()
        {
            // Find all of the backed up items and a it to the job list
            
            scanMisc();
            scanSteamAcf();
            scanSteamLostCommonFolders();
        }

        public override void setup(CheckedListBox chkList)
        {
            checkEnabledItems(chkList);
            setArgument();

            // Delete backup if the achive is not being updated (i.e all items are checked)
            if (deleteAll && Directory.Exists(backupDir))
                Directory.Delete(backupDir, true);

            makeConfigFile();
        }

        private void scanMisc()
        {
            // Add misc backup

            Job item = new Job();

            item.name = Settings.sourceEngineGames;
            item.dirSteam = steamDir + "\\steamapps\\";
            item.dirBackup = backupDir + "\\Source Games.7z";
            item.enabled = true;

            item.program = "7za_cmd.exe";
            item.status = "Waiting";

            if (File.Exists(item.dirBackup))
                item.alreadyArchived = true;
            else
                item.alreadyArchived = false;

            list.Add(item);
        }

        private void scanSteamAcf()
        {
            // appId,dependentOn|appId,dependentOn
            string dependentAppList = null;

            string[] libraries = Utilities.getLibraries(steamDir);

            foreach (string lib in libraries)
            {
                string[] files = Directory.GetFiles(lib, "*.acf", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    Job item = new Job();
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
                                    item.dirSteam = Utilities.getFileSystemCasing(dir);
                            }
                            else if (data.Equals("appinstalldir"))
                            {
                                i++;
                                string dir = lineData[i].Trim('\"');
                                if (filterAcfDir(dir))
                                    item.dirSteam = Utilities.getFileSystemCasing(dir);
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

                        list.Add(item);
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
                    foreach (Job item in list)
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
                                Job item = new Job();

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
                                                item.dirSteam = Utilities.getFileSystemCasing(dir);
                                        }
                                        else if (data.Equals("appinstalldir"))
                                        {
                                            i++;
                                            string dir = lineData[i].Trim('\"');
                                            if (filterAcfDir(dir))
                                                item.dirSteam = Utilities.getFileSystemCasing(dir);
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

                                    list.Add(item);
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        private void scanSteamLostCommonFolders()
        {

            string[] libraries = Utilities.getLibraries(steamDir);

            foreach (string lib in libraries)
            {

                if (Directory.Exists(lib + "common\\"))
                {
                    string[] folders = Directory.GetDirectories(lib + "common\\");
                    foreach (string folder in folders)
                    {

                        bool isNew = true;

                        foreach (Job itemSearch in list)
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

                            Job item = new Job();

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

                            list.Add(item);
                        }
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

        private void setArgument()
        {
            string compType = compresionLevelString(compresionLevel);

            foreach (Job item in list)
            {
                if (item.name.Equals(Settings.sourceEngineGames))
                {
                    item.argument = "a \"" + item.dirBackup + "\" \"" + item.dirSteam + "\" " + compresionLevelString(compresionLevel) + " -w\"" + backupDir + "\\\" -t7z -aoa -xr!*.acf -xr!common -xr!temp -xr!downloading";
                }
                else
                {
                    string[] splits = item.dirSteam.Split('\\');
                    string name = splits[splits.Length - 1];

                    item.argument = "a \"" + item.dirBackup + "\" \"" + item.dirSteam + "\" " + compType + " -w\"" + backupDir + "\\common\\\" -t7z -aoa";
                }
            }
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
                foreach (Job item in list)
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

    }
}
