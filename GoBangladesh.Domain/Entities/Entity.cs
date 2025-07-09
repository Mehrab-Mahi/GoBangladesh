using System;

namespace GoBangladesh.Domain.Entities
{
    public class Entity
    {
        public string Id { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        public Entity()
        {
            Id = Guid.NewGuid().ToString("N");
            CreateTime = DateTime.UtcNow;
        }
    }
}