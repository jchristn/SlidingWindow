using System;
using System.IO;

namespace SlidingWindow
{
    /// <summary>
    /// Perform a sliding window operation over data read from a stream.
    /// </summary>
    public class Streams
    {
        #region Public-Members

        /// <summary>
        /// The size of a chunk.
        /// </summary>
        public int ChunkSize
        {
            get
            {
                return _ChunkSize;
            }
        }

        /// <summary>
        /// The number of bytes by which the window should be shifted.
        /// </summary>
        public int ShiftSize
        {
            get
            {
                return _ShiftSize;
            }
        }

        /// <summary>
        /// Ordinal position for the next start.
        /// </summary>
        public long NextStartPosition
        {
            get
            {
                return _NextStartPosition;
            }
        }

        /// <summary>
        /// Number of bytes within the content length that have not yet been read from the stream.
        /// </summary>
        public long BytesRemaining
        {
            get
            {
                return _BytesRemaining;
            }
        }

        /// <summary>
        /// The previous chunk's data, or null if no chunk has been retrieved or AdvanceToNewChunk() was called.
        /// </summary>
        public byte[] PreviousChunk
        {
            get
            {
                return _PreviousChunk;
            }
        }

        #endregion

        #region Private-Members

        private Stream _Stream = null;
        private long _ContentLength = 0;
        private int _ChunkSize = 0;
        private int _ShiftSize = 0;
        private long _NextStartPosition = 0;
        private long _BytesRemaining = 0;

        private byte[] _PreviousChunk = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="stream">Stream containing data to chunk using a sliding window.</param>
        /// <param name="contentLength">The number of bytes to expect within the stream.</param>
        /// <param name="chunkSize">Size of the data chunks to return.</param>
        /// <param name="shiftSize">Number of bytes the sliding window should shift on each call to GetNextChunk().</param>
        public Streams(Stream stream, long contentLength, int chunkSize, int shiftSize)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new ArgumentException("Cannot read from supplied stream.");
            if (contentLength < 1) throw new ArgumentException("Content length must be one or greater.");
            if (chunkSize < 1) throw new ArgumentException("Chunk size must be at least one.");
            if (chunkSize > contentLength) throw new ArgumentException("Chunk size must not exceed content length.");
            if (shiftSize < 1) throw new ArgumentException("Shift size must be at least one.");
            if (shiftSize > chunkSize) throw new ArgumentException("Shift size must not exceed chunk size.");

            _Stream = stream;
            _ContentLength = contentLength;
            _ChunkSize = chunkSize;
            _ShiftSize = shiftSize;

            _NextStartPosition = 0;
            _BytesRemaining = contentLength;

            _PreviousChunk = null;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Retrieve the total number of chunks that will be returned.
        /// </summary>
        /// <returns>Number of chunks that will be returned.</returns>
        public int ChunkCount()
        {
            return 1 + (int)Math.Ceiling(
                (double)(_ContentLength - _ChunkSize)
                /
                (double)(_ShiftSize)
            );
        }

        /// <summary>
        /// Retrieve the next chunk.
        /// Chunks of size 'chunkSize' will be returned, unless fewer bytes remain.
        /// The final chunk will be of the remaining size.
        /// Once the final chunk has been returned, subsequent calls return null.
        /// </summary>
        /// <param name="position">Indicates the starting position of the chunk in the stream, or -1 if no data remains.</param>
        /// <param name="newData">Data included in this chunk that was not included in any previously returned chunk, or null if no data remains.</param>
        /// <param name="finalChunk">Indicates if the chunk retrieved is the final chunk, or if no data remains.</param>
        /// <returns>Byte array containing chunk data, or null if no data remains.</returns>
        /// <exception cref="EndOfStreamException">The stream ended before the specified content length was read.</exception>
        /// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
        public byte[] GetNextChunk(out long position, out byte[] newData, out bool finalChunk)
        {
            position = -1;
            newData = null;
            finalChunk = true;

            if (_BytesRemaining < 1) return null;

            long consumed = _ContentLength - _BytesRemaining;
            long start = _NextStartPosition;
            long end = Math.Min(start + _ChunkSize, _ContentLength);

            byte[] ret = new byte[end - start];

            int retained = 0;
            if (_PreviousChunk != null && start < consumed)
            {
                retained = (int)(consumed - start);
                Buffer.BlockCopy(_PreviousChunk, _PreviousChunk.Length - retained, ret, 0, retained);
            }

            int bytesToRead = ret.Length - retained;
            ReadExactly(_Stream, ret, retained, bytesToRead);
            _BytesRemaining -= bytesToRead;

            newData = new byte[bytesToRead];
            Buffer.BlockCopy(ret, retained, newData, 0, bytesToRead);

            _PreviousChunk = new byte[ret.Length];
            Buffer.BlockCopy(ret, 0, _PreviousChunk, 0, ret.Length);

            position = start;
            finalChunk = (end >= _ContentLength);
            _NextStartPosition += _ShiftSize;
            return ret;
        }

        /// <summary>
        /// Advance to the position starting after the end of the previous chunk.
        /// Calling GetNextChunk() will then return a chunk full of new data.
        /// Has no effect if no chunk has been retrieved since construction or the last advance.
        /// </summary>
        public void AdvanceToNewChunk()
        {
            if (_PreviousChunk == null) return;

            _PreviousChunk = null;
            _NextStartPosition = _ContentLength - _BytesRemaining;
        }

        #endregion

        #region Private-Methods

        private void ReadExactly(Stream stream, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int bytesRead = stream.Read(buffer, offset, count);
                if (bytesRead < 1)
                    throw new EndOfStreamException(
                        "Stream ended before the content length of " + _ContentLength + " bytes was reached; "
                        + count + " bytes of the current chunk could not be read.");

                offset += bytesRead;
                count -= bytesRead;
            }
        }

        #endregion
    }
}
