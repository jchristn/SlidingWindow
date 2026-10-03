namespace Test.Shared
{
    using System.Collections.Generic;
    using Touchstone.Core;

    /// <summary>
    /// Entry point exposing every SlidingWindow test suite to the runners.
    /// </summary>
    public static class SlidingWindowSuites
    {
        /// <summary>
        /// All test suites.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    BytesConstructorSuite.Create(),
                    BytesChunkingSuite.Create(),
                    BytesAdvanceSuite.Create(),
                    StreamsConstructorSuite.Create(),
                    StreamsChunkingSuite.Create(),
                    StreamsAdvanceSuite.Create(),
                    ConsistencySuite.Create()
                };
            }
        }
    }
}
