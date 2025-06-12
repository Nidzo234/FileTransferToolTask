using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileTransferToolTask {
    class Program
    {
        private static long offset = 0;
        private const int ChunkSize = 1024 * 1024;
        private const int MaxRetries = 3;
        private const int threadCount = 2;
        private static string ByteArrayToString(byte[] arr)
        {
            StringBuilder stringBuilder = new StringBuilder(arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                stringBuilder.Append(arr[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        private static void ComputeSHA256Hash(string sourcePath, string destinationPath)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                try
                {
                    var sourceFileStream = File.OpenRead(sourcePath);
                    var destinationFileStream = File.OpenRead(destinationPath);

                    sourceFileStream.Seek(0, SeekOrigin.Begin);
                    destinationFileStream.Seek(0, SeekOrigin.Begin);

                    byte[] sourceHashValue = mySHA256.ComputeHash(sourceFileStream);
                    byte[] destinationHashValue = mySHA256.ComputeHash(destinationFileStream);

                    Console.WriteLine($"Source file hash:      {ByteArrayToString(sourceHashValue)}");
                    Console.WriteLine($"Destination file hash: {ByteArrayToString(destinationHashValue)}");

                    if (sourceHashValue.SequenceEqual(destinationHashValue))
                    {
                        Console.WriteLine("File copied successfully.");
                    }
                    else
                    {
                        Console.WriteLine("File copy completed, but final hash verification failed!");
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine($"I/O Exception: {e.Message}");
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine($"Access Exception: {e.Message}");
                }
            }
        }

        private static void CopyChunks(string sourcePath, string destinationPath, long fileSize)
        {
 
            using FileStream fsRead = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using FileStream fsWrite = new FileStream(destinationPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            while (true)
            {
                long currentPosition = Interlocked.Add(ref offset, ChunkSize) - ChunkSize;

                if (currentPosition >= fileSize)
                    break;


                int actualSize = (int)Math.Min(ChunkSize, fileSize - currentPosition);
                byte [] buffer = new byte[actualSize];

                fsRead.Seek(currentPosition, SeekOrigin.Begin);
                int bytesRead = fsRead.Read(buffer, 0, actualSize);
                if (bytesRead == 0)
                    break;

                byte[] sourceChunk = buffer[..bytesRead];
                byte[] sourceHash = MD5.HashData(sourceChunk);

                long blockIndex = currentPosition / ChunkSize + 1;
                Console.WriteLine($"{blockIndex}. position = {currentPosition}, hash = {ByteArrayToString(sourceHash)}");

                bool verified = false;
                int retryCount = 0;

                while (!verified && retryCount < MaxRetries)
                {
                    fsWrite.Seek(currentPosition, SeekOrigin.Begin);
                    fsWrite.Write(sourceChunk, 0, bytesRead);
                    fsWrite.Flush();

                    fsWrite.Seek(currentPosition, SeekOrigin.Begin);
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
            }
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

            long fileLength = new FileInfo(sourcePath).Length;
            using (FileStream destInit = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
            {
                destInit.SetLength(fileLength);
            }
            offset = 0;

            Thread[] threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
                threads[i] = new Thread(() => CopyChunks(sourcePath, destinationPath, fileLength));
            
            DateTime startTime = DateTime.Now;

            foreach (Thread thread in threads)
                thread.Start();
            
            foreach (Thread thread in threads)
                thread.Join(); 

            DateTime endTime = DateTime.Now;

            using (FileStream flushStream = new FileStream(destinationPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            {
                flushStream.Flush();
            }
            Console.WriteLine(endTime.Subtract(startTime).TotalMilliseconds + "ms");

            ComputeSHA256Hash(sourcePath, destinationPath);

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