using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Application.DTOs.Bus;

public class BusDataDto
{
    public string Id { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime LastModifiedTime { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string BusNumber { get; set; }
    public string? BusName { get; set; }
    public string? RouteId { get; set; }
    public Domain.Entities.Route Route { get; set; }
    public string OrganizationId { get; set; }
    public Domain.Entities.Organization Organization { get; set; }
    public string PresentLatitude { get; set; }
    public string PresentLongitude { get; set; }
    public int TotalSession { get; set; }
    public int TotalPassenger { get; set; }
    public decimal TotalRevenue { get; set; }
}