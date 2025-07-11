using System.Collections.Generic;

namespace GoBangladesh.Application.DTOs.Dashboard.Recharge;

public class RechargeDashboardData
{
    public RechargeDashboardCardData CardData { get; set; }
    public List<RechargeDashboardTableData> TableData { get; set; }
    public int RowCount { get; set; }
}