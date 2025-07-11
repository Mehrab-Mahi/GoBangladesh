﻿using GoBangladesh.Application.DTOs.Otp;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IOtpService _otpService;

        public AuthController(IAuthService authService, 
            IOtpService otpService)
        {
            _authService = authService;
            _otpService = otpService;
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public IActionResult Token([FromBody] AuthRequest model)
        {
            var response = _authService.Authenticate(model);
            return Ok(new {data = response});
        }

        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("currentuser")]
        public IActionResult GetCurrentUser()
        {
            return Ok(_authService.GetCurrentUser());
        }
    }
}