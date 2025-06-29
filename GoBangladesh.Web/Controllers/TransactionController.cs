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
        [HttpPost("busFare")]
        public IActionResult BusFare([FromBody] BusFareRequest model)
        {
            var data = _transactionService.BusFare(model);
            return Ok(new { data });
        }
    }
}
