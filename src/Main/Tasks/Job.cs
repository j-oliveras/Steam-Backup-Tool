using System;
using System.Collections.Generic;
using System.Text;

namespace steamBackup
{
    public class Job
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
                "acfDir = " + acfDir;
        }
    }
}
