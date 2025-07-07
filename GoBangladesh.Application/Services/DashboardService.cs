using System;
using System.Linq;
using GoBangladesh.Application.DTOs.Dashboard;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly IBaseRepository _baseRepository;

    public DashboardService(ILoggedInUserService loggedInUserService,
        IBaseRepository baseRepository)
    {
        _loggedInUserService = loggedInUserService;
        _baseRepository = baseRepository;
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
                                         left join Users u2 on o.Id = u2.OrganizationId and u2.UserType = 'Passenger'
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