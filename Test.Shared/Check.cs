namespace Test.Shared
{
    using System;
    using System.Text;

    /// <summary>
    /// Assertion helpers that throw <see cref="TestAssertionException"/> on failure.
    /// </summary>
    public static class Check
    {
        /// <summary>
        /// Assert that a condition is true.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="message">Failure message.</param>
        public static void True(bool condition, string message)
        {
            if (!condition) throw new TestAssertionException(message);
        }

        /// <summary>
        /// Assert that a condition is false.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="message">Failure message.</param>
        public static void False(bool condition, string message)
        {
            if (condition) throw new TestAssertionException(message);
        }

        /// <summary>
        /// Assert that two values are equal.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="expected">Expected value.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="label">Label describing the value.</param>
        public static void Equal<T>(T expected, T actual, string label)
        {
            if (!Object.Equals(expected, actual))
                throw new TestAssertionException(label + ": expected '" + expected + "' but was '" + actual + "'");
        }

        /// <summary>
        /// Assert that a value is null.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="label">Label describing the value.</param>
        public static void Null(object? value, string label)
        {
            if (value != null) throw new TestAssertionException(label + ": expected null");
        }

        /// <summary>
        /// Assert that a value is not null.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="label">Label describing the value.</param>
        public static void NotNull(object? value, string label)
        {
            if (value == null) throw new TestAssertionException(label + ": expected non-null");
        }

        /// <summary>
        /// Assert that two byte arrays have identical contents.
        /// </summary>
        /// <param name="expected">Expected bytes.</param>
        /// <param name="actual">Actual bytes.</param>
        /// <param name="label">Label describing the value.</param>
        public static void BytesEqual(byte[]? expected, byte[]? actual, string label)
        {
            if (expected == null && actual == null) return;
            if (expected == null || actual == null)
                throw new TestAssertionException(label + ": expected " + Describe(expected) + " but was " + Describe(actual));

            bool match = expected.Length == actual.Length;
            for (int i = 0; match && i < expected.Length; i++)
            {
                if (expected[i] != actual[i]) match = false;
            }

            if (!match)
                throw new TestAssertionException(label + ": expected " + Describe(expected) + " but was " + Describe(actual));
        }

        /// <summary>
        /// Assert that an action throws an exception of exactly the specified type.
        /// </summary>
        /// <typeparam name="TException">Expected exception type.</typeparam>
        /// <param name="action">Action.</param>
        /// <param name="label">Label describing the action.</param>
        /// <returns>The thrown exception.</returns>
        public static TException Throws<TException>(Action action, string label) where TException : Exception
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(TException)) return (TException)e;
                throw new TestAssertionException(label + ": expected " + typeof(TException).Name + " but caught " + e.GetType().Name + ": " + e.Message);
            }

            throw new TestAssertionException(label + ": expected " + typeof(TException).Name + " but no exception was thrown");
        }

        private static string Describe(byte[]? data)
        {
            if (data == null) return "(null)";
            StringBuilder sb = new StringBuilder();
            sb.Append("[" + data.Length + " bytes] ");
            int max = Math.Min(data.Length, 64);
            for (int i = 0; i < max; i++)
            {
                byte b = data[i];
                if (b >= 0x20 && b < 0x7F) sb.Append((char)b);
                else sb.Append("\\x" + b.ToString("X2"));
            }
            if (data.Length > max) sb.Append("...");
            return sb.ToString();
        }
    }
}
