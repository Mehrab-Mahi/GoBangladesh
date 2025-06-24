using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Services;
using GoBangladesh.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/news")]
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        [GoBangladeshAuth]
        [HttpPost("create")]
        public IActionResult Create([FromBody] NewsVm newsData)
        {
            var response = _newsService.Create(newsData);
            return Ok(new { data = response });
        }

        [GoBangladeshAuth]
        [HttpPut("update")]
        public IActionResult Update([FromBody] NewsVm newsData)
        {
            var response = _newsService.Update(newsData);
            return Ok(new { data = response });
        }

        [GoBangladeshAuth]
        [HttpDelete("delete/{id}")]
        public IActionResult Delete(string id)
        {
            var response = _newsService.Delete(id);
            return Ok(new { data = response });
        }

        [AllowAnonymous]
        [HttpGet("get/{id}")]
        public IActionResult Get(string id)
        {
            var response = _newsService.Get(id);
            return Ok(new { data = response });
        }

        [AllowAnonymous]
        [HttpGet("getall")]
        public IActionResult GetAll(int pageNo = 1, int pageSize = 10)
        {
            var response = _newsService.GetAll(pageNo, pageSize);
            return Ok(response);
        }
    }
}
