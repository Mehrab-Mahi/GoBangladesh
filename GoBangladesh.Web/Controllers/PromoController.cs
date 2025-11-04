using GoBangladesh.Application.DTOs.Promo;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/promo")]
public class PromoController : Controller
{
    private readonly IPromoService _promoService;

    public PromoController(IPromoService promoService)
    {
        _promoService = promoService;
    }

    [GoBangladeshAuth]
    [HttpPost("insert")]
    public IActionResult PromoInsert([FromBody] PromoCreationRequest model)
    {
        var data = _promoService.PromoInsert(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getUserPromo")]
    public IActionResult GetUserPromo(string status, int pageNo = 1, int pageSize = 10)
    {
        var data = _promoService.GetUserPromo(status, pageNo, pageSize);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("applyPromo")]
    public IActionResult ApplyPromo([FromBody] ApplyPromoRequest applyPromoRequest)
    {
        var data = _promoService.ApplyPromo(applyPromoRequest);
        return Ok(new { data });
    } 
    
    [GoBangladeshAuth]
    [HttpPost("getAll")]
    public IActionResult GetAll([FromBody] PromoDataFilter filter)
    {
        var data = _promoService.GetAll(filter);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getById")]
    public IActionResult GetById(string id)
    {
        var data = _promoService.GetById(id); 
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getUsedCardByPromoId")]
    public IActionResult GetUsedCardByPromoId(string id, int pageNo = 1, int pageSize = 10)
    {
        var data = _promoService.GetUsedCardByPromoId(id, pageNo, pageSize);
        return Ok(new { data });
    }
}