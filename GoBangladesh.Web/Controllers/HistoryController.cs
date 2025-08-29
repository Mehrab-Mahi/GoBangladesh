using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/history")]
public class HistoryController : Controller
{
    private readonly IHistoryService _historyService;

    public HistoryController(IHistoryService historyService)
    {
        _historyService = historyService;
    }

    [GoBangladeshAuth]
    [HttpGet("passenger")]
    public IActionResult PassengerHistory(string id, int pageNo, int pageSize)
    {
        var data = _historyService.PassengerHistory(id, pageNo, pageSize);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("passengerRecharge")]
    public IActionResult PassengerRechargeHistory(string id, int pageNo, int pageSize)
    {
        var data = _historyService.PassengerRechargeHistory(id, pageNo, pageSize);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("passengerTrip")]
    public IActionResult PassengerTripHistory(string id, int pageNo, int pageSize)
    {
        var data = _historyService.PassengerTripHistory(id, pageNo, pageSize);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("agent")]
    public IActionResult AgentHistory(string id, int pageNo, int pageSize)
    {
        var data = _historyService.AgentHistory(id, pageNo, pageSize);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("session")]
    public IActionResult SessionHistory(string id, int pageNo, int pageSize)
    {
        var data = _historyService.SessionHistory(id, pageNo, pageSize);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("ticketExaminerRecharge")]
    public IActionResult TicketExaminerRechargeHistory(string id, int pageNo, int pageSize)
    {
        var data = _historyService.TicketExaminerRechargeHistory(id, pageNo, pageSize);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("ticketExaminerTrip")]
    public IActionResult TicketExaminerTripHistory(string id, int pageNo, int pageSize)
    {
        var data = _historyService.TicketExaminerTripHistory(id, pageNo, pageSize);
        return Ok(new { data });
    }
}