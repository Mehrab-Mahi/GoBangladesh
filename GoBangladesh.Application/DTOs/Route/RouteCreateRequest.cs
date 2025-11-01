using System.Collections.Generic;

namespace GoBangladesh.Application.DTOs.Route;

public class RouteCreateRequest
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
    public List<StoppageDto> StoppageList { get; set; }
}