using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class Card : Entity
{
    public string CardNumber { get; set; }
    public string Status { get; set; }
    public decimal Balance { get; set; }
    public string OrganizationId { get; set; }
    [ForeignKey("OrganizationId")]
    public Organization Organization { get; set; }
}