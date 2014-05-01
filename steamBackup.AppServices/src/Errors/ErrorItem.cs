namespace steamBackup.AppServices.Errors
{
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Properties;
    using System;

    public class ErrorItem
    {
        private readonly Job _mJob;
        private DateTime _mTime;
        private readonly string _mErrorString;
        private readonly string _mStackTrace;

        public ErrorItem(string errorString, Job job = null, string stackTrace = null)
        {
            _mJob = job;
            _mTime = DateTime.Now;
            _mErrorString = errorString;
            _mStackTrace = stackTrace;
        }

        public override string ToString()
        {
            var str = string.Format(Resources.JobErrorTime, _mTime.ToString("dd/MM/yyyy H:mm.ss")) + Environment.NewLine;

            if (_mJob != null)
                str += string.Format(Resources.JobErrorDetails, _mJob) + Environment.NewLine;

            if (_mErrorString != null)
                str += string.Format(Resources.JobErrorMsg, _mErrorString) + Environment.NewLine;

            if (_mStackTrace != null)
                str += string.Format(Resources.JobErrorStack, _mStackTrace) + Environment.NewLine;

            return str;
        }
    }
}