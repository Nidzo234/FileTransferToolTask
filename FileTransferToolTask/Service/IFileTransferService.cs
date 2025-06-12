using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTransferToolTask.Service
{
    public interface IFileTransferService
    {
        void CopyFile(string sourcePath, string destinationPath);
    }
}
