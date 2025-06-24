namespace GoBangladesh.Domain.Entities
{
    public class Contact : Entity
    {
        public string ContactType { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
