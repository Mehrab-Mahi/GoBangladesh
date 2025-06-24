using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GoBangladesh.Application.Services
{
    public class NoticeService : INoticeService
    {
        private readonly IRepository<Notice> _noticeRepository;
        private readonly IRepository<FileModelMapping> _fileModelRepository;
        private readonly IFileService _fileService;

        public NoticeService(IRepository<Notice> noticeRepository,
            IRepository<FileModelMapping> fileModelRepository,
            IFileService fileService)
        {
            _noticeRepository = noticeRepository;
            _fileModelRepository = fileModelRepository;
            _fileService = fileService;
        }

        public PayloadResponse Create(NoticeVm noticeData)
        {
            try
            {
                var notice = new Notice()
                {
                    Name = noticeData.Name,
                    Description = noticeData.Description
                };

                _noticeRepository.Insert(notice);
                _noticeRepository.SaveChanges();

                UploadNoticeFiles(notice.Id, noticeData.Files);

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Notice",
                    Content = null,
                    Message = "Notice created successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Notice",
                    Content = null,
                    Message = $"Notice creation failed because {ex.Message}"
                };
            }
        }

        public PayloadResponse Update(NoticeVm noticeData)
        {
            try
            {
                var notice = _noticeRepository.GetConditional(n => n.Id == noticeData.Id);

                notice.Name = noticeData.Name;
                notice.Description = noticeData.Description;

                if (noticeData.Files != null)
                {
                    UpdateNoticeFiles(notice.Id, noticeData.Files);
                }

                _noticeRepository.Update(notice);
                _noticeRepository.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Notice",
                    Content = null,
                    Message = "Notice update successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Notice",
                    Content = null,
                    Message = $"Notice update failed because {ex.Message}"
                };
            }
        }

        public PayloadResponse Delete(string id)
        {
            try
            {
                var notice = _noticeRepository.GetConditional(n => n.Id == id);

                if (notice == null)
                {
                    return new PayloadResponse
                    {
                        IsSuccess = false,
                        PayloadType = "Notice",
                        Content = null,
                        Message = "Notice not found"
                    };
                }

                _noticeRepository.Delete(notice);
                _noticeRepository.SaveChanges();

                var noticeFiles = _fileModelRepository
                    .GetAll()
                    .Where(f => f.ModelId == id && f.ModelName == "Notice")
                    .ToList();

                _fileModelRepository.Delete(noticeFiles);
                _fileModelRepository.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Notice",
                    Content = null,
                    Message = "Notice deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Notice",
                    Content = null,
                    Message = $"Notice deletion failed because {ex.Message}"
                };
            }
        }

        public NoticeVm Get(string id)
        {
            var notice = _noticeRepository.GetConditional(n => n.Id == id);

            var fileUrls = _fileModelRepository
                .GetAll()
                .Where(m => m.ModelId == id && m.ModelName == "Notice")
                .Select(f => f.FileUrl)
                .ToList();

            return new NoticeVm()
            {
                Id = id,
                Name = notice.Name,
                Description = notice.Description,
                FileUrls = fileUrls,
                PublishDate = notice.CreateTime
            };
        }

        public object GetAll(int pageNo, int pageSize)
        {
            var allNotices = _noticeRepository
                .GetAll()
                .OrderByDescending(n => n.LastModifiedTime);

            var totalRowCount = allNotices.Count();

            var notices = allNotices
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var noticeList = new List<NoticeVm>();

            var files = _fileModelRepository
                .GetAll()
                .Where(u => u.ModelName == "Notice");

            foreach (var notice in notices)
            {
                var fileList = files
                    .Where(f => f.ModelId == notice.Id)
                    .Select(u => u.FileUrl)
                    .ToList();

                var noticeVm = new NoticeVm()
                {
                    Id = notice.Id,
                    Name = notice.Name,
                    Description = notice.Description,
                    FileUrls = fileList,
                    PublishDate = notice.CreateTime
                };

                noticeList.Add(noticeVm);
            }

            return new
            {
                data = noticeList,
                rowCount = totalRowCount
            };
        }

        private void UpdateNoticeFiles(string noticeId, List<IFormFile> noticeFiles) 
        {
            if (noticeFiles.Count > 0)
            {
                var previousFileUrls = _fileModelRepository
                    .GetAll()
                    .Where(n => n.ModelId == noticeId && n.ModelName == "Notice")
                    .ToList();

                foreach (var file in previousFileUrls)
                {
                    _fileService.DeleteFile(file.FileUrl);
                    _fileModelRepository.Delete(file);
                }

                _fileModelRepository.SaveChanges();
            }

            UploadNoticeFiles(noticeId, noticeFiles);
        }

        private void UploadNoticeFiles(string noticeId, List<IFormFile> noticeFiles)
        {
            foreach (var file in noticeFiles)
            {
                var filePath = _fileService.UploadFile(file, "Notice");

                _fileModelRepository.Insert(new FileModelMapping()
                {
                    ModelName = "Notice",
                    FileUrl = filePath,
                    ModelId = noticeId,
                    Type = "Image"
                });
            }

            _fileModelRepository.SaveChanges();
        }
    }
}
