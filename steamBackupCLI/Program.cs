
namespace steamBackupCLI
{
    using NDesk.Options;
    using steamBackup.AppServices;
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Properties;
    using steamBackup.AppServices.Tasks.Backup;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Timer = System.Timers.Timer;

    public class Program
    {
        static bool m_showHelp;
        static bool m_useLzma2;
        static int m_compLevel = 3;
        static bool m_updateBackup;
        static bool m_updateLibrary;
        static bool m_deleteBackup;
        static int m_numThreads = Environment.ProcessorCount / 2;
        static string m_outDir = string.Empty;
        static string m_steamDir = string.Empty;

        public static BackupTask m_bupTask;

        public static int m_consoleWidth;

        internal static List<int> m_instanceLines = new List<int>();
        internal static int m_statusLine;

        internal static List<int> m_registeredInstances = new List<int>();

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
                    v => m_showHelp = v != null
                },
                {
                    "O|out-dir=",
                    "(required) Set backup directory",
                    v => m_outDir = v
                },
                {
                    "S|steam-dir=",
                    "Do not automatically detect Steam directory, use this directory instead",
                    v => m_steamDir = v
                },
                {
                    "2|lzma2",
                    "Use LZMA2 compression.",
                    v => { m_useLzma2 = true; }
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
                    (int comp) => m_compLevel = comp
                },
                {
                    "B|backup",
                    "Update backup" + Environment.NewLine + 
                    "Update games that have been changed since the last backup, EXCLUDING games that have not been backed up yet.",
                    v => m_updateBackup = v != null
                },
                {
                    "L|library",
                    "Update library" + Environment.NewLine + 
                    "Update games that have been changed since the last backup, INCLUDING games that have not been backed up yet.",
                    v => m_updateLibrary = v != null
                },
                {
                    "D|delete",
                    "Delete all backup files before starting" + Environment.NewLine + 
                    "ignored when either update library or update backup parameter is used",
                    v => m_deleteBackup = v != null
                },
                {
                    "T|threads=",
                    "Thread count" + Environment.NewLine + 
                    "LZMA:  number of concurrent instances," + Environment.NewLine + 
                    "LZMA2: number of threads used",
                    (int num) => m_numThreads = num
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
                m_showHelp = true;

            if (m_showHelp || string.IsNullOrEmpty(m_outDir))
            {
                Console.WriteLine(@"usage: steamBackupCLI [options]" + Environment.NewLine);
                Console.WriteLine(@"Parameters:");
                opts.WriteOptionDescriptions(Console.Out);
                Console.ReadLine();
                return;
            }

            Setup();

            StartCompression();

            Console.SetCursorPosition(0, m_instanceLines.Last() + 3);
            Console.WriteLine(Resources.BackupFinished);
            Thread.Sleep(2000);
        }

        private static void Setup()
        {
            if (Utilities.IsSteamRunning())
            {
                Console.WriteLine(@"");
                Console.WriteLine(@"Steam is running!");
                Console.WriteLine(@"Please exit Steam before backing up.");
                Console.WriteLine(@"To continue, exit Steam and restart this Application.");
                Console.WriteLine(@"Do not start Steam until the backup process is finished.");
                Console.WriteLine(@"");
                Console.WriteLine(@"Press enter to exit.");
                Console.ReadLine();
                Environment.Exit(2);
            }

            if (string.IsNullOrEmpty(m_steamDir))
            {
                try
                {
                    m_steamDir = Utilities.GetSteamDirectory();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Environment.Exit(1);
                }
            }

            if (!Utilities.IsValidSteamFolder(m_steamDir))
            {
                Console.WriteLine(string.Format(Resources.NotValidSteamDirectory, m_steamDir));
                Environment.Exit(3);
            }

            // create folders if needed
            if (!Utilities.SetupBackupDirectory(m_outDir))
            {
                Console.WriteLine(string.Format(Resources.UnwritableDirectory, m_outDir));
                Environment.Exit(4);
            }

            m_consoleWidth = Console.BufferWidth;
            m_statusLine = Console.CursorTop;

            m_instanceLines.Add(m_statusLine + 4);
            m_registeredInstances.Add(-1);

            if (!m_useLzma2)
            {
                for (var i = 1; i < m_numThreads; i++)
                {
                    m_instanceLines.Add(m_statusLine + 4 + (i * 3));
                    m_registeredInstances.Add(-1);
                }
            }

            Settings.BackupDir = m_outDir;

            m_bupTask = new BackupTask {m_steamDir = m_steamDir, m_backupDir = m_outDir};

            Console.SetCursorPosition(0, m_statusLine);
            Console.WriteLine(Resources.Scanning.PadRight(m_consoleWidth));

            m_bupTask.m_jobList.Clear();
            m_bupTask.Scan();

            m_bupTask.m_useLzma2 = m_useLzma2;
            m_bupTask.m_compLevel = m_compLevel;

            if (m_useLzma2)
                m_bupTask.m_lzma2Threads = m_numThreads;
            else
                m_bupTask.m_threadCount = m_numThreads;

            if (m_updateBackup)
                m_bupTask.SetEnableUpd(true);
            else if (m_updateLibrary)
                m_bupTask.SetEnableUpd(false);
            else
            {
                m_bupTask.SetEnableAll();
                m_bupTask.m_deleteAll = m_deleteBackup;
            }

            m_bupTask.Start();
        }

