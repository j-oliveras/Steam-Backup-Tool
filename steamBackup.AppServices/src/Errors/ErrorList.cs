namespace steamBackup.AppServices
{
    using System;
    using steamBackup.AppServices.Properties;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public static class ErrorList
    {
        static private readonly List<ErrorItem> m_errorList = new List<ErrorItem>();

        static public void Clear()
        {
            m_errorList.Clear();
        }

        static public void Add(ErrorItem item)
        {
            m_errorList.Add(item);

            ToFile();
        }

        static public bool HasErrors()
        {
            return m_errorList.Count != 0;
        }

        public new static string ToString()
        {
            string errorHeader = String.Format(Resources.ErrorListHeader,
                Environment.OSVersion,
                Environment.Is64BitOperatingSystem,
                Environment.Is64BitProcess,
                Environment.ProcessorCount,
                Directory.GetCurrentDirectory(),
                Assembly.GetExecutingAssembly().GetName().Version.ToString()
                );
            
            return m_errorList.Aggregate(errorHeader, (current, error) => current + error.ToString());
        }

        static public void ToFile()
        {
            ToFile(Path.Combine(Settings.BackupDir, "Error Log.txt"));
        }

        static public void ToFile(string dir)
        {
            File.WriteAllText(dir, ToString());
        }

    }
}
