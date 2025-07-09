using GoBangladesh.Application.DTOs.Dashboard.Recharge;
using GoBangladesh.Application.DTOs.Dashboard.Session;
using GoBangladesh.Application.DTOs.Dashboard.Trip;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/dashboard/")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [GoBangladeshAuth]
    [HttpGet("GetDashboardData")]
    public IActionResult GetDashboardData()
    {
        var data = _dashboardService.GetDashboardData();
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("GetTripDashboardData")]
    public IActionResult GetTripDashboardData([FromBody] TripDashboardFilter filter)
    {
        var data = _dashboardService.GetTripDashboardData(filter);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("GetSessionDashboardData")]
    public IActionResult GetSessionDashboardData([FromBody] SessionFilter filter)
    {
        var data = _dashboardService.GetSessionDashboardData(filter);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("GetRechargeDashboardData")]
    public IActionResult GetRechargeDashboardData([FromBody] RechargeFilter filter)
    {
        var data = _dashboardService.GetRechargeDashboardData(filter);
        return Ok(new { data });
    }
}