using FileTransferToolTask.Service;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileTransferToolTask {
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: FileTransferTool <sourcePath> <destinationPath>");
                return;
            }
            string source = args[0];
            string destination = args[1];

            int retryCount = 3;
            int chunkSize = 1024 * 1024;
            int numberOfThreads = 2;
            IHashService hashService = new HashService();

            IFileTransferService fileTransferService = new FileTransferService(retryCount, chunkSize, numberOfThreads, hashService);
            fileTransferService.CopyFile(source, destination);
        }
    }
}