using System;
using System.IO;
using System.Windows.Forms;

namespace steamBackup
{
    class RestoreJob : Job
    {
        private SevenZipWrapper wrapper = null;

        public RestoreJob()
        {            
            type = JobType.RESTORE;
        }

        ~RestoreJob()
        {
            if (wrapper != null)
            {
                wrapper.Dispose(true);
                wrapper = null;
            }
        }

        public override void start()
        {
            try
            {
                string libPath = Path.GetDirectoryName(Application.ExecutablePath);
                if (libPath != null)
                {
                    libPath = Path.Combine(libPath, "rsc", "7z.dll");
                    wrapper = new SevenZipWrapper(libPath, dirBackup, true);
                }

                wrapper.Extracting += new EventHandler<ProgressEventArgs>(working);
                wrapper.FileExtractionStarted += new EventHandler<FileNameEventArgs>(started);
                wrapper.ExtractionFinished += new EventHandler<EventArgs>(finished);
                wrapper.DecompressFileArchive(dirSteam);
                wrapper.Dispose(true);
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
        }
    }
}
