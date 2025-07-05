using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Services;
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
}