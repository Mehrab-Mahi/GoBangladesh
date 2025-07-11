﻿using GoBangladesh.Application.DTOs.Admin;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/admin/")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [GoBangladeshAuth]
    [HttpPost("registration")]
    public IActionResult AdminInsert([FromForm] AdminCreateRequest model)
    {
        var data = _adminService.AdminInsert(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPut("update")]
    public IActionResult UpdateAdmin([FromForm] AdminUpdateRequest model)
    {
        var data = _adminService.UpdateAdmin(model);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getById")]
    public IActionResult GetAdminById(string id)
    {
        var data = _adminService.GetAdminById(id);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("getAll")]
    public IActionResult GetAll([FromBody] AdminDataFilter filter)
    {
        var data = _adminService.GetAll(filter);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpDelete("delete")]
    public IActionResult Delete(string id)
    {
        var data = _adminService.Delete(id);
        return Ok(new { data });
    }
}