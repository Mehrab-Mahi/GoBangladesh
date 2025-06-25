using GoBangladesh.Application.Interfaces;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace GoBangladesh.Application.Services
{
    public class CommonService : ICommonService
    {
        private readonly IRepository<Entity> _repo;
        private readonly IFileService _fileService;
        public CommonService(IRepository<Entity> repo, 
            IFileService fileService)
        {
            _repo = repo;
            _fileService = fileService;
        }
        public bool Delete(string id, string table)
        {
            try
            {
                var query = $"delete from {table}s where Id='{id}'";
                _repo.ExecuteQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string GetPasswordHash(string password)
        {
            var defaultPass = Guid.NewGuid().ToString("N");
            if (!string.IsNullOrEmpty(password))
            {
                defaultPass = password;
            }
            return BCrypt.Net.BCrypt.HashPassword(defaultPass, workFactor: 12);
        }

        public string UploadAndGetImageUrl(IFormFile userProfilePicture, string fileSavePath)
        {
            if (userProfilePicture is null) return string.Empty;

            var fileName = GetFileName(userProfilePicture.FileName);

            return UploadFile(fileName, fileSavePath, userProfilePicture);
        }

        public void DeleteFile(string path)
        {
            _fileService.DeleteFile(path);
        }

        private string GetFileName(string fileName)
        {
            return Guid.NewGuid().ToString("N") + "-" + fileName;
        }

        private string UploadFile(string fileName, string fileSavePath, IFormFile file)
        {
            var path = Path.Combine(_fileService.GetRootPath(), fileSavePath);
            _fileService.CreateDirectoryIfNotExists(path);
            var filePath = Path.Combine(path, fileName);
            _fileService.SaveFile(filePath, file);
            return Path.Combine(fileSavePath, fileName);
        }
    }
}
