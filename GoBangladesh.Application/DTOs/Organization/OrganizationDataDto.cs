using System;

namespace GoBangladesh.Application.DTOs.Organization;

public class OrganizationDataDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string FocalPerson { get; set; }
    public string Designation { get; set; }
    public string Email { get; set; }
    public string MobileNumber { get; set; }
    public decimal PerKmFare { get; set; }
    public decimal BaseFare { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime LastModifiedTime { get; set; }
    public int TotalBus { get; set; } = 0;
    public int TotalDriver { get; set; } = 0;
    public int TotalAgent { get; set; } = 0;
    public int TotalPassenger { get; set; } = 0;
}