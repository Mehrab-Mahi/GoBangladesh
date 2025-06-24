using System;

namespace GoBangladesh.Application.ViewModels
{
    public class ContactVm
    {
        public string Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreatedBy { get; set; }
        public string ContactType { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public UserCreationVm UserData { get; set; }
    }
}
