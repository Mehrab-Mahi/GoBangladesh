using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class Route : Entity
{
    public string TripStartPlace { get; set; }
    public string TripEndPlace { get; set; }
    public string OrganizationId { get; set; }
    public decimal PerKmFare { get; set; }
    public decimal BaseFare { get; set; }
    public decimal MinimumBalance { get; set; }
    public decimal PenaltyAmount { get; set; }
    [ForeignKey("OrganizationId")]
    public Organization Organization { get; set; }
}