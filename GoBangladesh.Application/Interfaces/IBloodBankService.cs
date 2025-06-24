using System.Collections.Generic;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces
{
    public interface IBloodBankService
    {
        object GetBloodBankData(BloodBankFilter filter);
        DashboardDataVm GetDashboardData();
    }
}