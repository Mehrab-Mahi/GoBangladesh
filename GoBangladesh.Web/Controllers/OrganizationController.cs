using GoBangladesh.Application.DTOs.Organization;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/organization")]
public class OrganizationController : Controller
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [GoBangladeshAuth]
    [HttpPost("insert")]
    public IActionResult OrganizationInsert([FromBody] OrganizationCreateRequest model)
    {
        var data = _organizationService.OrganizationInsert(model);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPut("update")]
    public IActionResult OrganizationUpdate([FromBody] OrganizationUpdateRequest model)
    {
        var data = _organizationService.OrganizationUpdate(model);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("getById")]
    public IActionResult GetById(string id)
    {
        var data = _organizationService.GetById(id);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getAll")]
    public IActionResult GetAll(int pageNo, int pageSize)
    {
        var data = _organizationService.GetAll(pageNo, pageSize);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpDelete("delete")]
    public IActionResult Delete(string id)
    {
        var data = _organizationService.Delete(id);
        return Ok(new { data });
    }
}