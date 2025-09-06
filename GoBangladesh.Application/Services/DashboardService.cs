using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.DTOs.Dashboard;
using GoBangladesh.Application.DTOs.Dashboard.Recharge;
using GoBangladesh.Application.DTOs.Dashboard.Session;
using GoBangladesh.Application.DTOs.Dashboard.Trip;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly IBaseRepository _baseRepository;
    private readonly ICommonService _commonService;

    public DashboardService(ILoggedInUserService loggedInUserService,
        IBaseRepository baseRepository,
        ICommonService commonService)
    {
        _loggedInUserService = loggedInUserService;
        _baseRepository = baseRepository;
        _commonService = commonService;
    }

    public PayloadResponse GetDashboardData(string organizationId)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var whereCondition = "where o.IsActive = 1";

            if (!currentUser.IsSuperAdmin)
            {
                whereCondition += $" and o.Id = '{currentUser.OrganizationId}'";
            }
            else
            {
                if (!string.IsNullOrEmpty(organizationId))
                {
                    whereCondition += $" and o.Id = '{organizationId}'";
                }
            }

            var dashboardQuery = $@"
                                select count(distinct o.Id)  as TotalOrganization,
                                       count(distinct b.Id)  as TotalBus,
                                       count(distinct u.Id)  as TotalStaff,
                                       count(distinct u1.Id) as TotalAgent,
                                       count(distinct c.Id)  as TotalPassenger,
                                       count(distinct u2.Id) as TotalTicketExaminer,
                                       count(distinct r.Id) as TotalRoute
                                from Organizations o
                                         left join Buses b on o.Id = b.OrganizationId and b.IsActive = 1
                                         left join Users u on o.Id = u.OrganizationId and u.UserType = 'Staff' and u.IsActive = 1
                                         left join Users u1 on o.Id = u1.OrganizationId and u1.UserType = 'Agent' and u1.IsActive = 1
                                         left join Users u2 on o.Id = u2.OrganizationId and u2.UserType = 'TicketExaminer' and u2.IsActive = 1
                                         left join Cards c on o.Id = c.OrganizationId and c.Status = 'In Use'
                                         left join Routes r on o.Id = r.OrganizationId and r.IsActive = 1
                                {whereCondition}";

            var dashboardData = _baseRepository
                .Query<DashboardDto>(dashboardQuery)
                .FirstOrDefault();

            dashboardData!.DataOfToday = GetDataForToday(currentUser, organizationId);
            dashboardData.DataOfThisMonth = GetDataForThisMonth(currentUser, organizationId);
            dashboardData.DataOfAllTime = GetDataForallTime(currentUser, organizationId);
            dashboardData.PayableAmount = GetPayableAmount(currentUser, organizationId);
            dashboardData.InHandAmount = GetInHandAmount(currentUser, organizationId) + dashboardData.PayableAmount;
            dashboardData.ReceivableAmount = GetReceivableAmount(currentUser, organizationId);
            dashboardData.DueAmount = GetDueAmount(currentUser, organizationId);
            dashboardData.ObsoleteAmount = GetObsoleteAmount(currentUser, organizationId);

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Dashboard",
                Content = dashboardData,
                Message = "Dashboard data fetched successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Dashboard",
                Message = $"Dashboard data fetch failed because {ex.Message}!"
            };
        }
    }

    private decimal GetObsoleteAmount(User currentUser, string organizationId)
    {
        if (string.IsNullOrEmpty(organizationId))
        {
            if (!currentUser.IsSuperAdmin)
            {
                organizationId = currentUser.OrganizationId;
            }
        }

        var query = $@"
                    select coalesce(sum(Balance), 0) as ObsoleteAmount
                    from Cards
                    where Status = 'Obsolete'
                    {(string.IsNullOrEmpty(organizationId) ? string.Empty : $"and OrganizationId = '{organizationId}'")}";

        return _baseRepository
            .Query<decimal>(query)
            .FirstOrDefault();
    }

    private decimal GetDueAmount(User currentUser, string organizationId)
    {
        if (string.IsNullOrEmpty(organizationId))
        {
            if (!currentUser.IsSuperAdmin)
            {
                organizationId = currentUser.OrganizationId;
            }
        }

        var query = $@"
                    select coalesce(sum(Amount), 0) as DueAmount
                    from CardDue
                    {(string.IsNullOrEmpty(organizationId) ? string.Empty : $"where OrganizationId = '{organizationId}'")}";

        return _baseRepository
            .Query<decimal>(query)
            .FirstOrDefault();
    }

    private decimal GetReceivableAmount(User currentUser, string organizationId)
    {
        if (string.IsNullOrEmpty(organizationId))
        {
            if (!currentUser.IsSuperAdmin)
            {
                organizationId = currentUser.OrganizationId;
            }
        }

        var query = $@"with pending_amount as (
                            select coalesce(sum(Amount), 0) pendingAmountWithoutInvoice
                            from OrganizationSettlement
                            where InvoiceNumber is null {(string.IsNullOrEmpty(organizationId) ? string.Empty : $" and ToOrganizationId = '{organizationId}'")}
                        ),
                        invoice_data as (
                            select i.*,
                                   coalesce(SUM(case when ip.Status = 'Settled' then ip.Amount end), 0) as PaidAmount,
                                   i.Amount -
                                   coalesce(SUM(case when ip.Status = 'Settled' then ip.Amount end), 0) as DueAmount,
                                   coalesce(SUM(case when ip.Status = 'Pending' then ip.Amount end), 0) as PendingAmount,
                                   case
                                       when (coalesce(SUM(case when ip.Status = 'Settled' then ip.Amount end), 0) +
                                             coalesce(SUM(case when ip.Status = 'Pending' then ip.Amount end), 0)) < i.Amount
                                           then 1
                                       else 0
                                   end as IsPaymentButtonAvailable
                            from Invoices i
                                     left join InvoicePayment ip on i.InvoiceNumber = ip.InvoiceNumber
                            group by i.Id, i.InvoiceNumber, FromOrganizationId, ToOrganizationId, FromDate, ToDate, i.Amount,
                                     i.Status,
                                     i.CreateTime, i.LastModifiedTime, i.CreatedBy, i.LastModifiedBy, i.IsDeleted,
                                     InvoiceFilePath
                        ),
                        unsettled_invoice_amount as (
                            select coalesce(sum(Amount), 0) - coalesce(sum(PaidAmount), 0) as pendingAmountWithInvoice
                            from invoice_data {(string.IsNullOrEmpty(organizationId) ? string.Empty : $" Where ToOrganizationId = '{organizationId}'")}
                        )
                        select
                            coalesce(pa.pendingAmountWithoutInvoice,0) + coalesce(ua.pendingAmountWithInvoice,0) as TotalPendingAmount
                        from pending_amount pa
                        cross join unsettled_invoice_amount ua";

        return _baseRepository
            .Query<decimal>(query)
            .FirstOrDefault();
    }

    private decimal GetPayableAmount(User currentUser, string organizationId)
    {
        if (string.IsNullOrEmpty(organizationId))
        {
            if (!currentUser.IsSuperAdmin)
            {
                organizationId = currentUser.OrganizationId;
            }
        }

        var query = $@"with pending_amount as (
                            select coalesce(sum(Amount), 0) pendingAmountWithoutInvoice
                            from OrganizationSettlement
                            where InvoiceNumber is null {(string.IsNullOrEmpty(organizationId) ? string.Empty : $" and FromOrganizationId = '{organizationId}'")}
                        ),
                        invoice_data as (
                            select i.*,
                                   coalesce(SUM(case when ip.Status = 'Settled' then ip.Amount end), 0) as PaidAmount,
                                   i.Amount -
                                   coalesce(SUM(case when ip.Status = 'Settled' then ip.Amount end), 0) as DueAmount,
                                   coalesce(SUM(case when ip.Status = 'Pending' then ip.Amount end), 0) as PendingAmount,
                                   case
                                       when (coalesce(SUM(case when ip.Status = 'Settled' then ip.Amount end), 0) +
                                             coalesce(SUM(case when ip.Status = 'Pending' then ip.Amount end), 0)) < i.Amount
                                           then 1
                                       else 0
                                   end as IsPaymentButtonAvailable
                            from Invoices i
                                     left join InvoicePayment ip on i.InvoiceNumber = ip.InvoiceNumber
                            group by i.Id, i.InvoiceNumber, FromOrganizationId, ToOrganizationId, FromDate, ToDate, i.Amount,
                                     i.Status,
                                     i.CreateTime, i.LastModifiedTime, i.CreatedBy, i.LastModifiedBy, i.IsDeleted,
                                     InvoiceFilePath
                        ),
                        unsettled_invoice_amount as (
                            select coalesce(sum(Amount), 0) - coalesce(sum(PaidAmount), 0) as pendingAmountWithInvoice
                            from invoice_data {(string.IsNullOrEmpty(organizationId) ? string.Empty : $" Where FromOrganizationId = '{organizationId}'")}
                        )
                        select
                            coalesce(pa.pendingAmountWithoutInvoice,0) + coalesce(ua.pendingAmountWithInvoice,0) as TotalPendingAmount
                        from pending_amount pa
                        cross join unsettled_invoice_amount ua";

        return _baseRepository
            .Query<decimal>(query)
            .FirstOrDefault();
    }

    private decimal GetInHandAmount(User currentUser, string organizationId)
    {
        if (string.IsNullOrEmpty(organizationId))
        {
            if (!currentUser.IsSuperAdmin)
            {
                organizationId = currentUser.OrganizationId;
            }
        }

        var query = $@"
                    select sum(Balance) as InHandAmount
                    from OrganizationCardBalance 
                    {(string.IsNullOrEmpty(organizationId) ? string.Empty : $"where OrganizationId = '{organizationId}'")}";

       return _baseRepository
            .Query<decimal>(query)
            .FirstOrDefault();
    }

    public PayloadResponse GetTripDashboardData(TripDashboardFilter filter)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var condition = new List<string>();
            var extraCondition = $@" order by t.CreateTime desc
                                    OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                    FETCH NEXT {filter.PageSize} ROWS ONLY";

            if (!currentUser.IsSuperAdmin)
            {
                condition.Add($" o.Id = '{currentUser.OrganizationId}' ");
            }
            else
            {
                if (!string.IsNullOrEmpty(filter.OrganizationId))
                {
                    condition.Add($" o.Id = '{filter.OrganizationId}' ");
                }
            }

            if (filter.StartDate != null || filter.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(filter.StartDate, filter.EndDate);

                condition.Add($@" (t.TripStartTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}'
                                or t.TripEndTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            if (!string.IsNullOrEmpty(filter.BusId))
            {
                condition.Add($" b.Id = '{filter.BusId}' ");
            }
            
            if (!string.IsNullOrEmpty(filter.RouteId))
            {
                condition.Add($" r.Id = '{filter.RouteId}' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var tripDashboardData = new TripDashboardData()
            {
                CardData = GetTripDashboardCardData(whereCondition),
                TableData = GetTripDashboardTableData(whereCondition, extraCondition),
                RowCount = GetTripDashboardTableRowCount(whereCondition)
            };

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Trip Dashboard",
                Content = tripDashboardData,
                Message = "Trip dashboard data has been fetched successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Trip Dashboard",
                Message = $"Trip dashboard data fetching has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse GetSessionDashboardData(SessionFilter filter)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var condition = new List<string>();
            var extraCondition = $@"
                                    group by s.Id, s.SessionCode, o.Name, b.BusNumber, b.BusName, r.TripStartPlace, r.TripEndPlace, u.Name, u.MobileNumber,
                                             s.StartTime, s.EndTime, s.StartingLatitude, s.StartingLongitude, s.EndingLatitude, s.EndingLongitude,
                                             s.IsRunning, s.CreateTime, s.StopStatus
                                    order by s.CreateTime desc
                                    OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                    FETCH NEXT {filter.PageSize} ROWS ONLY";

            if (!currentUser.IsSuperAdmin)
            {
                condition.Add($" o.Id = '{currentUser.OrganizationId}' ");
            }
            else
            {
                if (!string.IsNullOrEmpty(filter.OrganizationId))
                {
                    condition.Add($" o.Id = '{filter.OrganizationId}' ");
                }
            }

            if (filter.StartDate != null || filter.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(filter.StartDate, filter.EndDate);

                condition.Add($@" (s.StartTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}'
                                or s.EndTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            if (!string.IsNullOrEmpty(filter.BusId))
            {
                condition.Add($" b.Id = '{filter.BusId}' ");
            }

            if (!string.IsNullOrEmpty(filter.RouteId))
            {
                condition.Add($" r.Id = '{filter.RouteId}' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var tripDashboardData = new SessionDashboardData()
            {
                CardData = GetSessionDashboardCardData(whereCondition),
                TableData = GetSessionDashboardTableData(whereCondition, extraCondition),
                RowCount = GetSessionDashboardTableRowCount(whereCondition)
            };

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Session Dashboard",
                Content = tripDashboardData,
                Message = "Session dashboard data has been fetched successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Session Dashboard",
                Message = $"Session dashboard data fetching has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse GetRechargeDashboardData(RechargeFilter filter)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var condition = new List<string> { " t.TransactionType in ('Recharge', 'Return') " };
            var extraCondition = $@" order by t.CreateTime desc
                                    OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                    FETCH NEXT {filter.PageSize} ROWS ONLY";

            if (!currentUser.IsSuperAdmin)
            {
                condition.Add($" o.Id = '{currentUser.OrganizationId}' ");
            }
            else
            {
                if (!string.IsNullOrEmpty(filter.OrganizationId))
                {
                    condition.Add($" o.Id = '{filter.OrganizationId}' ");
                }
            }

            if (filter.StartDate != null || filter.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(filter.StartDate, filter.EndDate);

                condition.Add($@" (t.CreateTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}'
                                or t.CreateTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            if (!string.IsNullOrEmpty(filter.AgentId))
            {
                condition.Add($" a.Id = '{filter.AgentId}' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rechargeDashboardData = new RechargeDashboardData()
            {
                CardData = GetRechargeDashboardCardData(whereCondition),
                TableData = GetRechargeDashboardTableData(whereCondition, extraCondition),
                RowCount = GetRechargeDashboardTableRowCount(whereCondition)
            };

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Recharge Dashboard",
                Content = rechargeDashboardData,
                Message = "Recharge dashboard data has been fetched successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Recharge Dashboard",
                Message = $"Recharge dashboard data fetching has been failed because {ex.Message}!"
            };
        }
    }

    private int GetRechargeDashboardTableRowCount(string whereCondition)
    {
        var query = $@"
                        select count(t.Id)
                        from Transactions t
                                 left join Cards c on t.CardId = c.Id
                                 left join PassengerCardHistory pch on c.Id = pch.CardId
                                 left join PassengerCardMappings pcm on c.Id = pcm.CardId
                                 left join Users u on pch.UserId = u.Id or pcm.UserId = u.Id
                                 left join Users a on t.AgentId = a.Id
                                 left join Organizations o on a.OrganizationId = o.Id {whereCondition}";


        var rowCount = _baseRepository
            .Query<int>(query)
            .FirstOrDefault();

        return rowCount;
    }

    private List<RechargeDashboardTableData> GetRechargeDashboardTableData(string whereCondition, string extraCondition)
    {
        var query = $@"
                       select  t.TransactionId                as TransactionId,
                               o.Name                         as OrganizationName,
                               t.CreateTime                   as TransactionTime,
                               u.PassengerId,
                               u.Name                         as PassengerName,
                               c.CardNumber                   as CardNumber,
                               'Agent'                        as RechargeMedium,
                               a.Name                         as RechargerName,
                               t.Amount,
                               t.TransactionType
                        from Transactions t
                                 left join Cards c on t.CardId = c.Id
                                 left join PassengerCardHistory pch on c.Id = pch.CardId
                                 left join PassengerCardMappings pcm on c.Id = pcm.CardId
                                 left join Users u on pch.UserId = u.Id or pcm.UserId = u.Id
                                 left join Users a on t.AgentId = a.Id
                                 left join Organizations o on a.OrganizationId = o.Id {whereCondition} {extraCondition}";


        var rechargeDashboardTableData = _baseRepository
            .Query<RechargeDashboardTableData>(query);

        return rechargeDashboardTableData;
    }

    private RechargeDashboardCardData GetRechargeDashboardCardData(string whereCondition)
    {
        var query = $@"
                       select count(distinct a.Id) as TotalAgent,
                               count(distinct u.Id) as TotalPassenger,
                               count(t.Id) as TotalRecharge,
                               sum(t.Amount) as TotalAmount
                        from Transactions t
                                 left join Cards c on t.CardId = c.Id
                                 left join PassengerCardHistory pch on c.Id = pch.CardId
                                 left join PassengerCardMappings pcm on c.Id = pcm.CardId
                                 left join Users u on pch.UserId = u.Id or pcm.UserId = u.Id
                                 left join Users a on t.AgentId = a.Id
                                 left join Organizations o on a.OrganizationId = o.Id {whereCondition}";

        var rechargeDashboardCardData = _baseRepository
            .Query<RechargeDashboardCardData>(query)
            .FirstOrDefault();

        return rechargeDashboardCardData;
    }

    private int GetSessionDashboardTableRowCount(string whereCondition)
    {
        var query = $@"
                        select count(s.Id)
                        from Sessions s
                         left join Buses b on s.BusId = b.Id
                         left join Organizations o on b.OrganizationId = o.Id
                         left join Users u on s.UserId = u.Id and u.UserType in ('Staff')
                         left join Routes r on b.RouteId = r.Id 
                         {whereCondition}";


        var rowCount = _baseRepository
            .Query<int>(query)
            .FirstOrDefault();

        return rowCount;
    }

    private List<SessionDashboardTableData> GetSessionDashboardTableData(string whereCondition, string extraCondition)
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


        var sessionDashboardTableData = _baseRepository
            .Query<SessionDashboardTableData>(query);

        return sessionDashboardTableData;
    }

    private SessionDashboardCardData GetSessionDashboardCardData(string whereCondition)
    {
        var query = $@"
                       select count(s.Id) as TotalSession,
                       sum(case when s.IsRunning = 1 then 1 else 0 end) as TotalRunningSession,
                       count(distinct b.Id) as TotalBus,
                       count(distinct u.Id) as TotalStaff
                from Sessions s
                         left join Buses b on s.BusId = b.Id
                         left join Routes r on b.RouteId = r.Id
                         left join Organizations o on b.OrganizationId = o.Id
                         left join Users u on s.UserId = u.Id and u.UserType in ('Staff') {whereCondition}";

        var sessionDashboardCardData = _baseRepository
            .Query<SessionDashboardCardData>(query)
            .FirstOrDefault();

        return sessionDashboardCardData;
    }

    private int GetTripDashboardTableRowCount(string whereCondition)
    {
        var query = $@"
                        select count(t.Id)
                        from Trips t
                                 left join Sessions s on t.SessionId = s.Id
                                 left join Buses b on s.BusId = b.Id
                                 left join Routes r on b.RouteId = r.Id
                                 left join Organizations o on b.OrganizationId = o.Id
                                 {whereCondition}";


        var rowCount = _baseRepository
            .Query<int>(query)
            .FirstOrDefault();

        return rowCount;
    }

    private List<TripDashBoardTableData> GetTripDashboardTableData(string whereCondition, string extraCondition)
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


        var tripDashBoardTableData = _baseRepository
            .Query<TripDashBoardTableData>(query);

        return tripDashBoardTableData; 
    }

    private TripDashboardCardData GetTripDashboardCardData(string whereCondition)
    {
        var query = $@"
                        select count(distinct t.Id) as TotalTrips,
                               count(distinct c.Id) as TotalPassengers,
                               sum(t.Amount) as TotalFare,
                               count(distinct b.Id) as TotalBus
                        from Trips t
                                 left join Cards c on t.CardId = c.Id
                                 left join PassengerCardHistory pch on c.Id = pch.CardId
                                 left join PassengerCardMappings pcm on c.Id = pcm.CardId
                                 left join Users u on pch.UserId = u.Id or pcm.UserId = u.Id
                                 left join Sessions s on t.SessionId = s.Id
                                 left join Buses b on s.BusId = b.Id 
                                 left join Routes r on b.RouteId = r.Id
                                 left join Organizations o on b.OrganizationId = o.Id 
                                 {whereCondition}";

        var tripDashBoardTableData = _baseRepository
            .Query<TripDashboardCardData>(query)
            .FirstOrDefault();

        return tripDashBoardTableData;
    }

    private DashboardCommonData GetDataForallTime(User currentUser, string organizationId)
    {
        var allTimeDataQuery = $@"
                                with trip_detail as (
                                    select
                                        count(distinct t.id) as TotalTrip,
                                        sum(t.Amount) as TotalRevenue
                                    from Trips t
                                    inner join Sessions s on t.SessionId = s.Id
                                    inner join Users u on s.UserId = u.Id
                                    {(currentUser.IsSuperAdmin ?
                                        string.IsNullOrEmpty(organizationId) ? string.Empty : $" and u.OrganizationId = '{organizationId}'" 
                                        : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                ),
                                recharge_detail as (
                                    select
                                        count(distinct t.Id) as TotalRecharge,
                                        sum(t.Amount) as RechargeAmount
                                    from Transactions t
                                    inner join Users u on t.CreatedBy = u.Id
                                    where t.TransactionType = 'Recharge'
                                    {(currentUser.IsSuperAdmin ?
                                        string.IsNullOrEmpty(organizationId) ? string.Empty : $" and u.OrganizationId = '{organizationId}'" 
                                        : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                ),
                                return_detail as (
                                    select
                                        count(distinct t.Id) as TotalReturn,
                                        sum(t.Amount) as ReturnAmount
                                    from Transactions t
                                    inner join Users u on t.CreatedBy = u.Id
                                    where t.TransactionType = 'Return'
                                    {(currentUser.IsSuperAdmin ?
                                        string.IsNullOrEmpty(organizationId) ? string.Empty : $" and u.OrganizationId = '{organizationId}'" 
                                        : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                )
                                select
                                    td.TotalTrip,
                                    td.TotalRevenue,
                                    rd.TotalRecharge,
                                    rd.RechargeAmount,
                                    rd1.TotalReturn,
                                    rd1.ReturnAmount
                                from trip_detail td
                                cross join recharge_detail rd
                                cross join return_detail rd1;";

        var allTimeData = _baseRepository
            .Query<DashboardCommonData>(allTimeDataQuery)
            .FirstOrDefault();

        return allTimeData;
    }

    private DashboardCommonData GetDataForThisMonth(User currentUser, string organizationId)
    {
        var thisMonthDataQuery = $@"
                                with trip_detail as (
                                    select
                                        count(distinct t.id) as TotalTrip,
                                        sum(t.Amount) as TotalRevenue
                                    from Trips t
                                    inner join Sessions s on t.SessionId = s.Id
                                    inner join Users u on s.UserId = u.Id
                                    where YEAR(t.CreateTime) = YEAR(GETUTCDATE()) 
                                    AND MONTH(t.CreateTime) = MONTH(GETUTCDATE())
                                    {(currentUser.IsSuperAdmin ?
                                        string.IsNullOrEmpty(organizationId) ? string.Empty : $" and u.OrganizationId = '{organizationId}'" 
                                        : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                ),
                                recharge_detail as (
                                    select
                                        count(distinct t.Id) as TotalRecharge,
                                        sum(t.Amount) as RechargeAmount
                                    from Transactions t
                                    inner join Users u on t.CreatedBy = u.Id
                                    where t.TransactionType = 'Recharge' and
                                    YEAR(t.CreateTime) = YEAR(GETUTCDATE()) 
                                    AND MONTH(t.CreateTime) = MONTH(GETUTCDATE())
                                    {(currentUser.IsSuperAdmin ?
                                        string.IsNullOrEmpty(organizationId) ? string.Empty : $" and u.OrganizationId = '{organizationId}'" 
                                        : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                ),
                                return_detail as (
                                    select
                                        count(distinct t.Id) as TotalReturn,
                                        sum(t.Amount) as ReturnAmount
                                    from Transactions t
                                    inner join Users u on t.CreatedBy = u.Id
                                    where t.TransactionType = 'Return' and
                                    YEAR(t.CreateTime) = YEAR(GETUTCDATE()) 
                                    AND MONTH(t.CreateTime) = MONTH(GETUTCDATE())
                                    {(currentUser.IsSuperAdmin ?
                                        string.IsNullOrEmpty(organizationId) ? string.Empty : $" and u.OrganizationId = '{organizationId}'" 
                                        : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                )
                                select
                                    td.TotalTrip,
                                    td.TotalRevenue,
                                    rd.TotalRecharge,
                                    rd.RechargeAmount,
                                    rd1.TotalReturn,
                                    rd1.ReturnAmount
                                from trip_detail td
                                cross join recharge_detail rd
                                cross join return_detail rd1;";

        var dataOfThisMonth = _baseRepository
            .Query<DashboardCommonData>(thisMonthDataQuery)
            .FirstOrDefault();

        return dataOfThisMonth;
    }

    private DashboardCommonData GetDataForToday(User currentUser, string organizationId)
    {
        var todaysDataQuery = $@"
                                with trip_detail as (
                                    select
                                        count(distinct t.id) as TotalTrip,
                                        sum(t.Amount) as TotalRevenue
                                    from Trips t
                                    inner join Sessions s on t.SessionId = s.Id
                                    inner join Users u on s.UserId = u.Id
                                    where CAST(t.CreateTime AS DATE) = CAST(GETUTCDATE() AS DATE)
                                    {(currentUser.IsSuperAdmin ?
                                        string.IsNullOrEmpty(organizationId) ? string.Empty : $" and u.OrganizationId = '{organizationId}'" 
                                        : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                ),
                                recharge_detail as (
                                    select
                                        count(distinct t.Id) as TotalRecharge,
                                        sum(t.Amount) as RechargeAmount
                                    from Transactions t
                                    inner join Users u on t.CreatedBy = u.Id
                                    where t.TransactionType = 'Recharge' and CAST(t.CreateTime AS DATE) = CAST(GETUTCDATE() AS DATE)
                                    {(currentUser.IsSuperAdmin ? 
                                        string.IsNullOrEmpty(organizationId) ? string.Empty : $" and u.OrganizationId = '{organizationId}'"
                                        : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                ),
                                return_detail as (
                                    select
                                        count(distinct t.Id) as TotalReturn,
                                        sum(t.Amount) as ReturnAmount
                                    from Transactions t
                                    inner join Users u on t.CreatedBy = u.Id
                                    where t.TransactionType = 'Return' and CAST(t.CreateTime AS DATE) = CAST(GETUTCDATE() AS DATE)
                                {(currentUser.IsSuperAdmin ? 
                                    string.IsNullOrEmpty(organizationId) ? string.Empty : $" and u.OrganizationId = '{organizationId}'"
                                    : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                )
                                select
                                    td.TotalTrip,
                                    td.TotalRevenue,
                                    rd.TotalRecharge,
                                    rd.RechargeAmount,
                                    rd1.TotalReturn,
                                    rd1.ReturnAmount
                                from trip_detail td
                                cross join recharge_detail rd
                                cross join return_detail rd1;";

        var dataOfToday = _baseRepository
            .Query<DashboardCommonData>(todaysDataQuery)
            .FirstOrDefault();

        return dataOfToday;
    }
}