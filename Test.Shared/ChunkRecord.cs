namespace Test.Shared
{
    /// <summary>
    /// One chunk returned by (or expected from) a sliding window.
    /// </summary>
    public class ChunkRecord
    {
        /// <summary>
        /// Starting position of the chunk.
        /// </summary>
        public long Position { get; set; } = -1;

        /// <summary>
        /// Chunk data.
        /// </summary>
        public byte[]? Chunk { get; set; } = null;

        /// <summary>
        /// Data in this chunk not present in any previously returned chunk.
        /// </summary>
        public byte[]? NewData { get; set; } = null;

        /// <summary>
        /// Whether this is the final chunk.
        /// </summary>
        public bool Final { get; set; } = false;
    }
}
