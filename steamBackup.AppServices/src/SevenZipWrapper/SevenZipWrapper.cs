namespace steamBackup.AppServices.SevenZipWrapper
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Reflection;

    public class SevenZipWrapper : IDisposable
    {
        private static readonly bool Is64Bit = Environment.Is64BitProcess;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void ProgressCallback(int value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void FileNameCallback([MarshalAs(UnmanagedType.LPWStr)]string fileName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void TotalSizeCallback(UInt64 value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void ProcessedSizeCallback(UInt64 value);

        // 64bit DLL Functions

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "InitCompressLibrary", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr InitCompressLibrary64(string libraryPath, string archiveName,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback pCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] FileNameCallback fnCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] TotalSizeCallback tsCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProcessedSizeCallback psCallback);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "InitDecompressLibrary", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr InitDecompressLibrary64(string libraryPath, string archiveName,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback pCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] FileNameCallback fnCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] TotalSizeCallback tsCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProcessedSizeCallback psCallback);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "DestroyCompressLibrary", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyCompressLibrary64(IntPtr handle);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "DestroyDecompressLibrary", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyDecompressLibrary64(IntPtr handle);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "SetCompressionLevel", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetCompressionLevel64(IntPtr handle, int compressionLevel);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "SetUseMt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseMt64(IntPtr handle, bool useMt);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "SetMtNumCores", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetMtNumCores64(IntPtr handle, int mtNumCores);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "SetUseSolid", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseSolid64(IntPtr handle, bool useSolid);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "SetUseLzma2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseLzma264(IntPtr handle, bool useLzma2);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "CancelCompression", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CancelCompression64(IntPtr handle);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "CompressFileList", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CompressFileList64(IntPtr handle, string pathPrefix, string[] filePaths, int count);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "CancelDecompression", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CancelDecompression64(IntPtr handle);

        [DllImport(@"rsc\64\SevenZip++Lib.dll", EntryPoint = "DecompressArchive", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DecompressArchive64(IntPtr handle, string targetPath);

        // 32bit DLL Functions

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "InitCompressLibrary", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr InitCompressLibrary32(string libraryPath, string archiveName,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback pCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] FileNameCallback fnCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] TotalSizeCallback tsCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProcessedSizeCallback psCallback);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "InitDecompressLibrary", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr InitDecompressLibrary32(string libraryPath, string archiveName,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback pCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] FileNameCallback fnCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] TotalSizeCallback tsCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProcessedSizeCallback psCallback);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "DestroyCompressLibrary", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyCompressLibrary32(IntPtr handle);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "DestroyDecompressLibrary", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyDecompressLibrary32(IntPtr handle);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "SetCompressionLevel", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetCompressionLevel32(IntPtr handle, int compressionLevel);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "SetUseMt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseMt32(IntPtr handle, bool useMt);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "SetMtNumCores", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetMtNumCores32(IntPtr handle, int mtNumCores);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "SetUseSolid", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseSolid32(IntPtr handle, bool useSolid);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "SetUseLzma2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseLzma232(IntPtr handle, bool useLzma2);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "CancelCompression", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CancelCompression32(IntPtr handle);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "CompressFileList", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CompressFileList32(IntPtr handle, string pathPrefix, string[] filePaths, int count);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "CancelDecompression", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CancelDecompression32(IntPtr handle);

        [DllImport(@"rsc\32\SevenZip++Lib.dll", EntryPoint = "DecompressArchive", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DecompressArchive32(IntPtr handle, string targetPath);

        private bool _useMt;
        private int _mtNumCores;
        private bool _useLzma2;
        private bool _useSolidCompression;
        private int _compressionLevel;

        public event EventHandler<ProgressEventArgs> Compressing;
        public event EventHandler<FileNameEventArgs> FileCompressionStarted;
        public event EventHandler<EventArgs> CompressionFinished;

        public event EventHandler<ProgressEventArgs> Extracting;
        public event EventHandler<FileNameEventArgs> FileExtractionStarted;
        public event EventHandler<EventArgs> ExtractionFinished;

        private IntPtr _libHandle;

        private int _progress;

        private ProgressCallback _progressCallback;
        private FileNameCallback _fileNameCallback;
        private TotalSizeCallback _totalSizeCallback;
        private ProcessedSizeCallback _processedSizeCallback;

        public bool UseMultithreading
        {
            get { return _useMt; }
            set
            {
                _useMt = value;

                if (Is64Bit)
                    SetUseMt64(_libHandle, value);
                else
                    SetUseMt32(_libHandle, value);
            }
        }

        public int MultithreadingNumThreads
        {
            get { return _mtNumCores; }
            set
            {
                _mtNumCores = value;

                if (Is64Bit)
                    SetMtNumCores64(_libHandle, value);
                else
                    SetMtNumCores32(_libHandle, value);
            }
        }

        public int CompressionLevel
        {
            get { return _compressionLevel; }
            set
            {
                _compressionLevel = value;

                if (Is64Bit)
                    SetCompressionLevel64(_libHandle, value);
                else
                    SetCompressionLevel32(_libHandle, value);
            }
        }

        public bool UseLzma2Compression
        {
            get { return _useLzma2; }
            set
            {
                _useLzma2 = value;

                if (Is64Bit)
                    SetUseLzma264(_libHandle, value);
                else
                    SetUseLzma232(_libHandle, value);
            }
        }

        public bool UseSolidCompression
        {
            get { return _useSolidCompression; }
            set
            {
                _useSolidCompression = value;

                if (Is64Bit)
                    SetUseSolid64(_libHandle, value);
                else
                    SetUseSolid32(_libHandle, value);
            }
        }

        public ulong TotalSize { get; set; }

        public ulong ProcessedSize { get; set; }

        public SevenZipWrapper(string archiveName, bool decompressor)
        {
            var rootDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            // check for IntPtr.Size == 8 does not work here, as we are in a 32 bit process, IntPtr.Size is always 4
            var libraryPath = Path.Combine(rootDir, "rsc", Is64Bit ? "64" : "32", "7z.dll");
            
            if (decompressor)
            {
                _fileNameCallback = value =>
                {
                    var fPath = Path.GetFileName(value);
                    if (FileExtractionStarted == null) return;

                    var ev = new FileNameEventArgs(fPath, (byte)_progress);
                    FileExtractionStarted(this, ev);
                    if (ev.Cancel)
                        Cancel(false);
                };
                _progressCallback = value =>
                {
                    _progress = value;
                    if (Extracting != null)
                    {
                        Extracting(this, new ProgressEventArgs((byte)_progress, (byte)(100 - _progress)));
                    }
                };
                _totalSizeCallback = value =>
                {
                    TotalSize = value;
                };
                _processedSizeCallback = value =>
                {
                    ProcessedSize = value;
                };

                if (Is64Bit)
                    _libHandle = InitDecompressLibrary64(libraryPath, archiveName, _progressCallback, _fileNameCallback,
                        _totalSizeCallback, _processedSizeCallback);
                else
                    _libHandle = InitDecompressLibrary32(libraryPath, archiveName, _progressCallback, _fileNameCallback,
                        _totalSizeCallback, _processedSizeCallback);
            }
            else
            {
                _fileNameCallback = value =>
                {
                    var fPath = Path.GetFileName(value);
                    if (FileCompressionStarted == null) return;

                    var ev = new FileNameEventArgs(fPath, (byte) _progress);
                    FileCompressionStarted(this, ev);
                    if (ev.Cancel)
                        Cancel(false);
                };
                _progressCallback = value =>
                {
                    _progress = value;
                    if (Compressing != null)
                    {
                        Compressing(this, new ProgressEventArgs((byte) _progress, (byte) (100 - _progress)));
                    }
                };
                _totalSizeCallback = value =>
                {
                    TotalSize = value;
                };
                _processedSizeCallback = value =>
                {
                    ProcessedSize = value;
                };

                if (Is64Bit)
                    _libHandle = InitCompressLibrary64(libraryPath, archiveName, _progressCallback, _fileNameCallback,
                        _totalSizeCallback, _processedSizeCallback);
                else
                    _libHandle = InitCompressLibrary32(libraryPath, archiveName, _progressCallback, _fileNameCallback,
                        _totalSizeCallback, _processedSizeCallback);
            }
        }


        ~SevenZipWrapper()
        {
            _fileNameCallback = null;
            _progressCallback = null;
            _processedSizeCallback = null;
            _totalSizeCallback = null;
        }

        public void Dispose(bool decompressor)
        {
            if (decompressor)
            {
                if (Is64Bit)
                    DestroyDecompressLibrary64(_libHandle);
                else
                    DestroyDecompressLibrary32(_libHandle);
            }
            else
            {
                if (Is64Bit)
                    DestroyCompressLibrary64(_libHandle);
                else
                    DestroyCompressLibrary32(_libHandle);
            }

            _fileNameCallback = null;
            _progressCallback = null;
            _processedSizeCallback = null;
            _totalSizeCallback = null;

            _libHandle = new IntPtr();
            Dispose();
        }

        public void Dispose()
        {
            
        }

        public void Cancel(bool decompressor)
        {
            if (decompressor)
            {
                if (Is64Bit)
                    CancelDecompression64(_libHandle);
                else
                    CancelDecompression32(_libHandle);
            }
            else
            {
                if (Is64Bit)
                    CancelCompression64(_libHandle);
                else
                    CancelCompression32(_libHandle);
            }
        }

        public void CompressFiles(string pathPrefix, string[] filePaths)
        {
            if (!pathPrefix.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                pathPrefix += Path.DirectorySeparatorChar;

            if (Is64Bit)
                CompressFileList64(_libHandle, pathPrefix, filePaths, filePaths.Length);
            else
                CompressFileList32(_libHandle, pathPrefix, filePaths, filePaths.Length);

            if (CompressionFinished != null)
            {
                CompressionFinished(this, new EventArgs());
            }
        }

        public void DecompressFileArchive(string targetPath)
        {
            if (Is64Bit)
                DecompressArchive64(_libHandle, targetPath);
            else
                DecompressArchive32(_libHandle, targetPath);

            if (ExtractionFinished != null)
            {
                ExtractionFinished(this, new EventArgs());
            }
        }
    }
}