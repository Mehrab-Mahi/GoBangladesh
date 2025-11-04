using Microsoft.AspNetCore.Http;

namespace GoBangladesh.Application.DTOs.Notification;

public class AdminNotificationCreateRequest
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string OrganizationId { get; set; }
    public IFormFile Banner { get; set; }
    public string CardStatus { get; set; }
}