namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using SlidingWindow;
    using Touchstone.Core;

    /// <summary>
    /// AdvanceToNewChunk tests for <see cref="Streams"/>.
    /// </summary>
    public static class StreamsAdvanceSuite
    {
        private const string _SuiteId = "StreamsAdvance";

        /// <summary>
        /// Create the suite.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Streams: AdvanceToNewChunk",
                cases: new List<TestCaseDescriptor>
                {
                    Descriptors.Case(_SuiteId, "BeforeFirstChunkIsNoOp", "Streams: advancing before the first chunk has no effect", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 2);
                        w.AdvanceToNewChunk();
                        Check.Equal(0L, w.NextStartPosition, "NextStartPosition");
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "abcd", "abcd", false, "chunk 1");
                    }),

                    Descriptors.Case(_SuiteId, "SkipsOverlap", "Streams: advancing after a chunk starts the next chunk after it", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 2);
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "abcd", "abcd", false, "chunk 1");
                        w.AdvanceToNewChunk();
                        Check.Equal(4L, w.NextStartPosition, "NextStartPosition after advance");
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(w), 6, "ghij", "ij", true, "chunk 3");
                    }),

                    Descriptors.Case(_SuiteId, "AdvanceIntoFinalFullChunk", "Streams: advancing into a final full-size chunk returns all of it as new data", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 2);
                        ChunkReader.Next(w);
                        ChunkAssert.Text(ChunkReader.Next(w), 2, "cdef", "ef", false, "chunk 2");
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 6, "ghij", "ghij", true, "chunk 3");
                    }),

                    Descriptors.Case(_SuiteId, "AdvanceIntoShortTail", "Streams: advancing into a tail shorter than a chunk returns the tail", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 4);
                        ChunkReader.Next(w);
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", false, "chunk 2");
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 8, "ij", "ij", true, "chunk 3");
                    }),

                    Descriptors.Case(_SuiteId, "AdvanceIntoTailShorterThanShift", "Streams: advancing into a tail shorter than the shift returns the tail", () =>
                    {
                        Streams w = Create("abcdefghi", 4, 2);
                        ChunkReader.Next(w);
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", false, "chunk 2");
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 8, "i", "i", true, "chunk 3");
                    }),

                    Descriptors.Case(_SuiteId, "AdvanceAfterFinalReturnsNull", "Streams: advancing after the final chunk yields no more data", () =>
                    {
                        Streams w = Create("abcdefgh", 4, 4);
                        ChunkReader.Next(w);
                        w.AdvanceToNewChunk();
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", true, "chunk 2");
                        w.AdvanceToNewChunk();
                        ChunkAssert.End(ChunkReader.Next(w), "after final");
                    }),

                    Descriptors.Case(_SuiteId, "DoubleAdvanceIsIdempotent", "Streams: advancing twice without reading equals advancing once", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 2);
                        ChunkReader.Next(w);
                        w.AdvanceToNewChunk();
                        w.AdvanceToNewChunk();
                        Check.Equal(4L, w.NextStartPosition, "NextStartPosition");
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", false, "chunk 2");
                    }),

                    Descriptors.Case(_SuiteId, "AdvanceClearsPreviousChunk", "Streams: advancing clears PreviousChunk without reading the stream", () =>
                    {
                        using (TrickleStream s = new TrickleStream(TestData.Ascii("abcdefghij")))
                        {
                            Streams w = new Streams(s, 10, 4, 2);
                            ChunkReader.Next(w);
                            w.AdvanceToNewChunk();
                            Check.Null(w.PreviousChunk, "PreviousChunk");
                            Check.Equal(4L, s.TotalBytesRead, "bytes consumed from stream");
                            Check.Equal(6L, w.BytesRemaining, "BytesRemaining");
                        }
                    }),

                    Descriptors.Case(_SuiteId, "RepeatedAdvanceTilesData", "Streams: reading and advancing repeatedly tiles the data without gaps", () =>
                    {
                        string text = "The quick brown fox jumped over the lazy dog...";
                        for (int chunkSize = 1; chunkSize <= 12; chunkSize++)
                        {
                            for (int shiftSize = 1; shiftSize <= chunkSize; shiftSize++)
                            {
                                string label = "(" + chunkSize + "," + shiftSize + ")";
                                Streams w = Create(text, chunkSize, shiftSize);
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

                    Descriptors.Case(_SuiteId, "BytesRemainingAfterAdvance", "Streams: BytesRemaining counts bytes not yet returned after advancing", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 2);
                        ChunkReader.Next(w);
                        ChunkReader.Next(w);
                        Check.Equal(4L, w.BytesRemaining, "after chunk 2");
                        w.AdvanceToNewChunk();
                        Check.Equal(4L, w.BytesRemaining, "after advance");
                        ChunkReader.Next(w);
                        Check.Equal(0L, w.BytesRemaining, "after chunk 3");
                    })
                });
        }

        private static Streams Create(string text, int chunkSize, int shiftSize)
        {
            return new Streams(new TrickleStream(TestData.Ascii(text)), text.Length, chunkSize, shiftSize);
        }
    }
}
