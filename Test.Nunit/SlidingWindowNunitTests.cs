namespace Test.Nunit
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// Runs each shared descriptor as a separate NUnit test case.
    /// </summary>
    [TestFixture]
    public sealed class SlidingWindowNunitTests
    {
        private static IEnumerable TestCases()
        {
            return new TouchstoneTestCaseSource(SlidingWindowSuites.All);
        }

        /// <summary>
        /// Run a single test case.
        /// </summary>
        /// <param name="testCase">Test case.</param>
        /// <returns>Task.</returns>
        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
