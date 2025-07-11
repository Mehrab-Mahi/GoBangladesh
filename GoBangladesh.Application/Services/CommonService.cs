using GoBangladesh.Application.Interfaces;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using GoBangladesh.Application.DTOs.Dashboard;

namespace GoBangladesh.Application.Services
{
    public class CommonService : ICommonService
    {
        private readonly IRepository<Entity> _repo;
        private readonly IFileService _fileService;
        private readonly IBaseRepository _baseRepository;
        public CommonService(IRepository<Entity> repo, 
            IFileService fileService,
            IBaseRepository baseRepository)
        {
            _repo = repo;
            _fileService = fileService;
            _baseRepository = baseRepository;
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

        public string GenerateWhereConditionFromConditionList(List<string> condition)
        {
            var whereCondition = string.Join(" and ", condition);

            return !string.IsNullOrEmpty(whereCondition) ?
                $"where {whereCondition}" :
                whereCondition;
        }

        public int GetRowCountForData(string tableName, string whereCondition)
        {
            var query = $"select count(*) from {tableName} {whereCondition}";

            var count = _baseRepository.FirstOrDefault<int>(query);

            return count;
        }

        public List<T> GetFinalData<T>(string tableName, string whereCondition, string extraCondition)
        {
            var query = $"select * from {tableName} {whereCondition} {extraCondition}";

            var data = _baseRepository.Query<T>(query);

            return data;
        }

        public DateTimeFilter GetDateTimeFilterData(DateTime? startDate, DateTime? endDate)
        {
            if (startDate != null && endDate != null)
            {
                if (startDate.Value > endDate.Value)
                    throw new Exception("End date can't be earlier than start date!");

                startDate = startDate.Value.AddHours(-6);
                endDate = endDate.Value.AddHours(17).AddMinutes(59).AddSeconds(59);
            }
            else if (startDate == null && endDate != null)
            {
                startDate = DateTime.MinValue;
                endDate = endDate.Value.AddHours(17).AddMinutes(59).AddSeconds(59);
            }
            else if (startDate != null && endDate == null)
            {
                startDate = startDate.Value.AddHours(-6);
                endDate = DateTime.Today.AddHours(17).AddMinutes(59).AddSeconds(59);
            }
            else
            {
                startDate = DateTime.MinValue;
                endDate = DateTime.Today.AddHours(17).AddMinutes(59).AddSeconds(59);
            }

            var dateTimeFilter = new DateTimeFilter()
            {
                StartDate = startDate!.Value,
                EndDate = endDate.Value
            };

            return dateTimeFilter;
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
