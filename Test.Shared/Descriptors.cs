namespace Test.Shared
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Touchstone.Core;

    /// <summary>
    /// Factory helpers for building test case descriptors from synchronous test bodies.
    /// </summary>
    public static class Descriptors
    {
        /// <summary>
        /// Create a descriptor for a synchronous test body.
        /// </summary>
        /// <param name="suiteId">Suite identifier.</param>
        /// <param name="caseId">Case identifier.</param>
        /// <param name="displayName">Display name.</param>
        /// <param name="body">Test body. Throws on failure.</param>
        /// <returns>Test case descriptor.</returns>
        public static TestCaseDescriptor Case(string suiteId, string caseId, string displayName, Action body)
        {
            return new TestCaseDescriptor(
                suiteId: suiteId,
                caseId: caseId,
                displayName: displayName,
                executeAsync: (CancellationToken ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    body();
                    return Task.CompletedTask;
                });
        }
    }
}
