namespace GoBangladesh.Domain.Entities;

public class News : Entity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string? ThumbnailUrl { get; set; }
}