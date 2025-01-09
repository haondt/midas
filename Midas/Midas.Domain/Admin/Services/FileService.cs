using Microsoft.Extensions.Options;
using Midas.Domain.Admin.Models;
using System.IO.Compression;

namespace Midas.Domain.Admin.Services
{
    public class FileService(IOptions<FileSettings> options) : IFileService
    {
        public string CreateTakeoutWorkDirectory(string jobId)
        {
            var workDirectory = options.Value.WorkDirectory;
            if (!workDirectory.EndsWith(Path.DirectorySeparatorChar))
                workDirectory += Path.DirectorySeparatorChar;
            var path = Path.Join(workDirectory, $"takeouts/{jobId}/");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Join(path, "files"));
            return path;
        }

        public Task CreateTakeoutFileAsync(string takeoutDir, string fileName, string content)
        {
            var path = GetTakeoutFilePath(takeoutDir, fileName);
            return File.WriteAllTextAsync(path, content);
        }

        public string GetTakeoutFilePath(string takeoutDir, string fileName)
        {
            return Path.Join(takeoutDir, "files", fileName);

        }

        public string ZipTakeoutDirectory(string takeoutDir)
        {
            var filesPath = Path.Join(takeoutDir, "files");
            var zipFilePath = Path.Join(takeoutDir, "takeout.zip");

            ZipFile.CreateFromDirectory(filesPath, zipFilePath);
            return zipFilePath;
        }
        public byte[] ReadBytes(string path)
        {
            return File.ReadAllBytes(path);
        }
    }
}
