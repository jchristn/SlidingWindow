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
        /// Enable or disable logging to the console.
        /// </summary>
        public bool ConsoleDebug = false;

        #endregion

        #region Private-Members

        private byte[] _Data = null;
        private int _ChunkSize = 0;
        private int _ShiftSize = 0;
        private int _NextStartPosition = 0;

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
        /// <param name="finalChunk">Indicates if the chunk retrieved is the final chunk.</param>
        /// <returns>Byte array containing chunk data.</returns>
        public byte[] GetNextChunk(out bool finalChunk)
        {
            finalChunk = false;
            byte[] ret = new byte[_ChunkSize];

            if ((_NextStartPosition + _ChunkSize) <= _Data.Length)
            {
                #region Full-Chunk

                Buffer.BlockCopy(_Data, _NextStartPosition, ret, 0, _ChunkSize); 

                Log(
                    "Returning chunk from position " + _NextStartPosition + 
                    " of size " + _ChunkSize + 
                    " [" + 
                    (_Data.Length - (_NextStartPosition + _ChunkSize)) + 
                    " bytes remaining]");

                _NextStartPosition += _ShiftSize;
                return ret;

                #endregion
            }
            else if (_NextStartPosition < _Data.Length)
            {
                #region Partial-Chunk

                int _BytesRemaining = _Data.Length - _NextStartPosition;
                ret = new byte[_BytesRemaining];
                Buffer.BlockCopy(_Data, _NextStartPosition, ret, 0, _BytesRemaining);
                _BytesRemaining -= _BytesRemaining;

                Log(
                    "Returning final chunk from position " + _NextStartPosition +
                    " of size " + _ChunkSize);

                _NextStartPosition = _Data.Length + 1;
                finalChunk = true;
                return ret;

                #endregion
            }
            else
            {
                #region No-More-Data

                Log("End of data");
                return null;

                #endregion
            } 
        }

        #endregion

        #region Private-Methods

        private void Log(string msg)
        {
            if (ConsoleDebug) Console.WriteLine(msg);
        }

        #endregion
    }
}
