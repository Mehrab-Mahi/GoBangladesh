namespace GoBangladesh.Domain.Entities;

public class CardDue : Entity
{
    public string CardId { get; set; }
    public string OrganizationId { get; set; }
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
}