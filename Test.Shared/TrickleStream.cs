namespace Test.Shared
{
    using System;
    using System.IO;

    /// <summary>
    /// Read-only stream that returns at most a fixed number of bytes per Read call, like a network stream.
    /// Throws <see cref="TestAssertionException"/> after repeated reads at end of stream so that a caller
    /// stuck in a read loop fails instead of hanging.
    /// </summary>
    public class TrickleStream : Stream
    {
        /// <summary>
        /// Maximum bytes returned per Read call.
        /// </summary>
        public int MaxBytesPerRead { get; }

        /// <summary>
        /// Total number of bytes returned to callers.
        /// </summary>
        public long TotalBytesRead
        {
            get { return _Inner.Position; }
        }

        /// <inheritdoc />
        public override bool CanRead
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <inheritdoc />
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <inheritdoc />
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <inheritdoc />
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        private readonly MemoryStream _Inner;
        private int _ReadsAtEnd = 0;
        private const int _MaxReadsAtEnd = 1000;

        /// <summary>
        /// Instantiate the stream, returning as many bytes as requested per Read call.
        /// </summary>
        /// <param name="data">Data to serve.</param>
        public TrickleStream(byte[] data) : this(data, Int32.MaxValue)
        {
        }

        /// <summary>
        /// Instantiate the stream.
        /// </summary>
        /// <param name="data">Data to serve.</param>
        /// <param name="maxBytesPerRead">Maximum bytes returned per Read call.</param>
        public TrickleStream(byte[] data, int maxBytesPerRead)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (maxBytesPerRead < 1) throw new ArgumentOutOfRangeException(nameof(maxBytesPerRead));
            _Inner = new MemoryStream(data, false);
            MaxBytesPerRead = maxBytesPerRead;
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = _Inner.Read(buffer, offset, Math.Min(count, MaxBytesPerRead));
            if (read == 0 && count > 0)
            {
                _ReadsAtEnd++;
                if (_ReadsAtEnd >= _MaxReadsAtEnd)
                    throw new TestAssertionException("Caller kept reading after end of stream (" + _ReadsAtEnd + " zero-byte reads)");
            }
            return read;
        }

        /// <inheritdoc />
        public override void Flush()
        {
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing) _Inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
