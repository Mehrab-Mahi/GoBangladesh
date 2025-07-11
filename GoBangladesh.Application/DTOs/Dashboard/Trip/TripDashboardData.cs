using System.Collections.Generic;

namespace GoBangladesh.Application.DTOs.Dashboard.Trip;

public class TripDashboardData
{
    public TripDashboardCardData CardData { get; set; }
    public List<TripDashBoardTableData> TableData { get; set; }
    public int RowCount { get; set; }
}