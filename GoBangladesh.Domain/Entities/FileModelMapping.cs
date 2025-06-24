namespace GoBangladesh.Domain.Entities
{
    public class FileModelMapping : Entity
    {
        public string ModelName { get; set; }
        public string FileUrl { get; set; }
        public string ModelId { get; set; }
        public string Type { get; set; }
    }
}
