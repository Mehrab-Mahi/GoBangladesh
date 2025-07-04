using GoBangladesh.Application.DTOs.Transaction;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/transaction/")]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [GoBangladeshAuth]
        [HttpPost("recharge")]
        public IActionResult Recharge([FromBody] RechargeRequest model)
        {
            var data = _transactionService.Recharge(model);
            return Ok(new { data });
        }
        
        [GoBangladeshAuth]
        [HttpPost("Tap")]
        public IActionResult Tap([FromBody] TapRequest tapRequest)
        {
            var data = _transactionService.Tap(tapRequest);
            return Ok(new { data });
        }
    }
}
