using GoBangladesh.Application.Util;

namespace GoBangladesh.Application.DTOs.Card;

public class CardCreateRequest
{
    public string CardNumber { get; set; }
    public string OrganizationId { get; set; }
    public string Status { get; set; }
}