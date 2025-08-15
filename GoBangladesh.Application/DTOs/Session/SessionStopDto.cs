using Newtonsoft.Json;

namespace GoBangladesh.Application.DTOs.Session;

public class SessionStopDto
{
    public string SessionId { get; set; }
    public string TripClosingType { get; set; }
    [JsonIgnore]
    public string StopStatus { get; set; }
}