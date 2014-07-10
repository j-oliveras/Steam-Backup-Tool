namespace steamBackup.AppServices.Errors
{
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Properties;
    using System;

    public class ErrorItem
    {
        private readonly Job m_job;
        private DateTime m_time;
        private readonly string m_errorString;
        private readonly string m_stackTrace;

        public ErrorItem(string errorString, Job job = null, string stackTrace = null)
        {
            m_job = job;
            m_time = DateTime.Now;
            m_errorString = errorString;
            m_stackTrace = stackTrace;
        }

        public override string ToString()
        {
            var str = string.Format(Resources.JobErrorTime, m_time.ToString("dd/MM/yyyy H:mm.ss")) + Environment.NewLine;

            if (m_job != null)
                str += string.Format(Resources.JobErrorDetails, m_job) + Environment.NewLine;

            if (m_errorString != null)
                str += string.Format(Resources.JobErrorMsg, m_errorString) + Environment.NewLine;

            if (m_stackTrace != null)
                str += string.Format(Resources.JobErrorStack, m_stackTrace) + Environment.NewLine;

            return str;
        }
    }
}