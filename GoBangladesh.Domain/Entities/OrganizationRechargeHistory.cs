namespace GoBangladesh.Domain.Entities;

public class OrganizationCardBalance : Entity
{
    public string CardId { get; set; }
    public string OrganizationId { get; set; }
    public decimal Balance { get; set; }= 0;
}