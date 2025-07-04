using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class Trip : Entity
{
    public string PassengerId { get; set; }
    public string SessionId { get; set; }
    public string TripStartPoint { get; set; }
    public string TripEndPoint { get; set; }
    public DateTime TripStartTime { get; set; }
    public DateTime? TripEndTime { get; set; }
    public decimal Amount { get; set; }
    public bool IsRunning { get; set; } = true;
    public decimal Distance { get; set; }
    [ForeignKey("SessionId")]
    public Session Session { get; set; }
}