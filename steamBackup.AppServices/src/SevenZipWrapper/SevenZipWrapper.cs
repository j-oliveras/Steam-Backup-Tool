namespace steamBackup.AppServices
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Reflection;

    public class SevenZipWrapper : IDisposable
    {
        private static readonly bool m_is64Bit = Environment.Is64BitProcess;

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

        private bool m_useMt;
        private int m_mtNumCores;
        private bool m_useLzma2;
        private bool m_useSolidCompression;
        private int m_compressionLevel;

        public event EventHandler<ProgressEventArgs> Compressing;
        public event EventHandler<FileNameEventArgs> FileCompressionStarted;
        public event EventHandler<EventArgs> CompressionFinished;

        public event EventHandler<ProgressEventArgs> Extracting;
        public event EventHandler<FileNameEventArgs> FileExtractionStarted;
        public event EventHandler<EventArgs> ExtractionFinished;

        private IntPtr m_libHandle;

        private int m_progress;

        private ProgressCallback m_progressCallback;
        private FileNameCallback m_fileNameCallback;
        private TotalSizeCallback m_totalSizeCallback;
        private ProcessedSizeCallback m_processedSizeCallback;

        public bool UseMultithreading
        {
            get { return m_useMt; }
            set
            {
                m_useMt = value;

                if (m_is64Bit)
                    SetUseMt64(m_libHandle, value);
                else
                    SetUseMt32(m_libHandle, value);
            }
        }

        public int MultithreadingNumThreads
        {
            get { return m_mtNumCores; }
            set
            {
                m_mtNumCores = value;

                if (m_is64Bit)
                    SetMtNumCores64(m_libHandle, value);
                else
                    SetMtNumCores32(m_libHandle, value);
            }
        }

        public int CompressionLevel
        {
            get { return m_compressionLevel; }
            set
            {
                m_compressionLevel = value;

                if (m_is64Bit)
                    SetCompressionLevel64(m_libHandle, value);
                else
                    SetCompressionLevel32(m_libHandle, value);
            }
        }

        public bool UseLzma2Compression
        {
            get { return m_useLzma2; }
            set
            {
                m_useLzma2 = value;

                if (m_is64Bit)
                    SetUseLzma264(m_libHandle, value);
                else
                    SetUseLzma232(m_libHandle, value);
            }
        }

        public bool UseSolidCompression
        {
            get { return m_useSolidCompression; }
            set
            {
                m_useSolidCompression = value;

                if (m_is64Bit)
                    SetUseSolid64(m_libHandle, value);
                else
                    SetUseSolid32(m_libHandle, value);
            }
        }

        public ulong m_totalSize { get; set; }

        public ulong m_processedSize { get; set; }

        public SevenZipWrapper(string archiveName, bool decompressor)
        {
            var rootDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            // check for IntPtr.Size == 8 does not work here, as we are in a 32 bit process, IntPtr.Size is always 4
            var libraryPath = Path.Combine(rootDir, "rsc", m_is64Bit ? "64" : "32", "7z.dll");
            
            if (decompressor)
            {
                m_fileNameCallback = value =>
                {
                    var fPath = Path.GetFileName(value);
                    if (FileExtractionStarted == null) return;

                    var ev = new FileNameEventArgs(fPath, (byte)m_progress);
                    FileExtractionStarted(this, ev);
                    if (ev.Cancel)
                        Cancel(false);
                };
                m_progressCallback = value =>
                {
                    m_progress = value;
                    if (Extracting != null)
                    {
                        Extracting(this, new ProgressEventArgs((byte)m_progress, (byte)(100 - m_progress)));
                    }
                };
                m_totalSizeCallback = value =>
                {
                    m_totalSize = value;
                };
                m_processedSizeCallback = value =>
                {
                    m_processedSize = value;
                };

                if (m_is64Bit)
                    m_libHandle = InitDecompressLibrary64(libraryPath, archiveName, m_progressCallback, m_fileNameCallback,
                        m_totalSizeCallback, m_processedSizeCallback);
                else
                    m_libHandle = InitDecompressLibrary32(libraryPath, archiveName, m_progressCallback, m_fileNameCallback,
                        m_totalSizeCallback, m_processedSizeCallback);
            }
            else
            {
                m_fileNameCallback = value =>
                {
                    var fPath = Path.GetFileName(value);
                    if (FileCompressionStarted == null) return;

                    var ev = new FileNameEventArgs(fPath, (byte) m_progress);
                    FileCompressionStarted(this, ev);
                    if (ev.Cancel)
                        Cancel(false);
                };
                m_progressCallback = value =>
                {
                    m_progress = value;
                    if (Compressing != null)
                    {
                        Compressing(this, new ProgressEventArgs((byte) m_progress, (byte) (100 - m_progress)));
                    }
                };
                m_totalSizeCallback = value =>
                {
                    m_totalSize = value;
                };
                m_processedSizeCallback = value =>
                {
                    m_processedSize = value;
                };

                if (m_is64Bit)
                    m_libHandle = InitCompressLibrary64(libraryPath, archiveName, m_progressCallback, m_fileNameCallback,
                        m_totalSizeCallback, m_processedSizeCallback);
                else
                    m_libHandle = InitCompressLibrary32(libraryPath, archiveName, m_progressCallback, m_fileNameCallback,
                        m_totalSizeCallback, m_processedSizeCallback);
            }
        }


        ~SevenZipWrapper()
        {
            m_fileNameCallback = null;
            m_progressCallback = null;
            m_processedSizeCallback = null;
            m_totalSizeCallback = null;
        }

        public void Dispose(bool decompressor)
        {
            if (decompressor)
            {
                if (m_is64Bit)
                    DestroyDecompressLibrary64(m_libHandle);
                else
                    DestroyDecompressLibrary32(m_libHandle);
            }
            else
            {
                if (m_is64Bit)
                    DestroyCompressLibrary64(m_libHandle);
                else
                    DestroyCompressLibrary32(m_libHandle);
            }

            m_fileNameCallback = null;
            m_progressCallback = null;
            m_processedSizeCallback = null;
            m_totalSizeCallback = null;

            m_libHandle = new IntPtr();
            Dispose();
        }

        public void Dispose()
        {
            
        }

        public void Cancel(bool decompressor)
        {
            if (decompressor)
            {
                if (m_is64Bit)
                    CancelDecompression64(m_libHandle);
                else
                    CancelDecompression32(m_libHandle);
            }
            else
            {
                if (m_is64Bit)
                    CancelCompression64(m_libHandle);
                else
                    CancelCompression32(m_libHandle);
            }
        }

        public void CompressFiles(string pathPrefix, string[] filePaths)
        {
            if (!pathPrefix.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                pathPrefix += Path.DirectorySeparatorChar;

            if (m_is64Bit)
                CompressFileList64(m_libHandle, pathPrefix, filePaths, filePaths.Length);
            else
                CompressFileList32(m_libHandle, pathPrefix, filePaths, filePaths.Length);

            if (CompressionFinished != null)
            {
                CompressionFinished(this, new EventArgs());
            }
        }

        public void DecompressFileArchive(string targetPath)
        {
            if (m_is64Bit)
                DecompressArchive64(m_libHandle, targetPath);
            else
                DecompressArchive32(m_libHandle, targetPath);

            if (ExtractionFinished != null)
            {
                ExtractionFinished(this, new EventArgs());
            }
        }
    }
}