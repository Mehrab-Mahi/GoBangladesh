namespace GoBangladesh.Domain.Entities;

public class Stoppage : Entity
{
    public string RouteId { get; set; }
    public string Name { get; set; }
    public string SortOrder { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
}