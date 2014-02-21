using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SevenZip;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace steamBackup
{
    class BackupJob : Job
    {
        private SevenZipCompressor sevenZip = null;
        private SevenZipWrapper wrapper = null;

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
                if (sevenZip.CompressionMethod == CompressionMethod.Lzma2)
                {
                    wrapper = new SevenZipWrapper(@"rsc\7z.dll", dirBackup);
                    wrapper.Compressing += new EventHandler<ProgressEventArgs>(working);
                    wrapper.FileCompressionStarted += new EventHandler<FileNameEventArgs>(started);
                    wrapper.CompressionFinished += new EventHandler<EventArgs>(finished);
                    wrapper.UseLzma2Compression = true;
                    int compLevel = (int) sevenZip.CompressionLevel;
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
                }
                else
                {
                    sevenZip.Compressing += new EventHandler<ProgressEventArgs>(working);
                    sevenZip.FileCompressionStarted += new EventHandler<FileNameEventArgs>(started);
                    sevenZip.CompressionFinished += new EventHandler<EventArgs>(finished);
                    sevenZip.CompressFiles(dirBackup, Utilities.upDirLvl(dirSteam).Length, fileList);
                }  
            }
            catch (System.Exception e)
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

        public void setCompression(CompressionLevel level)
        {
            sevenZip.CompressionLevel = level;
        }

        public void setCompressionMethod(CompressionMethod method)
        {
            sevenZip.CompressionMethod = method;
        }

    }
}
