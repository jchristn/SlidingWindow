using System;
using System.IO;
using System.Text;
using SlidingWindow;

namespace Test
{
    class Program
    {
        static Streams _Streams;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Input file [ENTER to exit]: ");
                string inFile = Console.ReadLine();
                if (String.IsNullOrEmpty(inFile)) break;
                 
                Console.Write("Chunk size: ");
                int chunkSize = Convert.ToInt32(Console.ReadLine());
                Console.Write("Shift size: ");
                int shiftSize = Convert.ToInt32(Console.ReadLine());

                FileInfo fi = new FileInfo(inFile);
                long contentLength = fi.Length;

                using (FileStream fs = new FileStream(inFile, FileMode.Open))
                {
                    _Streams = new Streams(fs, contentLength, chunkSize, shiftSize);
                    _Streams.ConsoleDebug = true;

                    Console.WriteLine("Input data size: " + contentLength);
                    Console.WriteLine("Chunk count: " + _Streams.ChunkCount());

                    byte[] bytes = null;
                    int chunkCount = 1;

                    while (true)
                    {
                        bool finalChunk = false;
                        long position = 0;
                        bytes = _Streams.GetNextChunk(out position, out finalChunk);
                        Console.WriteLine("Chunk " + chunkCount + " at index " + position + " [" + bytes.Length + " bytes]: '" + Encoding.UTF8.GetString(bytes) + "'");
                         
                        if (finalChunk) break;
                        chunkCount++;
                    }

                }

                Console.WriteLine();
            }
        }
    }
}
