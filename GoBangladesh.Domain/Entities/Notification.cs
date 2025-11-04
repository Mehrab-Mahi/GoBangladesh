namespace GoBangladesh.Domain.Entities;

public class Notification : Entity
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string OrganizationId { get; set; }
    public string BannerUrl { get; set; }
    public string CardStatus { get; set; }
}