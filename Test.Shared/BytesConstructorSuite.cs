namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using SlidingWindow;
    using Touchstone.Core;

    /// <summary>
    /// Construction, validation, and property tests for <see cref="Bytes"/>.
    /// </summary>
    public static class BytesConstructorSuite
    {
        private const string _SuiteId = "BytesConstructor";

        /// <summary>
        /// Create the suite.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Bytes: construction and validation",
                cases: new List<TestCaseDescriptor>
                {
                    Descriptors.Case(_SuiteId, "ValidPropertiesInitialized", "Bytes: valid arguments initialize properties", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdefghij"), 4, 2);
                        Check.Equal(4, b.ChunkSize, "ChunkSize");
                        Check.Equal(2, b.ShiftSize, "ShiftSize");
                        Check.Equal(0, b.NextStartPosition, "NextStartPosition");
                        Check.Equal(4, b.NextEndPosition, "NextEndPosition");
                        Check.Equal(10, b.BytesRemaining, "BytesRemaining");
                        Check.BytesEqual(TestData.Ascii("abcdefghij"), b.Data, "Data");
                    }),

                    Descriptors.Case(_SuiteId, "DataIsDefensiveCopy", "Bytes: input array is copied, later mutation has no effect", () =>
                    {
                        byte[] input = TestData.Ascii("abcdefghij");
                        Bytes b = new Bytes(input, 4, 2);
                        input[0] = (byte)'Z';
                        Check.False(Object.ReferenceEquals(input, b.Data), "Data should not reference the input array");
                        Check.Equal((byte)'a', b.Data[0], "Data[0]");
                        Check.Equal("abcd", TestData.Text(ChunkReader.Next(b).Chunk), "First chunk");
                    }),

                    Descriptors.Case(_SuiteId, "ChunkEqualsDataLengthAllowed", "Bytes: chunk size equal to data length is accepted", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abc"), 3, 1);
                        Check.Equal(1, b.ChunkCount(), "ChunkCount");
                    }),

                    Descriptors.Case(_SuiteId, "ShiftEqualsChunkAllowed", "Bytes: shift size equal to chunk size is accepted", () =>
                    {
                        Bytes b = new Bytes(TestData.Ascii("abcdef"), 3, 3);
                        Check.Equal(2, b.ChunkCount(), "ChunkCount");
                    }),

                    Descriptors.Case(_SuiteId, "SingleByteData", "Bytes: single byte with chunk 1 and shift 1 is accepted", () =>
                    {
                        Bytes b = new Bytes(new byte[] { 0x42 }, 1, 1);
                        Check.Equal(1, b.ChunkCount(), "ChunkCount");
                        Check.Equal(1, b.BytesRemaining, "BytesRemaining");
                    }),

                    Descriptors.Case(_SuiteId, "ChunkCountValues", "Bytes: ChunkCount matches expected values", () =>
                    {
                        AssertCount(47, 24, 8, 4);
                        AssertCount(10, 4, 2, 4);
                        AssertCount(10, 4, 3, 3);
                        AssertCount(10, 4, 4, 3);
                        AssertCount(10, 3, 2, 5);
                        AssertCount(10, 1, 1, 10);
                        AssertCount(10, 10, 1, 1);
                        AssertCount(10, 10, 10, 1);
                        AssertCount(10, 9, 1, 2);
                        AssertCount(11, 4, 3, 4);
                    }),

                    Descriptors.Case(_SuiteId, "NullDataThrows", "Bytes: null data throws ArgumentNullException", () =>
                    {
                        ArgumentNullException e = Check.Throws<ArgumentNullException>(() => new Bytes(null!, 1, 1), "null data");
                        Check.Equal("data", e.ParamName, "ParamName");
                    }),

                    Descriptors.Case(_SuiteId, "EmptyDataThrows", "Bytes: empty data throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Bytes(new byte[0], 1, 1), "empty data");
                    }),

                    Descriptors.Case(_SuiteId, "ZeroChunkSizeThrows", "Bytes: chunk size zero throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Bytes(TestData.Ascii("abc"), 0, 1), "chunk 0");
                    }),

                    Descriptors.Case(_SuiteId, "NegativeChunkSizeThrows", "Bytes: negative chunk size throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Bytes(TestData.Ascii("abc"), -1, 1), "chunk -1");
                        Check.Throws<ArgumentException>(() => new Bytes(TestData.Ascii("abc"), Int32.MinValue, 1), "chunk MinValue");
                    }),

                    Descriptors.Case(_SuiteId, "ChunkLargerThanDataThrows", "Bytes: chunk size larger than data throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Bytes(TestData.Ascii("abc"), 4, 1), "chunk 4 > 3");
                        Check.Throws<ArgumentException>(() => new Bytes(TestData.Ascii("abc"), Int32.MaxValue, 1), "chunk MaxValue");
                    }),

                    Descriptors.Case(_SuiteId, "ZeroShiftSizeThrows", "Bytes: shift size zero throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Bytes(TestData.Ascii("abc"), 2, 0), "shift 0");
                    }),

                    Descriptors.Case(_SuiteId, "NegativeShiftSizeThrows", "Bytes: negative shift size throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Bytes(TestData.Ascii("abc"), 2, -1), "shift -1");
                        Check.Throws<ArgumentException>(() => new Bytes(TestData.Ascii("abc"), 2, Int32.MinValue), "shift MinValue");
                    }),

                    Descriptors.Case(_SuiteId, "ShiftLargerThanChunkThrows", "Bytes: shift size larger than chunk size throws ArgumentException", () =>
                    {
                        Check.Throws<ArgumentException>(() => new Bytes(TestData.Ascii("abcdef"), 2, 3), "shift 3 > chunk 2");
                    })
                });
        }

        private static void AssertCount(int length, int chunkSize, int shiftSize, int expected)
        {
            Bytes b = new Bytes(TestData.Sequential(length), chunkSize, shiftSize);
            Check.Equal(expected, b.ChunkCount(), "ChunkCount(" + length + "," + chunkSize + "," + shiftSize + ")");
        }
    }
}
