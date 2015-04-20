namespace steamBackup.AppServices
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