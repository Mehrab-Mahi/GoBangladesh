using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IDashboardService
{
    PayloadResponse GetDashboardData();
}