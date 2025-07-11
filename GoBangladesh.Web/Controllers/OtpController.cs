using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/otp")]
public class OtpController : Controller
{
    private readonly IOtpService _otpService;

    public OtpController(IOtpService otpService)
    {
        _otpService = otpService;
    }

    [AllowAnonymous]
    [HttpGet("SendOtp")]
    public IActionResult SendOtp(string mobileNumber)
    {
        var response = _otpService.SendOtp(mobileNumber);
        return Ok(new { data = response });
    }

    [AllowAnonymous]
    [HttpGet("VerifyOtp")]
    public IActionResult VerifyOtp(string mobileNumber, string otp)
    {
        var response = _otpService.VerifyOtp(mobileNumber, otp);
        return Ok(new { data = response });
    }
}