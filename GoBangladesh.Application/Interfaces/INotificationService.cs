using System.Threading.Tasks;
using GoBangladesh.Application.DTOs.Notification;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface INotificationService
{
    Task<PayloadResponse> InsertAdminNotificationAsync(AdminNotificationCreateRequest model);
    void InsertEventNotification(EventNotificationCreateRequest model);
    PayloadResponse GetCardNotifications(string cardNumber, int pageNo, int pageSize);
    PayloadResponse MarkNotificationsAsRead(MarkAsReadRequest model);
}