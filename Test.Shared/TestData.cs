namespace Test.Shared
{
    using System;
    using System.Text;

    /// <summary>
    /// Deterministic test data generators.
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// The sentence used in the README example (47 bytes).
        /// </summary>
        public static readonly string ReadmeSentence = "The quick brown fox jumped over the lazy dog...";

        /// <summary>
        /// Encode a string as UTF-8.
        /// </summary>
        /// <param name="value">String.</param>
        /// <returns>Bytes.</returns>
        public static byte[] Ascii(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        /// <summary>
        /// Decode UTF-8 bytes to a string.
        /// </summary>
        /// <param name="data">Bytes.</param>
        /// <returns>String.</returns>
        public static string Text(byte[]? data)
        {
            if (data == null) return "(null)";
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Generate a byte array whose values cycle through 1 to 251, so that no byte is zero.
        /// Non-zero values make zero-padding defects visible.
        /// </summary>
        /// <param name="length">Length.</param>
        /// <returns>Bytes.</returns>
        public static byte[] Sequential(int length)
        {
            byte[] ret = new byte[length];
            for (int i = 0; i < length; i++) ret[i] = (byte)((i % 251) + 1);
            return ret;
        }

        /// <summary>
        /// Generate pseudo-random bytes using a fixed seed.
        /// </summary>
        /// <param name="length">Length.</param>
        /// <param name="seed">Seed.</param>
        /// <returns>Bytes.</returns>
        public static byte[] Random(int length, int seed)
        {
            byte[] ret = new byte[length];
            new System.Random(seed).NextBytes(ret);
            return ret;
        }

        /// <summary>
        /// Return a slice of a byte array.
        /// </summary>
        /// <param name="data">Source.</param>
        /// <param name="start">Start index.</param>
        /// <param name="length">Length.</param>
        /// <returns>Slice.</returns>
        public static byte[] Slice(byte[] data, int start, int length)
        {
            byte[] ret = new byte[length];
            Buffer.BlockCopy(data, start, ret, 0, length);
            return ret;
        }
    }
}
