using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class Session : Entity
{
    public string BusId { get; set; }
    public string UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsRunning { get; set; } = true;
    public int Serial { get; set; }
    public string SessionCode { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }
    [ForeignKey("BusId")]
    public Bus Bus { get; set; }
}