        private static void StartCompression()
        {
            Console.SetCursorPosition(0, m_statusLine);
            Console.Write(Resources.ArchivingGames.PadRight(m_consoleWidth));

            var lcts = new LimitedConcurrencyLevelTaskScheduler(m_useLzma2 ? 1 : m_numThreads);
            var tasks = new List<Task>();

            var factory = new TaskFactory(lcts);
            var cts = new CancellationTokenSource();

            var procList = m_bupTask.m_jobList.FindAll(job => job.m_status == JobStatus.Waiting);

            var statusTimer = new Timer(2500) { AutoReset = true };
            statusTimer.Elapsed += (sender, args) => UpdateStats();

            foreach (var job in procList)
            {
                var lJob = job;
                var jobId = procList.IndexOf(job) + 1;

                var t = factory.StartNew(() =>
                {
                    var tId = Thread.CurrentThread.ManagedThreadId;
                    RegisterInstance(tId);

                    lJob.m_status = JobStatus.Working;

                    WriteConsole(tId, jobId, lJob.m_name, 0, Resources.CompressionFindingFiles, " ");

                    var timer = new Timer(1000) {AutoReset = true, Enabled = true};
                    timer.Elapsed += (sender, args) =>
                    {
                        lock (lJob)
                        {
                            var eta = lJob.GetSpeedEta();
                            var file = lJob.m_curFileStr;
                            var perc = lJob.m_percDone;
                            WriteConsole(tId, jobId, lJob.m_name, perc, file, eta);
                        }
                    };

                    lJob.Start(m_bupTask);
                    Utilities.CopyAcfToBackup(lJob, m_outDir);

                    timer.Stop();

                    WriteConsole(tId, -1, string.Empty, 0, string.Empty, string.Empty);
                    UnRegisterInstance(tId);

                    UpdateStats();
                }, cts.Token);

                tasks.Add(t);
            }

            statusTimer.Start();
            Task.WaitAll(tasks.ToArray());
            statusTimer.Stop();
        }

        private static void UpdateStats()
        {
            int skippedCount;
            int waitingCount;
            int finishedCount;
            int totalCount;
            int compressingCount;

            lock (m_bupTask)
            {
                totalCount = m_bupTask.m_jobCount;
                skippedCount = m_bupTask.m_jobsToSkipCount;
                waitingCount = m_bupTask.m_jobList.FindAll(job => job.m_status == JobStatus.Waiting).Count;
                finishedCount = m_bupTask.m_jobList.FindAll(job => job.m_status == JobStatus.Finished).Count;
                compressingCount = m_bupTask.m_jobList.FindAll(job => job.m_status == JobStatus.Working).Count;
            }

            lock (Console.Out)
            {
                Console.SetCursorPosition(0, m_statusLine + 2);
                Console.Write(Resources.ConsoleCompressionStatus.PadRight(m_consoleWidth), totalCount, compressingCount, waitingCount, finishedCount, skippedCount);
            }
        }

        private static void WriteConsole(int instanceId, int jobId, string jobName, byte progress, string file, string eta)
        {
            int instanceLine;
            int instance;
            int lastInstanceLine;

            lock (m_registeredInstances)
            {
                instance = m_registeredInstances.FindIndex(i => i == instanceId);
            }

            lock (m_instanceLines)
            {
                instanceLine = m_instanceLines[instance];
                lastInstanceLine = m_instanceLines.Last() + 3;
            }

            lock (Console.Out)
            {
                Console.SetCursorPosition(0, instanceLine);
                var restWidth = m_consoleWidth - 9;
                var shorted = string.Empty;

                if (jobId != -1)
                {
                    shorted = jobName.Substring(0, jobName.Length > restWidth ? restWidth - 3 : jobName.Length);
                    shorted = shorted.Length < jobName.Length ? shorted + "..." : shorted;
                    shorted = shorted.Length < restWidth ? shorted.PadRight(restWidth) : shorted;
                    Console.Write(@" #{0,4}: {1}", jobId, shorted);
                }
                else
                {
                    Console.Write(@"{0}", shorted.PadRight(m_consoleWidth));
                }

                shorted = string.Empty;

                Console.SetCursorPosition(0, instanceLine + 1);
                restWidth = m_consoleWidth - eta.Length - 12;
                
                if (jobId != -1)
                {
                    shorted = file.Substring(0, file.Length > restWidth ? restWidth - 3 : file.Length);
                    shorted = shorted.Length < file.Length ? shorted + "..." : shorted;
                    shorted = shorted.Length < restWidth ? shorted.PadRight(restWidth) : shorted;
                    Console.Write(@" {0,3}% | {1} | {2}", progress, shorted, eta);
                }
                else
                {
                    Console.Write(@"{0}", shorted.PadRight(m_consoleWidth));
                }

                Console.SetCursorPosition(0, lastInstanceLine);
            }
        }

        private static void RegisterInstance(int instanceId)
        {
            lock (m_registeredInstances)
            {
                var freeInstance = m_registeredInstances.FindIndex(i => i == -1);
                m_registeredInstances[freeInstance] = instanceId;
            }
        }

        private static void UnRegisterInstance(int instanceId)
        {
            lock (m_registeredInstances)
            {
                var instance = m_registeredInstances.FindIndex(i => i == instanceId);
                m_registeredInstances[instance] = -1;
            }
        }
    }
}
