namespace GoBangladesh.Application.DTOs.Route;

public class RouteCreateRequest
{
    public string TripStartPlace { get; set; }
    public string TripEndPlace { get; set; }
    public decimal PerKmFare { get; set; }
    public decimal BaseFare { get; set; }
    public decimal MinimumBalance { get; set; }
    public decimal PenaltyAmount { get; set; }
}