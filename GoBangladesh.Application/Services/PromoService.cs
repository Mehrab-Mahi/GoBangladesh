using GoBangladesh.Application.DTOs.Promo;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util.Promo;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GoBangladesh.Application.Services;

public class PromoService : IPromoService
{
    private readonly IRepository<Promo> _promoRepository;
    private readonly IRepository<PromoCard> _promoCardRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly IBaseRepository _baseRepository;
    private readonly ICardService _cardService;
    private readonly ICommonService _commonService;

    public PromoService(IRepository<Promo> promoRepository, 
        IRepository<PromoCard> promoCardRepository,
        ILoggedInUserService loggedInUserService,
        IBaseRepository baseRepository,
        ICardService cardService, 
        ICommonService commonService)
    {
        _promoRepository = promoRepository;
        _promoCardRepository = promoCardRepository;
        _loggedInUserService = loggedInUserService;
        _baseRepository = baseRepository;
        _cardService = cardService;
        _commonService = commonService;
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
                PassengerStatus = model.PassengerStatus,
                CardStatus = model.CardStatus,
                MaxUsagePerCard = model.MaxUsagePerCard
            };

            if (model.EndTime < DateTime.UtcNow)
            {
                promo.Status = PromoStatus.Expired;
            }
            else if (model.StartTime > DateTime.UtcNow)
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

            if (!string.IsNullOrEmpty(status))
            {
                finalData = allPromoData
                    .Where(pc => pc.Status == status)
                    .Skip((pageNo - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            else
            {
                finalData = allPromoData
                    .Skip((pageNo - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            return new PayloadResponse
            {
                IsSuccess = true,
                Message = "User promos retrieved successfully.",
                Content = finalData
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

    public decimal GetPromoAmountByCardId(string cardId, decimal fare)
    {
        var existingPromoCard = _promoCardRepository
            .GetAll()
            .Include(pc => pc.Promo)
            .FirstOrDefault(pc => pc.CardId == cardId
                                  && pc.Status == PromoCardStatus.Applied
                                  && pc.Promo.Status == PromoStatus.Running);

        if (existingPromoCard == null) return 0;

        var promo = existingPromoCard.Promo;

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

    public void MarkPromoAsUsedByCardId(string cardId)
    {
        var existingPromoCard = _promoCardRepository
            .GetAll()
            .Include(pc => pc.Promo)
            .FirstOrDefault(pc => pc.CardId == cardId
                                  && pc.Status == PromoCardStatus.Applied
                                  && pc.Promo.Status == PromoStatus.Running);

        if (existingPromoCard == null) return;

        existingPromoCard.UsageCount += 1;
        existingPromoCard.Status = PromoCardStatus.Used;
        existingPromoCard.UsageDate = DateTime.UtcNow;
        _promoCardRepository.Update(existingPromoCard);
        _promoCardRepository.SaveChanges();
    }

    public PromoCard GetPromoCardByCardId(string cardId)
    {
        var existingPromoCard = _promoCardRepository
            .GetAll()
            .Include(pc => pc.Promo)
            .FirstOrDefault(pc => pc.CardId == cardId
                                  && pc.Status == PromoCardStatus.Used
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
                condition.Add($" (Code like '%{filter.SearchQuery}%' or Description like '%{filter.SearchQuery}%' or Status like '%{filter.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Promos", whereCondition);

            var finalQueryData = _commonService.GetFinalData<Promo>("Promos", whereCondition, extraCondition);

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalQueryData, rowCount },
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
                         AND Status IN ('{PromoCardStatus.Available}', '{PromoCardStatus.AvailableSoon}');";

        _baseRepository.ExecuteQuery(query);
    }

    private void AddPromoToCard(Promo promo)
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
            filters.Add($"PassengerStatus = '{promo.PassengerStatus}'");
        }

        if (!string.IsNullOrEmpty(promo.CardStatus))
        {
            filters.Add($"Status = '{promo.CardStatus}'");
        }

        var whereCondition = string.Join(" AND ", filters);

        var status = promo.Status == PromoStatus.AvailableSoon ? PromoCardStatus.AvailableSoon : 
                promo.Status == PromoStatus.Running ? PromoCardStatus.Available :
                PromoCardStatus.Expired;

        var query = @$"WITH card_data AS (
                            SELECT *
                            FROM Cards
                            WHERE {whereCondition}
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
    }

    private bool IfDuplicateCode(string code)
    {
        var existingPromo = _promoRepository.GetConditional(p => p.Code == code);

        return existingPromo != null;
    }
}