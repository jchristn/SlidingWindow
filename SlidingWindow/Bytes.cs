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
        /// Number of bytes remaining.
        /// </summary>
        public int BytesRemaining
        {
            get
            {
                if (_NextStartPosition == 0) return _Data.Length;
                return _Data.Length - (_NextEndPosition - _ShiftSize);
            }
        }

        #endregion

        #region Private-Members

        private byte[] _Data = null;
        private int _ChunkSize = 0;
        private int _ShiftSize = 0;
         
        private int _NextStartPosition = 0;
        private int _NextEndPosition = 0;

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
            if (shiftSize > chunkSize) throw new ArgumentException("Shift size must be less than chunk size.");

            _Data = new byte[data.Length];
            Buffer.BlockCopy(data, 0, _Data, 0, data.Length);

            _ChunkSize = chunkSize;
            _ShiftSize = shiftSize;
             
            _NextStartPosition = 0;
            _NextEndPosition = _NextStartPosition + _ChunkSize;
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
        /// </summary>
        /// <param name="position">Indicates the starting position of the chunk in the byte array.</param>
        /// <param name="newData">Removing data from the previous chunk, new data found that was included in this chunk.</param>
        /// <param name="finalChunk">Indicates if the chunk retrieved is the final chunk.</param>
        /// <returns>Byte array containing chunk data.</returns>
        public byte[] GetNextChunk(out long position, out byte[] newData, out bool finalChunk)
        {
            position = -1;
            byte[] ret = null;

            if (_NextStartPosition == 0)
            {
                #region First-Chunk

                if (_ChunkSize > _Data.Length)
                {
                    #region Only-Chunk

                    ret = new byte[_Data.Length];
                    newData = new byte[_Data.Length];
                    Buffer.BlockCopy(_Data, 0, ret, 0, _Data.Length);
                    Buffer.BlockCopy(_Data, 0, newData, 0, _Data.Length);
                    finalChunk = true;
                    position = 0;
                    _NextStartPosition += _ShiftSize;
                    _NextEndPosition += _ShiftSize;
                    return ret;

                    #endregion
                }
                else
                {
                    #region Full-Chunk

                    ret = new byte[_ChunkSize];
                    newData = new byte[_ChunkSize];
                    Buffer.BlockCopy(_Data, _NextStartPosition, ret, 0, _ChunkSize);
                    Buffer.BlockCopy(_Data, _NextStartPosition, newData, 0, _ChunkSize);
                    finalChunk = false;
                    position = 0;
                    _NextStartPosition += _ShiftSize;
                    _NextEndPosition += _ShiftSize;
                    return ret;

                    #endregion
                }

                #endregion
            }
            else
            {
                #region Subsequent-Chunk

                if (_NextStartPosition >= _Data.Length)
                {
                    #region End

                    ret = null;
                    newData = null;
                    finalChunk = true;
                    position = -1;
                    return ret;

                    #endregion
                }
                else
                {
                    if (_NextEndPosition < _Data.Length)
                    {
                        #region Full-Chunk

                        ret = new byte[_ChunkSize];
                        Buffer.BlockCopy(_Data, _NextStartPosition, ret, 0, _ChunkSize);

                        newData = new byte[_ShiftSize];
                        int newDataStart = _NextEndPosition - _ShiftSize;
                        Buffer.BlockCopy(_Data, newDataStart, newData, 0, _ShiftSize);
                        finalChunk = false;
                        position = _NextStartPosition;
                        _NextStartPosition += _ShiftSize;
                        _NextEndPosition += _ShiftSize;

                        if (_NextStartPosition >= _Data.Length) finalChunk = true;

                        return ret;

                        #endregion
                    }
                    else
                    {
                        #region Partial-Chunk-and-End

                        int windowLength = _Data.Length - _NextStartPosition; 
                        position = _Data.Length - (_Data.Length - _NextStartPosition); 

                        ret = new byte[windowLength];
                        Buffer.BlockCopy(_Data, (int)position, ret, 0, windowLength);

                        int newDataLength = _Data.Length - (_NextEndPosition - _ShiftSize); 
                        int newDataPosition = _Data.Length - newDataLength; 
                        newData = new byte[newDataLength];
                        Buffer.BlockCopy(_Data, newDataPosition, newData, 0, newDataLength);

                        finalChunk = true;
                        _NextStartPosition += _ShiftSize;
                        _NextEndPosition += _ShiftSize;
                        return ret;

                        #endregion
                    }
                }

                #endregion
            } 
        }

        /// <summary>
        /// Advance processing to the next new chunk.
        /// </summary>
        public void AdvanceToNewChunk()
        {
            if (_NextStartPosition == 0) return;

            int nextStartPosition = _NextStartPosition;
            int nextEndPosition = _NextEndPosition;

            int prevChunkStartPosition = _NextStartPosition - _ShiftSize;
            int prevChunkEndPosition = _NextEndPosition - _ShiftSize;

            _NextStartPosition = prevChunkStartPosition + _ChunkSize;
            _NextEndPosition = prevChunkEndPosition + _ChunkSize;

            return;
        }

        #endregion 
    }
}
