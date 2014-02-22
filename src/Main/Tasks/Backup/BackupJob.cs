using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace steamBackup
{
    class BackupJob : Job
    {
        private SevenZipWrapper wrapper = null;
        private bool compressionLzma2 = false;
        private int compLevel = 5;
        public BackupJob()
        {
            type = JobType.BACKUP;
        }

        ~BackupJob()
        {
            if (wrapper != null)
            {
                wrapper.Dispose(false);
                wrapper = null;
            }
        }

        public override void start()
        {
            
            string[] fileList;
            
            if (name.Equals(Settings.sourceEngineGames))
            {                
                List<string> fileListBuilder = new List<string>();
                
                fileListBuilder.AddRange(Directory.GetFiles(dirSteam, "*.gcf", SearchOption.TopDirectoryOnly));
                fileListBuilder.AddRange(Directory.GetFiles(dirSteam, "*.ncf", SearchOption.TopDirectoryOnly));

                string[] folderList = Directory.GetDirectories(dirSteam, "*", SearchOption.TopDirectoryOnly);
                foreach (string folder in folderList)
                {
                    if (!folder.Contains(@"\steamapps\common") && !folder.Contains(@"\steamapps\downloading") && !folder.Contains(@"\steamapps\temp"))
                    {
                        fileListBuilder.AddRange(Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories));
                    }
                }

                fileList = fileListBuilder.ToArray();
            }
            else
            {
                fileList = Directory.GetFiles(dirSteam, "*.*", SearchOption.AllDirectories);
            }

            try
            {
                string libPath = Path.GetDirectoryName(Application.ExecutablePath);
                if (libPath != null)
                {
                    libPath = Path.Combine(libPath, "rsc", "7z.dll");
                    wrapper = new SevenZipWrapper(libPath, dirBackup, false);
                }
                wrapper.Compressing += new EventHandler<ProgressEventArgs>(working);
                wrapper.FileCompressionStarted += new EventHandler<FileNameEventArgs>(started);
                wrapper.CompressionFinished += new EventHandler<EventArgs>(finished);
                if (compressionLzma2)
                    wrapper.UseLzma2Compression = true;
                switch (compLevel)
                {
                    case 2:
                        compLevel = 3;
                        break;
                    case 3:
                        compLevel = 5;
                        break;
                    case 4:
                        compLevel = 7;
                        break;
                    case 5:
                        compLevel = 9;
                        break;
                }
                wrapper.CompressionLevel = compLevel;
                wrapper.UseMultithreading = true;
                wrapper.MultithreadingNumThreads = Settings.lzma2Threads;
                wrapper.CompressFiles(Utilities.upDirLvl(dirSteam), fileList);
                wrapper.Dispose(false);
            }
            catch (System.Exception)
            {
                Utilities.addToErrorList(this);
            }
        }

        public override void setSteamDir(string dir)
        {
            dirSteam = dir;
        }

        public override void setBackupDir(string dir)
        {
            dirBackup = dir;
            //sevenZip.TempFolderPath = dir + ".tmp";
        }

        public void setCompression(int level)
        {
            compLevel = level;
        }

        public void setLzma2Compression(bool lzma2Compression)
        {
            compressionLzma2 = lzma2Compression;
        }

    }
}
