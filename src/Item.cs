using System;
using System.Collections.Generic;
using System.Text;

namespace steamBackup
{
    public class Item
    {
        public string appId;
        public string name;
        public string dirSteam;
        public string dirBackup;

        public bool enabled;
        public bool alreadyArchived;

        public string program;
        public string argument;
        public string status;
        public string acfDir;

        public DateTime folderTime;
        public DateTime archiveTime;

        public string toString()
        {
            return "appId = " + appId + Environment.NewLine +
                "name = " + name + Environment.NewLine +
                "dirSteam = " + dirSteam + Environment.NewLine +
                "dirBackup = " + dirBackup + Environment.NewLine +
                "enabled = " + enabled + Environment.NewLine +
                "alreadyArchived = " + alreadyArchived + Environment.NewLine +
                "program = " + program + Environment.NewLine +
                "argument = " + argument + Environment.NewLine +
                "status = " + status + Environment.NewLine +
                "acfDir = " + acfDir + Environment.NewLine +
                "folderTime = " + folderTime + Environment.NewLine +
                "archiveTime = " + archiveTime;
        }
    }
}
