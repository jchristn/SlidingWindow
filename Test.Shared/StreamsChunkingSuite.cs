namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using SlidingWindow;
    using Touchstone.Core;

    /// <summary>
    /// Chunk retrieval tests for <see cref="Streams"/>.
    /// </summary>
    public static class StreamsChunkingSuite
    {
        private const string _SuiteId = "StreamsChunking";

        /// <summary>
        /// Create the suite.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Streams: chunk retrieval",
                cases: new List<TestCaseDescriptor>
                {
                    Descriptors.Case(_SuiteId, "ReadmeExample", "Streams: README example (47 bytes, chunk 24, shift 8)", () =>
                    {
                        Streams w = Create(TestData.ReadmeSentence, 24, 8);
                        Check.Equal(4, w.ChunkCount(), "ChunkCount");
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "The quick brown fox jump", "The quick brown fox jump", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(w), 8, "k brown fox jumped over ", "ed over ", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(w), 16, "fox jumped over the lazy", "the lazy", false, "chunk 3");
                        ChunkAssert.Text(ChunkReader.Next(w), 24, "ed over the lazy dog...", " dog...", true, "chunk 4");
                    }),

                    Descriptors.Case(_SuiteId, "OverlappingEvenShift", "Streams: overlapping windows, shift divides remainder (10, 4, 2)", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 2);
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "abcd", "abcd", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(w), 2, "cdef", "ef", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "gh", false, "chunk 3");
                        ChunkAssert.Text(ChunkReader.Next(w), 6, "ghij", "ij", true, "chunk 4");
                    }),

                    Descriptors.Case(_SuiteId, "OverlappingUnevenShift", "Streams: overlapping windows, partial final chunk (10, 3, 2)", () =>
                    {
                        Streams w = Create("abcdefghij", 3, 2);
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "abc", "abc", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(w), 2, "cde", "de", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efg", "fg", false, "chunk 3");
                        ChunkAssert.Text(ChunkReader.Next(w), 6, "ghi", "hi", false, "chunk 4");
                        ChunkAssert.Text(ChunkReader.Next(w), 8, "ij", "j", true, "chunk 5");
                    }),

                    Descriptors.Case(_SuiteId, "PartialFinalChunkNotPadded", "Streams: partial final chunk has no zero padding (11, 4, 3)", () =>
                    {
                        Streams w = Create("abcdefghijk", 4, 3);
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "abcd", "abcd", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(w), 3, "defg", "efg", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(w), 6, "ghij", "hij", false, "chunk 3");
                        ChunkAssert.Text(ChunkReader.Next(w), 9, "jk", "k", true, "chunk 4");
                    }),

                    Descriptors.Case(_SuiteId, "NonOverlapping", "Streams: shift equal to chunk produces non-overlapping windows (10, 4, 4)", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 4);
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "abcd", "abcd", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(w), 8, "ij", "ij", true, "chunk 3");
                    }),

                    Descriptors.Case(_SuiteId, "NonOverlappingExact", "Streams: non-overlapping windows that end exactly at content length (8, 4, 4)", () =>
                    {
                        Streams w = Create("abcdefgh", 4, 4);
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "abcd", "abcd", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(w), 4, "efgh", "efgh", true, "chunk 2");
                    }),

                    Descriptors.Case(_SuiteId, "ShiftOne", "Streams: shift of one byte (6, 3, 1)", () =>
                    {
                        Streams w = Create("abcdef", 3, 1);
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "abc", "abc", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(w), 1, "bcd", "d", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(w), 2, "cde", "e", false, "chunk 3");
                        ChunkAssert.Text(ChunkReader.Next(w), 3, "def", "f", true, "chunk 4");
                    }),

                    Descriptors.Case(_SuiteId, "SingleByte", "Streams: single byte content returns one final chunk", () =>
                    {
                        Streams w = Create("q", 1, 1);
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "q", "q", true, "chunk 1");
                    }),

                    Descriptors.Case(_SuiteId, "ChunkEqualsContentLength", "Streams: chunk equal to content length returns one final chunk", () =>
                    {
                        Streams w = Create("abcdefghij", 10, 3);
                        ChunkAssert.Text(ChunkReader.Next(w), 0, "abcdefghij", "abcdefghij", true, "chunk 1");
                    }),

                    Descriptors.Case(_SuiteId, "CallAfterFinalReturnsNull", "Streams: GetNextChunk after the final chunk returns null", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 2);
                        List<ChunkRecord> chunks = ChunkReader.ReadAll(w, 10);
                        Check.Equal(4, chunks.Count, "chunks before final");
                        for (int i = 0; i < 3; i++) ChunkAssert.End(ChunkReader.Next(w), "after final, call " + i);
                    }),

                    Descriptors.Case(_SuiteId, "BytesRemainingProgression", "Streams: BytesRemaining tracks unread stream bytes", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 2);
                        Check.Equal(10L, w.BytesRemaining, "before chunk 1");
                        ChunkReader.Next(w);
                        Check.Equal(6L, w.BytesRemaining, "after chunk 1");
                        ChunkReader.Next(w);
                        Check.Equal(4L, w.BytesRemaining, "after chunk 2");
                        ChunkReader.Next(w);
                        Check.Equal(2L, w.BytesRemaining, "after chunk 3");
                        ChunkReader.Next(w);
                        Check.Equal(0L, w.BytesRemaining, "after chunk 4");
                    }),

                    Descriptors.Case(_SuiteId, "NextStartPositionProgression", "Streams: NextStartPosition advances by shift size", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 3);
                        Check.Equal(0L, w.NextStartPosition, "before chunk 1");
                        ChunkReader.Next(w);
                        Check.Equal(3L, w.NextStartPosition, "after chunk 1");
                        ChunkReader.Next(w);
                        Check.Equal(6L, w.NextStartPosition, "after chunk 2");
                    }),

                    Descriptors.Case(_SuiteId, "PreviousChunkTracksLastChunk", "Streams: PreviousChunk holds the most recently returned chunk", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 2);
                        ChunkReader.Next(w);
                        Check.Equal("abcd", TestData.Text(w.PreviousChunk), "after chunk 1");
                        ChunkReader.Next(w);
                        Check.Equal("cdef", TestData.Text(w.PreviousChunk), "after chunk 2");
                    }),

                    Descriptors.Case(_SuiteId, "ReturnedChunkNotMutatedLater", "Streams: a returned chunk is not modified by later calls", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 2);
                        byte[] first = w.GetNextChunk(out long p1, out byte[] n1, out bool f1);
                        ChunkReader.Next(w);
                        ChunkReader.Next(w);
                        ChunkReader.Next(w);
                        Check.Equal("abcd", TestData.Text(first), "first chunk after subsequent reads");
                    }),

                    Descriptors.Case(_SuiteId, "ContentLengthShorterThanStream", "Streams: only content length bytes are consumed from a longer stream", () =>
                    {
                        using (TrickleStream s = new TrickleStream(TestData.Ascii("abcdefghijXYZ")))
                        {
                            Streams w = new Streams(s, 10, 4, 2);
                            List<ChunkRecord> chunks = ChunkReader.ReadAll(w, 10);
                            Check.Equal("abcdefghij", TestData.Text(ChunkReader.ConcatNewData(chunks)), "reconstructed");
                            Check.Equal(10L, s.TotalBytesRead, "bytes consumed from stream");
                        }
                    }),

                    Descriptors.Case(_SuiteId, "TruncatedStreamThrows", "Streams: stream shorter than content length throws EndOfStreamException", () =>
                    {
                        using (TrickleStream s = new TrickleStream(TestData.Ascii("abcdef")))
                        {
                            Streams w = new Streams(s, 10, 4, 2);
                            ChunkAssert.Text(ChunkReader.Next(w), 0, "abcd", "abcd", false, "chunk 1");
                            ChunkAssert.Text(ChunkReader.Next(w), 2, "cdef", "ef", false, "chunk 2");
                            Check.Throws<EndOfStreamException>(() => ChunkReader.Next(w), "chunk 3");
                        }
                    }),

                    Descriptors.Case(_SuiteId, "TruncatedTrickleStreamThrows", "Streams: truncated stream with partial reads throws EndOfStreamException", () =>
                    {
                        using (TrickleStream s = new TrickleStream(TestData.Ascii("abc"), 1))
                        {
                            Streams w = new Streams(s, 10, 4, 2);
                            Check.Throws<EndOfStreamException>(() => ChunkReader.Next(w), "chunk 1");
                        }
                    }),

                    Descriptors.Case(_SuiteId, "EmptyStreamThrows", "Streams: empty stream with positive content length throws EndOfStreamException", () =>
                    {
                        using (TrickleStream s = new TrickleStream(new byte[0]))
                        {
                            Streams w = new Streams(s, 5, 2, 1);
                            Check.Throws<EndOfStreamException>(() => ChunkReader.Next(w), "chunk 1");
                        }
                    }),

                    Descriptors.Case(_SuiteId, "StartsAtCurrentStreamPosition", "Streams: reading begins at the stream's current position", () =>
                    {
                        using (MemoryStream ms = new MemoryStream(TestData.Ascii("XYZabcdefg")))
                        {
                            ms.Position = 3;
                            Streams w = new Streams(ms, 7, 4, 2);
                            ChunkAssert.Text(ChunkReader.Next(w), 0, "abcd", "abcd", false, "chunk 1");
                            ChunkAssert.Text(ChunkReader.Next(w), 2, "cdef", "ef", false, "chunk 2");
                            ChunkAssert.Text(ChunkReader.Next(w), 4, "efg", "g", true, "chunk 3");
                        }
                    }),

                    Descriptors.Case(_SuiteId, "MutatingReturnedChunkIsIsolated", "Streams: mutating a returned chunk does not affect later chunks", () =>
                    {
                        Streams w = Create("abcdefghij", 4, 2);
                        byte[] first = w.GetNextChunk(out long p1, out byte[] n1, out bool f1);
                        Check.False(Object.ReferenceEquals(first, n1), "chunk and newData should be distinct arrays");
                        for (int i = 0; i < first.Length; i++) first[i] = (byte)'Z';
                        Check.Equal("abcd", TestData.Text(w.PreviousChunk), "PreviousChunk after mutation");
                        ChunkAssert.Text(ChunkReader.Next(w), 2, "cdef", "ef", false, "chunk 2");
                    }),

                    Descriptors.Case(_SuiteId, "PartialReads", "Streams: streams returning partial reads (1 to 7 bytes per Read) are handled", () =>
                    {
                        byte[] data = TestData.Sequential(100);
                        for (int perRead = 1; perRead <= 7; perRead++)
                        {
                            using (TrickleStream s = new TrickleStream(data, perRead))
                            {
                                Streams w = new Streams(s, data.Length, 16, 5);
                                List<ChunkRecord> expected = WindowModel.Expected(data, 16, 5);
                                List<ChunkRecord> actual = ChunkReader.ReadAll(w, expected.Count + 2);
                                WindowModel.AssertMatches(expected, actual, "perRead " + perRead);
                            }
                        }
                    }),

                    Descriptors.Case(_SuiteId, "StreamDisposedDuringRead", "Streams: disposing the stream mid-traversal surfaces ObjectDisposedException", () =>
                    {
                        MemoryStream ms = new MemoryStream(TestData.Ascii("abcdefghij"));
                        Streams w = new Streams(ms, 10, 4, 2);
                        ChunkReader.Next(w);
                        ms.Dispose();
                        Check.Throws<ObjectDisposedException>(() => ChunkReader.Next(w), "read after dispose");
                    }),

                    Descriptors.Case(_SuiteId, "FileStreamRoundTrip", "Streams: file contents reconstruct from newData", () =>
                    {
                        byte[] data = TestData.Random(64 * 1024 + 13, 99);
                        string path = Path.Combine(Path.GetTempPath(), "slidingwindow-" + Guid.NewGuid().ToString("N") + ".bin");
                        try
                        {
                            File.WriteAllBytes(path, data);
                            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                            {
                                Streams w = new Streams(fs, fs.Length, 1024, 300);
                                List<ChunkRecord> expected = WindowModel.Expected(data, 1024, 300);
                                List<ChunkRecord> actual = ChunkReader.ReadAll(w, expected.Count + 2);
                                WindowModel.AssertMatches(expected, actual, "file");
                                Check.BytesEqual(data, ChunkReader.ConcatNewData(actual), "reconstructed");
                            }
                        }
                        finally
                        {
                            if (File.Exists(path)) File.Delete(path);
                        }
                    }),

                    Descriptors.Case(_SuiteId, "LargeDataReconstructs", "Streams: 1 MiB of random data reconstructs from newData", () =>
                    {
                        byte[] data = TestData.Random(1024 * 1024, 4321);
                        using (TrickleStream s = new TrickleStream(data))
                        {
                            Streams w = new Streams(s, data.Length, 4096, 1000);
                            List<ChunkRecord> chunks = ChunkReader.ReadAll(w, w.ChunkCount() + 2);
                            Check.Equal(w.ChunkCount(), chunks.Count, "chunk count");
                            Check.BytesEqual(data, ChunkReader.ConcatNewData(chunks), "reconstructed");
                        }
                    }),

                    Descriptors.Case(_SuiteId, "ExhaustiveSmallInputs", "Streams: every chunk/shift combination for lengths 1 to 32 matches the reference model", () =>
                    {
                        for (int length = 1; length <= 32; length++)
                        {
                            byte[] data = TestData.Sequential(length);
                            for (int chunkSize = 1; chunkSize <= length; chunkSize++)
                            {
                                for (int shiftSize = 1; shiftSize <= chunkSize; shiftSize++)
                                {
                                    string label = "(" + length + "," + chunkSize + "," + shiftSize + ")";
                                    using (TrickleStream s = new TrickleStream(data))
                                    {
                                        Streams w = new Streams(s, length, chunkSize, shiftSize);
                                        List<ChunkRecord> expected = WindowModel.Expected(data, chunkSize, shiftSize);
                                        Check.Equal(expected.Count, w.ChunkCount(), label + " ChunkCount");
                                        List<ChunkRecord> actual = ChunkReader.ReadAll(w, expected.Count + 2);
                                        WindowModel.AssertMatches(expected, actual, label);
                                        Check.BytesEqual(data, ChunkReader.ConcatNewData(actual), label + " reconstructed");
                                    }
                                }
                            }
                        }
                    })
                });
        }

        private static Streams Create(string text, int chunkSize, int shiftSize)
        {
            byte[] data = TestData.Ascii(text);
            return new Streams(new TrickleStream(data), data.Length, chunkSize, shiftSize);
        }
    }
}
