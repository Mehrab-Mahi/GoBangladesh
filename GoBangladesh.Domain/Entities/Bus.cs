using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class Bus : Entity
{
    public string BusNumber { get; set; }
    public string? BusName { get; set; }
    public string? RouteId { get; set; }
    [ForeignKey("RouteId")]
    public Route Route { get; set; }
    public string OrganizationId { get; set; }
    [ForeignKey("OrganizationId")]
    public Organization Organization { get; set; }
    public string PresentLatitude { get; set; }
    public string PresentLongitude { get; set; }
}