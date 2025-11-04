using System;

namespace GoBangladesh.Domain.Entities;

public class UserNotification : Entity
{
    public string? NotificationId { get; set; }
    public string? UserId { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string BannerUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}