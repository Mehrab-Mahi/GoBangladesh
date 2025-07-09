using System.Collections.Generic;

namespace GoBangladesh.Application.DTOs.Dashboard.Session;

public class SessionDashboardData
{
    public SessionDashboardCardData CardData { get; set; }
    public List<SessionDashboardTableData> TableData { get; set; }
    public int RowCount { get; set; }
}