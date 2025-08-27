using System;
using System.Collections.Generic;
using GoBangladesh.Application.DTOs;
using GoBangladesh.Application.DTOs.Dashboard;
using GoBangladesh.Domain.Entities;
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
        DateTimeFilter GetDateTimeFilterData(DateTime? startDate, DateTime? endDate);
        List<PassengerCardMappingDto> GetUserListByCardIds(List<string> cardId);
        User GetPassengerDataFromMappingDto(PassengerCardMappingDto passengerCardMappingDto);
        List<string> GetCardIdsFromPassengerId(string passengerId);
        string UploadMultipleFilesAndGetCommaSeparatedUrl(List<IFormFile> file, string fileSavePath);
    }
}