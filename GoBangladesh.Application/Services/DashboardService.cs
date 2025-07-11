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

    public PayloadResponse GetDashboardData()
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var whereCondition = string.Empty;

            if (!currentUser.IsSuperAdmin)
            {
                whereCondition = $" where o.Id = '{currentUser.OrganizationId}'";
            }

            var dashboardQuery = $@"
                                select count(distinct o.Id) as TotalOrganization,
                                       count(distinct b.Id) as TotalBus,
                                       count(distinct u.Id) as TotalStaff,
                                       count(distinct u1.Id) as TotalAgent,
                                       count(distinct u2.Id) as TotalPassenger
                                from Organizations o
                                         left join Buses b on o.Id = b.OrganizationId
                                         left join Users u on o.Id = u.OrganizationId and u.UserType = 'Staff'
                                         left join Users u1 on o.Id = u1.OrganizationId and u1.UserType = 'Agent'
                                         left join Users u2 on o.Id = u2.OrganizationId and u2.UserType in ('Public', 'Private')
                                {whereCondition}";

            var dashboardData = _baseRepository
                .Query<DashboardDto>(dashboardQuery)
                .FirstOrDefault();

            dashboardData!.DataOfToday = GetDataForToday(currentUser);
            dashboardData.DataOfThisMonth = GetDataForThisMonth(currentUser);
            dashboardData.DataOfAllTime = GetDataForallTime(currentUser);

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
            var extraCondition = $@" order by s.CreateTime desc
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

            var condition = new List<string> { " t.TransactionType = 'Recharge' " };
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
                                 left join Users p on t.PassengerId = p.Id
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
                       select  t.Id                           as TransactionId,
                               o.Name                         as OrganizationName,
                               t.CreateTime                   as TransactionTime,
                               p.PassengerId,
                               p.Name                         as PassengerName,
                               p.CardNumber                   as CardNumber,
                               'Agent'                        as RechargeMedium,
                               a.Name                         as RechargerName,
                               t.Amount
                        from Transactions t
                                 left join Users p on t.PassengerId = p.Id
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
                               count(distinct p.Id) as TotalPassenger,
                               count(t.Id) as TotalRecharge,
                               sum(t.Amount) as TotalAmount
                        from Transactions t
                                 left join Users p on t.PassengerId = p.Id
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
                                 left join Users u on s.UserId = u.Id and u.UserType in ('Staff') {whereCondition}";


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
                       case
                           when s.IsRunning = 1 then 'Running'
                           else 'Complete' end                   as Status
                from Sessions s
                         left join Buses b on s.BusId = b.Id
                         left join Organizations o on b.OrganizationId = o.Id
                         left join Users u on s.UserId = u.Id and u.UserType in ('Staff')
                         left join Routes r on b.RouteId = r.Id
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
                                 left join Users u on t.PassengerId = u.Id and U.UserType in ('Public', 'Private')
                                 left join Organizations o on u.OrganizationId = o.Id
                                 left join Sessions s on t.SessionId = s.Id
                                 left join Buses b on s.BusId = b.Id {whereCondition}";


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
                               u.CardNumber,
                               u.Name as PassengerName,
                               t.TripStartTime,
                               t.TripEndTime,
                               t.StartingLatitude,
                               t.StartingLongitude,
                               t.EndingLatitude,
                               t.EndingLongitude,
                               t.Distance,
                               t.Amount as Fare,
                               case when t.IsRunning = 1 then 'Running'
                                else 'Complete' end as Status
                        from Trips t
                                 left join Users u on t.PassengerId = u.Id and U.UserType in ('Public', 'Private')
                                 left join Organizations o on u.OrganizationId = o.Id
                                 left join Sessions s on t.SessionId = s.Id
                                 left join Buses b on s.BusId = b.Id
                                 left join Routes r on b.RouteId = r.Id
                                 {whereCondition} {extraCondition}";


        var tripDashBoardTableData = _baseRepository
            .Query<TripDashBoardTableData>(query);

        return tripDashBoardTableData; 
    }

    private TripDashboardCardData GetTripDashboardCardData(string whereCondition)
    {
        var query = $@"
                        select count(distinct t.Id) as TotalTrips,
                               count(distinct u.Id) as TotalPassengers,
                               sum(t.Amount) as TotalFare,
                               count(distinct b.Id) as TotalBus
                        from Trips t
                                 left join Users u on t.PassengerId = u.Id and U.UserType in ('Public', 'Private')
                                 left join Organizations o on u.OrganizationId = o.Id
                                 left join Sessions s on t.SessionId = s.Id
                                 left join Buses b on s.BusId = b.Id {whereCondition}";

        var tripDashBoardTableData = _baseRepository
            .Query<TripDashboardCardData>(query)
            .FirstOrDefault();

        return tripDashBoardTableData;
    }

    private DashboardCommonData GetDataForallTime(User currentUser)
    {
        var allTimeDataQuery = $@"
                                with trip_detail as (
                                    select
                                        count(distinct t.id) as TotalTrip,
                                        sum(t.Amount) as TotalRevenue
                                    from Trips t
                                    inner join Sessions s on t.SessionId = s.Id
                                    inner join Users u on s.UserId = u.Id
                                    {(currentUser.IsSuperAdmin ? string.Empty : $" where u.OrganizationId = '{currentUser.OrganizationId}'")}
                                ),
                                recharge_detail as (
                                    select
                                        count(distinct t.Id) as TotalRecharge,
                                        sum(t.Amount) as RechargeAmount
                                    from Transactions t
                                    inner join Users u on t.CreatedBy = u.Id
                                    where t.TransactionType = 'Recharge'
                                    {(currentUser.IsSuperAdmin ? string.Empty : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                )
                                select
                                    td.TotalTrip,
                                    td.TotalRevenue,
                                    rd.TotalRecharge,
                                    rd.RechargeAmount
                                from trip_detail td
                                cross join recharge_detail rd;";

        var allTimeData = _baseRepository
            .Query<DashboardCommonData>(allTimeDataQuery)
            .FirstOrDefault();

        return allTimeData;
    }

    private DashboardCommonData GetDataForThisMonth(User currentUser)
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
                                    {(currentUser.IsSuperAdmin ? string.Empty : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
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
                                    {(currentUser.IsSuperAdmin ? string.Empty : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                )
                                select
                                    td.TotalTrip,
                                    td.TotalRevenue,
                                    rd.TotalRecharge,
                                    rd.RechargeAmount
                                from trip_detail td
                                cross join recharge_detail rd;";

        var dataOfThisMonth = _baseRepository
            .Query<DashboardCommonData>(thisMonthDataQuery)
            .FirstOrDefault();

        return dataOfThisMonth;
    }

    private DashboardCommonData GetDataForToday(User currentUser)
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
                                    {(currentUser.IsSuperAdmin ? string.Empty : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                ),
                                recharge_detail as (
                                    select
                                        count(distinct t.Id) as TotalRecharge,
                                        sum(t.Amount) as RechargeAmount
                                    from Transactions t
                                    inner join Users u on t.CreatedBy = u.Id
                                    where t.TransactionType = 'Recharge' and CAST(t.CreateTime AS DATE) = CAST(GETUTCDATE() AS DATE)
                                    {(currentUser.IsSuperAdmin ? string.Empty : $" and u.OrganizationId = '{currentUser.OrganizationId}'")}
                                )
                                select
                                    td.TotalTrip,
                                    td.TotalRevenue,
                                    rd.TotalRecharge,
                                    rd.RechargeAmount
                                from trip_detail td
                                cross join recharge_detail rd;";

        var dataOfToday = _baseRepository
            .Query<DashboardCommonData>(todaysDataQuery)
            .FirstOrDefault();

        return dataOfToday;
    }
}