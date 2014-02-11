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
        public RestoreTask()
        {
            type = TaskType.RESTORE;
        }
        
        public override int ramUsage(bool useLzma2)
        {
            return (useLzma2 ? 1 : threadCount) * 40;
        }

        public override void scan()
        {
            // Find all of the backed up items and a it to the job list

            string[] files = Directory.GetFiles(backupDir + "\\common\\", "*.7z");
            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);

                Job job = new RestoreJob();


                job.name = name;
                job.setSteamDir(steamDir + "\\steamapps\\common\\");
                job.setBackupDir(backupDir + "\\common\\" + job.name + ".7z");
                job.status = JobStatus.WAITING;

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

                                foreach (Job job in list)
                                {
                                    if (job.name.Equals(name))
                                    {
                                        job.acfFiles = acfId;
                                        job.acfDir = steamDir + "\\steamapps\\";

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
            foreach (Job job in list)
                job.name = textInfo.ToTitleCase(job.name);

            // if we are using v2 of the archiver add 'Source Games.7z'
            if (currentArchiveVer == 2 && File.Exists(backupDir + "\\Source Games.7z"))
            {
                Job job = new RestoreJob();

                job.setSteamDir(steamDir + "\\");
                job.setBackupDir(backupDir + "\\Source Games.7z");
                job.name = Settings.sourceEngineGames;
                job.status = JobStatus.WAITING;

                list.Insert(0, job);
            }
        }

        public override void setup()
        {
            if (currentArchiveVer == 1)
                addMiscItems();

            sharedStart();
        }

        private void addMiscItems()
        {
            // for legacy backups (version 1)

            Job job = new RestoreJob();

            job.setBackupDir(backupDir + "\\");
            job.name = "steamapps";
            job.status = JobStatus.WAITING;

            list.Insert(0, job);

        }
    }
}
