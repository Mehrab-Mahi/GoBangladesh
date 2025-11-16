using GoBangladesh.Application.DTOs.Promo;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util.Promo;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.DTOs.Notification;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Application.Services;

public class PromoService : IPromoService
{
    private readonly IRepository<Promo> _promoRepository;
    private readonly IRepository<PromoCard> _promoCardRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly IBaseRepository _baseRepository;
    private readonly ICardService _cardService;
    private readonly ICommonService _commonService;
    private readonly INotificationService _notificationService;
    private readonly IRepository<Organization> _organizationRepository;

    public PromoService(IRepository<Promo> promoRepository, 
        IRepository<PromoCard> promoCardRepository,
        ILoggedInUserService loggedInUserService,
        IBaseRepository baseRepository,
        ICardService cardService, 
        ICommonService commonService,
        INotificationService notificationService,
        IRepository<Organization> organizationRepository)
    {
        _promoRepository = promoRepository;
        _promoCardRepository = promoCardRepository;
        _loggedInUserService = loggedInUserService;
        _baseRepository = baseRepository;
        _cardService = cardService;
        _commonService = commonService;
        _notificationService = notificationService;
        _organizationRepository = organizationRepository;
    }

    public PayloadResponse PromoInsert(PromoCreationRequest model)
    {
        try
        {
            if (model == null)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    Message = "Invalid promo data."
                };
            }

