using GoBangladesh.Application.DTOs.Notification;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GoBangladesh.Web.Controllers;

[Route("api/Notification")]
[GoBangladeshAuth]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    
    [HttpPost("insertAdminNotification")]
    public async Task<IActionResult> InsertAdminNotification([FromForm] AdminNotificationCreateRequest model)
    {
        var data = await _notificationService.InsertAdminNotificationAsync(model);
        return Ok(new { data });
    }

    [HttpGet("getCardNotifications")]
    public IActionResult GetCardNotifications(string cardNumber, int pageNo = 1, int pageSize = 10)
    {
        var data = _notificationService.GetCardNotifications(cardNumber, pageNo, pageSize);
        return Ok(new { data });
    }
    
    [HttpGet("getUserNotifications")]
    public IActionResult GetUserNotifications(string userId, int pageNo = 1, int pageSize = 10)
    {
        var data = _notificationService.GetUserNotifications(userId, pageNo, pageSize);
        return Ok(new { data });
    }
    
    [HttpPost("markAsRead")]
    public IActionResult MarkNotificationsAsRead([FromBody] MarkAsReadRequest model)
    {
        var data = _notificationService.MarkNotificationsAsRead(model);
        return Ok(new { data });
    }
    
    [HttpPost("markAllAsRead")]
    public IActionResult MarkAllNotificationsAsRead([FromBody] MarkAllAsReadRequest model)
    {
        var data = _notificationService.MarkAllNotificationsAsRead(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("getAll")]
    public IActionResult GetAll([FromBody] NotificationDataFilter filter)
    {
        var data = _notificationService.GetAll(filter);
        return Ok(new { data });
    }
}