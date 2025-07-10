using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Application.DTOs.Bus;

public class BusMapDataDto
{
    public string Id { get; set; }
    public string BusNumber { get; set; }
    public string? BusName { get; set; }
    public string PresentLatitude { get; set; }
    public string PresentLongitude { get; set; }
}