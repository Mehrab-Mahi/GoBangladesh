using BloodDonation.Application.Interfaces;
using BloodDonation.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonation.Web.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IUserService _userService;
        private readonly IInvitationService _inviteService;
        public string Message { get; set; }

        public RegisterController(IUserService UserService, IInvitationService inviteService)
        {
            _userService = UserService;
            _inviteService = inviteService;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Create([FromForm] UserVm model)
        {
            var inviteModel = _inviteService.GetByEmail(model.EmailAddress);
            if (inviteModel == null)
            {
                return Json(new { success = false, responseText = "Your Invitation has been deleted.Please contact with the Authority" });
            }
            model.IsApproved = false;
            model.IsSuperAdmin = false;
            model.RoleId = inviteModel.RoleId;

            //var data = _userService.Insert(model);
            inviteModel.Status = "2";
            _inviteService.Update(inviteModel);
            return Json(new { success = true, responseText = "Registration Successful.Please contact with the Admin for approval." });
        }

        public IActionResult Invitation(string email = null)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var model = _inviteService.GetByEmail(email);
                if (model != null)
                {
                    if (model.Status == "1")
                    {
                        ViewBag.Email = model.EmailAddress;
                        return View(model);
                    }
                    else
                    {
                        return RedirectToAction("Index", "AccessDenied", new { message = "You are already registered to the system.Please log in." });
                    }
                }
                else
                {
                    return RedirectToAction("Index", "AccessDenied", new { message = "Your Invitation has been deleted.Please contact with the Authority" });
                }
            }
            else
            {
                return RedirectToAction("Index", "AccessDenied");
            }
        }
    }
}