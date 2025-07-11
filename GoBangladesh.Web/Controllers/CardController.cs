using GoBangladesh.Application.DTOs.Card;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/card")]
public class CardController : Controller
{
    private readonly ICardService _cardService;

    public CardController(ICardService cardService)
    {
        _cardService = cardService;
    }

    [GoBangladeshAuth]
    [HttpPost("insert")]
    public IActionResult CardInsert([FromBody] CardCreateRequest model)
    {
        var data = _cardService.CardInsert(model);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("update")]
    public IActionResult CardUpdate([FromBody] CardUpdateRequest model)
    {
        var data = _cardService.CardUpdate(model);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("getById")]
    public IActionResult GetById(string id)
    {
        var data = _cardService.GetById(id);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpDelete("delete")]
    public IActionResult Delete(string id)
    {
        var data = _cardService.Delete(id);
        return Ok(new { data });
    }

    [AllowAnonymous]
    [HttpGet("CheckCardValidity")]
    public IActionResult CheckCardValidity(string cardNumber)
    {
        var data = _cardService.CheckCardValidity(cardNumber);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("CheckCardAvailability")]
    public IActionResult CheckCardAvailability(string cardNumber)
    {
        var data = _cardService.CheckCardAvailability(cardNumber);
        return Ok(new { data });
    }
}