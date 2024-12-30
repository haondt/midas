
namespace SpendLess.Admin.Services
{
    public interface IFileService
    {
        Task CreateTakeoutFile(string takeoutDir, string fileName, string content);
        string CreateTakeoutWorkDirectory(string jobId);
        byte[] ReadBytes(string path);
        string ZipTakeoutDirectory(string takeoutDir);
    }
}