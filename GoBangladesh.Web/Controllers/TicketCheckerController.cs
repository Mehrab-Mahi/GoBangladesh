using GoBangladesh.Application.DTOs.Staff;
using GoBangladesh.Application.DTOs.TicketChecker;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/ticketChecker")]
public class TicketCheckerController : Controller
{
    private readonly ITicketCheckerService _ticketCheckerService;

    public TicketCheckerController(ITicketCheckerService ticketCheckerService)
    {
        _ticketCheckerService = ticketCheckerService;
    }

    [GoBangladeshAuth]
    [HttpPost("insert")]
    public IActionResult Insert([FromForm] TicketCheckerCreateRequest model)
    {
        var data = _ticketCheckerService.TicketCheckerCreate(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPut("update")]
    public IActionResult Update([FromForm] TicketCheckerUpdateRequest model)
    {
        var data = _ticketCheckerService.TicketCheckerUpdate(model);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("getById")]
    public IActionResult GetById(string id)
    {
        var data = _ticketCheckerService.GetById(id);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("getAll")]
    public IActionResult GetAll([FromBody] TicketCheckerDataFilter filter)
    {
        var data = _ticketCheckerService.GetAll(filter);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpDelete("delete")]
    public IActionResult Delete(string id)
    {
        var data = _ticketCheckerService.Delete(id);
        return Ok(new { data });
    }
}