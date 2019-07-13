using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SlidingWindow
{
    /// <summary>
    /// Perform a sliding window operation over data read from a stream.
    /// </summary>
    public class Streams
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable logging to the console.
        /// </summary>
        public bool ConsoleDebug = false;

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
            if (shiftSize > chunkSize) throw new ArgumentException("Shift size must be less than chunk size.");

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
        /// </summary>
        /// <param name="position">Indicates the starting position of the chunk in the byte array.</param>
        /// <param name="finalChunk">Indicates if the chunk retrieved is the final chunk.</param>
        /// <returns>Byte array containing chunk data.</returns>
        public byte[] GetNextChunk(out long position, out bool finalChunk)
        {
            finalChunk = false;
            int bytesRead = 0;

            if (_BytesRemaining < 1)
            {
                position = -1;
                finalChunk = true;
                return null;
            }

            if (_BytesRemaining >= _ChunkSize)
            {
                #region Full-Chunk

                if (_PreviousChunk == null)
                {
                    _PreviousChunk = new byte[_ChunkSize];
                    _PreviousChunk = ReadBytesFromStream(_Stream, _ChunkSize, out bytesRead);
                }
                else
                {
                    _PreviousChunk = ShiftLeft(_PreviousChunk, _ShiftSize, 0x00);
                    byte[] newData = ReadBytesFromStream(_Stream, _ShiftSize, out bytesRead);
                    Buffer.BlockCopy(
                        newData, 
                        0, 
                        _PreviousChunk,
                        (_ChunkSize - _ShiftSize),
                        _ShiftSize
                        );
                }

                _BytesRemaining -= bytesRead;

                Log(
                    "Returning chunk from position " + _NextStartPosition +
                    " of size " + _ChunkSize +
                    " [" +
                    _BytesRemaining +
                    " bytes remaining]");

                position = _NextStartPosition;
                _NextStartPosition += _ShiftSize;
                return _PreviousChunk;

                #endregion
            }
            else if (_BytesRemaining > 0 && _BytesRemaining < _ChunkSize && _BytesRemaining >= _ShiftSize)
            {
                #region Full-Chunk

                if (_PreviousChunk == null)
                {
                    _PreviousChunk = new byte[_ChunkSize];
                    _PreviousChunk = ReadBytesFromStream(_Stream, _ChunkSize, out bytesRead);
                }
                else
                {
                    _PreviousChunk = ShiftLeft(_PreviousChunk, _ShiftSize, 0x00);
                    byte[] newData = ReadBytesFromStream(_Stream, _ShiftSize, out bytesRead);
                    Buffer.BlockCopy(
                        newData,
                        0,
                        _PreviousChunk,
                        (_ChunkSize - _ShiftSize),
                        _ShiftSize
                        );
                }

                _BytesRemaining -= bytesRead;

                Log(
                    "Returning chunk from position " + _NextStartPosition +
                    " of size " + _ChunkSize +
                    " [" +
                    _BytesRemaining +
                    " bytes remaining] ***");

                position = _NextStartPosition;
                _NextStartPosition += _ShiftSize;
                return _PreviousChunk;

                #endregion
            }
            else if (_BytesRemaining > 0 && _BytesRemaining < _ChunkSize && _BytesRemaining < _ShiftSize)
            {
                #region Final-Chunk

                if (_PreviousChunk == null)
                {
                    _PreviousChunk = new byte[_BytesRemaining];
                    _PreviousChunk = ReadBytesFromStream(_Stream, (int)_BytesRemaining, out bytesRead);
                }
                else
                {
                    _PreviousChunk = ShiftLeft(_PreviousChunk, _ShiftSize, 0x00);

                    Log("");
                    Log("Previous chunk after shift: '" + Encoding.UTF8.GetString(_PreviousChunk) + "'");

                    byte[] previousChunk = new byte[(_PreviousChunk.Length - _ShiftSize)];
                    Buffer.BlockCopy(
                        _PreviousChunk,
                        0,
                        previousChunk,
                        0,
                        (_PreviousChunk.Length - _ShiftSize)
                        );

                    Log("Previous chunk after shrink: '" + Encoding.UTF8.GetString(previousChunk) + "'");

                    byte[] newData = ReadBytesFromStream(_Stream, (int)_BytesRemaining, out bytesRead);

                    Log("New data: '" + Encoding.UTF8.GetString(newData) + "'");

                    _PreviousChunk = new byte[previousChunk.Length + newData.Length];

                    Console.WriteLine("New chunk length: " + _PreviousChunk.Length);

                    // copy the previous data
                    Buffer.BlockCopy(
                        previousChunk,
                        0,
                        _PreviousChunk,
                        0,
                        previousChunk.Length
                        );

                    Console.WriteLine("Previous chunk after copying previous data: '" + Encoding.UTF8.GetString(_PreviousChunk) + "'");

                    // copy the new data
                    Buffer.BlockCopy(
                        newData,
                        0,
                        _PreviousChunk,
                        previousChunk.Length,
                        bytesRead
                        );

                    Console.WriteLine("Previous chunk after copying new data: '" + Encoding.UTF8.GetString(_PreviousChunk) + "'");

                    Log(
                        "Final block: " + Environment.NewLine + 
                        "  Byte array length : " + _PreviousChunk.Length + Environment.NewLine +
                        "  New data length   : " + newData.Length + Environment.NewLine +
                        "  Chunk size        : " + _ChunkSize + Environment.NewLine + 
                        "  Bytes remaining   : " + _BytesRemaining
                        ); 
                }

                Log("");
                _BytesRemaining -= bytesRead;

                Log(
                    "Returning final chunk from position " + _NextStartPosition +
                    " of size " + _PreviousChunk.Length);

                position = _NextStartPosition;
                _NextStartPosition = _ContentLength + 1;
                finalChunk = true;
                return _PreviousChunk;

                #endregion
            }
            else
            {
                #region No-More-Data

                Log("End of data");
                position = -1;
                finalChunk = true;
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

        private byte[] ReadBytesFromStream(Stream stream, int count, out int bytesRead)
        {
            bytesRead = 0;
            byte[] data = new byte[count];

            long bytesRemaining = count;

            while (bytesRemaining > 0)
            {
                int bytesReadCurr = stream.Read(data, 0, data.Length);
                if (bytesReadCurr > 0)
                {
                    bytesRead += bytesReadCurr;
                    bytesRemaining -= bytesReadCurr;
                }
            }

            return data;
        }

        private byte[] ShiftLeft(byte[] data, int shiftCount, byte fill)
        {
            if (data == null || data.Length < 1) return null;
            byte[] ret = InitBytes(data.Length, fill);
            if (data.Length < shiftCount) return ret;

            Buffer.BlockCopy(data, shiftCount, ret, 0, (data.Length - shiftCount));
            return ret;
        }

        private byte[] ShiftRight(byte[] data, int shiftCount, byte fill)
        {
            if (data == null || data.Length < 1) return null;
            byte[] ret = InitBytes(data.Length, fill);
            if (data.Length < shiftCount) return ret;

            Buffer.BlockCopy(data, 0, ret, shiftCount, (data.Length - shiftCount));
            return ret;
        }

        private byte[] InitBytes(long count, byte val)
        {
            byte[] ret = new byte[count];
            for (long i = 0; i < ret.Length; i++)
            {
                ret[i] = val;
            }
            return ret;
        }

        #endregion
    }
}
