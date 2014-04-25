namespace steamBackup.AppServices.Jobs
{
    public enum JobStatus
    {
        Unset = -1,
        Skipped,
        Waiting,
        Working,
        Paused,
        Finished,
        Canceled,
        Error
    }
}