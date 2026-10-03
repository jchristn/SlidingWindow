namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using SlidingWindow;
    using Touchstone.Core;

    /// <summary>
    /// AdvanceToNewChunk tests for <see cref="Bytes"/>.
    /// </summary>
    public static class BytesAdvanceSuite
    {
        private const string _SuiteId = "BytesAdvance";

        /// <summary>
        /// Create the suite.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Bytes: AdvanceToNewChunk",
                cases: new List<TestCaseDescriptor>
                {
                    Descriptors.Case(_SuiteId, "BeforeFirstChunkIsNoOp", "Bytes: advancing before the first chunk has no effect", () =>
                    {
                        Bytes w = Create("abcdefghij", 4, 2);
                        w.AdvanceToNewChunk();
                        Check.Equal(0, w.NextStartPosition, "NextStartPosition");
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "abcd", "abcd", false, "chunk 1");
                    }),

                    Descriptors.Case(_SuiteId, "SkipsOverlap", "Bytes: advancing after a chunk starts the next chunk after it", () =>
                    {
                        Bytes w = Create("abcdefghij", 4, 2);
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "abcd", "abcd", false, "chunk 1");
                        w.AdvanceToNewChunk();
                        Check.Equal(4, w.NextStartPosition, "NextStartPosition after advance");
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(w), 6, "ghij", "ij", true, "chunk 3");
                    }),

                    Descriptors.Case(_SuiteId, "AdvanceIntoFinalFullChunk", "Bytes: advancing into a final full-size chunk returns all of it as new data", () =>
                    {
                        Bytes w = Create("abcdefghij", 4, 2);
                        ChunkReader.Next(w);
                        ChunkAssert.Text(ChunkReader.Next(w), 2, "cdef", "ef", false, "chunk 2");
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 6, "ghij", "ghij", true, "chunk 3");
                    }),

                    Descriptors.Case(_SuiteId, "AdvanceIntoShortTail", "Bytes: advancing into a tail shorter than a chunk returns the tail", () =>
                    {
                        Bytes w = Create("abcdefghij", 4, 4);
                        ChunkReader.Next(w);
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", false, "chunk 2");
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 8, "ij", "ij", true, "chunk 3");
                    }),

                    Descriptors.Case(_SuiteId, "AdvanceIntoTailShorterThanShift", "Bytes: advancing into a tail shorter than the shift returns the tail", () =>
                    {
                        Bytes w = Create("abcdefghi", 4, 2);
                        ChunkReader.Next(w);
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", false, "chunk 2");
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 8, "i", "i", true, "chunk 3");
                    }),

                    Descriptors.Case(_SuiteId, "AdvanceAfterFinalReturnsNull", "Bytes: advancing after the final chunk yields no more data", () =>
                    {
                        Bytes w = Create("abcdefgh", 4, 4);
                        ChunkReader.Next(w);
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", true, "chunk 2");
                        w.AdvanceToNewChunk();
                        ChunkAssert.End(ChunkReader.Next(w), "after final");
                    }),

                    Descriptors.Case(_SuiteId, "DoubleAdvanceIsIdempotent", "Bytes: advancing twice without reading equals advancing once", () =>
                    {
                        Bytes w = Create("abcdefghij", 4, 2);
                        ChunkReader.Next(w);
                        w.AdvanceToNewChunk();
                        w.AdvanceToNewChunk();
                        Check.Equal(4, w.NextStartPosition, "NextStartPosition");
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", false, "chunk 2");
                    }),

                    Descriptors.Case(_SuiteId, "AdvanceUpdatesNextEndPosition", "Bytes: advancing moves NextEndPosition one chunk past the new start", () =>
                    {
                        Bytes w = Create("abcdefghij", 4, 3);
                        ChunkReader.Next(w);
                        w.AdvanceToNewChunk();
                        Check.Equal(4, w.NextStartPosition, "NextStartPosition");
                        Check.Equal(8, w.NextEndPosition, "NextEndPosition");
                    }),

                    Descriptors.Case(_SuiteId, "RepeatedAdvanceTilesData", "Bytes: reading and advancing repeatedly tiles the data without gaps", () =>
                    {
                        string text = "The quick brown fox jumped over the lazy dog...";
                        for (int chunkSize = 1; chunkSize <= 12; chunkSize++)
                        {
                            for (int shiftSize = 1; shiftSize <= chunkSize; shiftSize++)
                            {
                                string label = "(" + chunkSize + "," + shiftSize + ")";
                                Bytes w = Create(text, chunkSize, shiftSize);
                                List<ChunkRecord> chunks = new List<ChunkRecord>();
                                for (int i = 0; i <= text.Length; i++)
                                {
                                    ChunkRecord c = ChunkReader.Next(w);
                                    Check.NotNull(c.Chunk, label + " chunk " + i);
                                    Check.Equal((long)(i * chunkSize), c.Position, label + " chunk " + i + " position");
                                    Check.Equal(TestData.Text(c.Chunk), TestData.Text(c.NewData), label + " chunk " + i + " newData");
                                    chunks.Add(c);
                                    if (c.Final) break;
                                    w.AdvanceToNewChunk();
                                }
                                Check.Equal(text, TestData.Text(ChunkReader.ConcatNewData(chunks)), label + " reconstructed");
                            }
                        }
                    }),

                    Descriptors.Case(_SuiteId, "BytesRemainingAfterAdvance", "Bytes: BytesRemaining counts bytes not yet returned after advancing", () =>
                    {
                        Bytes w = Create("abcdefghij", 4, 2);
                        ChunkReader.Next(w);
                        ChunkReader.Next(w);
                        Check.Equal(4, w.BytesRemaining, "after chunk 2");
                        w.AdvanceToNewChunk();
                        Check.Equal(4, w.BytesRemaining, "after advance");
                        ChunkReader.Next(w);
                        Check.Equal(0, w.BytesRemaining, "after chunk 3");
                    })
                });
        }

        private static Bytes Create(string text, int chunkSize, int shiftSize)
        {
            return new Bytes(TestData.Ascii(text), chunkSize, shiftSize);
        }
    }
}
