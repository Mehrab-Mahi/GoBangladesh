using GoBangladesh.Application.DTOs.Session;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/session/")]
public class SessionController : Controller
{
    private readonly ISessionService _sessionService;

    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [GoBangladeshAuth]
    [HttpPost("StartSession")]
    public IActionResult StartSession([FromBody] SessionStartDto sessionStartDto)
    {
        var data = _sessionService.StartSession(sessionStartDto);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("StopSession")]
    public IActionResult StopSession([FromBody] SessionStopDto sessionStopDto)
    {
        var data = _sessionService.StopSession(sessionStopDto);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("GetSessionStatistics")]
    public IActionResult GetSessionStatistics(string sessionId)
    {
        var data = _sessionService.GetSessionStatistics(sessionId);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("ForceStopSession")]
    public IActionResult ForceStopSession([FromBody] SessionStopDto sessionStopDto)
    {
        var data = _sessionService.StopSession(sessionStopDto);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("CheckIfSessionRunning")]
    public IActionResult CheckIfSessionRunning(string sessionId)
    {
        var data = _sessionService.CheckIfSessionRunning(sessionId);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getStatistics")]
    public IActionResult GetStatistics(string sessionId)
    {
        var data = _sessionService.GetStatistics(sessionId);
        return Ok(new { data });
    }
}