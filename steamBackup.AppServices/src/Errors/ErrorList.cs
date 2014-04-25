namespace steamBackup.AppServices.Errors
{
    using steamBackup.AppServices.Properties;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class ErrorList
    {
        static private readonly List<ErrorItem> MList = new List<ErrorItem>();

        static public void Clear()
        {
            MList.Clear();
        }

        static public void Add(ErrorItem item)
        {
            MList.Add(item);

            ToFile();
        }

        static public bool HasErrors()
        {
            if (MList.Count == 0)
                return false;
            return true;
        }

        public new static string ToString()
        {
            return MList.Aggregate(Resources.ErrorListHeader, (current, error) => current + error.ToString());
        }

        static public void ToFile()
        {
            ToFile(Settings.BackupDir + "\\Error Log.txt");
        }

        static public void ToFile(string dir)
        {
            File.WriteAllText(dir, ToString());
        }

    }
}
