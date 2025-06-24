namespace GoBangladesh.Domain.Entities
{
    public class Location : Entity
    {
        public string Name { get; set; } 
        public string Type { get; set; }
        public string Code { get; set; }
        public string ParentId { get; set; }
        public int Level { get; set; }
    }
}
