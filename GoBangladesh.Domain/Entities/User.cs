using System;

namespace GoBangladesh.Domain.Entities
{
    public class User : Entity
    {
        public bool IsSuperAdmin { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string PasswordHash { get; set; }
        public string ImageUrl { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; } = true;
        public string RoleId { get; set; }
        public string FullName { get; set; }
        public string BloodGroup { get; set; }
        public string DateOfBirth { get; set; }
        public DateTime Dob { get; set; }
        public string MobileNumber { get; set; }
        public string District { get; set; }
        public string Upazila { get; set; }
        public string Union { get; set; }
        public string Address { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public string BloodDonationStatus { get; set; }
        public string Gender { get; set; }
        public string UserType { get; set; }
        public DateTime? LastDonationTime { get; set; }
        public int BloodDonationCount { get; set; }
        public string? PhysicalComplexity { get; set; }
        public string NidUrls { get; set; }
        public int Serial { get; set; }
        public string Code { get; set; }
        public string? InstituteName { get; set; }
        public string? LeaderType { get; set; }
        public string? CampaignId { get; set; }
        public string? Designation { get; set; }
    }
}