using GoBangladesh.Application.DTOs.Agent;
using GoBangladesh.Application.DTOs.Passenger;
using GoBangladesh.Application.DTOs.Staff;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPassengerService _passengerService;
        private readonly IStaffService _staffService;
        private readonly IAgentService _agentService;

        public UserController(IUserService userService, 
            IPassengerService passengerService, 
            IStaffService staffService, 
            IAgentService agentService)
        {
            _userService = userService;
            _passengerService = passengerService;
            _staffService = staffService;
            _agentService = agentService;
        }

        [AllowAnonymous]
        [HttpPost("passenger/Registration")]
        public IActionResult PassengerInsert([FromForm] PassengerCreateRequest model)
        {
            var data = _passengerService.PassengerInsert(model);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpPut("passenger/update")]
        public IActionResult UpdatePassenger([FromForm] PassengerUpdateRequest model)
        {
            var data = _passengerService.UpdatePassenger(model);
            return Ok(new { data });
        }
        
        [GoBangladeshAuth]
        [HttpGet("passenger/getById")]
        public IActionResult GetPassengerById(string id)
        {
            var data = _passengerService.GetPassengerById(id);
            return Ok(new { data });
        }

        [AllowAnonymous]
        [HttpPost("staff/Registration")]
        public IActionResult StaffInsert([FromForm] StaffCreateRequest model)
        {
            var data = _staffService.StaffInsert(model);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpPut("staff/update")]
        public IActionResult UpdateStaff([FromForm] StaffUpdateRequest model)
        {
            var data = _staffService.UpdateStaff(model);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpGet("staff/getById")]
        public IActionResult GetStaffById(string id)
        {
            var data = _staffService.GetStaffById(id);
            return Ok(new { data });
        }

        [AllowAnonymous]
        [HttpPost("agent/Registration")]
        public IActionResult AgentInsert([FromForm] AgentCreateRequest model)
        {
            var data = _agentService.AgentInsert(model);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpPut("agent/update")]
        public IActionResult UpdateAgent([FromForm] AgentUpdateRequest model)
        {
            var data = _agentService.UpdateAgent(model);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpGet("agent/getById")]
        public IActionResult GetAgentById(string id)
        {
            var data = _agentService.GetAgentById(id);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpDelete("user/delete/{id}")]
        public IActionResult DeleteUser(string id)
        {
            var response = _userService.DeleteUser(id);
            return Ok(new {data = response});
        }
        
        [GoBangladeshAuth]
        [HttpPost("user/changePassword")]
        public IActionResult ChangePassword([FromBody]ChangePassword changePassword)
        {
            var response = _userService.ChangePassword(changePassword);
            return Ok(new { data = response });
        }
    }
}