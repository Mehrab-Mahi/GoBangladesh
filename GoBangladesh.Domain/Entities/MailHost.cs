namespace GoBangladesh.Domain.Entities
{
    public class MailHost : Entity
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string CcAddress { get; set; }
        public string BccAddress { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsSendgrid { get; set; } = false;
        public string SendGridApiKey { get; set; }
    }
}