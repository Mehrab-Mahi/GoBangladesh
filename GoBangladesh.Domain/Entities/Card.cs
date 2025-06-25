namespace GoBangladesh.Domain.Entities;

public class Card : Entity
{
    public string CardNumber { get; set; }
    public int Balance { get; set; } = 0;
}