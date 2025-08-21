using GoBangladesh.Application.DTOs.Bus;
using GoBangladesh.Application.DTOs.Route;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/route")]
public class RouteController : Controller
{
    private readonly IRouteService _routeService;

    public RouteController(IRouteService routeService)
    {
        _routeService = routeService;
    }

    [GoBangladeshAuth]
    [HttpPost("insert")]
    public IActionResult RouteInsert([FromBody] RouteCreateRequest model)
    {
        var data = _routeService.RouteInsert(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPut("update")]
    public IActionResult RouteUpdate([FromBody] RouteUpdateRequest model)
    {
        var data = _routeService.RouteUpdate(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getById")]
    public IActionResult GetById(string id)
    {
        var data = _routeService.GetById(id);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("getAll")]
    public IActionResult GetAll([FromBody] RouteDataFilter filter)
    {
        var data = _routeService.GetAll(filter);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpDelete("delete")]
    public IActionResult Delete(string id)
    {
        var data = _routeService.Delete(id);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("routeDropdown")]
    public IActionResult GetRouteDropdown(string organizationId)
    {
        var data = _routeService.GetRouteDropdown(organizationId);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("routeDropdownForMobile")]
    public IActionResult RouteDropdownForMobile(string organizationId)
    {
        var data = _routeService.RouteDropdownForMobile(organizationId);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("activate")]
    public IActionResult ActivateRoute([FromBody] RouteActivationDto routeActivation)
    {
        var data = _routeService.ActivateRoute(routeActivation);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("deactivate")]
    public IActionResult DeactivateRoute([FromBody] RouteActivationDto routeActivation)
    {
        var data = _routeService.DeactivateRoute(routeActivation);
        return Ok(new { data });
    }
}