using System.Security.Cryptography;

namespace FileTransferToolTask {
    class Program
    {
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

            const int ChunkSize = 1024 * 1024;
            byte[] buffer = new byte[ChunkSize];
            int totalBytesCopied = 0;

            using FileStream fsRead = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            using FileStream fsWrite = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite);

            while (true)
            {
                int bytesRead = fsRead.Read(buffer, 0, ChunkSize);
                if (bytesRead == 0)
                    break;

                byte[] sourceChunk = buffer[..bytesRead];
                byte[] sourceHash = MD5.HashData(sourceChunk);

                fsWrite.Write(sourceChunk, 0, bytesRead);
                fsWrite.Seek(totalBytesCopied, SeekOrigin.Begin);

                byte[] destinationChunk = new byte[bytesRead];
                fsWrite.Read(destinationChunk, 0, bytesRead);
                byte[] destinationHash = MD5.HashData(destinationChunk);

                if (!sourceHash.SequenceEqual(destinationHash))
                {
                    Console.WriteLine("Chunk failed verification!!!");
                }
                totalBytesCopied += bytesRead;
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