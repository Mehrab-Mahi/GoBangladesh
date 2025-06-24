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
    public class MediaService : IMediaService
    {
        private readonly IRepository<FileModelMapping> _fileModelRepository;
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IFileService _fileService;

        public MediaService(IRepository<FileModelMapping> fileModelRepository,
            IFileService fileService, 
            IRepository<Campaign> campaignRepository)
        {
            _fileModelRepository = fileModelRepository;
            _fileService = fileService;
            _campaignRepository = campaignRepository;
        }

        public PayloadResponse UploadCampaignMedia(MediaVm mediaData)
        {
            try
            {
                if (mediaData.Images is not null)
                {
                    UploadImages(mediaData.ModelId, mediaData.Images, "Campaign");
                }

                if (mediaData.VideoUrls is not null)
                {
                    UploadVideo(mediaData.ModelId, mediaData.VideoUrls, "Campaign");
                }
                
                _fileModelRepository.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Media",
                    Content = null,
                    Message = "Media uploaded successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Media",
                    Content = null,
                    Message = $"Media upload failed because {ex.Message}"
                };
            }
        }

        public MediaDataVm GetAllMedia(MediaDataSizeVm mediaDataSize)
        {
            var imageData = new List<MediaUrlWithCampaignDataDto>();
            var videoData = new List<MediaUrlWithCampaignDataDto>();

            if (mediaDataSize.ImagePageNo > 0)
            {
                var imageModelMapping = _fileModelRepository
                    .GetAll()
                    .Where(u => u.ModelName == "Campaign" && u.Type == "Image")
                    .OrderByDescending(c => c.LastModifiedTime)
                    .Skip((mediaDataSize.ImagePageNo - 1) * mediaDataSize.ImagePageSize)
                    .Take(mediaDataSize.ImagePageSize);

                imageData = (from imageModel in imageModelMapping
                    join campaign in _campaignRepository.GetAll()
                        on imageModel.ModelId equals campaign.Id
                    select new MediaUrlWithCampaignDataDto()
                    {
                        CampaignName = campaign.Name,
                        Institute = campaign.Institute,
                        ImageUrl = imageModel.FileUrl
                    })
                    .ToList();
            }

            if (mediaDataSize.VideoPageNo > 0)
            {
                var videoModelMapping = _fileModelRepository
                    .GetAll()
                    .Where(u => u.ModelName == "Campaign" && u.Type == "Video")
                    .OrderByDescending(c => c.LastModifiedTime)
                    .Skip((mediaDataSize.VideoPageNo - 1) * mediaDataSize.VideoPageSize)
                    .Take(mediaDataSize.VideoPageSize);

                videoData = (from videoModel in videoModelMapping
                             join campaign in _campaignRepository.GetAll()
                            on videoModel.ModelId equals campaign.Id
                        select new MediaUrlWithCampaignDataDto()
                        {
                            CampaignName = campaign.Name,
                            Institute = campaign.Institute,
                            ImageUrl = videoModel.FileUrl
                        })
                    .ToList();
            }

            return new MediaDataVm()
            {
                ImageData = imageData,
                VideoData = videoData
            };
        }

        public PayloadResponse DeleteCampaignMedia(MediaDeleteVm mediaDeleteData)
        {
            try
            {
                var fileModel = _fileModelRepository.GetConditional(f =>
                    f.ModelId == mediaDeleteData.CampaignId && f.Id == mediaDeleteData.MediaId);

                if (fileModel.Type == "Image")
                {
                    _fileService.DeleteFile(fileModel.FileUrl);
                }

                _fileModelRepository.Delete(fileModel);
                _fileModelRepository.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Media",
                    Content = null,
                    Message = "Media deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Media",
                    Content = null,
                    Message = $"Media deletion failed because {ex.Message}"
                };
            }
        }

        public MediaDataWithIdVm GetCampaignMedia(string campaignId)
        {
            var imageData = _fileModelRepository
                .GetConditionalList(f => f.ModelId == campaignId && f.Type == "Image")
                .Select(i => new ImageData()
                {
                    Id = i.Id,
                    ImageUrl = i.FileUrl
                })
                .ToList();

            var videoData = _fileModelRepository
                .GetConditionalList(f => f.ModelId == campaignId && f.Type == "Video")
                .Select(i => new VideoData()
                {
                    Id = i.Id,
                    VideoUrl = i.FileUrl
                })
                .ToList();

            return new MediaDataWithIdVm()
            {
                ImageData = imageData,
                VideoData = videoData
            };
        }

        private void UploadVideo(string modelId, List<string> videoUrls, string modelName)
        {
            foreach (var url in videoUrls)
            {
                _fileModelRepository.Insert(new FileModelMapping()
                {
                    ModelName = modelName,
                    ModelId = modelId,
                    Type = "Video",
                    FileUrl = url
                });
            }
        }

        private void UploadImages(string modelId, List<IFormFile> mediaImages, string modelName)
        {
            foreach (var image in mediaImages)
            {
                var filePath = _fileService.UploadFile(image, "Media");
                _fileModelRepository.Insert(new FileModelMapping()
                {
                    ModelName = modelName,
                    ModelId = modelId,
                    Type = "Image",
                    FileUrl = filePath
                });
            }
        }
    }
}
