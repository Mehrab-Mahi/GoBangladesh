using BloodDonation.Application.Helper;
using BloodDonation.Application.Interfaces;
using BloodDonation.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonation.Web.Controllers
{
    [BloodDonationAuth]
    public class InviteController : Controller
    {
        private readonly IInvitationService _inviteService;

        public InviteController(IInvitationService inviteService)
        {
            _inviteService = inviteService;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet]
        public IActionResult GetAll()
        {
            var data = _inviteService.GetAll();
            return Json(data);
        }

        [HttpPost]
        public IActionResult Send([FromBody] InviteUserVm model)
        {
            var url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host.Value, "/");
            model.PublicRegisterUrl = url;

            var data = _inviteService.Invite(model);
            return Json(new { isSuccess = data });
        }

        public IActionResult Approve(string id)
        {
            var data = _inviteService.Approve(id);
            return Json(new { isSuccess = data });
        }
    }
}