namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using SlidingWindow;
    using Touchstone.Core;

    /// <summary>
    /// Construction, validation, and property tests for <see cref="Streams"/>.
    /// </summary>
    public static class StreamsConstructorSuite
    {
        private const string _SuiteId = "StreamsConstructor";

        /// <summary>
        /// Create the suite.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Streams: construction and validation",
                cases: new List<TestCaseDescriptor>
                {
                    Descriptors.Case(_SuiteId, "ValidPropertiesInitialized", "Streams: valid arguments initialize properties", () =>
                    {
                        using (TrickleStream s = new TrickleStream(TestData.Ascii("abcdefghij")))
                        {
                            Streams w = new Streams(s, 10, 4, 2);
                            Check.Equal(4, w.ChunkSize, "ChunkSize");
                            Check.Equal(2, w.ShiftSize, "ShiftSize");
                            Check.Equal(0L, w.NextStartPosition, "NextStartPosition");
                            Check.Equal(10L, w.BytesRemaining, "BytesRemaining");
                            Check.Null(w.PreviousChunk, "PreviousChunk");
                            Check.Equal(0L, s.TotalBytesRead, "constructor should not read from the stream");
                        }
                    }),

                    Descriptors.Case(_SuiteId, "ChunkEqualsContentLengthAllowed", "Streams: chunk size equal to content length is accepted", () =>
                    {
                        using (TrickleStream s = new TrickleStream(TestData.Ascii("abc")))
                        {
                            Streams w = new Streams(s, 3, 3, 1);
                            Check.Equal(1, w.ChunkCount(), "ChunkCount");
                        }
                    }),

                    Descriptors.Case(_SuiteId, "ShiftEqualsChunkAllowed", "Streams: shift size equal to chunk size is accepted", () =>
                    {
                        using (TrickleStream s = new TrickleStream(TestData.Ascii("abcdef")))
                        {
                            Streams w = new Streams(s, 6, 3, 3);
                            Check.Equal(2, w.ChunkCount(), "ChunkCount");
                        }
                    }),

                    Descriptors.Case(_SuiteId, "ChunkCountValues", "Streams: ChunkCount matches expected values", () =>
                    {
                        AssertCount(47, 24, 8, 4);
                        AssertCount(10, 4, 2, 4);
                        AssertCount(10, 4, 3, 3);
                        AssertCount(10, 4, 4, 3);
                        AssertCount(10, 3, 2, 5);
                        AssertCount(10, 1, 1, 10);
                        AssertCount(10, 10, 1, 1);
                        AssertCount(10, 10, 10, 1);
                        AssertCount(11, 4, 3, 4);
                    }),

                    Descriptors.Case(_SuiteId, "NullStreamThrows", "Streams: null stream throws ArgumentNullException", () =>
                    {
                        ArgumentNullException e = Check.Throws<ArgumentNullException>(() => new Streams(null!, 10, 4, 2), "null stream");
                        Check.Equal("stream", e.ParamName, "ParamName");
                    }),

                    Descriptors.Case(_SuiteId, "DisposedStreamThrows", "Streams: disposed (unreadable) stream throws ArgumentException", () =>
                    {
                        MemoryStream ms = new MemoryStream(TestData.Ascii("abcdefghij"));
                        ms.Dispose();
                        Check.Throws<ArgumentException>(() => new Streams(ms, 10, 4, 2), "disposed stream");
                    }),

                    Descriptors.Case(_SuiteId, "WriteOnlyStreamThrows", "Streams: write-only stream throws ArgumentException", () =>
                    {
                        string path = Path.Combine(Path.GetTempPath(), "slidingwindow-" + Guid.NewGuid().ToString("N") + ".bin");
                        try
                        {
                            using (FileStream fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                            {
                                fs.Write(TestData.Ascii("abcdefghij"), 0, 10);
                                Check.Throws<ArgumentException>(() => new Streams(fs, 10, 4, 2), "write-only stream");
                            }
                        }
                        finally
                        {
                            if (File.Exists(path)) File.Delete(path);
                        }
                    }),

                    Descriptors.Case(_SuiteId, "ZeroContentLengthThrows", "Streams: content length zero throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Streams(new MemoryStream(), 0, 1, 1), "content length 0");
                    }),

                    Descriptors.Case(_SuiteId, "NegativeContentLengthThrows", "Streams: negative content length throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Streams(new MemoryStream(), -1, 1, 1), "content length -1");
                        Check.Throws<ArgumentException>(() => new Streams(new MemoryStream(), Int64.MinValue, 1, 1), "content length MinValue");
                    }),

                    Descriptors.Case(_SuiteId, "ZeroChunkSizeThrows", "Streams: chunk size zero throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Streams(new MemoryStream(new byte[3]), 3, 0, 1), "chunk 0");
                    }),

                    Descriptors.Case(_SuiteId, "NegativeChunkSizeThrows", "Streams: negative chunk size throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Streams(new MemoryStream(new byte[3]), 3, -1, 1), "chunk -1");
                    }),

                    Descriptors.Case(_SuiteId, "ChunkLargerThanContentThrows", "Streams: chunk size larger than content length throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Streams(new MemoryStream(new byte[3]), 3, 4, 1), "chunk 4 > 3");
                    }),

                    Descriptors.Case(_SuiteId, "ZeroShiftSizeThrows", "Streams: shift size zero throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Streams(new MemoryStream(new byte[3]), 3, 2, 0), "shift 0");
                    }),

                    Descriptors.Case(_SuiteId, "NegativeShiftSizeThrows", "Streams: negative shift size throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Streams(new MemoryStream(new byte[3]), 3, 2, -1), "shift -1");
                    }),

                    Descriptors.Case(_SuiteId, "ShiftLargerThanChunkThrows", "Streams: shift size larger than chunk size throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Streams(new MemoryStream(new byte[6]), 6, 2, 3), "shift 3 > chunk 2");
                    })
                });
        }

        private static void AssertCount(int length, int chunkSize, int shiftSize, int expected)
        {
            using (TrickleStream s = new TrickleStream(TestData.Sequential(length)))
            {
                Streams w = new Streams(s, length, chunkSize, shiftSize);
                Check.Equal(expected, w.ChunkCount(), "ChunkCount(" + length + "," + chunkSize + "," + shiftSize + ")");
            }
        }
    }
}
