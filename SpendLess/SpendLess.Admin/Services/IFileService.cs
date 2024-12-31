
namespace SpendLess.Admin.Services
{
    public interface IFileService
    {
        Task CreateTakeoutFileAsync(string takeoutDir, string fileName, string content);
        string CreateTakeoutWorkDirectory(string jobId);
        string GetTakeoutFilePath(string takeoutDir, string fileName);
        byte[] ReadBytes(string path);
        string ZipTakeoutDirectory(string takeoutDir);
    }
}