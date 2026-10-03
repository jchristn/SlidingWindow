namespace Test.Nunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// Runs every shared descriptor sequentially through the Touchstone executor in a single NUnit test.
    /// </summary>
    [TestFixture]
    public sealed class SlidingWindowNunitFactTests : TouchstoneNunitBase
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
        [Test]
        public async Task RunAll()
        {
            await RunAllAsync().ConfigureAwait(false);
        }
    }
}
