using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileTransferToolTask.Service
{
    public interface IHashService
    {
        string ByteArrayToString(byte[] arr);
        void ComputeFileHash(string sourcePath, string destinationPath, HashAlgorithm hashAlgorithm);
        HashAlgorithm GetHashAlgorithm(HashType hashType);
    }
}
