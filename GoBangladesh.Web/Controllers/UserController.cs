using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
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