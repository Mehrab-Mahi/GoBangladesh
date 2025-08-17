using GoBangladesh.Application.DTOs.Account;
using GoBangladesh.Application.DTOs.Organization;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/account")]
public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [GoBangladeshAuth]
    [HttpPost("insert")]
    public IActionResult Insert([FromBody] AccountCreateRequest model)
    {
        var data = _accountService.Insert(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("update")]
    public IActionResult Update([FromBody] AccountUpdateRequest model)
    {
        var data = _accountService.Update(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getById")]
    public IActionResult GetById(string id)
    {
        var data = _accountService.GetById(id);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("getAll")]
    public IActionResult GetAll([FromBody] AccountDataFilter filter)
    {
        var data = _accountService.GetAll(filter);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpDelete("delete")]
    public IActionResult Delete(string id)
    {
        var data = _accountService.Delete(id);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("activate")]
    public IActionResult Activate([FromBody] AccountActivationDto accountActivation)
    {
        var data = _accountService.Activate(accountActivation);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("deactivate")]
    public IActionResult Deactivate([FromBody] AccountActivationDto accountActivation)
    {
        var data = _accountService.Deactivate(accountActivation);
        return Ok(new { data });
    }
}