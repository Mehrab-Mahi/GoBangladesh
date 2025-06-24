using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/notice")]
    public class NoticeController : Controller
    {
        private readonly INoticeService _noticeService;

        public NoticeController(INoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        [GoBangladeshAuth]
        [HttpPost("create")]
        public IActionResult Create([FromForm] NoticeVm noticeData)
        {
            var response = _noticeService.Create(noticeData);
            return Ok(new {data = response});
        }

        [GoBangladeshAuth]
        [HttpPut("update")]
        public IActionResult Update([FromForm] NoticeVm noticeData)
        {
            var response = _noticeService.Update(noticeData);
            return Ok(new {data = response});
        }

        [GoBangladeshAuth]
        [HttpDelete("delete/{id}")]
        public IActionResult Delete(string id)
        {
            var response = _noticeService.Delete(id);
            return Ok(new {data = response});
        }

        [AllowAnonymous]
        [HttpGet("get/{id}")]
        public IActionResult Get(string id)
        {
            var response = _noticeService.Get(id);
            return Ok(new {data = response});
        }

        [AllowAnonymous]
        [HttpGet("getall")]
        public IActionResult GetAll(int pageNo = 1, int pageSize = 10)
        {
            var response = _noticeService.GetAll(pageNo, pageSize);
            return Ok(response);
        }
    }
}
