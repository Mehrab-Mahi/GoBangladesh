namespace GoBangladesh.Application.DTOs.Notification;

public class EventNotificationCreateRequest
{
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
}