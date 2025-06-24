using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/bloodbank")]
    public class BloodBankController : Controller
    {
        private readonly IBloodBankService _bloodBankService;

        public BloodBankController(IBloodBankService bloodBankService)
        {
            _bloodBankService = bloodBankService;
        }

        [AllowAnonymous]
        [HttpPost("getbloodbankdata")]
        public IActionResult GetBloodBankData([FromBody]BloodBankFilter filter)
        {
            var data = _bloodBankService.GetBloodBankData(filter);
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpGet("getdashboarddata")]
        public IActionResult GetDashboardData()
        {
            var data = _bloodBankService.GetDashboardData();
            return Ok(new {data});
        }
    }
}
