using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PROG7312_POE.Services
{
    public class FileService : IFileService
    {
        private readonly string _basePath;

        public FileService()
        {
            _basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MunicipalityIssueTracker");
            Directory.CreateDirectory(_basePath);
        }

        public string OpenFileDialog()
        {
            using (var dialog = new OpenFileDialog())
            {
                return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
            }
        }

        public Task<string> SaveFileAsync(string filePath, byte[] fileData)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var fullPath = Path.Combine(_basePath, fileName);
                File.WriteAllBytes(fullPath, fileData);
                return Task.FromResult(fullPath);
            }
            catch (Exception)
            {
                // Log the error if needed
                return Task.FromResult<string>(null);
            }
        }

        public string GetFilePath(string fileName)
        {
            var fullPath = Path.Combine(_basePath, fileName);
            return File.Exists(fullPath) ? fullPath : null;
        }
    }
}