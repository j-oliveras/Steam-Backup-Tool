using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading;

namespace steamBackup
{
    public class RestoreTask : Task
    {
        public override int ramUsage()
        {
            return 0;
        }

        public override void scan()
        {
            // Find all of the backed up items and a it to the job list

            string[] files = Directory.GetFiles(backupDir + "\\common\\", "*.7z");
            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);

                Job job = new Job();


                job.name = name;
                job.dirSteam = steamDir + "\\steamapps\\common\\";
                job.dirBackup = backupDir + "\\common\\" + job.name + ".7z";
                job.enabled = true;

                job.program = "7za_cmd.exe";
                job.status = "Waiting";

                list.Add(job);
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
                            currentArchiveVer = Convert.ToInt32(reader.Value.ToString());
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

                                foreach (Job item in list)
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
            foreach (Job item in list)
                item.name = textInfo.ToTitleCase(item.name);

            // if we are using v2 of the archiver add 'Source Games.7z'
            if (currentArchiveVer.Equals("2") && File.Exists(backupDir + "\\Source Games.7z"))
            {
                Job item = new Job();

                item.dirSteam = steamDir + "\\";
                item.dirBackup = backupDir + "\\Source Games.7z";
                item.name = Settings.sourceEngineGames;
                item.enabled = true;

                item.program = "7za_cmd.exe";

                list.Insert(0, item);
            }
        }

        public void setupRestore(CheckedListBox chkList)
        {
            checkEnabledItems(chkList);
            setArgument();
            if (currentArchiveVer.Equals("1"))
                addMiscItems();
        }

        private void addMiscItems()
        {
            // for legacy backups (version 1)

            Job item = new Job();

            item.dirSteam = backupDir + "\\";
            item.name = "steamapps";
            item.enabled = true;

            item.program = "7za_cmd.exe";
            item.argument = "x \"" + item.dirSteam + "\\steamapps.7z\" -o\"" + steamDir + "\\\" -aoa";

            list.Insert(0, item);

        }

        private void setArgument()
        {
            foreach (Job item in list)
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
