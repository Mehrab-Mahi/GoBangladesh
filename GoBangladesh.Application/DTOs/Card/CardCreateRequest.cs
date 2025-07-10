using GoBangladesh.Application.Util;

namespace GoBangladesh.Application.DTOs.Card;

public class CardCreateRequest
{
    public string CardId { get; set; }
    public string CardNumber { get; set; }
    public string Status { get; set; } = CardStatus.NotUsed;
}