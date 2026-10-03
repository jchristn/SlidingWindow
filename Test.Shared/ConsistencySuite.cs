namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using SlidingWindow;
    using Touchstone.Core;

    /// <summary>
    /// Tests verifying that <see cref="Bytes"/> and <see cref="Streams"/> produce identical results.
    /// </summary>
    public static class ConsistencySuite
    {
        private const string _SuiteId = "Consistency";

        /// <summary>
        /// Create the suite.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Bytes and Streams consistency",
                cases: new List<TestCaseDescriptor>
                {
                    Descriptors.Case(_SuiteId, "RandomizedParameters", "Bytes and Streams return identical chunks for randomized parameters", () =>
                    {
                        Random rng = new Random(2026);
                        for (int i = 0; i < 200; i++)
                        {
                            int length = rng.Next(1, 2000);
                            int chunkSize = rng.Next(1, length + 1);
                            int shiftSize = rng.Next(1, chunkSize + 1);
                            string label = "(" + length + "," + chunkSize + "," + shiftSize + ")";
                            byte[] data = TestData.Random(length, i);

                            Bytes b = new Bytes(data, chunkSize, shiftSize);
                            Check.Equal(b.ChunkCount(), WindowModel.ChunkCount(length, chunkSize, shiftSize), label + " ChunkCount");
                            List<ChunkRecord> fromBytes = ChunkReader.ReadAll(b, b.ChunkCount() + 2);

                            using (TrickleStream s = new TrickleStream(data, rng.Next(1, 64)))
                            {
                                Streams w = new Streams(s, length, chunkSize, shiftSize);
                                Check.Equal(b.ChunkCount(), w.ChunkCount(), label + " ChunkCount");
                                List<ChunkRecord> fromStreams = ChunkReader.ReadAll(w, w.ChunkCount() + 2);
                                WindowModel.AssertMatches(fromBytes, fromStreams, label);
                            }
                        }
                    }),

                    Descriptors.Case(_SuiteId, "RandomizedAdvance", "Bytes and Streams agree when AdvanceToNewChunk is interleaved randomly", () =>
                    {
                        Random rng = new Random(7);
                        for (int i = 0; i < 200; i++)
                        {
                            int length = rng.Next(1, 500);
                            int chunkSize = rng.Next(1, length + 1);
                            int shiftSize = rng.Next(1, chunkSize + 1);
                            string label = "(" + length + "," + chunkSize + "," + shiftSize + ")";
                            byte[] data = TestData.Random(length, i);

                            Bytes b = new Bytes(data, chunkSize, shiftSize);
                            using (TrickleStream s = new TrickleStream(data, rng.Next(1, 64)))
                            {
                                Streams w = new Streams(s, length, chunkSize, shiftSize);
                                List<ChunkRecord> fromBytes = new List<ChunkRecord>();
                                List<ChunkRecord> fromStreams = new List<ChunkRecord>();

                                for (int call = 0; call <= length + 1; call++)
                                {
                                    if (rng.Next(3) == 0)
                                    {
                                        b.AdvanceToNewChunk();
                                        w.AdvanceToNewChunk();
                                    }

                                    ChunkRecord fromB = ChunkReader.Next(b);
                                    ChunkRecord fromW = ChunkReader.Next(w);
                                    fromBytes.Add(fromB);
                                    fromStreams.Add(fromW);
                                    Check.Equal((long)b.BytesRemaining, w.BytesRemaining, label + " call " + call + " BytesRemaining");
                                    if (fromB.Chunk == null && fromW.Chunk == null) break;
                                }

                                WindowModel.AssertMatches(fromBytes, fromStreams, label);
                                fromBytes.RemoveAt(fromBytes.Count - 1);
                                Check.BytesEqual(data, ChunkReader.ConcatNewData(fromBytes), label + " reconstructed");
                            }
                        }
                    })
                });
        }
    }
}
