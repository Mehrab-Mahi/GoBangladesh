using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GoBangladesh.Application.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<CampaignVolunteerMapping> _campaignVolunteerMappingRepository;
        private readonly IFileService _fileService;
        private readonly ILoggedInUserService _loggedInUserService;

        public CampaignService(IRepository<Campaign> campaignRepository,
            IRepository<CampaignVolunteerMapping> campaignVolunteerMappingRepository,
            IFileService fileService,
            ILoggedInUserService loggedInUserService)
        {
            _campaignRepository = campaignRepository;
            _campaignVolunteerMappingRepository = campaignVolunteerMappingRepository;
            _fileService = fileService;
            _loggedInUserService = loggedInUserService;
        }

        public PayloadResponse Create(CampaignVm campaignData)
        {
            try
            {
                List<string> volunteerList;

                if (campaignData.VolunteerList is { Count: 1 } && campaignData.VolunteerList[0] != null)
                {
                    volunteerList = campaignData.VolunteerList[0].Trim('"', '"').Split(',').ToList();
                }
                else
                {
                    volunteerList = campaignData.VolunteerList;
                }
                
                var bannerUrl = UploadAndGetBannerUrl(campaignData.Banner);
                var campaign = new Campaign()
                {
                    Name = campaignData.Name,
                    StartDate = campaignData.StartDate,
                    EndDate = campaignData.EndDate,
                    Address = campaignData.Address,
                    BannerUrl = bannerUrl,
                    Institute = campaignData.Institute
                };

                _campaignRepository.Insert(campaign);
                _campaignRepository.SaveChanges();

                AssignVolunteerToCampaign(campaign.Id, volunteerList);

                return new PayloadResponse
                {
                    IsSuccess = true,
                    PayloadType = "Campaign created",
                    Content = null,
                    Message = "Campaign created successfully"
                };
            }
            catch
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Campaign",
                    Content = null,
                    Message = "Campaign creation failed"
                };
            }
        }

        public PayloadResponse Update(CampaignVm campaignData)
        {
            try
            {
                List<string> volunteerList;

                if (campaignData.VolunteerList.Count == 1 && campaignData.VolunteerList[0] != null)
                {
                    volunteerList = campaignData.VolunteerList[0].Trim('"', '"').Split(',').ToList();
                }
                else
                {
                    volunteerList = campaignData.VolunteerList;
                }

                var previousData = _campaignRepository.GetConditional(c => c.Id == campaignData.Id);
                var newBannerUrl = string.Empty;

                if (previousData == null)
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Campaign",
                        Content = null,
                        Message = "Data not found"
                    };
                }

                if (campaignData.Banner is { Length: > 0 })
                {
                    _fileService.DeleteFile(previousData.BannerUrl);
                    newBannerUrl = UploadAndGetBannerUrl(campaignData.Banner);
                }

                previousData.Name = campaignData.Name;
                previousData.StartDate = campaignData.StartDate;
                previousData.EndDate = campaignData.EndDate;
                previousData.Address = campaignData.Address;
                previousData.Institute = campaignData.Institute;
                previousData.BannerUrl = !string.IsNullOrEmpty(newBannerUrl) ? newBannerUrl : previousData.BannerUrl;

                _campaignRepository.Update(previousData);
                _campaignRepository.SaveChanges();

                UpdateVolunteerToCampaign(campaignData.Id, volunteerList);

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Campaign updated",
                    Content = null,
                    Message = "Campaign updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Campaign",
                    Content = null,
                    Message = $"Campaign update failed because {ex.Message}"
                };
            }
        }

        public PayloadResponse Delete(string id)
        {
            var campaign = _campaignRepository.GetConditional(c => c.Id == id);

            if (campaign == null)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Campaign",
                    Content = null,
                    Message = "Campaign not found"
                };
            }

            try
            {
                _campaignRepository.Delete(campaign);
                _campaignRepository.SaveChanges();

                var campaignVolunteer = _campaignVolunteerMappingRepository
                    .GetConditionalList(c => c.CampaignId == id)
                    .ToList();

                _campaignVolunteerMappingRepository.Delete(campaignVolunteer);
                _campaignVolunteerMappingRepository.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Campaign delete",
                    Content = null,
                    Message = "Campaign deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Campaign",
                    Content = null,
                    Message = $"Campaign deletion failed because {ex.Message}"
                };
            }
        }
        
        public CampaignVm Get(string id)
        {
            var campaign = _campaignRepository.GetConditional(c => c.Id == id);

            if (campaign == null)
            {
                return new CampaignVm();
            }

            var volunteerList = _campaignVolunteerMappingRepository
                .GetConditionalList(c => c.CampaignId == id)
                .Select(c => c.VolunteerId)
                .ToList();

            return new CampaignVm()
            {
                Id = id,
                Name = campaign.Name,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Address = campaign.Address,
                Institute = campaign.Institute,
                BannerUrl = campaign.BannerUrl,
                VolunteerList = volunteerList
            };
        }

        public object GetAll(int pageNo, int pageSize)
        {
            var totalRowCount = _campaignRepository
                .GetAll()
                .OrderByDescending(c => c.EndDate)
                .Count();

            var campaignList = _campaignRepository
                .GetAll()
                .OrderByDescending(c => c.EndDate)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var campaignVolunteer = _campaignVolunteerMappingRepository.GetAll()
                .Where(cv => campaignList.Select(c => c.Id).Contains(cv.CampaignId));

            var permittedCampaignList = new List<CampaignVm>();

            foreach (var campaign in campaignList)
            {
                permittedCampaignList.Add(new CampaignVm()
                {
                    Id = campaign.Id,
                    Name = campaign.Name,
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    Address = campaign.Address,
                    Institute = campaign.Institute,
                    BannerUrl = campaign.BannerUrl,
                    VolunteerList = campaignVolunteer
                        .Where(c => c.CampaignId == campaign.Id)
                        .Select(v => v.VolunteerId)
                        .ToList()

                });
            }

            return new
            {
                data = permittedCampaignList,
                rowCount = totalRowCount
            };
        }

        public object GetRunningAndUpcomingCampaign(int pageNo, int pageSize)
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser is { UserType: UserTypes.Volunteer })
            {
                return VolunteerPermittedRunningAndUpcomingCampaign(currentUser.Id, pageNo, pageSize);
            }

            var totalRowCount = _campaignRepository
                .GetConditionalList(c => c.EndDate >= DateTime.Now.Date)
                .OrderByDescending(c => c.CreateTime)
                .Count();

            var campaignList = _campaignRepository
                .GetConditionalList(c => c.EndDate >= DateTime.Now.Date)
                .OrderByDescending(c => c.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var campaignVolunteer = _campaignVolunteerMappingRepository.GetAll()
                .Where(cv => campaignList.Select(c => c.Id).Contains(cv.CampaignId));

            var permittedCampaignList = new List<CampaignVm>();

            foreach (var campaign in campaignList)
            {
                permittedCampaignList.Add(new CampaignVm()
                {
                    Id = campaign.Id,
                    Name = campaign.Name,
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    Address = campaign.Address,
                    Institute = campaign.Institute,
                    BannerUrl = campaign.BannerUrl,
                    VolunteerList = campaignVolunteer
                        .Where(c => c.CampaignId == campaign.Id)
                        .Select(v => v.VolunteerId)
                        .ToList()

                });
            }

            return new
            {
                data = permittedCampaignList,
                rowCount = totalRowCount
            };
        }

        public object GetVolunteerPermittedCampaigns(int pageNo, int pageSize)
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser is { UserType: UserTypes.Volunteer })
            {
                return VolunteerPermittedCampaign(currentUser.Id, pageNo, pageSize);
            }

            return new
            {
                data = new List<CampaignVm>(),
                rowCount = 0
            };
        }

        private object VolunteerPermittedRunningAndUpcomingCampaign(string currentUserId, int pageNo, int pageSize)
        {
            var permittedCampaign = _campaignVolunteerMappingRepository
                .GetConditionalList(v => v.VolunteerId == currentUserId)
                .Select(c => c.CampaignId)
                .Distinct()
                .ToList();

            var allCampaign = _campaignRepository
                .GetConditionalList(c => c.EndDate >= DateTime.Now.Date)
                .Where(c => permittedCampaign.Contains(c.Id))
                .OrderByDescending(c => c.CreateTime);

            var totalRowCount = allCampaign.Count();

            var campaignList = allCampaign
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Select(campaign => new CampaignVm()
                {
                    Id = campaign.Id,
                    Name = campaign.Name,
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    Address = campaign.Address,
                    BannerUrl = campaign.BannerUrl
                })
                .ToList();

            return new
            {
                data = campaignList,
                rowCount = totalRowCount
            };
        }

        private object VolunteerPermittedCampaign(string currentUserId, int pageNo, int pageSize)
        {
            var permittedCampaign = _campaignVolunteerMappingRepository
                .GetConditionalList(v => v.VolunteerId == currentUserId)
                .Select(c => c.CampaignId)
                .Distinct()
                .ToList();

            var allCampaign = _campaignRepository
                .GetAll()
                .Where(c => permittedCampaign.Contains(c.Id))
                .OrderByDescending(c => c.CreateTime);

            var totalRowCount = allCampaign.Count();

            var campaignList = allCampaign
                .Skip((pageNo - 1)*pageSize)
                .Take(pageSize)
                .Select(campaign => new CampaignVm()
                {
                    Id = campaign.Id,
                    Name = campaign.Name,
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    Address = campaign.Address,
                    BannerUrl = campaign.BannerUrl,
                    Institute = campaign.Institute
                })
                .ToList();

            return new
            {
                data = campaignList,
                rowCount = totalRowCount
            };
        }

        private void UpdateVolunteerToCampaign(string id, List<string> volunteerList)
        {
            var previousVolunteer = _campaignVolunteerMappingRepository
                .GetConditionalList(c => c.CampaignId == id)
                .ToList();

            _campaignVolunteerMappingRepository.Delete(previousVolunteer);

            if (volunteerList is not null)
            {
                foreach (var volunteerId in volunteerList)
                {
                    _campaignVolunteerMappingRepository.Insert(new CampaignVolunteerMapping()
                    {
                        CampaignId = id,
                        VolunteerId = volunteerId
                    });
                }
            }

            _campaignVolunteerMappingRepository.SaveChanges();
        }

        private string UploadAndGetBannerUrl(IFormFile campaignBanner)
        {
            if(campaignBanner is null) return string.Empty;

            var fileName = GetFileName(campaignBanner.FileName);
            var path = Path.Combine(_fileService.GetRootPath(), "Campaign");
            _fileService.CreateDirectoryIfNotExists(path);
            var filePath = Path.Combine(path, fileName);
            _fileService.SaveFile(filePath, campaignBanner);
            return Path.Combine("Campaign", fileName);
        }

        private static string GetFileName(string campaignBannerFileName)
        {
            return Guid.NewGuid().ToString("N") + "-" + campaignBannerFileName;
        }

        private void AssignVolunteerToCampaign(string campaignId, List<string> campaignDataVolunteerList)
        {
            if(campaignDataVolunteerList is null) return;

            foreach (var volunteerId in campaignDataVolunteerList)
            {
                _campaignVolunteerMappingRepository.Insert(new CampaignVolunteerMapping()
                {
                    CampaignId = campaignId,
                    VolunteerId = volunteerId
                });
            }

            _campaignVolunteerMappingRepository.SaveChanges();
        }
    }
}
