using System.Security.Cryptography;
using System.Text;

namespace FileTransferToolTask {
    class Program
    {
        public static string ByteArrayToString(byte[] arr)
        {
            StringBuilder stringBuilder = new StringBuilder(arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                stringBuilder.Append(arr[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }
        public static void CopyFile(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"Source does not exist at '{sourcePath}'.");
                return;
            }
            Console.WriteLine("Starting copying file...");

            string fileName = Path.GetFileName(sourcePath);
            destinationPath = Path.Combine(destinationPath, fileName);

            const int ChunkSize = 1024;
            const int MaxRetries = 3;
            byte[] buffer = new byte[ChunkSize];
            int totalBytesCopied = 0;
            int blockIndex = 1;

            using FileStream fsRead = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            using FileStream fsWrite = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite);

            while (true)
            {
                int bytesRead = fsRead.Read(buffer, 0, ChunkSize);
                if (bytesRead == 0)
                    break;

                byte[] sourceChunk = buffer[..bytesRead];
                byte[] sourceHash = MD5.HashData(sourceChunk);

                Console.WriteLine($"{blockIndex}. position = {totalBytesCopied}, hash = {ByteArrayToString(sourceHash)}");

                bool verified = false;
                int retryCount = 0;

                while (!verified && retryCount<MaxRetries)
                {
                    fsWrite.Seek(totalBytesCopied, SeekOrigin.Begin);
                    fsWrite.Write(sourceChunk, 0, bytesRead);
                    fsWrite.Flush();

                    fsWrite.Seek(totalBytesCopied, SeekOrigin.Begin);
                    byte[] destinationChunk = new byte[bytesRead];
                    fsWrite.Read(destinationChunk, 0, bytesRead);
                    byte[] destinationHash = MD5.HashData(destinationChunk);

                    if (sourceHash.SequenceEqual(destinationHash))
                    {
                        verified = true;
                    }
                    else
                    {
                        retryCount++;
                        Console.WriteLine("Chunk failed verification!!!");
                    }
                }

                if (!verified)
                {
                    Console.WriteLine($"Copying failed after {retryCount} retries");
                    return;
                }
                totalBytesCopied += bytesRead;
                blockIndex++;
            }

            Console.WriteLine("File copied successfully.");

        }
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: FileTransferTool <sourcePath> <destinationPath>");
                return;
            }
            string source = args[0];
            string destination = args[1];
            CopyFile(source, destination);
        }
    }
}