using System;
using System.Text;
using SlidingWindow;

namespace Test
{
    class Program
    {
        static Bytes _Bytes;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Data [ENTER to exit]: ");
                string data = Console.ReadLine();
                if (String.IsNullOrEmpty(data)) break;

                Console.Write("Chunk size: ");
                int chunkSize = Convert.ToInt32(Console.ReadLine());
                Console.Write("Shift size: ");
                int shiftSize = Convert.ToInt32(Console.ReadLine());

                byte[] byteData = Encoding.UTF8.GetBytes(data);
                _Bytes = new Bytes(byteData, chunkSize, shiftSize); 

                Console.WriteLine("Input data size: " + byteData.Length);
                Console.WriteLine("Chunk count: " + _Bytes.ChunkCount());

                byte[] bytes = null;
                int chunkCount = 1;
                byte[] ret = null;

                while (true)
                {
                    bool finalChunk = false;
                    long position = 0;
                    byte[] newData = null;
                    bytes = _Bytes.GetNextChunk(out position, out newData, out finalChunk);

                    Console.WriteLine(
                        "Chunk " + chunkCount + " at index " + position + " [" + bytes.Length + " bytes]: " + Environment.NewLine +
                        "   Chunk data : '" + Encoding.UTF8.GetString(bytes) + "'" + Environment.NewLine +
                        "   New data   : '" + Encoding.UTF8.GetString(newData) + "'");

                    if (ret == null)
                    {
                        ret = new byte[newData.Length];
                        Buffer.BlockCopy(newData, 0, ret, 0, newData.Length);
                        Console.WriteLine("Returned data is now: '" + Encoding.UTF8.GetString(ret) + "'");
                    }
                    else
                    {
                        ret = AppendBytes(ret, newData);
                        Console.WriteLine("Returned data is now: '" + Encoding.UTF8.GetString(ret) + "'");
                    }

                    if (finalChunk) break;
                    chunkCount++;
                }

                Console.WriteLine("Source data   : '" + Encoding.UTF8.GetString(byteData) + "'");
                Console.WriteLine("Returned data : '" + Encoding.UTF8.GetString(ret) + "'");
                Console.WriteLine();
            }
        }

        static byte[] AppendBytes(byte[] head, byte[] tail)
        {
            byte[] arrayCombined = new byte[head.Length + tail.Length];
            Array.Copy(head, 0, arrayCombined, 0, head.Length);
            Array.Copy(tail, 0, arrayCombined, head.Length, tail.Length);
            return arrayCombined;
        }
    }
}
