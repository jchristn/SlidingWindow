namespace Test.Xunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.XunitAdapter;
    using global::Xunit;

    /// <summary>
    /// Runs every shared descriptor sequentially through the Touchstone executor in a single xUnit fact.
    /// </summary>
    public sealed class SlidingWindowFactTests : TouchstoneFactBase
    {
        /// <summary>
        /// Suites to execute.
        /// </summary>
        protected override IReadOnlyList<TestSuiteDescriptor> Suites
        {
            get { return SlidingWindowSuites.All; }
        }

        /// <summary>
        /// Run all suites.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task RunAll()
        {
            await RunAllAsync();
        }
    }
}
