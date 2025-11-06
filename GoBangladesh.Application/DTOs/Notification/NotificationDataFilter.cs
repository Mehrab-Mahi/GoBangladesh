namespace GoBangladesh.Application.DTOs.Notification;

public class NotificationDataFilter
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string OrganizationId { get; set; }
    public string SearchQuery { get; set; }
}