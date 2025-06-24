using Microsoft.AspNetCore.Http;

namespace GoBangladesh.Application.Interfaces
{
    public interface IFileService
    {
        string GetRootPath();
        void CreateDirectoryIfNotExists(string path);
        void SaveFile(string filePath, IFormFile campaignBanner);
        void DeleteFile(string filePath);
        string UploadFile(IFormFile file, string folderName);
    }
}