using GoBangladesh.Application.DTOs;
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

        //[GoBangladeshAuth]
        //[HttpDelete("delete/{id}")]
        //public IActionResult DeleteUser(string id)
        //{
        //    var response = _userService.DeleteUser(id);
        //    return Ok(new {data = response});
        //}
        
        [GoBangladeshAuth]
        [HttpPost("changePassword")]
        public IActionResult ChangePassword([FromBody]ChangePassword changePassword)
        {
            var response = _userService.ChangePassword(changePassword);
            return Ok(new { data = response });
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public IActionResult ForgotPassword([FromBody] ForgotPassword forgotPassword)
        {
            var response = _userService.ForgotPassword(forgotPassword);
            return Ok(new { data = response });
        }
        
        [GoBangladeshAuth]
        [HttpPost("DeleteUserImage")]
        public IActionResult DeleteUserImage([FromBody] DeleteFileByUrl fileUrl)
        {
            var response = _userService.DeleteUserImage(fileUrl);
            return Ok(new { data = response });
        }
        
        [GoBangladeshAuth]
        [HttpPost("DeactivateAccount")]
        public IActionResult DeactivateAccount([FromBody] UserAccountActivationDto model)
        {
            var response = _userService.DeactivateAccount(model);
            return Ok(new { data = response });
        }
        
        [GoBangladeshAuth]
        [HttpPost("ActivateAccount")]
        public IActionResult ActivateAccount([FromBody] UserAccountActivationDto model)
        {
            var response = _userService.ActivateAccount(model);
            return Ok(new { data = response });
        }
    }
}