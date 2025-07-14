using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class Transaction : Entity
{
    public string TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string CardId { get; set; }
    public string? AgentId { get; set; }
    public string? TripId { get; set; }
    [ForeignKey("CardId")]
    public Card Card { get; set; }
    [ForeignKey("AgentId")]
    public User? Agent { get; set; }
    [ForeignKey("TripId")]
    public Trip? Trip { get; set; }
    [NotMapped]
    public User Passenger { get; set; }
    [NotMapped]
    public string PassengerId { get; set; }
}