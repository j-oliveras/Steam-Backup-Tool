namespace steamBackup.AppServices.SevenZipWrapper
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    public class SevenZipWrapper : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void ProgressCallback(int value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void FileNameCallback([MarshalAs(UnmanagedType.LPWStr)]string fileName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void TotalSizeCallback(UInt64 value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void ProcessedSizeCallback(UInt64 value);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr InitCompressLibrary(string libraryPath, string archiveName,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback pCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] FileNameCallback fnCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] TotalSizeCallback tsCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProcessedSizeCallback psCallback);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr InitDecompressLibrary(string libraryPath, string archiveName,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback pCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] FileNameCallback fnCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] TotalSizeCallback tsCallback,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] ProcessedSizeCallback psCallback);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyCompressLibrary(IntPtr handle);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyDecompressLibrary(IntPtr handle);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetCompressionLevel(IntPtr handle, int compressionLevel);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseMt(IntPtr handle, bool useMt);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetMtNumCores(IntPtr handle, int mtNumCores);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseSolid(IntPtr handle, bool useSolid);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetUseLzma2(IntPtr handle, bool useLzma2);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CancelCompression(IntPtr handle);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CompressFileList(IntPtr handle, string pathPrefix, string[] filePaths, int count);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CancelDecompression(IntPtr handle);

        [DllImport(@"rsc\SevenZip++Lib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DecompressArchive(IntPtr handle, string targetPath);

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

        public ulong TotalSize { get; set; }

        public ulong ProcessedSize { get; set; }

        public SevenZipWrapper(string libraryPath, string archiveName, bool decompressor)
        {
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

                _libHandle = InitDecompressLibrary(libraryPath, archiveName, _progressCallback, _fileNameCallback, _totalSizeCallback, _processedSizeCallback);
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

                _libHandle = InitCompressLibrary(libraryPath, archiveName, _progressCallback, _fileNameCallback, _totalSizeCallback, _processedSizeCallback);
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
                DestroyDecompressLibrary(_libHandle);
            else
                DestroyCompressLibrary(_libHandle);

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
                CancelDecompression(_libHandle);
            else
                CancelCompression(_libHandle);
        }

        public void CompressFiles(string pathPrefix, string[] filePaths)
        {
            CompressFileList(_libHandle, pathPrefix, filePaths, filePaths.Length);
            if (CompressionFinished != null)
            {
                CompressionFinished(this, new EventArgs());
            }
        }

        public void DecompressFileArchive(string targetPath)
        {
            DecompressArchive(_libHandle, targetPath);
            if (ExtractionFinished != null)
            {
                ExtractionFinished(this, new EventArgs());
            }
        }
    }
}