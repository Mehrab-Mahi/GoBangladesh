using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("registration")]
        public IActionResult Create([FromForm] UserCreationVm model)
        {
            var data = _userService.Insert(model);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpPut("update")]
        public IActionResult Update([FromForm] UserCreationVm model)
        {
            var response = _userService.Update(model);
            return Ok(new {data = response});
        }
        
        [GoBangladeshAuth]
        [HttpPost("approvevolunteer")]
        public IActionResult ApproveUser([FromBody] UserApproval userApproval)
        {
            var data = _userService.ApproveUser(userApproval.Id);
            return Ok(new { data });
        }
        
        [GoBangladeshAuth]
        [HttpPost("disapprovevolunteer")]
        public IActionResult DisapproveUser([FromBody] UserApproval userApproval)
        {
            var data = _userService.DisapproveUser(userApproval.Id);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpGet("getUnapprovedVolunteer")]
        public IActionResult GetUnapprovedVolunteer(int pageNo = 1, int pageSize = 10)
        {
            var data = _userService.GetUnapprovedVolunteer(pageNo, pageSize);
            return Ok(data);
        }
        
        [GoBangladeshAuth]
        [HttpGet("getApprovedVolunteer")]
        public IActionResult GetAllApprovedVolunteer(int pageNo = 1, int pageSize = 10)
        {
            var data = _userService.GetApprovedVolunteer(pageNo, pageSize);
            return Ok(data);
        }

        [GoBangladeshAuth]
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteUser(string id)
        {
            var response = _userService.DeleteUser(id);
            return Ok(new {data = response});
        }


        [GoBangladeshAuth]
        [HttpPost("getall")]
        public IActionResult GetAll([FromBody] UserFilter userFilter)
        {
            var data = _userService.GetAll(userFilter);
            return Ok(data);
        }
        
        [GoBangladeshAuth]
        [HttpPost("getApprovedDonor")]
        public IActionResult GetApprovedDonor([FromBody] DonorFilter donorFilter)
        {
            var data = _userService.GetApprovedDonor(donorFilter);
            return Ok(data);
        }
        
        [GoBangladeshAuth]
        [HttpPost("getUnapprovedDonor")]
        public IActionResult GetUnapprovedDonor([FromBody] DonorFilter donorFilter)
        {
            var data = _userService.GetUnapprovedDonor(donorFilter);
            return Ok(data);
        }
        
        [GoBangladeshAuth]
        [HttpGet("getAllAdmin")]
        public IActionResult GetAllAdmin(int pageNo = 1, int pageSize = 10)
        {
            var data = _userService.GetAllAdmin(pageNo, pageSize);
            return Ok(data);
        }
        
        [GoBangladeshAuth]
        [HttpGet("getPermittedDonors")]
        public IActionResult GetPermittedDonors(int pageNo = 1, int pageSize = 10)
        {
            var data = _userService.GetPermittedDonors(pageNo, pageSize);
            return Ok(data);
        }
        
        [AllowAnonymous]
        [HttpGet("getOfficialLeaders")]
        public IActionResult GetOfficialLeaders()
        {
            var data = _userService.GetOfficialLeaders();
            return Ok(data);
        }
        
        [AllowAnonymous]
        [HttpGet("getScoutLeaders")]
        public IActionResult GetScoutLeaders(int pageNo = 1, int pageSize = 10)
        {
            var data = _userService.GetScoutLeaders(pageNo, pageSize);
            return Ok(data);
        }

        [GoBangladeshAuth]
        [HttpPost("donorRegistration")]
        public IActionResult DonorRegistration([FromForm] UserCreationVm model)
        {
            var data = _userService.Insert(model);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpGet("getbyid/{id}")]
        public IActionResult GetById(string id)
        {
            var userData = _userService.GetById(id);
            return Ok(new { data = userData });
        }
        
        [GoBangladeshAuth]
        [HttpPost("changePassword")]
        public IActionResult ChangePassword([FromBody]ChangePassword changePassword)
        {
            var response = _userService.ChangePassword(changePassword);
            return Ok(new { data = response });
        }
    }
}