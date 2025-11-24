using GoBangladesh.Application.DTOs.Export;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using System;
using System.Collections.Generic;

namespace GoBangladesh.Application.Services;

public class ExportService : IExportService
{
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly ICommonExportService _commonExportService;

    public ExportService(ILoggedInUserService loggedInUserService,
        ICommonService commonService,
        ICommonExportService commonExportService)
    {
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _commonExportService = commonExportService;
    }

    public PayloadResponse ExportTripHistoryData(TripHistoryExportFilterDto model)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var condition = new List<string>();
            var extraCondition = " order by t.IsRunning desc, t.CreateTime desc";

            if (!currentUser.IsSuperAdmin)
            {
                condition.Add($" o.Id = '{currentUser.OrganizationId}' ");
            }
            else
            {
                if (!string.IsNullOrEmpty(model.OrganizationId))
                {
                    condition.Add($" o.Id = '{model.OrganizationId}' ");
                }
            }

            if (model.StartDate != null || model.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(model.StartDate, model.EndDate);

                condition.Add($@" (t.TripStartTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}'
                                or t.TripEndTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            if (!string.IsNullOrEmpty(model.BusId))
            {
                condition.Add($" b.Id = '{model.BusId}' ");
            }

            if (!string.IsNullOrEmpty(model.RouteId))
            {
                condition.Add($" r.Id = '{model.RouteId}' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var tripDashboardDataQuery = GetTripDashboardQuery(whereCondition, extraCondition);
            var excelFilePath = _commonExportService.ExportDataToExcel(tripDashboardDataQuery, "TripHistory");

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Trip Dashboard Export",
                Content = excelFilePath,
                Message = "Trip dashboard excel file data has been fetched successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Trip Dashboard Export",
                Message = $"Trip dashboard data export has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse ExportSessionHistoryData(SessionHistoryExportFilterDto model)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var condition = new List<string>();
            var extraCondition = @"
                                    group by s.Id, s.SessionCode, o.Name, b.BusNumber, b.BusName, r.TripStartPlace, r.TripEndPlace, u.Name, u.MobileNumber,
                                             s.StartTime, s.EndTime, s.StartingLatitude, s.StartingLongitude, s.EndingLatitude, s.EndingLongitude,
                                             s.IsRunning, s.CreateTime, s.StopStatus, s.Distance
                                    order by s.IsRunning desc, s.CreateTime desc";

            if (!currentUser.IsSuperAdmin)
            {
                condition.Add($" o.Id = '{currentUser.OrganizationId}' ");
            }
            else
            {
                if (!string.IsNullOrEmpty(model.OrganizationId))
                {
                    condition.Add($" o.Id = '{model.OrganizationId}' ");
                }
            }

            if (model.StartDate != null || model.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(model.StartDate, model.EndDate);

                condition.Add($@" (s.StartTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}'
                                or s.EndTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            if (!string.IsNullOrEmpty(model.BusId))
            {
                condition.Add($" b.Id = '{model.BusId}' ");
            }

            if (!string.IsNullOrEmpty(model.RouteId))
            {
                condition.Add($" r.Id = '{model.RouteId}' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var sessionHistoryDataQuery = GetSessionHistoryDataQuery(whereCondition, extraCondition);
            var excelFilePath = _commonExportService.ExportDataToExcel(sessionHistoryDataQuery, "SessionHistory");

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Session Dashboard Export",
                Content = excelFilePath,
                Message = "Session dashboard data has been exported successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Session Dashboard Export",
                Message = $"Session dashboard data export has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse ExportBusList(BusListExportFilterDto model)
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
                    PayloadType = "Bus",
                    Message = "Bus not found"
                };
            }

            var condition = new List<string>();
            var extraCondition = "ORDER BY b.LastModifiedTime desc";

            if (!currentUser.IsSuperAdmin)
            {
                if (string.IsNullOrEmpty(currentUser.OrganizationId))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Bus",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                model.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(model.SearchQuery))
            {
                condition.Add($" (b.BusNumber like '%{model.SearchQuery}%' or b.BusName like '%{model.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(model.OrganizationId))
            {
                condition.Add($" OrganizationId = '{model.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var busListQuery = GetBusListQuery(whereCondition, extraCondition);
            var excelFilePath = _commonExportService.ExportDataToExcel(busListQuery, "BusList");

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus Export",
                Content = excelFilePath,
                Message = "Bus data export is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus export is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse ExportCardList(CardListExportFilterDto model)
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
                    PayloadType = "Card",
                    Message = "Card not found"
                };
            }

            var condition = new List<string>();
            var extraCondition = "ORDER BY c.LastModifiedTime desc";

            if (!currentUser.IsSuperAdmin)
            {
                if (string.IsNullOrEmpty(currentUser.OrganizationId))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Card",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                model.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(model.SearchQuery))
            {
                condition.Add($" (c.CardNumber like '%{model.SearchQuery}%' or c.Status like '%{model.SearchQuery}%' or c.PassengerStatus like '%{model.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(model.OrganizationId))
            {
                condition.Add($" c.OrganizationId = '{model.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var cardExportQuery = GetCardExportQuery(whereCondition, extraCondition);
            var excelFilePath = _commonExportService.ExportDataToExcel(cardExportQuery, "CardList");

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card Export",
                Content = excelFilePath,
                Message = "Card data export is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card Export",
                Message = $"Card export is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse ExportPassengerList(PassengerListExportFilterDto model)
    {
        try
        {
            var currentUser = _loggedInUserService
                .GetLoggedInUser();

            if (currentUser is null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Passenger",
                    Message = "Current User not found!"
                };
            }

            var condition = new List<string> { " u.UserType in ('Public', 'Private') " };
            var extraCondition = "ORDER BY c.LastModifiedTime desc, u.LastModifiedTime desc";

            if (!currentUser.IsSuperAdmin)
            {
                if (string.IsNullOrEmpty(currentUser.OrganizationId))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Passenger",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                model.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(model.SearchQuery))
            {
                condition.Add($" (u.Name like '%{model.SearchQuery}%' or u.MobileNumber like '%{model.SearchQuery}%' or u.PassengerId like '%{model.SearchQuery}%' or c.CardNumber like '%{model.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(model.OrganizationId))
            {
                condition.Add($" u.OrganizationId = '{model.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var passengerListQuery = GetPassengerListQuery(whereCondition, extraCondition);
            var excelFilePath = _commonExportService.ExportDataToExcel(passengerListQuery, "PassengerList");

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Passenger Data Export",
                Content = excelFilePath,
                Message = "Passenger data export is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Passenger Data Export",
                Message = $"Passenger data export is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse ExportPromoList(PromoListExportFilterDto model)
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
                                    END, p.CreateTime desc";

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

                model.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(model.SearchQuery))
            {
                condition.Add($" (p.Code like '%{model.SearchQuery}%' or p.Description like '%{model.SearchQuery}%' or p.Status like '%{model.SearchQuery}%') ");
            }

            if (model.StartDate != null || model.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(model.StartDate, model.EndDate);

                condition.Add($@" (p.StartTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}'
                                or p.EndTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            if (!string.IsNullOrEmpty(model.OrganizationId))
            {
                condition.Add($" p.OrganizationId = '{model.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var promoListQuery = GetPromoListQuery(whereCondition, extraCondition);
            var excelFilePath = _commonExportService.ExportDataToExcel(promoListQuery, "PromoList");

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = excelFilePath,
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

    public PayloadResponse ExportCardStatement(RechargeReturnHistoryExportFilterDto model)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var condition = new List<string> { " t.TransactionType in ('Recharge', 'Return') " };
            var extraCondition = " order by t.CreateTime desc";

            if (!currentUser.IsSuperAdmin)
            {
                condition.Add($" o.Id = '{currentUser.OrganizationId}' ");
            }
            else
            {
                if (!string.IsNullOrEmpty(model.OrganizationId))
                {
                    condition.Add($" o.Id = '{model.OrganizationId}' ");
                }
            }

            if (model.StartDate != null || model.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(model.StartDate, model.EndDate);

                condition.Add($" (t.CreateTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            if (!string.IsNullOrEmpty(model.AgentId))
            {
                condition.Add($" a.Id = '{model.AgentId}' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var cardStatementQuery = GetCardStatementQuery(whereCondition, extraCondition);
            var excelFilePath = _commonExportService.ExportDataToExcel(cardStatementQuery, "CardStatement");

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card Statement Export",
                Content = excelFilePath,
                Message = "Card statement export has been successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card Statement Export",
                Message = $"Card statement export has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse DeleteExportedFile(string filePath)
    {
        try
        {
            _commonService.DeleteFile(filePath);

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Delete Exported File",
                Message = "Exported file has been deleted successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Delete Exported File",
                Message = $"Exported file deletion has been failed because {ex.Message}!"
            };
        }
    }

    private string GetCardStatementQuery(string whereCondition, string extraCondition)
    {
        var query = $@"
                       select  t.TransactionId                as TransactionId,
                               o.Name                         as OrganizationName,
                               t.CreateTime                   as TransactionTime,
                               u.Name                         as PassengerName,
                               c.CardNumber                   as CardNumber,
                               t.Medium                       as Medium,
                               a.Name                         as MediumName,
                               t.Amount,
                               t.TransactionType
                        from Transactions t
                                 left join Cards c on t.CardId = c.Id
                                 left join PassengerCardHistory pch on c.Id = pch.CardId
                                 left join PassengerCardMappings pcm on c.Id = pcm.CardId
                                 left join Users u on pch.UserId = u.Id or pcm.UserId = u.Id
                                 left join Users a on t.AgentId = a.Id
                                 left join Organizations o on a.OrganizationId = o.Id {whereCondition} {extraCondition}";

        return query;
    }

    private string GetPromoListQuery(string whereCondition, string extraCondition) 
    {
        var query = $@"select o.Name                                                                       as OrganizationName,
                           p.Code                                                                       as PromoCode,
                           p.PromoType,
                           p.DiscountValue,
                           case when p.Status = 'AvailableSoon' then 'Available Soon' else p.Status end as Status,
                           DATEADD(HOUR, 6, p.StartTime)                                                AS StartTime,
                           DATEADD(HOUR, 6, p.EndTime)                                                  AS EndTime,
                           P.PassengerStatus,
                           p.CardStatus,
                           p.MaxUsagePerCard,
                           p.MaxDiscountAmount
                    from Promos p
                             left join Organizations o on p.OrganizationId = o.Id
                    {whereCondition} {extraCondition}";

        return query;
    }

    private string GetPassengerListQuery(string whereCondition, string extraCondition)
    {
        var query = $@"select u.Name     as PassengerName,
                           o.Name     as OrganizationName,
                           c.CardNumber,
                           u.PassengerId,
                           u.UserType as Type,
                           U.MobileNumber,
                           c.Balance,
                           U.IsActive
                    from Users u
                             left join Organizations o on u.OrganizationId = o.Id
                             left join PassengerCardMappings pcm on u.Id = pcm.UserId
                             left join Cards c on pcm.CardId = c.Id
                    {whereCondition} {extraCondition}";

        return query;
    }

    private string GetCardExportQuery(string whereCondition, string extraCondition)
    {
        var query = $@"select o.Name, c.CardNumber, c.Status, c.PassengerStatus, c.Balance
                    from Cards c
                             left join Organizations o on c.OrganizationId = o.Id
                                            {whereCondition} {extraCondition}";

        return query;
    }

    private string GetBusListQuery(string whereCondition, string extraCondition)
    {
        var query = $@"select b.BusNumber, b.BusName, r.TripStartPlace, r.TripEndPlace, o.Name as OrganizationName, b.IsActive
                    from Buses b
                             left join Organizations o on b.OrganizationId = o.Id
                             left join Routes r on b.RouteId = r.Id
                    {whereCondition} {extraCondition}";

        return query;
    }

    private string GetSessionHistoryDataQuery(string whereCondition, string extraCondition)
    {
        var query = $@"
                       select s.Id as SessionId,
                       s.SessionCode,
                       o.Name                                    as OrganizationName,
                       b.BusNumber,
                       b.BusName,
                       r.TripStartPlace + ' - ' + r.TripEndPlace as Route,
                       u.Name                                    as StaffName,
                       u.MobileNumber,
                       s.StartTime,
                       s.EndTime,
                       s.StartingLatitude,
                       s.StartingLongitude,
                       s.EndingLatitude,
                       s.EndingLongitude,
                       s.Distance,
                       case
                           when s.IsRunning = 1 then 'Running'
                           else 'Complete' end                   as Status,
                       count(t.Id)                               as TotalTrips,
                       sum(CASE WHEN t.IsRunning = 1 THEN 1 ELSE 0 END) as CurrentRunningTrips,
                       COALESCE(SUM(t.Amount), 0) as Revenue,
                       s.StopStatus
                from Sessions s
                         left join Buses b on s.BusId = b.Id
                         left join Organizations o on b.OrganizationId = o.Id
                         left join Users u on s.UserId = u.Id and u.UserType in ('Staff')
                         left join Routes r on b.RouteId = r.Id
                         left join Trips t on s.Id = t.SessionId
                         {whereCondition} {extraCondition}";

        return query;
    }

    private string GetTripDashboardQuery(string whereCondition, string extraCondition)
    {
        var query = $@"
                        select t.Id                                      as TripId,
                               o.Name                                    as OrganizationName,
                               r.TripStartPlace + ' - ' + r.TripEndPlace as Route,
                               b.BusNumber,
                               tr.TransactionId,
                               c.CardNumber,
                               u.Name as PassengerName,
                               t.TripStartTime,
                               t.TripEndTime,
                               t.StartingLatitude,
                               t.StartingLongitude,
                               t.EndingLatitude,
                               t.EndingLongitude,
                               t.Distance,
                               t.Amount as Fare,
                               t.TapInType as TapInType,                                 
                               t.TapOutStatus as TapOutStatus,
                               case when t.IsRunning = 1 then 'Running'
                                else 'Complete' end as Status,
                               case when t.IsRunning = 1 then null else u1.UserType end as LastModifiedByUserType   
                        from Trips t
                                 left join Cards c on t.CardId = c.Id
                                 left join PassengerCardHistory pch on c.Id = pch.CardId
                                 left join PassengerCardMappings pcm on c.Id = pcm.CardId
                                 left join Users u on pch.UserId = u.Id or pcm.UserId = u.Id
                                 left join Sessions s on t.SessionId = s.Id
                                 left join Buses b on s.BusId = b.Id
                                 left join Routes r on b.RouteId = r.Id
                                 left join Organizations o on b.OrganizationId = o.Id
                                 left join Users u1 on t.LastModifiedBy = u1.Id
                                 left join Transactions tr on t.Id = tr.TripId
                                 {whereCondition} {extraCondition}";

        return query;
    }
}