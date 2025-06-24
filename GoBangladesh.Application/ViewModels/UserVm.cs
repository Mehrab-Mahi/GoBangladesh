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
        public string Password { get; set; }
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string MobileNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string UserType { get; set; }
        public string ImageUrl { get; set; }
        public IFormFile ProfilePicture { get; set; }
        public string PassengerId { get; set; }
    }

    public class UserAuthVm : UserCreationVm
    {
        public bool IsAuthenticate { get; set; }
        public string Name { get; set; }
        public string RoleId { get; set; }
        public string EmailAddress { get; set; }
        public bool IsSuperAdmin { get; set; }
    }
}
