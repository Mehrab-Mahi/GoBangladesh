using GoBangladesh.Application.DTOs.Contact;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/contact")]
public class ContactController : Controller
{
    private readonly IContactService _contactService;
    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [AllowAnonymous]
    [HttpPost("submit")]
    public IActionResult SubmitContactForm([FromBody] ContactFormDto contactForm)
    {
        var result = _contactService.Submit(contactForm);
        return Ok(new { result });
    }

    [GoBangladeshAuth]
    [HttpGet("getAllUnreadContacts")]
    public IActionResult GetAllUnreadContacts(int pageNo = 1, int pageSize = 10)
    {
        var result = _contactService.GetAllUnreadContacts(pageNo, pageSize);
        return Ok(new { result });
    }

    [GoBangladeshAuth]
    [HttpGet("getAllReadContacts")]
    public IActionResult GetAllReadContacts(int pageNo = 1, int pageSize = 10)
    {
        var result = _contactService.GetAllReadContacts(pageNo, pageSize);
        return Ok(new { result });
    }

    [GoBangladeshAuth]
    [HttpPost("markAsRead")]
    public IActionResult MarkAsRead([FromBody] MarkAsReadDto markAsReadData)
    {
        var result = _contactService.MarkAsRead(markAsReadData);
        return Ok(new { result });
    }

    [GoBangladeshAuth]
    [HttpGet("getAllContactCount")]
    public IActionResult GetAllContactCount()
    {
        var result = _contactService.GetAllContactCount();
        return Ok(new { result });
    }
}