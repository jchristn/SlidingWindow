namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using SlidingWindow;

    /// <summary>
    /// Reads chunks from sliding window instances into <see cref="ChunkRecord"/> lists.
    /// </summary>
    public static class ChunkReader
    {
        /// <summary>
        /// Read one chunk from a byte array sliding window.
        /// </summary>
        /// <param name="window">Window.</param>
        /// <returns>Chunk record.</returns>
        public static ChunkRecord Next(Bytes window)
        {
            byte[] chunk = window.GetNextChunk(out long position, out byte[] newData, out bool finalChunk);
            return Record(position, chunk, newData, finalChunk);
        }

        /// <summary>
        /// Read one chunk from a stream sliding window.
        /// </summary>
        /// <param name="window">Window.</param>
        /// <returns>Chunk record.</returns>
        public static ChunkRecord Next(Streams window)
        {
            byte[] chunk = window.GetNextChunk(out long position, out byte[] newData, out bool finalChunk);
            return Record(position, chunk, newData, finalChunk);
        }

        /// <summary>
        /// Read chunks until the final chunk is reported.
        /// </summary>
        /// <param name="window">Window.</param>
        /// <param name="maxCalls">Maximum number of calls before failing.</param>
        /// <returns>Chunks.</returns>
        public static List<ChunkRecord> ReadAll(Bytes window, int maxCalls)
        {
            return ReadAll(() => Next(window), maxCalls);
        }

        /// <summary>
        /// Read chunks until the final chunk is reported.
        /// </summary>
        /// <param name="window">Window.</param>
        /// <param name="maxCalls">Maximum number of calls before failing.</param>
        /// <returns>Chunks.</returns>
        public static List<ChunkRecord> ReadAll(Streams window, int maxCalls)
        {
            return ReadAll(() => Next(window), maxCalls);
        }

        /// <summary>
        /// Concatenate the new data from each chunk.
        /// </summary>
        /// <param name="chunks">Chunks.</param>
        /// <returns>Concatenated bytes.</returns>
        public static byte[] ConcatNewData(List<ChunkRecord> chunks)
        {
            int total = 0;
            foreach (ChunkRecord c in chunks) total += (c.NewData != null ? c.NewData.Length : 0);

            byte[] ret = new byte[total];
            int offset = 0;
            foreach (ChunkRecord c in chunks)
            {
                if (c.NewData == null) continue;
                Buffer.BlockCopy(c.NewData, 0, ret, offset, c.NewData.Length);
                offset += c.NewData.Length;
            }

            return ret;
        }

        private static List<ChunkRecord> ReadAll(Func<ChunkRecord> next, int maxCalls)
        {
            List<ChunkRecord> ret = new List<ChunkRecord>();
            for (int i = 0; i < maxCalls; i++)
            {
                ChunkRecord record = next();
                if (record.Chunk == null)
                    throw new TestAssertionException("Call " + i + " returned null before a final chunk was reported");

                ret.Add(record);
                if (record.Final) return ret;
            }

            throw new TestAssertionException("No final chunk reported within " + maxCalls + " calls");
        }

        private static ChunkRecord Record(long position, byte[] chunk, byte[] newData, bool finalChunk)
        {
            return new ChunkRecord
            {
                Position = position,
                Chunk = Copy(chunk),
                NewData = Copy(newData),
                Final = finalChunk
            };
        }

        private static byte[]? Copy(byte[]? data)
        {
            if (data == null) return null;
            byte[] ret = new byte[data.Length];
            Buffer.BlockCopy(data, 0, ret, 0, data.Length);
            return ret;
        }
    }
}
