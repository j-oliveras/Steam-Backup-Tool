namespace steamBackup.AppServices
{
    using Properties;
    using System;

    /// <summary>
    /// The definition of the interface which supports the cancellation of a process.
    /// </summary>
    public interface ICancellable
    {
        /// <summary>
        /// Gets or sets whether to stop the current archive operation.
        /// </summary>
        bool Cancel { get; set; }
    }

    /// <summary>
    /// EventArgs for storing PercentDone property.
    /// </summary>
    public class PercentDoneEventArgs : EventArgs
    {
        private readonly byte _percentDone;

        /// <summary>
        /// Initializes a new instance of the PercentDoneEventArgs class.
        /// </summary>
        /// <param name="percentDone">The percent of finished work.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        public PercentDoneEventArgs(byte percentDone)
        {
            if (percentDone > 100)
            {
                throw new ArgumentOutOfRangeException("percentDone", Resources.PercentRangeError);
            }
            _percentDone = percentDone;
        }

        /// <summary>
        /// Gets the percent of finished work.
        /// </summary>
        public byte PercentDone
        {
            get
            {
                return _percentDone;
            }
        }

        /// <summary>
        /// Converts a [0, 1] rate to its percent equivalent.
        /// </summary>
        /// <param name="doneRate">The rate of the done work.</param>
        /// <returns>Percent integer equivalent.</returns>
        /// <exception cref="System.ArgumentException"/>
        internal static byte ProducePercentDone(float doneRate)
        {
            return (byte)Math.Round(Math.Min(100 * doneRate, 100), MidpointRounding.AwayFromZero);
        }
    }

    /// <summary>
    /// The EventArgs class for accurate progress handling.
    /// </summary>
    public sealed class ProgressEventArgs : PercentDoneEventArgs
    {
        private readonly byte _delta;

        /// <summary>
        /// Initializes a new instance of the ProgressEventArgs class.
        /// </summary>
        /// <param name="percentDone">The percent of finished work.</param>
        /// <param name="percentDelta">The percent of work done after the previous event.</param>
        public ProgressEventArgs(byte percentDone, byte percentDelta)
            : base(percentDone)
        {
            _delta = percentDelta;
        }

        /// <summary>
        /// Gets the change in done work percentage.
        /// </summary>
        public byte PercentDelta
        {
            get
            {
                return _delta;
            }
        }
    }

    /// <summary>
    /// EventArgs class which stores the file name.
    /// </summary>
    public sealed class FileNameEventArgs : PercentDoneEventArgs, ICancellable
    {
        private readonly string _fileName;

        /// <summary>
        /// Initializes a new instance of the FileNameEventArgs class.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="percentDone">The percent of finished work</param>
        public FileNameEventArgs(string fileName, byte percentDone) :
            base(percentDone)
        {
            _fileName = fileName;
        }

        /// <summary>
        /// Gets or sets whether to stop the current archive operation.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }
    }
}