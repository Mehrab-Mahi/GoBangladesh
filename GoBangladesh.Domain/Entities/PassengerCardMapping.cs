using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class PassengerCardMapping : Entity
{
    public string CardId { get; set; }
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }
    [ForeignKey("CardId")]
    public Card Card { get; set; }
}