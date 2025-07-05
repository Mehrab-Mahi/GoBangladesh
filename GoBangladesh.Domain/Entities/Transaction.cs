using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class Transaction : Entity
{
    public string TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string PassengerId { get; set; }
    public string? AgentId { get; set; }
    public string? TripId { get; set; }
    [ForeignKey("PassengerId")]
    public User Passenger { get; set; }
    [ForeignKey("AgentId")]
    public User? Agent { get; set; }
    [ForeignKey("TripId")]
    public Trip? Trip { get; set; }
}