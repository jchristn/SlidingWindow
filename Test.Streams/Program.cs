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
            Console.WriteLine("Please select the test");
            Console.WriteLine("1) Read and rewrite a file");
            Console.WriteLine("2) Interact with a stream");
            Console.Write("Test: ");
            string userInput = Console.ReadLine();

            if (String.IsNullOrEmpty(userInput)) return;
            if (userInput.Equals("1")) RewriteFile();
            if (userInput.Equals("2")) Interactive();
            return; 
        }

        static void RewriteFile()
        {
            while (true)
            {
                Console.Write("Input file [ENTER to exit]: ");
                string inFile = Console.ReadLine();
                if (String.IsNullOrEmpty(inFile)) break;

                Console.Write("Output file: ");
                string outFile = Console.ReadLine();

                Console.Write("Chunk size: ");
                int chunkSize = Convert.ToInt32(Console.ReadLine());
                Console.Write("Shift size: ");
                int shiftSize = Convert.ToInt32(Console.ReadLine());

                FileInfo fi = new FileInfo(inFile);
                long contentLength = fi.Length;

                using (FileStream fs = new FileStream(inFile, FileMode.Open))
                {
                    _Streams = new Streams(fs, contentLength, chunkSize, shiftSize);

                    Console.WriteLine("Input data size : " + contentLength);
                    Console.WriteLine("Chunk count     : " + _Streams.ChunkCount());

                    byte[] bytes = null;
                    int chunkCount = 1;
                    byte[] ret = null;

                    while (true)
                    {
                        bool finalChunk = false;
                        long position = 0;
                        byte[] newData = null;
                        bytes = _Streams.GetNextChunk(out position, out newData, out finalChunk);

                        /*
                         * 
                         * Uncomment these lines to debug on small files
                         * 
                         * 
                        Console.WriteLine(
                            "Chunk " + chunkCount + " at index " + position + " [" + bytes.Length + " bytes]: " + Environment.NewLine +
                            "   Chunk data : '" + Encoding.UTF8.GetString(bytes) + "'" + Environment.NewLine +
                            "   New data   : '" + Encoding.UTF8.GetString(newData) + "'");
                         *
                         */

                        if (ret == null)
                        {
                            ret = new byte[newData.Length];
                            Buffer.BlockCopy(newData, 0, ret, 0, newData.Length);
                            // Console.WriteLine("Returned data is now: '" + Encoding.UTF8.GetString(ret) + "'");
                        }
                        else
                        {
                            ret = AppendBytes(ret, newData);
                            // Console.WriteLine("Returned data is now: '" + Encoding.UTF8.GetString(ret) + "'");
                        }

                        if (!String.IsNullOrEmpty(outFile))
                        {
                            using (FileStream outFs = new FileStream(outFile, FileMode.Append))
                            {
                                outFs.Write(newData);
                            }
                        }

                        if (finalChunk) break;
                        chunkCount++;
                    }

                }

                Console.WriteLine();
            }
        }

        static void Interactive()
        {
            Console.Write("Input file [ENTER to exit]: ");
            string inFile = Console.ReadLine();
            if (String.IsNullOrEmpty(inFile)) return;
            
            Console.Write("Chunk size: ");
            int chunkSize = Convert.ToInt32(Console.ReadLine());
            Console.Write("Shift size: ");
            int shiftSize = Convert.ToInt32(Console.ReadLine());

            FileInfo fi = new FileInfo(inFile);
            long contentLength = fi.Length;

            using (FileStream fs = new FileStream(inFile, FileMode.Open))
            {
                _Streams = new Streams(fs, contentLength, chunkSize, shiftSize);

                Console.WriteLine("Input data size : " + contentLength);
                Console.WriteLine("Chunk count     : " + _Streams.ChunkCount());

                byte[] bytes = null;  
                bool finalChunk = false;
                long position = 0;
                byte[] newData = null;

                while (true)
                {
                    Console.Write("Command [next advance q chunksize shiftsize nextstart remaining prev]: ");
                    string userInput = null;
                    while (String.IsNullOrEmpty(userInput)) userInput = Console.ReadLine();

                    switch (userInput)
                    {
                        case "next":
                            bytes = _Streams.GetNextChunk(out position, out newData, out finalChunk);
                            if (bytes != null && bytes.Length > 0
                                && newData != null && newData.Length > 0)
                            {
                                Console.WriteLine("Position   : " + position);
                                Console.WriteLine("Chunk data : '" + Encoding.UTF8.GetString(bytes) + "'");
                                Console.WriteLine("New data   : '" + Encoding.UTF8.GetString(newData) + "'");
                                if (finalChunk) Console.WriteLine("*** Final chunk ***");
                            }
                            else
                            {
                                Console.WriteLine("No data");
                            }
                            break;
                        case "advance":
                            Console.WriteLine("Advancing to next new chunk");
                            _Streams.AdvanceToNewChunk();
                            break;
                        case "q":
                            return;
                        case "chunksize":
                            Console.WriteLine("Chunk size: " + _Streams.ChunkSize);
                            break;
                        case "shiftsize":
                            Console.WriteLine("Shift size: " + _Streams.ShiftSize);
                            break;
                        case "nextstart":
                            Console.WriteLine("Next start position: " + _Streams.NextStartPosition);
                            break;
                        case "remaining":
                            Console.WriteLine("Remaining bytes: " + _Streams.BytesRemaining);
                            break;
                        case "prev":
                            if (_Streams.PreviousChunk == null || _Streams.PreviousChunk.Length < 1)
                            {
                                Console.WriteLine("(null)");
                            }
                            else
                            {
                                Console.WriteLine("Previous chunk: '" + Encoding.UTF8.GetString(_Streams.PreviousChunk) + "'");
                            }
                            break;                            
                        default:
                            break;
                    } 
                } 
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
