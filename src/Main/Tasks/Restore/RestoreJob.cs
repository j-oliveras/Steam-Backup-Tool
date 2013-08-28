using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SevenZip;
using System.Windows.Forms;

namespace steamBackup
{
    class RestoreJob : Job
    {
        private SevenZipExtractor sevenZip = null;

        public RestoreJob()
        {            
            type = JobType.RESTORE;
        }

        ~RestoreJob()
        {
            if (sevenZip != null)
            {
                sevenZip.Dispose();
                sevenZip = null;
            }
        }

        public override void start(ProgressBar pgsBar)
        {
            progressBar = pgsBar;

            sevenZip = new SevenZipExtractor(dirBackup);

            sevenZip.Extracting += new EventHandler<ProgressEventArgs>(working);
            sevenZip.FileExtractionStarted += new EventHandler<FileInfoEventArgs>(started);
            sevenZip.ExtractionFinished += new EventHandler<EventArgs>(finished);

            sevenZip.BeginExtractArchive(dirSteam);
        }

        public override void setSteamDir(string dir)
        {
            dirSteam = dir;
        }

        public override void setBackupDir(string dir)
        {
            dirBackup = dir;
        }
    }
}
