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
                _Bytes.ConsoleDebug = true;

                Console.WriteLine("Input data size: " + byteData.Length);
                Console.WriteLine("Chunk count: " + _Bytes.ChunkCount());

                byte[] bytes = null;
                int chunkCount = 1;

                while (true)
                {
                    bool finalChunk = false;
                    bytes = _Bytes.GetNextChunk(out finalChunk);
                    Console.WriteLine("Chunk " + chunkCount + " [" + bytes.Length + " bytes]: '" + Encoding.UTF8.GetString(bytes) + "'");
                    if (finalChunk) break;
                    chunkCount++;
                }

                Console.WriteLine();
            }
        }
    }
}