            if (string.IsNullOrEmpty(model.Code))
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    Message = "Promo code cannot be empty."
                };
            }

            if (model.EndTime <= model.StartTime)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    Message = "End time must be greater than start time."
                };
            }

            if (IfDuplicateCode(model.Code))
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    Message = "Promo code already exists."
                };
            }

            var promo = new Promo
            {
                Code = model.Code,
                PromoType = model.PromoType,
                DiscountValue = model.DiscountValue,
                MaxDiscountAmount = model.MaxDiscountAmount,
                Description = model.Description,
                StartTime = model.StartTime.AddHours(-6),
                EndTime = model.EndTime.AddHours(-6),
                OrganizationId = model.OrganizationId,
                PassengerStatus = string.Join(",", model.PassengerStatusList),
                CardStatus = string.Join(",", model.CardStatusList),
                MaxUsagePerCard = model.MaxUsagePerCard
            };

            if (promo.EndTime < DateTime.UtcNow)
            {
                promo.Status = PromoStatus.Expired;
            }
            else if (promo.StartTime > DateTime.UtcNow)
            {
                promo.Status = PromoStatus.AvailableSoon;
            }
            else
            {
                promo.Status = PromoStatus.Running;
            }

            _promoRepository.Insert(promo);
            _promoRepository.SaveChanges();

            AddPromoToCard(promo);

            if (promo.Status == PromoStatus.AvailableSoon)
            {
                var delayUntilStart = promo.StartTime - DateTime.UtcNow;
                BackgroundJob.Schedule(() => ActivatePromo(promo), delayUntilStart);
            }

            if (promo.EndTime > DateTime.UtcNow)
            {
                var delayUntilEnd = promo.EndTime - DateTime.UtcNow;
                BackgroundJob.Schedule(() => ExpirePromo(promo), delayUntilEnd);
            }

            var adminList = _commonService.GetAdminListByOrganizationId(promo.OrganizationId);

            SendPromoCreationNotificationToAdmins(adminList, model);

            return new PayloadResponse
            {
                IsSuccess = true,
                Message = "Promo created successfully."
            };
        }
        catch(Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                Message = $"Promo creation failed because : {ex.Message}"
            };
        }
    }

    private void SendPromoCreationNotificationToAdmins(List<User> adminList, PromoCreationRequest model)
    {
        var currentUser = _loggedInUserService.GetLoggedInUser();

        foreach (var admin in adminList)
        {
            _notificationService.InsertEventNotification(new EventNotificationCreateRequest()
            {
                UserId = admin.Id,
                Title = "New Promo Created",
                Message = $"A new promo '{model.Code}' has been created by {currentUser.Name}. Promo is set to start from {model.StartTime} to {model.EndTime}."
            });
        }
    }

    public PayloadResponse GetUserPromo(string status, int pageNo, int pageSize)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var cardIds = _cardService.GetAllCardForByUserId(currentUser.Id).Select(c => c.Id);

            var allPromoData = _promoCardRepository
                .GetAll()
                .Where(pc => cardIds.Contains(pc.CardId))
                .OrderByDescending(pc => pc.LastModifiedTime)
                .Include(pc => pc.Promo);

            List<PromoCard> finalData;

            int rowCount;

            if (!string.IsNullOrEmpty(status))
            {
                rowCount = allPromoData.Count(pc => pc.Status == status);

                finalData = allPromoData
                    .Where(pc => pc.Status == status)
                    .Skip((pageNo - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

            }
            else
            {
                rowCount = allPromoData.Count();

                finalData = allPromoData
                    .Skip((pageNo - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            return new PayloadResponse
            {
                IsSuccess = true,
                Message = "User promos retrieved successfully.",
                Content = new {finalData, rowCount}
            };
        }
        catch(Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                Message = $"Failed to retrieve user promos because : {ex.Message}"
            };
        }
    }

    public PayloadResponse ApplyPromo(ApplyPromoRequest applyPromoRequest)
    {
        try
        {
            var card = _cardService.GetCardDetailByCardNumber(applyPromoRequest.CardNumber);

            var promoCard = _promoCardRepository
                .GetAll()
                .FirstOrDefault(pc => pc.PromoId == applyPromoRequest.PromoId
                                      && pc.CardId == card.Id
                                      && pc.Status == PromoCardStatus.Available);

            if (promoCard == null)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    Message = "Valid promo cannot be found."
                };
            }

            promoCard.Status = PromoCardStatus.Applied;
            _promoCardRepository.Update(promoCard);
            _promoCardRepository.SaveChanges();

            return new PayloadResponse
            {
                IsSuccess = true,
                Message = "Promo applied successfully.",
                Content = promoCard.Promo
            };
        }
        catch(Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                Message = $"Failed to apply promo because : {ex.Message}"
            };
        }
    }

    public bool IfAnyPromoAvailableByCardId(string cardId)
    {
        var existingPromoCard = _promoCardRepository
            .GetAll()
            .Include(pc => pc.Promo)
            .FirstOrDefault(pc => pc.CardId == cardId
                                  && pc.Status == PromoCardStatus.Applied
                                  && pc.Promo.Status == PromoStatus.Running
                                  && pc.UsageCount <= pc.Promo.MaxUsagePerCard);
        
        return existingPromoCard != null;
    }

    public decimal GetPromoAmount(PromoCard promoCard, decimal fare)
    {
        var promo = promoCard.Promo;

        if(promo.PromoType == "Percentage")
        {
            var discountAmount = (fare * promo.DiscountValue) / 100;
            return discountAmount > promo.MaxDiscountAmount ? promo.MaxDiscountAmount : discountAmount;
        }

        if(promo.PromoType == "Fixed")
        {
            return promo.DiscountValue > fare ? fare : promo.DiscountValue;
        }

        return 0;
    }

    public void MarkPromoAsUsedAndUpdateUsageAmount(PromoCard promoCard, decimal promoAmount)
    {
        promoCard.UsageCount += 1;
        promoCard.Status = PromoCardStatus.Used;
        promoCard.UsageDate = DateTime.UtcNow;
        promoCard.UsageAmount += promoAmount;
        _promoCardRepository.Update(promoCard);
        _promoCardRepository.SaveChanges();
    }

    public PromoCard GetPromoCardByCardId(string cardId)
    {
        var existingPromoCard = _promoCardRepository
            .GetAll()
            .Include(pc => pc.Promo)
            .OrderBy(pc => pc.Promo.EndTime)
            .FirstOrDefault(pc => pc.CardId == cardId
                                  && (pc.Status == PromoCardStatus.Applied 
                                      || (pc.Promo.MaxUsagePerCard > pc.UsageCount 
                                          && pc.Status == PromoCardStatus.Used))
                                  && pc.Promo.Status == PromoStatus.Running);

        return existingPromoCard;
    }

    public PayloadResponse GetAll(PromoDataFilter filter)
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
            var extraCondition = $@"ORDER BY 
                                    CASE 
                                        WHEN p.Status = 'Running' THEN 1
                                        WHEN p.Status = 'AvailableSoon' THEN 2
                                        WHEN p.Status = 'Expired' THEN 3
                                        ELSE 4                          
                                    END, p.CreateTime desc
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
                condition.Add($" (p.Code like '%{filter.SearchQuery}%' or p.Description like '%{filter.SearchQuery}%' or p.Status like '%{filter.SearchQuery}%') ");
            }

            if (filter.StartDate != null || filter.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(filter.StartDate, filter.EndDate);

                condition.Add($@" (p.StartTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}'
                                or p.EndTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" p.OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Promos p", whereCondition);

            var promoIds = _commonService.GetFinalData<Promo>("Promos p", whereCondition, extraCondition).Select(p => p.Id);

            var finalQueryData = _promoRepository
                .GetAll()
                .Where(p => promoIds.Contains(p.Id))
                .Include(p => p.Organization)
                .ToList();

            foreach (var data in finalQueryData)
            {
                data.PassengerStatusList = data.PassengerStatus?.Split(",").ToList() ?? new List<string>();
                data.CardStatusList = data.CardStatus?.Split(",").ToList() ?? new List<string>();
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { CardData = GetAllPromoDashboardData(whereCondition),
                    data = finalQueryData,
                    rowCount },
                Message = "Promo data fetch is successful"
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

    private PromoCardData GetAllPromoDashboardData(string whereCondition)
    {
        var query = $@"SELECT COUNT(DISTINCT p.OrganizationId) AS OrganizationCount,
                           COUNT(DISTINCT p.Id)             AS PromoCount,
                           COUNT(DISTINCT pc.CardId)        AS CardCount,
                           SUM(pc.UsageCount)               AS TotalUsageCount,
                           SUM(pc.UsageAmount)              AS TotalUsageAmount
                    FROM dbo.Promos p
                             LEFT JOIN dbo.PromoCards pc ON p.Id = pc.PromoId
                    {whereCondition};";

        var result = _baseRepository.Query<PromoCardData>(query).FirstOrDefault();

        return result;
    }

    public void UpdatePromoUsageAmount(string promoCardId, decimal promoAmount)
    {
        var promoCard = _promoCardRepository.GetConditional(pc => pc.Id == promoCardId);

        if (promoCard != null)
        {
            promoCard.UsageAmount += promoAmount;
            _promoCardRepository.Update(promoCard);
            _promoCardRepository.SaveChanges();
        }
    }

    public PayloadResponse GetById(string id)
    {
        try
        {
            var promo = _promoRepository.GetConditional(p => p.Id == id);

            if (promo == null)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    Message = "Promo not found."
                };
            }

            var query = $@"SELECT
                        p.Id,
                        p.Code,
                        p.PromoType,
                        p.DiscountValue,
                        p.MaxDiscountAmount,
                        p.Description,
                        p.Status,
                        p.StartTime,
                        p.EndTime,
                        p.OrganizationId,
                        p.PassengerStatus,
                        p.CardStatus,
                        p.MaxUsagePerCard,
                        p.CreateTime,
                        p.LastModifiedTime,
                        p.CreatedBy,
                        p.LastModifiedBy,
                        p.IsDeleted,

                        -- Aggregated data from PromoCard
                        ISNULL(SUM(pc.UsageAmount), 0) AS PromoUsageAmount,
                        COUNT(pc.Id) AS EligibleCards,
                        SUM(CASE WHEN pc.Status = 'Used' THEN 1 ELSE 0 END) AS NumberOfPromoUsed,
                        SUM(CASE WHEN pc.Status = 'Applied' THEN 1 ELSE 0 END) AS AppliedCount,
                        SUM(CASE WHEN pc.Status = 'Active' THEN 1 ELSE 0 END) AS ActiveCount,
                        SUM(CASE WHEN pc.Status = 'Expired' THEN 1 ELSE 0 END) AS ExpiredCount

                    FROM Promos p
                    LEFT JOIN PromoCards pc ON p.Id = pc.PromoId
                    WHERE p.Id = '{id}'
                    GROUP BY
                        p.Id, p.Code, p.PromoType, p.DiscountValue, p.MaxDiscountAmount,
                        p.Description, p.Status, p.StartTime, p.EndTime, p.OrganizationId,
                        p.PassengerStatus, p.CardStatus, p.MaxUsagePerCard,
                        p.CreateTime, p.LastModifiedTime, p.CreatedBy, p.LastModifiedBy, p.IsDeleted;";

            var promoDetails = _baseRepository.Query<PromoDataDto>(query).FirstOrDefault();

            if (promoDetails is null)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    Message = "Promo details not found."
                };
            }

            var organization = _organizationRepository
                .GetConditional(o => o.Id == promoDetails.OrganizationId);

            if (organization != null)
            {
                promoDetails.Organization = organization;
            }

            promoDetails.PassengerStatusList = promoDetails.PassengerStatus?.Split(",").ToList() ?? new List<string>();
            promoDetails.CardStatusList = promoDetails.CardStatus?.Split(",").ToList() ?? new List<string>();

            return new PayloadResponse
            {
                IsSuccess = true,
                Message = "Promo details retrieved successfully.",
                Content = promoDetails
            };
        }
        catch(Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                Message = $"Failed to retrieve promo details because : {ex.Message}"
            };
        }
    }

    public PayloadResponse GetUsedCardByPromoId(string id, int pageNo, int pageSize)
    {
        var promo = _promoRepository.GetConditional(p => p.Id == id);

        if (promo == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Promo not found"
            };
        }

        var rowCount = GetUsedPromoCardRowCount(id);

        var data = GetUsedPromoCardData(id, pageNo, pageSize);

        var cardData = GetPromoCardHistoryCardData(id);

        return new PayloadResponse()
        {
            IsSuccess = true,
            Message = "Used card details fetched successfully",
            Content = new { cardData, data, rowCount }
        };
    }

    private PromoHistoryCardDto GetPromoCardHistoryCardData(string id)
    {
        var query = $@"select count(distinct CardId) TotalCardCount, sum(UsageCount) TotalUsageCount, sum(UsageAmount) TotalUsageAmount
                    from PromoCards
                    where PromoId = '{id}'
                      and Status = 'Used';";

        var result = _baseRepository.Query<PromoHistoryCardDto>(query).FirstOrDefault();

        return result;
    }

    private List<UsedPromoCard> GetUsedPromoCardData(string id, int pageNo, int pageSize)
    {
        var query = $@"select u.Name, c.CardNumber, tr.TransactionId, pc.LastModifiedTime as UsedTime, b.BusNumber, pc.UsageAmount
                    from PromoCards pc
                             left join Cards c on pc.CardId = c.Id
                             left join PassengerCardMappings pcm on c.Id = pcm.CardId
                             left join Users u on pcm.UserId = u.Id
                             left join Promos p on pc.PromoId = p.Id
                             left join Trips t on p.Id = t.PromoId
                             left join Transactions tr on t.Id = tr.TripId
                             left join Sessions s on t.SessionId = s.Id
                             left join Buses b on s.BusId = b.Id
                    where pc.PromoId = '{id}'
                      and pc.Status = 'Used'
                    ORDER BY pc.LastModifiedTime desc
                    OFFSET ({pageNo} - 1) * {pageSize} ROWS
                    FETCH NEXT {pageSize} ROWS ONLY";

        var result = _baseRepository.Query<UsedPromoCard>(query).ToList();

        return result;
    }

    private int GetUsedPromoCardRowCount(string id)
    {
        var query = $@"select count(pc.Id)
                    from PromoCards pc
                    where pc.PromoId = '{id}'
                      and pc.Status = 'Used';";

        var result = _baseRepository.Query<int>(query).FirstOrDefault();

        return result;
    }

    public void ActivatePromo(Promo promo)
    {
        promo.Status = PromoStatus.Running;
        _promoRepository.Update(promo);
        _promoRepository.SaveChanges();

        var query = @$"UPDATE PromoCards
                       SET Status = '{PromoCardStatus.Available}',
                           LastModifiedTime = GETUTCDATE()
                       WHERE PromoId = '{promo.Id}' 
                         AND Status = '{PromoCardStatus.AvailableSoon}';";

        _baseRepository.ExecuteQuery(query);
    }

    public void ExpirePromo(Promo promo)
    {
        promo.Status = PromoStatus.Expired;
        _promoRepository.Update(promo);
        _promoRepository.SaveChanges();

        var query = @$"UPDATE PromoCards
                       SET Status = '{PromoCardStatus.Expired}',
                           LastModifiedTime = GETUTCDATE()
                       WHERE PromoId = '{promo.Id}' 
                         AND Status IN ('{PromoCardStatus.Available}', '{PromoCardStatus.AvailableSoon}', '{PromoCardStatus.Applied}');";

        _baseRepository.ExecuteQuery(query);
    }

    public void AddPromoToCard(Promo promo)
    {
        var currentUser = _loggedInUserService.GetLoggedInUser();

        var organizationId = string.IsNullOrEmpty(promo.OrganizationId) ?
            currentUser.Id : promo.OrganizationId;

        var filters = new List<string>();

        if (!string.IsNullOrEmpty(organizationId))
        {
            filters.Add($"OrganizationId = '{organizationId}'");
        }

        if (!string.IsNullOrEmpty(promo.PassengerStatus))
        {
            filters.Add($"PassengerStatus in '('{string.Join("','", promo.PassengerStatus.Split(","))}')'");
        }

        if (!string.IsNullOrEmpty(promo.CardStatus))
        {
            filters.Add($"Status in '('{string.Join("','", promo.CardStatus.Split(","))}')'");
        }

        var whereCondition = filters.Any() ? $"WHERE {string.Join(" AND ", filters)}" : "";

        var status = promo.Status == PromoStatus.AvailableSoon ? PromoCardStatus.AvailableSoon : 
                promo.Status == PromoStatus.Running ? PromoCardStatus.Available :
                PromoCardStatus.Expired;

        var query = @$"WITH card_data AS (
                            SELECT *
                            FROM Cards
                            {whereCondition}
                        )
                        INSERT INTO PromoCards (
                            Id,
                            PromoId,
                            CardId,
                            Status,
                            UsageAmount,
                            UsageCount,
                            CreateTime,
                            LastModifiedTime,
                            CreatedBy,
                            LastModifiedBy,
                            IsDeleted
                        )
                        SELECT 
                            REPLACE(CONVERT(NVARCHAR(50), NEWID()), '-', '') AS Id,
                            '{promo.Id}' AS PromoId,
                            c.Id AS CardId,
                            '{status}' AS Status,
                            0          AS UsageAmount,
                            0 AS UsageCount,
                            GETUTCDATE() AS CreateTime,
                            GETUTCDATE() AS LastModifiedTime,
                            '{currentUser.Id}' AS CreatedBy,
                            '{currentUser.Id}' AS LastModifiedBy,
                            0 AS IsDeleted
                        FROM card_data c;";

        _baseRepository.ExecuteQuery(query);

        SendNotificationToCardOwners(whereCondition, promo);
    }

    public void SendNotificationToCardOwners(string whereCondition, Promo promo)
    {
        var query =
            $"select UserId from PassengerCardMappings where CardId in (select Id from Cards {whereCondition});";

        var userIds = _baseRepository.Query<string>(query).Distinct().ToList();

        BackgroundJob.Enqueue(() => SendPromoNotifications(userIds, promo));
    }

    public void SendPromoNotifications(List<string> userIds, Promo promo)
    {
        var discountValue = promo.PromoType == "Percentage"
            ? $"{promo.MaxDiscountAmount}"
            : $"{promo.DiscountValue}";

        foreach (var userId in userIds)
        {
            _notificationService.InsertEventNotification(new EventNotificationCreateRequest()
            {
                UserId = userId,
                Title = "New Promo Available!",
                Message = $"A new promo '{promo.Code}' is now available for you. Enjoy up to {discountValue} bdt discount on your next rides. Hurry up, don't miss out!"
            });
        }
    }

    private bool IfDuplicateCode(string code)
    {
        var existingPromo = _promoRepository.GetConditional(p => p.Code == code);

        return existingPromo != null;
    }
}