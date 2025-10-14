using System;
using System.Threading.Tasks;

namespace PROG7312_POE.Services
{
    public interface IFileService
    {
        string OpenFileDialog();
        Task<string> SaveFileAsync(string filePath, byte[] fileData);
        string GetFilePath(string fileName);
    }
}
