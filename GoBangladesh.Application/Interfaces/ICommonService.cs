using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace GoBangladesh.Application.Interfaces
{
    public interface ICommonService
    {
        bool Delete(string id, string table);
        string GetPasswordHash(string password);
        string UploadAndGetImageUrl(IFormFile imageFile, string fileSavePath);
        void DeleteFile(string path);
        string GenerateWhereConditionFromConditionList(List<string> condition);
        int GetRowCountForData(string tableName, string whereCondition);
        List<T> GetFinalData<T>(string tableName, string whereCondition, string extraCondition);
    }
}