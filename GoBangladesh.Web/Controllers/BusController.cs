using GoBangladesh.Application.DTOs.Bus;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/bus")]
public class BusController : Controller
{
    private readonly IBusService _busService;

    public BusController(IBusService busService)
    {
        _busService = busService;
    }

    [GoBangladeshAuth]
    [HttpPost("insert")]
    public IActionResult BusInsert([FromBody] BusCreateRequest model)
    {
        var data = _busService.BusInsert(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPut("update")]
    public IActionResult BusUpdate([FromBody] BusUpdateRequest model)
    {
        var data = _busService.BusUpdate(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getById")]
    public IActionResult GetById(string id)
    {
        var data = _busService.GetById(id);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("getAll")]
    public IActionResult GetAll([FromBody] BusDataFilter filter)
    {
        var data = _busService.GetAll(filter);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpDelete("delete")]
    public IActionResult Delete(string id)
    {
        var data = _busService.Delete(id);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("UpdateLocation")]
    public IActionResult UpdateLocation([FromBody] LocationUpdateDto locationData)
    {
        var data = _busService.UpdateLocation(locationData);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getAllForDropDown")]
    public IActionResult GetAllForDropDown(string organizationId, string routeId)
    {
        var data = _busService.GetAllForDropDown(organizationId, routeId);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("getAllBusMapData")]
    public IActionResult GetAllBusMapData(string organizationId, string busId, string routeId)
    {
        var data = _busService.GetAllBusMapData(organizationId, busId, routeId);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("getAllRunningBus")]
    public IActionResult GetAllRunningBus()
    {
        var data = _busService.GetAllRunningBus();
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("activate")]
    public IActionResult ActivateBus([FromBody] BusActivationDto busActivation)
    {
        var data = _busService.ActivateBus(busActivation);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("deactivate")]
    public IActionResult DeactivateBus([FromBody] BusActivationDto busActivation)
    {
        var data = _busService.DeactivateBus(busActivation);
        return Ok(new { data });
    }
}