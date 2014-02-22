using System;

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

            wrapper = new SevenZipWrapper(@"rsc\7z.dll", dirBackup, true);

            wrapper.Extracting += new EventHandler<ProgressEventArgs>(working);
            wrapper.FileExtractionStarted += new EventHandler<FileNameEventArgs>(started);
            wrapper.ExtractionFinished += new EventHandler<EventArgs>(finished);

            try
            {
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
