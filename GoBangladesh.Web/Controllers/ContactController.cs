using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/contact")]
    public class ContactController : Controller
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [GoBangladeshAuth]
        [HttpPost("create")]
        public IActionResult Create([FromBody] Contact contactData)
        {
            var response = _contactService.Create(contactData);
            return Ok(new {data = response});
        }
        
        [GoBangladeshAuth]
        [HttpGet("getall")]
        public IActionResult GetAll(string contactType, int pageNo = 1, int pageSize = 10)
        {
            var response = _contactService.GetAll(contactType, pageNo, pageSize);
            return Ok(response);
        }
        
        [GoBangladeshAuth]
        [HttpPost("read")]
        public IActionResult Read([FromBody] NoticeReadVm noticeReadData)
        {
            var response = _contactService.ReadContact(noticeReadData.Id);
            return Ok(new {data = response});
        }

        [GoBangladeshAuth]
        [HttpGet("get")]
        public IActionResult Get(string id)
        {
            var response = _contactService.Get(id);
            return Ok(new {data = response});
        }
    }
}
