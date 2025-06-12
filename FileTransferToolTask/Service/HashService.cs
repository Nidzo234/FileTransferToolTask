using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileTransferToolTask.Service
{
    public class HashService : IHashService
    {
        public string ByteArrayToString(byte[] arr)
        {
            StringBuilder stringBuilder = new StringBuilder(arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                stringBuilder.Append(arr[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }
        public void ComputeFileHash(string sourcePath, string destinationPath, HashAlgorithm hashAlgorithm)
        {
            try
            {
                using var sourceFileStream = File.OpenRead(sourcePath);
                using var destinationFileStream = File.OpenRead(destinationPath);

                sourceFileStream.Seek(0, SeekOrigin.Begin);
                destinationFileStream.Seek(0, SeekOrigin.Begin);

                byte[] sourceHashValue = hashAlgorithm.ComputeHash(sourceFileStream);
                byte[] destinationHashValue = hashAlgorithm.ComputeHash(destinationFileStream);

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

        public HashAlgorithm GetHashAlgorithm(HashType hashType) => hashType switch
        {
            HashType.MD5 => MD5.Create(),
            HashType.SHA1 => SHA1.Create(),
            HashType.SHA256 => SHA256.Create(),
            _ => throw new NotSupportedException($"Hash type {hashType} is not supported")
        };

    }
}
