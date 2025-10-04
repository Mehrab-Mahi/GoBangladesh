using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Application.DTOs.Bus;

public class BusMapDataDto
{
    public string Id { get; set; }
    public string BusNumber { get; set; }
    public string? BusName { get; set; }
    public string? OrganizationName { get; set; }
    public string PresentLatitude { get; set; }
    public string PresentLongitude { get; set; }
    public int RunningTrips { get; set; }
    public string Route { get; set; }
}