using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GoBangladesh.Web.Controllers
{
    public class AccountController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;

        public AccountController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //public IActionResult Login()
        //{
        //    return View();
        //}

        //public async Task<IActionResult> Logout()
        //{
        //    _httpContextAccessor.HttpContext.Session.Clear();
        //    return RedirectToAction("Login");
        //}
    }
}