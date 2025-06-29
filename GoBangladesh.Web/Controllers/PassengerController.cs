using GoBangladesh.Application.DTOs.Passenger;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/passenger/")]
public class PassengerController : Controller
{
    private readonly IPassengerService _passengerService;

    public PassengerController(IPassengerService passengerService)
    {
        _passengerService = passengerService;
    }

    [AllowAnonymous]
    [HttpPost("registration")]
    public IActionResult PassengerInsert([FromForm] PassengerCreateRequest model)
    {
        var data = _passengerService.PassengerInsert(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPut("update")]
    public IActionResult UpdatePassenger([FromForm] PassengerUpdateRequest model)
    {
        var data = _passengerService.UpdatePassenger(model);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPut("updateCardNumber")]
    public IActionResult UpdateCardNumber([FromBody] CardNumberUpdateRequest model)
    {
        var data = _passengerService.UpdateCardNumber(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getById")]
    public IActionResult GetPassengerById(string id)
    {
        var data = _passengerService.GetPassengerById(id);
        return Ok(new { data });
    }
}