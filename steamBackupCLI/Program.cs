
namespace steamBackupCLI
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NDesk.Options;
    using steamBackup.AppServices;
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Properties;
    using steamBackup.AppServices.Tasks.Backup;
    using System;
    using System.Reflection;
    using Timer = System.Timers.Timer;

    public class Program
    {
        static bool _showHelp;
        static bool _useLzma2;
        static int _compLevel = 3;
        static bool _updateBackup;
        static bool _updateLibrary;
        static bool _deleteBackup;
        static int _numThreads = Environment.ProcessorCount / 2;
        static string _outDir = string.Empty;
        static string _steamDir = string.Empty;

        public static BackupTask BupTask;

        public static int ConsoleWidth;

        internal static List<int> InstanceLines = new List<int>();
        internal static int StatusLine;

        internal static List<int> RegisteredInstances = new List<int>();

        static void Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var appTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute), false)).Title;
            var appCopyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof (AssemblyCopyrightAttribute), false)).Copyright;
            var appVersion = assembly.GetName().Version;

            var verString = string.Format(@"{0} v{1}.{2}.{3}.{4}", appTitle,
                                                                      appVersion.Major,
                                                                      appVersion.Minor,
                                                                      appVersion.Build,
                                                                      appVersion.Revision);
            Console.Title = verString;
            Console.SetWindowSize(100, 47);
            Console.SetBufferSize(100, 300);
            Console.WriteLine(verString);
            Console.WriteLine(@"https://bitbucket.org/Du-z/steam-backup-tool");
            Console.WriteLine(Environment.NewLine + appCopyright + Environment.NewLine);
            Console.WriteLine(Resources.AppCredits + Environment.NewLine);

            var opts = new OptionSet
            {
                {
                    "h|?|help",
                    "show this message and exit.",
                    v => _showHelp = v != null
                },
                {
                    "O|out-dir=",
                    "(required) Set backup directory",
                    v => _outDir = v
                },
                {
                    "S|steam-dir=",
                    "Do not automatically detect Steam directory, use this directory instead",
                    v => _steamDir = v
                },
                {
                    "2|lzma2",
                    "Use LZMA2 compression.",
                    v => { _useLzma2 = true; }
                },
                {
                    "C|compression=",
                    "Set compression level. Possible values 0 - 5:" + Environment.NewLine + 
                    "\t0 : Copy" + Environment.NewLine + 
                    "\t1 : Fastest" + Environment.NewLine + 
                    "\t2 : Fast" + Environment.NewLine + 
                    "\t3 : Normal" + Environment.NewLine + 
                    "\t4 : Maximum" + Environment.NewLine + 
                    "\t5 : Ultra",
                    (int comp) => _compLevel = comp
                },
                {
                    "B|backup",
                    "Update backup" + Environment.NewLine + 
                    "Update games that have been changed since the last backup, EXCLUDING games that have not been backed up yet.",
                    v => _updateBackup = v != null
                },
                {
                    "L|library",
                    "Update library" + Environment.NewLine + 
                    "Update games that have been changed since the last backup, INCLUDING games that have not been backed up yet.",
                    v => _updateLibrary = v != null
                },
                {
                    "D|delete",
                    "Delete all backup files before starting" + Environment.NewLine + 
                    "ignored when either update library or update backup parameter is used",
                    v => _deleteBackup = v != null
                },
                {
                    "T|threads=",
                    "Thread count" + Environment.NewLine + 
                    "LZMA:  number of concurrent instances," + Environment.NewLine + 
                    "LZMA2: number of threads used",
                    (int num) => _numThreads = num
                }
            };

            if (args.Length > 0)
            {
                try
                {
                    opts.Parse(args);
                }
                catch (Exception)
                {
                    Console.WriteLine(@"Internal error occured, please check parameters");
                }
            }
            else
                _showHelp = true;

            if (_showHelp || string.IsNullOrEmpty(_outDir))
            {
                Console.WriteLine(@"usage: steamBackupCLI [options]" + Environment.NewLine);
                Console.WriteLine(@"Parameters:");
                opts.WriteOptionDescriptions(Console.Out);
                Console.ReadLine();
                return;
            }

            Setup();
        }

        private static void Setup()
        {
            if (Utilities.IsSteamRunning())
            {
                Console.WriteLine(@"Steam is running!");
                Console.WriteLine(@"Please exit Steam before backing up.");
                Console.WriteLine(@"To continue, exit Steam and restart this Application.");
                Console.WriteLine(@"Do not start Steam until the backup process is finished.");
                Environment.Exit(2);
            }

            if (string.IsNullOrEmpty(_steamDir))
            {
                try
                {
                    _steamDir = Utilities.GetSteamDirectory();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Environment.Exit(1);
                }
            }

            if (!Utilities.IsValidSteamFolder(_steamDir))
            {
                Console.WriteLine(Resources.NotValidSteamDirectory);
                Environment.Exit(3);
            }

            Utilities.SetupBackupDirectory(_outDir);

            ConsoleWidth = Console.BufferWidth;

            StatusLine = Console.CursorTop;
            var lastLine = StatusLine + 2;

            InstanceLines.Add(lastLine);
            RegisteredInstances.Add(-1);

            if (!_useLzma2)
            {
                for (var i = 1; i < _numThreads; i++)
                {
                    InstanceLines.Add(lastLine + 3);
                    RegisteredInstances.Add(-1);
                    lastLine += 3;
                }
            }

            Settings.BackupDir = _outDir;

            BupTask = new BackupTask {SteamDir = _steamDir, BackupDir = _outDir};

            Console.WriteLine(Resources.Scanning);

            BupTask.JobList.Clear();
            BupTask.Scan();

            BupTask.SetCompMethod(_useLzma2);
            BupTask.SetCompLevel(_compLevel);
            
            BupTask.ThreadCount = _numThreads;

            if (_useLzma2)
                Settings.Lzma2Threads = _numThreads;

            if (_updateBackup)
                BupTask.SetEnableUpd(true);
            else if (_updateLibrary)
                BupTask.SetEnableUpd(false);
            else
            {
                BupTask.SetEnableAll();
                BupTask.DeleteAll = _deleteBackup;
            }

            BupTask.Setup();

            Console.SetCursorPosition(0, StatusLine);
            Console.Write(Resources.ArchivingGames.PadRight(ConsoleWidth));

            StartCompression();

            Console.SetCursorPosition(0, lastLine + 3);
            Console.WriteLine(Resources.BackupFinished);
            Thread.Sleep(2000);
        }

        private static void StartCompression()
        {
            var lcts = new LimitedConcurrencyLevelTaskScheduler(_useLzma2 ? 1 : _numThreads);
            var tasks = new List<Task>();

            var factory = new TaskFactory(lcts);
            var cts = new CancellationTokenSource();

            var procList = BupTask.JobList.FindAll(job => job.Status == JobStatus.Waiting);

            foreach (var job in procList)
            {
                var lJob = job;
                var jobId = procList.IndexOf(job) + 1;

                var t = factory.StartNew(() =>
                {
                    var tId = Thread.CurrentThread.ManagedThreadId;
                    RegisterInstance(tId);

                    WriteConsole(tId, jobId, lJob.Name, 0, "Finding Files...", " ");

                    var timer = new Timer(1000) {AutoReset = true, Enabled = true};
                    timer.Elapsed += (sender, args) =>
                    {
                        lock (lJob)
                        {
                            var eta = lJob.GetSpeedEta(true);
                            var file = lJob.GetCurFileStr();
                            var perc = lJob.GetPercDone();
                            WriteConsole(tId, jobId, lJob.Name, perc, file, eta);
                        }
                    };

                    lJob.Start();
                    Utilities.CopyAcfToBackup(lJob, _outDir);

                    timer.Stop();

                    WriteConsole(tId, -1, string.Empty, 0, string.Empty, string.Empty);
                    UnRegisterInstance(tId);
                }, cts.Token);

                tasks.Add(t);
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static void WriteConsole(int instanceId, int jobId, string jobName, byte progress, string file, string eta)
        {
            int instanceLine;
            int instance;

            lock (RegisteredInstances)
            {
                instance = RegisteredInstances.FindIndex(i => i == instanceId);
            }

            lock (InstanceLines)
            {
                instanceLine = InstanceLines[instance];
            }

            lock (Console.Out)
            {
                Console.SetCursorPosition(0, instanceLine);
                var restWidth = ConsoleWidth - 7;
                var shorted = string.Empty;

                if (jobId != -1)
                {
                    shorted = jobName.Substring(0, jobName.Length > restWidth ? restWidth - 3 : jobName.Length);
                    shorted = shorted.Length < jobName.Length ? shorted + "..." : shorted;
                    shorted = shorted.Length < restWidth ? shorted.PadRight(restWidth) : shorted;
                    Console.Write(@"#{0,4}: {1}", jobId, shorted);
                }
                else
                {
                    Console.Write(@"{0}", shorted.PadRight(ConsoleWidth));
                }

                shorted = string.Empty;

                Console.SetCursorPosition(0, instanceLine + 1);
                restWidth = ConsoleWidth - eta.Length - 10;
                
                if (jobId != -1)
                {
                    shorted = file.Substring(0, file.Length > restWidth ? restWidth - 3 : file.Length);
                    shorted = shorted.Length < file.Length ? shorted + "..." : shorted;
                    shorted = shorted.Length < restWidth ? shorted.PadRight(restWidth) : shorted;
                    Console.Write(@"{0,3}% | {1} | {2}", progress, shorted, eta);
                }
                else
                {
                    Console.Write(@"{0}", shorted.PadRight(ConsoleWidth));
                }
            }
        }

        private static void RegisterInstance(int instanceId)
        {
            lock (RegisteredInstances)
            {
                var freeInstance = RegisteredInstances.FindIndex(i => i == -1);
                RegisteredInstances[freeInstance] = instanceId;
            }
        }

        private static void UnRegisterInstance(int instanceId)
        {
            lock (RegisteredInstances)
            {
                var instance = RegisteredInstances.FindIndex(i => i == instanceId);
                RegisteredInstances[instance] = -1;
            }
        }
    }
}
