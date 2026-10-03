namespace Test.Shared
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Reference model describing the chunks a correct sliding window produces.
    /// </summary>
    public static class WindowModel
    {
        /// <summary>
        /// Expected number of chunks.
        /// </summary>
        /// <param name="length">Data length.</param>
        /// <param name="chunkSize">Chunk size.</param>
        /// <param name="shiftSize">Shift size.</param>
        /// <returns>Chunk count.</returns>
        public static int ChunkCount(long length, int chunkSize, int shiftSize)
        {
            long remainder = length - chunkSize;
            return 1 + (int)((remainder + shiftSize - 1) / shiftSize);
        }

        /// <summary>
        /// Build the expected chunk sequence for a linear traversal.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="chunkSize">Chunk size.</param>
        /// <param name="shiftSize">Shift size.</param>
        /// <returns>Expected chunks.</returns>
        public static List<ChunkRecord> Expected(byte[] data, int chunkSize, int shiftSize)
        {
            List<ChunkRecord> ret = new List<ChunkRecord>();
            int count = ChunkCount(data.Length, chunkSize, shiftSize);
            int previousEnd = 0;

            for (int i = 0; i < count; i++)
            {
                int start = i * shiftSize;
                int end = Math.Min(start + chunkSize, data.Length);
                int newStart = Math.Max(start, previousEnd);

                ret.Add(new ChunkRecord
                {
                    Position = start,
                    Chunk = TestData.Slice(data, start, end - start),
                    NewData = TestData.Slice(data, newStart, end - newStart),
                    Final = (i == count - 1)
                });

                previousEnd = end;
            }

            return ret;
        }

        /// <summary>
        /// Compare an actual chunk sequence to an expected one.
        /// </summary>
        /// <param name="expected">Expected chunks.</param>
        /// <param name="actual">Actual chunks.</param>
        /// <param name="label">Label describing the scenario.</param>
        public static void AssertMatches(List<ChunkRecord> expected, List<ChunkRecord> actual, string label)
        {
            int max = Math.Min(expected.Count, actual.Count);
            for (int i = 0; i < max; i++)
            {
                string prefix = label + " chunk " + i;
                Check.Equal(expected[i].Position, actual[i].Position, prefix + " position");
                Check.BytesEqual(expected[i].Chunk, actual[i].Chunk, prefix + " data");
                Check.BytesEqual(expected[i].NewData, actual[i].NewData, prefix + " newData");
                Check.Equal(expected[i].Final, actual[i].Final, prefix + " finalChunk");
            }

            Check.Equal(expected.Count, actual.Count, label + " chunk count");
        }
    }
}
