using System;

namespace GoBangladesh.Application.ViewModels
{
    public class ProjectUserVm
    {
        public string FullName { get; set; }
        public string BloodGroup { get; set; }
        public string DateOfBirth { get; set; }
        public string MobileNumber { get; set; }
        public string District { get; set; }
        public string Upazila { get; set; }
        public string Union { get; set; }
        public string Address { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public string GoBangladeshStatus { get; set; }
        public string Gender { get; set; }
        public string UserType { get; set; }
        public DateTime? LastDonationTime { get; set; }
        public bool IsApproved { get; set; }
    }
}
