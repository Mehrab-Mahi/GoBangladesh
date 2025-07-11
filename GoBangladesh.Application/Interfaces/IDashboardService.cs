using GoBangladesh.Application.DTOs.Dashboard.Recharge;
using GoBangladesh.Application.DTOs.Dashboard.Session;
using GoBangladesh.Application.DTOs.Dashboard.Trip;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IDashboardService
{
    PayloadResponse GetDashboardData();
    PayloadResponse GetTripDashboardData(TripDashboardFilter filter);
    PayloadResponse GetSessionDashboardData(SessionFilter filter);
    PayloadResponse GetRechargeDashboardData(RechargeFilter filter);
}