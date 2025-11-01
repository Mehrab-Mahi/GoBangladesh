using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace GoBangladesh.Domain.Entities;

public class Route : Entity
{
    public string TripStartPlace { get; set; }
    public string TripStartLatitude { get; set; }
    public string TripStartLongitude { get; set; }
    public string TripEndPlace { get; set; }
    public string TripEndLatitude { get; set; }
    public string TripEndLongitude { get; set; }
    public string OrganizationId { get; set; }
    public decimal PerKmFare { get; set; }
    public decimal BaseFare { get; set; }
    public decimal MinimumBalance { get; set; }
    public decimal PenaltyAmount { get; set; }
    [ForeignKey("OrganizationId")]
    public Organization Organization { get; set; }
    public bool IsActive { get; set; } = true;
    public LineString RoutePath { get; set; }
}