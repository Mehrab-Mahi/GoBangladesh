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
}