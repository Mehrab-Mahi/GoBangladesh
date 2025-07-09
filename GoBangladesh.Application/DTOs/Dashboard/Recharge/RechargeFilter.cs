using System;

namespace GoBangladesh.Application.DTOs.Dashboard.Recharge;

public class RechargeFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string AgentId { get; set; }
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}