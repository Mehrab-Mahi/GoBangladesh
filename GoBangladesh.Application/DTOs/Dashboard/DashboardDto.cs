namespace GoBangladesh.Application.DTOs.Dashboard;

public class DashboardDto
{
    public int TotalOrganization { get; set; }
    public int TotalBus { get; set; }
    public int TotalStaff { get; set; }
    public int TotalAgent { get; set; }
    public int TotalPassenger { get; set; }
    public DashboardCommonData DataOfToday { get; set; }
    public DashboardCommonData DataOfThisMonth { get; set; }
    public DashboardCommonData DataOfAllTime { get; set; }
}

public class DashboardCommonData
{
    public int TotalTrip { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalRecharge { get; set; }
    public decimal RechargeAmount{ get; set; }
}