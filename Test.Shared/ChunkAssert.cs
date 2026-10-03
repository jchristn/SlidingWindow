namespace Test.Shared
{
    /// <summary>
    /// Assertions for individual chunks expressed as text.
    /// </summary>
    public static class ChunkAssert
    {
        /// <summary>
        /// Assert that a chunk has the expected position, data, new data, and final flag.
        /// </summary>
        /// <param name="actual">Actual chunk.</param>
        /// <param name="position">Expected position.</param>
        /// <param name="chunk">Expected chunk text.</param>
        /// <param name="newData">Expected new data text.</param>
        /// <param name="final">Expected final flag.</param>
        /// <param name="label">Label describing the chunk.</param>
        public static void Text(ChunkRecord actual, long position, string chunk, string newData, bool final, string label)
        {
            Check.Equal(position, actual.Position, label + " position");
            Check.Equal(chunk, TestData.Text(actual.Chunk), label + " data");
            Check.Equal(newData, TestData.Text(actual.NewData), label + " newData");
            Check.Equal(final, actual.Final, label + " finalChunk");
        }

        /// <summary>
        /// Assert that a call returned no chunk and reported the end of data.
        /// </summary>
        /// <param name="actual">Actual chunk.</param>
        /// <param name="label">Label describing the call.</param>
        public static void End(ChunkRecord actual, string label)
        {
            Check.Null(actual.Chunk, label + " data");
            Check.Null(actual.NewData, label + " newData");
            Check.Equal(-1L, actual.Position, label + " position");
            Check.True(actual.Final, label + " finalChunk should be true");
        }
    }
}
