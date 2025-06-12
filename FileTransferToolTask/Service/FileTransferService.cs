using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileTransferToolTask.Service
{
    public class FileTransferService : IFileTransferService
    {
        private long offset = 0;
        private readonly int MaxRetries;
        private readonly int ChunkSize;
        private readonly int numberOfThreads;
        private readonly IHashService _hashService;

        public FileTransferService(int maxRetries, int chunkSize, int numberOfThreads, IHashService hashService)
        {
            MaxRetries = maxRetries;
            ChunkSize = chunkSize;
            this.numberOfThreads = numberOfThreads;
            _hashService = hashService;
        }

        private static byte[] ReadChunk(FileStream stream, long position, int size)
        {
            stream.Seek(position, SeekOrigin.Begin);
            byte[] buffer = new byte[size];
            int bytesRead = stream.Read(buffer, 0, size);
            return buffer[..bytesRead];
        }

        private static void WriteChunk(FileStream stream, byte[] data, long position)
        {
            stream.Seek(position, SeekOrigin.Begin);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        private static bool VerifyChunk(FileStream stream, byte[] expectedHash, long position, int size)
        {
            stream.Seek(position, SeekOrigin.Begin);
            byte[] buffer = new byte[size];
            stream.Read(buffer, 0, size);
            byte[] actualHash = MD5.HashData(buffer);
            return expectedHash.SequenceEqual(actualHash);
        }

        private bool RetryWriteAndVerify(FileStream fsWrite, byte[] sourceChunk, byte[] sourceHash, long currentPosition)
        {
            int retryCount = 0;

            while (retryCount < MaxRetries)
            {
                WriteChunk(fsWrite, sourceChunk, currentPosition);
                bool isVerified = VerifyChunk(fsWrite, sourceHash, currentPosition, sourceChunk.Length);
                if (isVerified)
                    return true;
                
                retryCount++;
                Console.WriteLine("Chunk failed verification!!!");
            }
            return false;
        }

        private void CopyChunks(string sourcePath, string destinationPath, long fileSize)
        {
            using FileStream fsRead = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using FileStream fsWrite = new FileStream(destinationPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            while (true)
            {
                long currentPosition = Interlocked.Add(ref offset, ChunkSize) - ChunkSize;

                if (currentPosition >= fileSize)
                    break;

                int actualSize = (int)Math.Min(ChunkSize, fileSize - currentPosition);
                
                byte[] sourceChunk = ReadChunk(fsRead, currentPosition, actualSize);
                byte[] sourceHash = MD5.HashData(sourceChunk);

                long blockIndex = currentPosition / ChunkSize + 1;
                string hash = _hashService.ByteArrayToString(sourceHash);
                Console.WriteLine($"{blockIndex}. position = {currentPosition}, hash = {hash}");

                bool verified = RetryWriteAndVerify(fsWrite, sourceChunk, sourceHash, currentPosition);

                if (!verified)
                {
                    Console.WriteLine($"Copying failed after {MaxRetries} retries");
                    return;
                }
            }
        }
        public void CopyFile(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"Source does not exist at '{sourcePath}'.");
                return;
            }

            if (!Directory.Exists(destinationPath))
            {
                Console.WriteLine($"Directory does not exist at '{destinationPath}'.");
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
            Thread[] threads = new Thread[numberOfThreads];

            for (int i = 0; i < numberOfThreads; i++)
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
            Console.WriteLine("Verifying file copy by calculating source and destination hashes. This may take a moment...");

            _hashService.ComputeFileHash(sourcePath, destinationPath, _hashService.GetHashAlgorithm(HashType.SHA256));
        }
    }
}
