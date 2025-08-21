using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GoBangladesh.Domain.Entities;

public class Transaction : Entity
{
    public string TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string CardId { get; set; }
    public string? AgentId { get; set; }
    public string? TripId { get; set; }
    public string? Medium { get; set; }
    public string TransactionId { get; set; } = new(Enumerable.Range(0, 10)
        .Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[new Random().Next(36)])
        .ToArray());

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