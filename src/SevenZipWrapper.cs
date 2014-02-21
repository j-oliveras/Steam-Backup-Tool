namespace steamBackup
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using SevenZip;


    public class SevenZipWrapper
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void ProgressCallback(int value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void FileNameCallback([MarshalAs(UnmanagedType.LPWStr)]string filter);

        [DllImport("SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr InitLibrary(string libraryPath, string archiveName,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback pCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] FileNameCallback fnCallback);

        [DllImport("SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetCompressionLevel(IntPtr handle, int compressionLevel);

        [DllImport("SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseMt(IntPtr handle, bool useMt);

        [DllImport("SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetMtNumCores(IntPtr handle, int mtNumCores);

        [DllImport("SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseSolid(IntPtr handle, bool useSolid);

        [DllImport("SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseLzma2(IntPtr handle, bool useLzma2);

        [DllImport("SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern string GetFileName(IntPtr handle);

        [DllImport("SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetProgress(IntPtr handle);

        [DllImport("SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CancelCompression(IntPtr handle);

        [DllImport("SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CompressFileList(IntPtr handle, string pathPrefix, string[] filePaths, int count);

        private bool _useMt;
        private int _mtNumCores;
        private bool _useLzma2;
        private bool _useSolidCompression;
        private int _compressionLevel;
        private bool _cancel;
        private bool _compressionActive;

        public event EventHandler<ProgressEventArgs> Compressing;
        public event EventHandler<FileNameEventArgs> FileCompressionStarted;
        public event EventHandler<EventArgs> CompressionFinished;

        private IntPtr _libHandle;

        private ProgressCallback progressCallback;
        private FileNameCallback fileNameCallback;

        private int progress;
        private int fileName;

        public bool UseMultithreading
        {
            get { return _useMt; }
            set
            {
                _useMt = value;
                SetUseMt(_libHandle, value);
            }
        }

        public int MultithreadingNumThreads
        {
            get { return _mtNumCores; }
            set
            {
                _mtNumCores = value;
                SetMtNumCores(_libHandle, value);
            }
        }

        public int CompressionLevel
        {
            get { return _compressionLevel; }
            set
            {
                _compressionLevel = value;
                SetCompressionLevel(_libHandle, value);
            }
        }

        public bool UseLzma2Compression
        {
            get { return _useLzma2; }
            set
            {
                _useLzma2 = value;
                SetUseLzma2(_libHandle, value);
            }
        }

        public bool UseSolidCompression
        {
            get { return _useSolidCompression; }
            set
            {
                _useSolidCompression = value;
                SetUseSolid(_libHandle, value);
            }
        }

        public SevenZipWrapper(string libraryPath, string archiveName)
        {
            fileNameCallback = value =>
            {
                string fPath = Path.GetFileName(value);
                if (FileCompressionStarted != null)
                {
                    FileNameEventArgs ev = new FileNameEventArgs(fPath, (byte) progress);
                    FileCompressionStarted(this, ev);
                    if (ev.Cancel)
                        Cancel();
                }
            };
            progressCallback = value =>
            {
                progress = value;
                if (Compressing != null)
                {
                    Compressing(this, new ProgressEventArgs((byte)progress, (byte)(100 - progress)));
                }
            };
            _libHandle = InitLibrary(libraryPath, archiveName, progressCallback, fileNameCallback);
        }

        public void Cancel()
        {
            _cancel = true;
            CancelCompression(_libHandle);
        }

        public void CompressFiles(string pathPrefix, string[] filePaths)
        {
            _compressionActive = true;
            CompressFileList(_libHandle, pathPrefix, filePaths, filePaths.Length);
            _compressionActive = false;
            if (CompressionFinished != null)
            {
                CompressionFinished(this, new EventArgs());
            }
            _cancel = false;
        }
    }
}