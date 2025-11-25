using GoBangladesh.Application.Interfaces;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoBangladesh.Application.DTOs.Dashboard;
using GoBangladesh.Application.DTOs;
using GoBangladesh.Application.Util;

namespace GoBangladesh.Application.Services
{
    public class CommonService : ICommonService
    {
        private readonly IRepository<Entity> _repo;
        private readonly IFileService _fileService;
        private readonly IBaseRepository _baseRepository;
        private readonly IRepository<User> _userRepository;
        public CommonService(IRepository<Entity> repo, 
            IFileService fileService,
            IBaseRepository baseRepository, 
            IRepository<User> userRepository)
        {
            _repo = repo;
            _fileService = fileService;
            _baseRepository = baseRepository;
            _userRepository = userRepository;
        }
        public bool Delete(string id, string table)
        {
            try
            {
                var query = $"delete from {table}s where Id='{id}'";
                _repo.ExecuteQuery(query);
                return true;
            }
            catch (Exception)
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

        public string UploadAndGetImageUrl(IFormFile imageFile, string fileSavePath)
        {
            if (imageFile is null) return string.Empty;

            var fileName = GetFileName(imageFile.FileName);

            return UploadFile(fileName, fileSavePath, imageFile);
        }

        public bool IsFileExists(string path)
        {
            return _fileService.IsFileExists(path);
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

        public List<PassengerCardMappingDto> GetUserListByCardIds(List<string> cardIds)
        {
            var query = $@"select u.*,c.Id as CardId, c.CardNumber as CardNumber from Cards c
                        left join PassengerCardMappings pcm on c.Id = pcm.CardId
                        left join PassengerCardHistory pch on c.Id = pch.CardId
                        left join Users u on pcm.UserId = u.Id or pch.UserId = u.Id
                        where c.Id in ('{string.Join("','", cardIds)}')";
            var userList = _baseRepository.Query<PassengerCardMappingDto>(query);

            return userList;
        }

        public User GetPassengerDataFromMappingDto(PassengerCardMappingDto passengerCardMappingDto)
        {
            if (passengerCardMappingDto == null)
            {
                return null;
            }

            return new User()
            {
                Id = passengerCardMappingDto.Id,
                Name = passengerCardMappingDto.Name,
                EmailAddress = passengerCardMappingDto.EmailAddress,
                MobileNumber = passengerCardMappingDto.MobileNumber,
                DateOfBirth = passengerCardMappingDto.DateOfBirth,
                Gender = passengerCardMappingDto.Gender,
                UserType = passengerCardMappingDto.UserType,
                PassengerId = passengerCardMappingDto.PassengerId,
                OrganizationId = passengerCardMappingDto.OrganizationId,
                Code = passengerCardMappingDto.Code,
                Designation = passengerCardMappingDto.Designation,
                CreateTime = passengerCardMappingDto.CreateTime,
            };
        }

        public List<string> GetCardIdsFromPassengerId(string passengerId)
        {
            var query = $@"select c.Id as CardId from Cards c
                        left join PassengerCardMappings pcm on c.Id = pcm.CardId
                        left join PassengerCardHistory pch on c.Id = pch.CardId
                        left join Users u on pcm.UserId = u.Id or pch.UserId = u.Id
                        where u.Id = '{passengerId}'";
            var cardIds = _baseRepository.Query<string>(query);

            return cardIds;
        }

        public string UploadMultipleFilesAndGetCommaSeparatedUrl(List<IFormFile> fileList, string fileSavePath)
        {
            if (!fileList.Any()) return string.Empty;
            var uploadedFilePaths = new List<string>();

            foreach (var file in fileList)
            {
                var fileName = GetFileName(file.FileName);
                var uploadedFilePath = UploadFile(fileName, fileSavePath, file);
                uploadedFilePaths.Add(uploadedFilePath);
            }

            return string.Join(",", uploadedFilePaths);
        }

        public List<User> GetAdminListByOrganizationId(string organizationId)
        {
            if (string.IsNullOrEmpty(organizationId)) return new List<User>();

            var adminList = _userRepository.GetConditionalList(u =>
                    u.OrganizationId == organizationId &&
                    u.UserType == UserTypes.Admin)
                .ToList();
            return adminList;
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
