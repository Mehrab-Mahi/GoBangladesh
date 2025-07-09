using System;

namespace GoBangladesh.Application.DTOs.Dashboard.Session;

public class SessionDashboardTableData
{
    public string SessionId { get; set; }
    public string SessionCode { get; set; }
    public string OrganizationName { get; set; }
    public string BusNumber { get; set; }
    public string BusName { get; set; }
    public string Route { get; set; }
    public string StaffName { get; set; }
    public string MobileNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; }
}