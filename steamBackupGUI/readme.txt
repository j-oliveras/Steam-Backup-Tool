# README #

### Steam Backup Tool (SBT) ###

This tool has been created to replace the built in Steam backup and restore utility. Generally speaking it will compress games faster and/or smaller (depending on the settings) than the default backup feature in Steam. This tool also tends to be a lot more stable and it doesn't break the file into segments unlike the utility built into steam.

More information is available at the Wiki: bitbucket.org/Du-z/steam-backup-tool/wiki

### Main Features ###

* Includes a command  line interface so you can setup scheduled backups.
* After the initial Backup you have the ability to only backup games that have been updated since the previous update.
* Choose what games to backup and restore.
* Choose to restore to an alternative steam library.*
* Automatically installs games after restore.*
* Fully multithreaded and optimized for up to 8 core CPUs.
* Choose the compression level and how many thread to use.
* Will only use "spare" CPU time.
* Can automatically find steam folder.
* Checks online for new updates.
* High compression ratio due to 7z's excellent LZMA and LZMA2 compression algorithms.

(* If the game uses steams new cache format and this cache has the correct information available.)

### Using Steam Backup Tool with the Command Line ###

```
usage: steamBackupCLI [options]

Parameters:
  -h, -?, --help             show this message and exit.
  -O, --out-dir=VALUE        (required) Set backup directory
  -S, --steam-dir=VALUE      Do not automatically detect Steam directory, use
                               this directory instead
  -2, --lzma2                Use LZMA2 compression.
  -C, --compression=VALUE    Set compression level. Possible values 0 - 5:
                                0 : Copy
                                1 : Fastest
                                2 : Fast
                                3 : Normal
                                4 : Maximum
                                5 : Ultra
  -B, --backup               Update backup
                               Update games that have been changed since the
                               last backup, EXCLUDING games that have not been
                               backed up yet.
  -L, --library              Update library
                               Update games that have been changed since the
                               last backup, INCLUDING games that have not been
                               backed up yet.
  -D, --delete               Delete all backup files before starting
                               ignored when either update library or update
                               backup parameter is used
  -T, --threads=VALUE        Thread count
                               LZMA:  number of concurrent instances,
                               LZMA2: number of threads used
```


### How do I report bugs? ###

You can report bugs at the following link: bitbucket.org/Du-z/steam-backup-tool/issues

If you have programming experience you are welcome to submit a GIT patch.

### How do I contribute? ###

If you wish to contribute to the tool you can submit a enhancement through a couple of ways.
The issue tracker: bitbucket.org/Du-z/steam-backup-tool/issues
Overclock.net forum thread: overclock.net/t/969143/open-source-automatic-steam-backup-restore-tool

If you wish to contribute directly to the code base you can submit GIT patches through the issue tracker.

If you want to contribute more heavily you can request write permissions to the Bitbucket repository by messaging Du-z.

All contributors are recognized in the tools credits. 

### Version Changelist ###

```
v1.8.2
* Target path was generated incorrectly during a restore.

v1.8.1
+ Command-line interface added
^ Reduced application executable size
^ Internal path processing/building now done using .NET's built-in Classes
^ A lot of code refactoring/polishing, i.e. splitting execution code from UI code etc.

v1.7.1
* Fixed a bug that could create more threads in LZMA2 than is possible
+ Added check box that removes thread limitation for LZMA2
+ Website label to About Box
^ Error logging
^ Ram usage prediction

v1.7.0
+ Checks online for tool updates
^ Main Menu title blurb
* Fixed a crash during start of backup when config.sbt is missing

v1.6.2.1
* suppress exceptions at ETA calculation

v1.6.2
+ display compression/decompression speed and ETA for each instance
^ Improved JSON (de-)serialization
^ use statically linked 7z wrapper, installed VC++ Redistributable Packages no longer required
^ extract hardcoded text into application resources
^ use data binding on CheckedListBox controls, instead accessing them directly

v1.6.1.1
+ Notifies user if Visual C++ Redistributable Packages for Visual Studio 2013 is not installed.

v1.6.1
+ Enable thread count selection when using LZMA2 compression
^ Now uses a higher performance 7zip Wrapper
^ Updated 7z.dll
^ Disable backup buttons while the check list box is being populated
* Removed backup/restore of the generic "steamapps" folder (Valve has made this redundant)
* Fixed the library drop down box on the restore menu not getting a default selection.
* Can now determine if steam is using a 'steamapps' or 'SteamApps' folder. (Could be either, Cause of the above bug)
^ Added people to credits

v1.6.0
+ Add support for lzma2 compression
* Fix backup of .acf files, where 'installdir' contains relaative installation path

v1.5.3
^ Improved the data that is written to the info boxes.
^ Update Backup button now only selects archives that already exists.
+ Update library will update all available games.

v1.5.2
+ Improved checks for valid backups and steam folder.

v1.5.1
* Fixed the tool from considering a new installation of steam as invalid (ie missing steamapps folder)

v1.5.0
^ Totally refactored code (Improved performance and reliability)
^ Now uses a 7z library rather than command line to a external process
* Fixed a few typos
______________________________________

+ Added
- Removed
^ Updated
* Bugfix
```