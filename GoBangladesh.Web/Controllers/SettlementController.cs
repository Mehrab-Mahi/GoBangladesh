using GoBangladesh.Application.DTOs.Settlement;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/settlement")]
public class SettlementController : Controller
{
    private readonly ISettlementService _settlementService;
    public SettlementController(ISettlementService settlementService)
    {
        _settlementService = settlementService;
    }

    [GoBangladeshAuth]
    [HttpPost("getPayableSummaryData")]
    public IActionResult GetSettlementPayableSummaryData([FromBody] SettlementDataFilter filter)
    {
        var data = _settlementService.GetSettlementPayableSummaryData(filter);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("getReceivableSummaryData")]
    public IActionResult GetSettlementReceivableSummaryData([FromBody] SettlementDataFilter filter)
    {
        var data = _settlementService.GetSettlementReceivableSummaryData(filter);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("getDetailData")]
    public IActionResult GetSettlementDetailData([FromBody] SettlementDataFilter filter)
    {
        var data = _settlementService.GetSettlementDetailData(filter);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("getPayableUnsettledInvoices")]
    public IActionResult GetPayableUnsettledInvoices([FromBody] SettlementFilter filter)
    {
        var data = _settlementService.GetPayableUnsettledInvoices(filter);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("getPayableInReviewInvoices")]
    public IActionResult GetPayableInReviewInvoices([FromBody] SettlementFilter filter)
    {
        var data = _settlementService.GetPayableInReviewInvoices(filter);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("getPayableSettledInvoices")]
    public IActionResult GetPayableSettledInvoices([FromBody] SettlementFilter filter)
    {
        var data = _settlementService.GetPayableSettledInvoices(filter);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("getReceivableUnsettledInvoices")]
    public IActionResult GetReceivableUnsettledInvoices([FromBody] SettlementFilter filter)
    {
        var data = _settlementService.GetReceivableUnsettledInvoices(filter);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("getReceivableInReviewInvoices")]
    public IActionResult GetReceivableInReviewInvoices([FromBody] SettlementFilter filter)
    {
        var data = _settlementService.GetReceivableInReviewInvoices(filter);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("getReceivableSettledInvoices")]
    public IActionResult GetReceivableSettledInvoices([FromBody] SettlementFilter filter)
    {
        var data = _settlementService.GetReceivableSettledInvoices(filter);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getInvoiceWiseTransactions")]
    public IActionResult GetInvoiceWiseTransactions(string invoiceNumber, int pageNo = 1, int pageSize = 10)
    {
        var data = _settlementService.GetInvoiceWiseTransactions(invoiceNumber, pageNo, pageSize);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpPost("payment")]
    public IActionResult Payment([FromForm] InvoiceWisePaymentDto payment)
    {
        var data = _settlementService.Payment(payment);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpPost("verifyPayment")]
    public IActionResult VerifyPayment([FromBody] PaymentVerificationDto verification)
    {
        var data = _settlementService.VerifyPayment(verification);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("getInvoiceWisePayments")]
    public IActionResult GetInvoiceWisePayments(string invoiceNumber)
    {
        var data = _settlementService.GetInvoiceWisePayments(invoiceNumber);
        return Ok(new { data });
    }
}