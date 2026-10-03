namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using SlidingWindow;
    using Touchstone.Core;

    /// <summary>
    /// Chunk retrieval tests for <see cref="Bytes"/>.
    /// </summary>
    public static class BytesChunkingSuite
    {
        private const string _SuiteId = "BytesChunking";

        /// <summary>
        /// Create the suite.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Bytes: chunk retrieval",
                cases: new List<TestCaseDescriptor>
                {
                    Descriptors.Case(_SuiteId, "ReadmeExample", "Bytes: README example (47 bytes, chunk 24, shift 8)", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii(TestData.ReadmeSentence), 24, 8);
                        Check.Equal(4, b.ChunkCount(), "ChunkCount");
                        ChunkAssert.Text(ChunkReader.Next(b), 0, "The quick brown fox jump", "The quick brown fox jump", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(b), 8, "k brown fox jumped over ", "ed over ", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(b), 16, "fox jumped over the lazy", "the lazy", false, "chunk 3");
                        ChunkAssert.Text(ChunkReader.Next(b), 24, "ed over the lazy dog...", " dog...", true, "chunk 4");
                    }),

                    Descriptors.Case(_SuiteId, "OverlappingEvenShift", "Bytes: overlapping windows, shift divides remainder (10, 4, 2)", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdefghij"), 4, 2);
                        ChunkAssert.Text(ChunkReader.Next(b), 0, "abcd", "abcd", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(b), 2, "cdef", "ef", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(b), 4, "efgh", "gh", false, "chunk 3");
                        ChunkAssert.Text(ChunkReader.Next(b), 6, "ghij", "ij", true, "chunk 4");
                    }),

                    Descriptors.Case(_SuiteId, "OverlappingUnevenShift", "Bytes: overlapping windows, partial final chunk (10, 3, 2)", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdefghij"), 3, 2);
                        ChunkAssert.Text(ChunkReader.Next(b), 0, "abc", "abc", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(b), 2, "cde", "de", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(b), 4, "efg", "fg", false, "chunk 3");
                        ChunkAssert.Text(ChunkReader.Next(b), 6, "ghi", "hi", false, "chunk 4");
                        ChunkAssert.Text(ChunkReader.Next(b), 8, "ij", "j", true, "chunk 5");
                    }),

                    Descriptors.Case(_SuiteId, "NonOverlapping", "Bytes: shift equal to chunk produces non-overlapping windows (10, 4, 4)", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdefghij"), 4, 4);
                        ChunkAssert.Text(ChunkReader.Next(b), 0, "abcd", "abcd", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(b), 4, "efgh", "efgh", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(b), 8, "ij", "ij", true, "chunk 3");
                    }),

                    Descriptors.Case(_SuiteId, "ShiftOne", "Bytes: shift of one byte (6, 3, 1)", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdef"), 3, 1);
                        ChunkAssert.Text(ChunkReader.Next(b), 0, "abc", "abc", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(b), 1, "bcd", "d", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(b), 2, "cde", "e", false, "chunk 3");
                        ChunkAssert.Text(ChunkReader.Next(b), 3, "def", "f", true, "chunk 4");
                    }),

                    Descriptors.Case(_SuiteId, "ChunkOneShiftOne", "Bytes: chunk and shift of one byte returns each byte", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("xyz"), 1, 1);
                        ChunkAssert.Text(ChunkReader.Next(b), 0, "x", "x", false, "chunk 1");
                        ChunkAssert.Text(ChunkReader.Next(b), 1, "y", "y", false, "chunk 2");
                        ChunkAssert.Text(ChunkReader.Next(b), 2, "z", "z", true, "chunk 3");
                    }),

                    Descriptors.Case(_SuiteId, "SingleByte", "Bytes: single byte data returns one final chunk", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("q"), 1, 1);
                        ChunkAssert.Text(ChunkReader.Next(b), 0, "q", "q", true, "chunk 1");
                    }),

                    Descriptors.Case(_SuiteId, "ChunkEqualsLength", "Bytes: chunk equal to data length returns one final chunk", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdefghij"), 10, 3);
                        ChunkAssert.Text(ChunkReader.Next(b), 0, "abcdefghij", "abcdefghij", true, "chunk 1");
                    }),

                    Descriptors.Case(_SuiteId, "ChunkAndShiftEqualLength", "Bytes: chunk and shift equal to data length returns one final chunk", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdefghij"), 10, 10);
                        ChunkAssert.Text(ChunkReader.Next(b), 0, "abcdefghij", "abcdefghij", true, "chunk 1");
                    }),

                    Descriptors.Case(_SuiteId, "CallAfterFinalReturnsNull", "Bytes: GetNextChunk after the final chunk returns null", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdefghij"), 4, 2);
                        List<ChunkRecord> chunks = ChunkReader.ReadAll(b, 10);
                        Check.Equal(4, chunks.Count, "chunks before final");
                        for (int i = 0; i < 3; i++) ChunkAssert.End(ChunkReader.Next(b), "after final, call " + i);
                    }),

                    Descriptors.Case(_SuiteId, "BytesRemainingProgression", "Bytes: BytesRemaining decreases as chunks are read", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdefghij"), 4, 2);
                        Check.Equal(10, b.BytesRemaining, "before chunk 1");
                        ChunkReader.Next(b);
                        Check.Equal(6, b.BytesRemaining, "after chunk 1");
                        ChunkReader.Next(b);
                        Check.Equal(4, b.BytesRemaining, "after chunk 2");
                        ChunkReader.Next(b);
                        Check.Equal(2, b.BytesRemaining, "after chunk 3");
                        ChunkReader.Next(b);
                        Check.Equal(0, b.BytesRemaining, "after chunk 4");
                    }),

                    Descriptors.Case(_SuiteId, "PositionProgression", "Bytes: NextStartPosition and NextEndPosition advance by shift size", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdefghij"), 4, 3);
                        Check.Equal(0, b.NextStartPosition, "start before chunk 1");
                        Check.Equal(4, b.NextEndPosition, "end before chunk 1");
                        ChunkReader.Next(b);
                        Check.Equal(3, b.NextStartPosition, "start after chunk 1");
                        Check.Equal(7, b.NextEndPosition, "end after chunk 1");
                        ChunkReader.Next(b);
                        Check.Equal(6, b.NextStartPosition, "start after chunk 2");
                        Check.Equal(10, b.NextEndPosition, "end after chunk 2");
                    }),

                    Descriptors.Case(_SuiteId, "ReturnedArraysAreCopies", "Bytes: mutating returned arrays does not affect the window", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdefghij"), 4, 2);
                        byte[] chunk = b.GetNextChunk(out long position, out byte[] newData, out bool finalChunk);
                        Check.False(Object.ReferenceEquals(chunk, newData), "chunk and newData should be distinct arrays");
                        chunk[2] = (byte)'Z';
                        newData[3] = (byte)'Z';
                        Check.Equal("abcdefghij", TestData.Text(b.Data), "Data after mutation");
                        ChunkAssert.Text(ChunkReader.Next(b), 2, "cdef", "ef", false, "chunk 2");
                    }),

                    Descriptors.Case(_SuiteId, "BinaryData", "Bytes: binary data including zero and 0xFF bytes is preserved", () =>
                    {
                        byte[] data = new byte[] { 0x00, 0xFF, 0x00, 0x00, 0x7F, 0x80, 0xFF, 0x00, 0x01 };
                        Bytes b = new Bytes(data, 4, 3);
                        List<ChunkRecord> chunks = ChunkReader.ReadAll(b, 10);
                        WindowModel.AssertMatches(WindowModel.Expected(data, 4, 3), chunks, "binary");
                        Check.BytesEqual(data, ChunkReader.ConcatNewData(chunks), "reconstructed");
                    }),

                    Descriptors.Case(_SuiteId, "LargeDataReconstructs", "Bytes: 1 MiB of random data reconstructs from newData", () =>
                    {
                        byte[] data = TestData.Random(1024 * 1024, 1234);
                        Bytes b = new Bytes(data, 4096, 1000);
                        List<ChunkRecord> chunks = ChunkReader.ReadAll(b, b.ChunkCount() + 2);
                        Check.Equal(b.ChunkCount(), chunks.Count, "chunk count");
                        Check.BytesEqual(data, ChunkReader.ConcatNewData(chunks), "reconstructed");
                    }),

                    Descriptors.Case(_SuiteId, "ExhaustiveSmallInputs", "Bytes: every chunk/shift combination for lengths 1 to 32 matches the reference model", () =>
                    {
                        for (int length = 1; length <= 32; length++)
                        {
                            byte[] data = TestData.Sequential(length);
                            for (int chunkSize = 1; chunkSize <= length; chunkSize++)
                            {
                                for (int shiftSize = 1; shiftSize <= chunkSize; shiftSize++)
                                {
                                    string label = "(" + length + "," + chunkSize + "," + shiftSize + ")";
                                    Bytes b = new Bytes(data, chunkSize, shiftSize);
                                    List<ChunkRecord> expected = WindowModel.Expected(data, chunkSize, shiftSize);
                                    Check.Equal(expected.Count, b.ChunkCount(), label + " ChunkCount");
                                    List<ChunkRecord> actual = ChunkReader.ReadAll(b, expected.Count + 2);
                                    WindowModel.AssertMatches(expected, actual, label);
                                    Check.BytesEqual(data, ChunkReader.ConcatNewData(actual), label + " reconstructed");
                                }
                            }
                        }
                    })
                });
        }
    }
}
