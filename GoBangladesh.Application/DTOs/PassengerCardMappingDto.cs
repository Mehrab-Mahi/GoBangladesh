using GoBangladesh.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace GoBangladesh.Application.DTOs;

public class PassengerCardMappingDto
{
    public string Id { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime LastModifiedTime { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
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
    public string OrganizationId { get; set; }
    [ForeignKey("OrganizationId")]
    public Domain.Entities.Organization Organization { get; set; }
    public int Serial { get; set; } = 0;
    public string Code { get; set; }
    public string CardId { get; set; }
    public string CardNumber { get; set; }
    public string Designation { get; set; }
    public decimal Balance { get; set; } = 0;
}