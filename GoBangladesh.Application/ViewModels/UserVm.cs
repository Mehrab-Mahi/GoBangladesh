using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace GoBangladesh.Application.ViewModels
{
    public class UserVm
    {
        public string Id { get; set; }
        public bool IsSuperAdmin { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
    }

    public class UserCreationVm
    {
        public string Id { get; set; }
        [JsonIgnore]
        public bool IsSuperAdmin { get; set; }
        public string Password { get; set; }
        [JsonIgnore]
        public bool IsApproved { get; set; }
        public string FullName { get; set; }
        public string BloodGroup { get; set; }
        public string DateOfBirth { get; set; }
        public string MobileNumber { get; set; }
        public string District { get; set; }
        public string DistrictName { get; set; }
        public string Upazila { get; set; }
        public string UpazilaName { get; set; }
        public string Union { get; set; }
        public string UnionName { get; set; }
        public string Address { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public string BloodDonationStatus { get; set; }
        public string Gender { get; set; }
        public string UserType { get; set; }
        public DateTime? LastDonationTime { get; set; }
        public string ImageUrl { get; set; }
        public IFormFile ProfilePicture { get; set; }
        public int BloodDonationCount { get; set; }
        public string? PhysicalComplexity { get; set; }
        public List<IFormFile> Nid { get; set; }
        public List<string> NidUrls { get; set; }
        public int Serial { get; set; }
        public string Code { get; set; }
        public string? InstituteName { get; set; }
        public string? LeaderType { get; set; }
        public string? CampaignId { get; set; }
        public string? CampaignName { get; set; }
        public string? Designation { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class UserAuthVm : UserCreationVm
    {
        public bool IsAuthenticate { get; set; }
        public string Name { get; set; }
        public string RoleId { get; set; }
        public string EmailAddress { get; set; }
    }
}
