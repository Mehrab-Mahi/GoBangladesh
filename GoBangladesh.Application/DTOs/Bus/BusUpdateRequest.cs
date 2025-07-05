namespace GoBangladesh.Application.DTOs.Bus;

public class BusUpdateRequest
{
    public string Id { get; set; }
    public string BusNumber { get; set; }
    public string? BusName { get; set; }
    public string? TripStartPlace { get; set; }
    public string? TripEndPlace { get; set; }
    public string OrganizationId { get; set; }
}