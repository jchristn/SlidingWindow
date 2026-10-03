using System;
using System.Text;

namespace SlidingWindow
{
    /// <summary>
    /// Perform a sliding window operation over a byte array.
    /// </summary>
    public class Bytes
    {
        #region Public-Members

        /// <summary>
        /// Byte array data.
        /// </summary>
        public byte[] Data
        {
            get
            {
                return _Data;
            }
        }

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
        public int NextStartPosition
        {
            get
            {
                return _NextStartPosition;
            }
        }

        /// <summary>
        /// Ordinal position of the next end.
        /// </summary>
        public int NextEndPosition
        {
            get
            {
                return _NextEndPosition;
            }
        }

        /// <summary>
        /// Number of bytes that have not yet been returned in any chunk.
        /// </summary>
        public int BytesRemaining
        {
            get
            {
                return _Data.Length - _ReturnedEndPosition;
            }
        }

        #endregion

        #region Private-Members

        private byte[] _Data = null;
        private int _ChunkSize = 0;
        private int _ShiftSize = 0;
         
        private int _NextStartPosition = 0;
        private int _NextEndPosition = 0;
        private int _ReturnedEndPosition = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="data">Byte data to chunk using a sliding window.</param>
        /// <param name="chunkSize">Size of the data chunks to return.</param>
        /// <param name="shiftSize">Number of bytes the sliding window should shift on each call to GetNextChunk().</param>
        public Bytes(byte[] data, int chunkSize, int shiftSize)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 1) throw new ArgumentException("Data must have at least one byte.");
            if (chunkSize < 1) throw new ArgumentException("Chunk size must be at least one.");
            if (chunkSize > data.Length) throw new ArgumentException("Chunk size must not exceed data length.");
            if (shiftSize < 1) throw new ArgumentException("Shift size must be at least one.");
            if (shiftSize > chunkSize) throw new ArgumentException("Shift size must not exceed chunk size.");

            _Data = new byte[data.Length];
            Buffer.BlockCopy(data, 0, _Data, 0, data.Length);

            _ChunkSize = chunkSize;
            _ShiftSize = shiftSize;
             
            _NextStartPosition = 0;
            _NextEndPosition = _NextStartPosition + _ChunkSize;
            _ReturnedEndPosition = 0;
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
                (double)(_Data.Length - _ChunkSize)
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
        /// <param name="position">Indicates the starting position of the chunk in the byte array, or -1 if no data remains.</param>
        /// <param name="newData">Data included in this chunk that was not included in any previously returned chunk, or null if no data remains.</param>
        /// <param name="finalChunk">Indicates if the chunk retrieved is the final chunk, or if no data remains.</param>
        /// <returns>Byte array containing chunk data, or null if no data remains.</returns>
        public byte[] GetNextChunk(out long position, out byte[] newData, out bool finalChunk)
        {
            if (_ReturnedEndPosition >= _Data.Length || _NextStartPosition >= _Data.Length)
            {
                position = -1;
                newData = null;
                finalChunk = true;
                return null;
            }

            int start = _NextStartPosition;
            int end = Math.Min(start + _ChunkSize, _Data.Length);
            int newDataStart = Math.Max(start, _ReturnedEndPosition);

            byte[] ret = new byte[end - start];
            Buffer.BlockCopy(_Data, start, ret, 0, ret.Length);

            newData = new byte[end - newDataStart];
            Buffer.BlockCopy(_Data, newDataStart, newData, 0, newData.Length);

            position = start;
            finalChunk = (end >= _Data.Length);

            _ReturnedEndPosition = end;
            _NextStartPosition += _ShiftSize;
            _NextEndPosition = _NextStartPosition + _ChunkSize;
            return ret;
        }

        /// <summary>
        /// Advance to the position starting after the end of the previous chunk.
        /// Calling GetNextChunk() will then return a chunk full of new data.
        /// Has no effect if no chunk has been retrieved.
        /// </summary>
        public void AdvanceToNewChunk()
        {
            if (_ReturnedEndPosition == 0) return;

            _NextStartPosition = _ReturnedEndPosition;
            _NextEndPosition = _NextStartPosition + _ChunkSize;
        }

        #endregion 
    }
}
