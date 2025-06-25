using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class Bus : Entity
{
    public string BusNumber { get; set; }
    public string? BusName { get; set; }
    public string? TripStartPlace { get; set; }
    public string? TripEndPlace { get; set; }
    public string OrganizationId { get; set; }
    [ForeignKey("OrganizationId")]
    public Organization Organization { get; set; }
}