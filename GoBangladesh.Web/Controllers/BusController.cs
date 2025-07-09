using GoBangladesh.Application.DTOs.Bus;
using GoBangladesh.Application.DTOs.Organization;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Services;
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
    public IActionResult GetAllForDropDown()
    {
        var data = _busService.GetAllForDropDown();
        return Ok(new { data });
    }
}