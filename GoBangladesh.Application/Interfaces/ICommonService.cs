using Microsoft.AspNetCore.Http;

namespace GoBangladesh.Application.Interfaces
{
    public interface ICommonService
    {
        bool Delete(string id, string table);
        string GetPasswordHash(string password);
        string UploadAndGetImageUrl(IFormFile imageFile, string fileSavePath);
        void DeleteFile(string path);
    }
}