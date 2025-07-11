using System;

namespace GoBangladesh.Application.DTOs.Dashboard.Trip;

public class TripDashboardFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string BusId { get; set; }
    public string OrganizationId { get; set; }
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}