using Microsoft.AspNetCore.Http;
using System;

namespace GoBangladesh.Application.DTOs.Passenger;

public class PassengerCreateRequest
{
    public string Password { get; set; }
    public string Name { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string MobileNumber { get; set; }
    public string EmailAddress { get; set; }
    public string Address { get; set; }
    public string Gender { get; set; }
    public string UserType { get; set; } = "Passenger";
    public IFormFile ProfilePicture { get; set; }
    public string PassengerId { get; set; }
    public string OrganizationId { get; set; }
}