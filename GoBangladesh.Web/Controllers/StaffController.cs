using GoBangladesh.Application.DTOs.Staff;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/staff/")]
    public class StaffController : Controller
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [AllowAnonymous]
        [HttpPost("registration")]
        public IActionResult StaffInsert([FromForm] StaffCreateRequest model)
        {
            var data = _staffService.StaffInsert(model);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpPut("update")]
        public IActionResult UpdateStaff([FromForm] StaffUpdateRequest model)
        {
            var data = _staffService.UpdateStaff(model);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpGet("getById")]
        public IActionResult GetStaffById(string id)
        {
            var data = _staffService.GetStaffById(id);
            return Ok(new { data });
        }
        
        [GoBangladeshAuth]
        [HttpPost("mapStaffWithBus")]
        public IActionResult MapStaffWithBus([FromBody] StaffBusMappingDto staffBusMapping)
        {
            var data = _staffService.MapStaffWithBus(staffBusMapping);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpGet("getAll")]
        public IActionResult GetAll(int pageNo, int pageSize)
        {
            var data = _staffService.GetAll(pageNo, pageSize);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpDelete("delete")]
        public IActionResult Delete(string id)
        {
            var data = _staffService.Delete(id);
            return Ok(new { data });
        }
    }
}