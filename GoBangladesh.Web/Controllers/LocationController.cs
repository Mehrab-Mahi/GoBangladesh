using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/location")]
    public class LocationController : Controller
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [AllowAnonymous]
        [HttpGet("GetByParentId/{parentId}")]
        public IActionResult GetLocationByParentId(string parentId)
        {
            var locations = _locationService.GetLocationByParentId(parentId);

            return Ok(new {data = locations});
        }

        [GoBangladeshAuth]
        [HttpPost("create")]
        public IActionResult Create([FromBody] LocationVm locationData)
        {
            var locations = _locationService.Create(locationData);

            return Ok(new {data = locations});
        }
        
        [GoBangladeshAuth]
        [HttpPut("update")]
        public IActionResult Update([FromBody] LocationVm locationData)
        {
            var locations = _locationService.Update(locationData);

            return Ok(new {data = locations});
        }

        [GoBangladeshAuth]
        [HttpDelete("delete/{id}")]
        public IActionResult Delete(string id)
        {
            var locations = _locationService.Delete(id);

            return Ok(new {data = locations});
        }
        
        [AllowAnonymous]
        [HttpGet("get/{id}")]
        public IActionResult Get(string id)
        {
            var locations = _locationService.Get(id);

            return Ok(new {data = locations});
        }
    }
}
