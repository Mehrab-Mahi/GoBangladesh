using System;

namespace GoBangladesh.Application.DTOs.Dashboard.Session;

public class SessionFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string BusId { get; set; }
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}