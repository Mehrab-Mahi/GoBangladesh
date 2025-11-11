using System;
using GoBangladesh.Application.DTOs.Notification;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly IBaseRepository _baseRepository;
    private readonly IRepository<UserNotification> _userNotificationRepository;
    private readonly ICardService _cardService;

    public NotificationService(IRepository<Notification> notificationRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService, 
        IBaseRepository baseRepository,
        IRepository<UserNotification> userNotificationRepository,
        ICardService cardService)
    {
        _notificationRepository = notificationRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _baseRepository = baseRepository;
        _userNotificationRepository = userNotificationRepository;
        _cardService = cardService;
    }

    public async Task<PayloadResponse> InsertAdminNotificationAsync(AdminNotificationCreateRequest model)
    {
        var currentUser = _loggedInUserService.GetLoggedInUser();

        if(currentUser == null)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                Message = "Unauthorized access."
            };
        }

        var notification = new Notification
        {
            Title = model.Title,
            Message = model.Message,
            OrganizationId = string.IsNullOrEmpty(model.OrganizationId) ?
                currentUser.OrganizationId : model.OrganizationId,
            BannerUrl = model.Banner == null ? null : GetBannerUrl(model.Banner)
        };

        _notificationRepository.Insert(notification);
        _notificationRepository.SaveChanges();

        await SaveAndSendNotificationToCardsAsync(notification, currentUser);

        return new PayloadResponse
        {
            IsSuccess = true,
            Message = "Notification created and sent successfully.",
        };
    }

    public void InsertEventNotification(EventNotificationCreateRequest model)
    {
        var userNotification = new UserNotification()
        {
            UserId = model.UserId,
            Title = model.Title,
            Message = model.Message
        };

        _userNotificationRepository.Insert(userNotification);
        _userNotificationRepository.SaveChanges();
    }

    public PayloadResponse GetCardNotifications(string cardNumber, int pageNo, int pageSize)
    {
        var cardOwner = _cardService.GetUserByCardNumber(cardNumber); 

        if(cardOwner == null)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                Message = "Card owner not found."
            };
        }

        var cardNotificationList = _userNotificationRepository.GetAll()
            .Where(cn => cn.UserId == cardOwner.Id)
            .OrderByDescending(cn => cn.CreateTime)
            .Skip((pageNo - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PayloadResponse
        {
            IsSuccess = true,
            Message = "Card notifications retrieved successfully.",
            Content = cardNotificationList
        };
    }

    public PayloadResponse MarkNotificationsAsRead(MarkAsReadRequest model)
    {
        var userNotification = _userNotificationRepository.GetConditional(cn => cn.Id == model.UserNotificationId);

        if (userNotification == null)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                Message = "Card notification not found."
            };
        }

        userNotification.IsRead = true;
        userNotification.ReadAt = DateTime.UtcNow;

        _userNotificationRepository.Update(userNotification);
        _userNotificationRepository.SaveChanges();

        return new PayloadResponse
        {
            IsSuccess = true,
            Message = "Card notification marked as read successfully."
        };
    }

    public PayloadResponse GetAll(NotificationDataFilter filter)
    {
        try
        {
            var currentUser = _loggedInUserService
                .GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "User not found"
                };
            }

            var condition = new List<string>();
            var extraCondition = $@"ORDER BY CreateTime desc
                                    OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                    FETCH NEXT {filter.PageSize} ROWS ONLY";

            if (!currentUser.IsSuperAdmin)
            {
                if (string.IsNullOrEmpty(currentUser.OrganizationId))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        Message = "Current User is not associated with any organization!"
                    };
                }

                filter.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" (Title like '%{filter.SearchQuery}%' or Message like '%{filter.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Notifications", whereCondition);

            var notificationsIds = _commonService.GetFinalData<Notification>("Notifications", whereCondition, extraCondition).Select(p => p.Id);

            var finalQueryData = _notificationRepository
                .GetAll()
                .Where(p => notificationsIds.Contains(p.Id))
                .Include(p => p.Organization)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalQueryData, rowCount },
                Message = "Notification data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = $"Promo fetching is failed because {ex.Message}!"
            };
        }
    }

    private Task SaveAndSendNotificationToCardsAsync(Notification notification, User currentUser)
    {
        notification.OrganizationId = string.IsNullOrEmpty(notification.OrganizationId) ?
            currentUser.Id : notification.OrganizationId;

        var filters = new List<string>();

        if (!string.IsNullOrEmpty(notification.OrganizationId))
        {
            filters.Add($"u.OrganizationId = '{notification.OrganizationId}'");
        }

        var whereCondition =filters.Any() ? $"WHERE {string.Join(" AND ", filters)}" : "";

        var query = $@"WITH user_data AS (select u.id from Users u
                    left join PassengerCardMappings pcm on u.Id = pcm.UserId
                    left join Cards c on pcm.CardId = c.Id
                    {whereCondition})
                    INSERT
                    INTO UserNotifications (Id,
                                            NotificationId,
                                            UserId,
                                            Title,
                                            Message,
                                            BannerUrl,
                                            IsRead,
                                            ReadAt,
                                            CreateTime,
                                            LastModifiedTime,
                                            CreatedBy,
                                            LastModifiedBy,
                                            IsDeleted)
                    SELECT REPLACE(CONVERT(NVARCHAR(50), NEWID()), '-', '') AS Id,
                           '{notification.Id}'                              AS NotificationId,
                            u.Id AS CardId,
                           '{notification.Title}'                           AS Title,
                           '{notification.Message}'                         AS Message,
                           '{notification.BannerUrl}'                      AS BannerUrl,
                           0                                                AS IsRead,
                           null                                             AS ReadAt,
                           GETUTCDATE()                                     AS CreateTime,
                           GETUTCDATE()                                     AS LastModifiedTime,
                           '{currentUser.Id}'                               AS CreatedBy,
                           '{currentUser.Id}'                               AS LastModifiedBy,
                           0                                                AS IsDeleted
                    FROM user_data u;";

        _baseRepository.ExecuteQuery(query);
        return Task.CompletedTask;
    }

    private string GetBannerUrl(IFormFile banner)
    {
        var path = _commonService
            .UploadAndGetImageUrl(banner, "NotificationBanner");
        return path;
    }
}