using GoBangladesh.Application.DTOs.Card;
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

    [AllowAnonymous]
    [HttpPost("insert")]
    public IActionResult CardInsert([FromBody] CardCreateRequest model)
    {
        var data = _cardService.CardInsert(model);
        return Ok(new { data });
    }
    
    [AllowAnonymous]
    [HttpGet("CheckCardValidity")]
    public IActionResult CheckCardValidity(string cardNumber)
    {
        var data = _cardService.CheckCardValidity(cardNumber);
        return Ok(new { data });
    }
}