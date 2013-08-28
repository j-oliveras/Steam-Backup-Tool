using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SevenZip;
using System.IO;
using System.Windows.Forms;

namespace steamBackup
{
    class BackupJob : Job
    {
        private SevenZipCompressor sevenZip = null;

        public BackupJob()
        {
            type = JobType.BACKUP;

            sevenZip = new SevenZipCompressor();
            
            sevenZip.ArchiveFormat = OutArchiveFormat.SevenZip;
            sevenZip.CompressionMethod = CompressionMethod.Lzma;
            sevenZip.CompressionMode = CompressionMode.Create;

            sevenZip.IncludeEmptyDirectories = true;
            sevenZip.DirectoryStructure = true;
            sevenZip.PreserveDirectoryRoot = true;
            sevenZip.FastCompression = false;
        }

        public override void start(ProgressBar pgsBar)
        {
            progressBar = pgsBar;
            
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

            sevenZip.Compressing += new EventHandler<ProgressEventArgs>(working);
            sevenZip.FileCompressionStarted += new EventHandler<FileNameEventArgs>(started);
            sevenZip.CompressionFinished += new EventHandler<EventArgs>(finished);
            
            sevenZip.BeginCompressFiles(dirBackup, Utilities.upDirLvl(dirSteam).Length, fileList);
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

        public void setCompression(CompressionLevel level)
        {
            sevenZip.CompressionLevel = level;
        }

    }
}
