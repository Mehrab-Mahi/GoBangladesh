using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using System;

namespace GoBangladesh.Application.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string GetRootPath()
        {
            return _webHostEnvironment.WebRootPath;
        }

        public void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void SaveFile(string filePath, IFormFile campaignBanner)
        {
            if (campaignBanner.Length <= 0) return;
            using MemoryStream ms = new();
            campaignBanner.CopyTo(ms);
            var fileBytes = ms.ToArray();
            File.WriteAllBytes(filePath, fileBytes);
        }

        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public string UploadFile(IFormFile file, string folderName)
        {
            if (file is null) return string.Empty;

            var fileName = GetFileName(file.FileName);
            var path = Path.Combine(GetRootPath(), $"{folderName}");
            CreateDirectoryIfNotExists(path);
            var filePath = Path.Combine(path, fileName);
            SaveFile(filePath, file);
            return Path.Combine(folderName,fileName);
        }

        private static string GetFileName(string fileName)
        {
            return Guid.NewGuid().ToString("N") + "-" + fileName;
        }
    }
}
