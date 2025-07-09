using System;

namespace GoBangladesh.Application.DTOs.Dashboard.Recharge;

public class RechargeDashboardTableData
{
    public string TransactionId { get; set; }
    public string OrganizationName { get; set; }
    public DateTime TransactionTime { get; set; }
    public string PassengerId { get; set; }
    public string PassengerName { get; set; }
    public string RechargeMedium { get; set; }
    public string RechargerName { get; set; }
    public decimal Amount { get; set; }
}