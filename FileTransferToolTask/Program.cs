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

            string fileName = Path.GetFileName(sourcePath);
            destinationPath = Path.Combine(destinationPath, fileName);

            const int ChunkSize = 1024 * 1024;
            using FileStream fsRead = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            using FileStream fsWrite = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite);

            byte[] buffer = new byte[ChunkSize];
            while (true)
            {
                int bytesRead = fsRead.Read(buffer, 0, ChunkSize);
                if (bytesRead == 0)
                    break;

                fsWrite.Write(buffer, 0, bytesRead);
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