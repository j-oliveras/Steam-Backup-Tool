﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace steamBackup
{
    public static class Settings
    {

        private static string settingsDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SteamBackupTool";

        public static string steamDir = "Steam Install Directory";
        public static string backupDir = "Backup Directory";
        public static int compresion = 5;
        public static int threadsBup = Math.Min(4, Environment.ProcessorCount / 2);
        public static int threadsRest = Math.Min(4, Environment.ProcessorCount);
        public static bool checkSteamRun = true;
        public static bool debugMode = false;
        public static bool useLzma2 = false;
        public static bool lzma2UnlockThreads = false;
        public static int lzma2Threads = Math.Min(8, Environment.ProcessorCount);

        public static string sourceEngineGames = " Old Source Engine Games";

        // Load Settings
        public static void load()
        {
            if (File.Exists(settingsDir + "\\settings.cfg"))
            {
                StreamReader streamReader = new StreamReader(settingsDir + "\\settings.cfg");
                JsonTextReader reader = new JsonTextReader(new StringReader(streamReader.ReadToEnd()));

                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        if (reader.TokenType.ToString() == "PropertyName")
                        {
                            if (reader.Value.ToString() == "steamDir")
                            {
                                reader.Read();
                                steamDir = reader.Value.ToString();
                            }
                            else if (reader.Value.ToString() == "backupDir")
                            {
                                reader.Read();
                                backupDir = reader.Value.ToString();
                            }
                            else if (reader.Value.ToString() == "compresion")
                            {
                                reader.Read();
                                compresion = int.Parse(reader.Value.ToString());
                            }
                            else if (reader.Value.ToString() == "threadsBup")
                            {
                                reader.Read();
                                threadsBup = int.Parse(reader.Value.ToString());
                            }
                            else if (reader.Value.ToString() == "threadsRest")
                            {
                                reader.Read();
                                threadsRest = int.Parse(reader.Value.ToString());
                            }
                            else if (reader.Value.ToString() == "checkSteamRun")
                            {
                                reader.Read();
                                checkSteamRun = bool.Parse(reader.Value.ToString());
                            }
                            else if (reader.Value.ToString() == "debugMode")
                            {
                                reader.Read();
                                debugMode = bool.Parse(reader.Value.ToString());
                            }
                            else if (reader.Value.ToString() == "useLzma2")
                            {
                                reader.Read();
                                useLzma2 = bool.Parse(reader.Value.ToString());
                            }
                            else if (reader.Value.ToString() == "lzma2UnlockThreads")
                            {
                                reader.Read();
                                lzma2UnlockThreads = bool.Parse(reader.Value.ToString());
                            }
                            else if (reader.Value.ToString() == "lzma2Threads")
                            {
                                reader.Read();
                                lzma2Threads = int.Parse(reader.Value.ToString());
                            }
                        }
                    }
                }

                streamReader.Close();
            }
        }

        // Save Settings
        public static void save()
        {
            
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
              writer.Formatting = Formatting.Indented;

              writer.WriteStartObject();
              writer.WriteComment("Do not edit this file, you might break something!");
              writer.WritePropertyName("backupDir");
              writer.WriteValue(backupDir);
              writer.WritePropertyName("steamDir");
              writer.WriteValue(steamDir);
              writer.WritePropertyName("compresion");
              writer.WriteValue(compresion);
              writer.WritePropertyName("threadsBup");
              writer.WriteValue(threadsBup);
              writer.WritePropertyName("threadsRest");
              writer.WriteValue(threadsRest);
              writer.WritePropertyName("checkSteamRun");
              writer.WriteValue(checkSteamRun);
              writer.WritePropertyName("debugMode");
              writer.WriteValue(debugMode);
              writer.WritePropertyName("useLzma2");
              writer.WriteValue(useLzma2);
              writer.WritePropertyName("lzma2UnlockThreads");
              writer.WriteValue(lzma2UnlockThreads);
              writer.WritePropertyName("lzma2Threads");
              writer.WriteValue(lzma2Threads);

              writer.WriteEndObject();
            }
            sw.Close();
            
            Directory.CreateDirectory(settingsDir);
            File.WriteAllText(settingsDir + "\\settings.cfg", sb.ToString());
        }
    }
}
