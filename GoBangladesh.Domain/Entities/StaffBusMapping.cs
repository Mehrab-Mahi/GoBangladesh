using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class StaffBusMapping : Entity
{
    public string BusId { get; set; }
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }
    [ForeignKey("BusId")]
    public Bus Bus { get; set; }
}