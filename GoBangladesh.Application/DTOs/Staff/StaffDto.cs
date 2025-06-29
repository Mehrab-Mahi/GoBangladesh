using GoBangladesh.Domain.Entities;
using System;

namespace GoBangladesh.Application.DTOs.Staff;

public class StaffDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string MobileNumber { get; set; }
    public string EmailAddress { get; set; }
    public string Address { get; set; }
    public string Gender { get; set; }
    public string UserType { get; set; }
    public string ImageUrl { get; set; }
    public string OrganizationId { get; set; }
    public Domain.Entities.Organization Organization { get; set; }
}