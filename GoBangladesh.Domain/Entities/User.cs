using System;

namespace GoBangladesh.Domain.Entities
{
    public class User : Entity
    {
        public bool IsSuperAdmin { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string PasswordHash { get; set; }
        public string ImageUrl { get; set; }
        public bool IsApproved { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string RoleId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string MobileNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string UserType { get; set; }
        public string PassengerId { get; set; }
    }
}