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
            Console.WriteLine("Please select the test");
            Console.WriteLine("1) Chunk input strings");
            Console.WriteLine("2) Interact with a string");
            Console.Write("Test: ");
            string userInput = Console.ReadLine();

            if (String.IsNullOrEmpty(userInput)) return;
            if (userInput.Equals("1")) ChunkStrings();
            if (userInput.Equals("2")) Interactive();
            return; 
        }

        static void ChunkStrings()
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

        static void Interactive()
        {
            Console.Write("Data [ENTER to exit]: ");
            string data = Console.ReadLine();
            if (String.IsNullOrEmpty(data)) return;
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            Console.Write("Chunk size: ");
            int chunkSize = Convert.ToInt32(Console.ReadLine());
            Console.Write("Shift size: ");
            int shiftSize = Convert.ToInt32(Console.ReadLine());

            _Bytes = new Bytes(byteData, chunkSize, shiftSize);

            Console.WriteLine("Input data size : " + byteData.Length);
            Console.WriteLine("Chunk count     : " + _Bytes.ChunkCount());

            byte[] bytes = null; 
            bool finalChunk = false;
            long position = 0;
            byte[] newData = null;

            while (true)
            {
                Console.Write("Command [next advance q chunksize shiftsize nextstart remaining]: ");
                string userInput = null;
                while (String.IsNullOrEmpty(userInput)) userInput = Console.ReadLine();

                switch (userInput)
                {
                    case "next":
                        bytes = _Bytes.GetNextChunk(out position, out newData, out finalChunk);
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
                        _Bytes.AdvanceToNewChunk();
                        break;
                    case "q":
                        return;
                    case "chunksize":
                        Console.WriteLine("Chunk size: " + _Bytes.ChunkSize);
                        break;
                    case "shiftsize":
                        Console.WriteLine("Shift size: " + _Bytes.ShiftSize);
                        break;
                    case "nextstart":
                        Console.WriteLine("Next start position: " + _Bytes.NextStartPosition);
                        break;
                    case "remaining":
                        Console.WriteLine("Remaining bytes: " + _Bytes.BytesRemaining);
                        break; 
                    default:
                        break;
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
