using GoBangladesh.Application.Util;
using Newtonsoft.Json;

namespace GoBangladesh.Application.DTOs.Card;

public class CardCreateRequest
{
    public string CardNumber { get; set; }
    public string OrganizationId { get; set; }
    [JsonIgnore]
    public string Status { get; set; }
}