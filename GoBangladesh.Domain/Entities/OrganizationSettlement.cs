namespace GoBangladesh.Domain.Entities;

public class OrganizationSettlement : Entity
{
    public string FromOrganizationId { get; set; }
    public string ToOrganizationId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionId { get; set; }
    public string TransactionType { get; set; }
}