namespace GoBangladesh.Domain.Entities;

public class Contact : Entity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; } = false;
}