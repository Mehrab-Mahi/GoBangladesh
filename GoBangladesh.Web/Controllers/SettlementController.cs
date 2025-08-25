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
    [HttpGet("getPayableUnsettledInvoices")]
    public IActionResult GetPayableUnsettledInvoices(string organizationId, int pageNo = 1, int pageSize = 10)
    {
        var data = _settlementService.GetPayableUnsettledInvoices(organizationId, pageNo, pageSize);
        return Ok(new { data });
    }
    
    [GoBangladeshAuth]
    [HttpGet("getPayableSettledInvoices")]
    public IActionResult GetPayableSettledInvoices(string organizationId, int pageNo = 1, int pageSize = 10)
    {
        var data = _settlementService.GetPayableSettledInvoices(organizationId, pageNo, pageSize);
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
    [HttpGet("getReceivableUnsettledInvoices")]
    public IActionResult GetReceivableUnsettledInvoices(string organizationId, int pageNo = 1, int pageSize = 10)
    {
        var data = _settlementService.GetReceivableUnsettledInvoices(organizationId, pageNo, pageSize);
        return Ok(new { data });
    }

    [GoBangladeshAuth]
    [HttpGet("getReceivableSettledInvoices")]
    public IActionResult GetReceivableSettledInvoices(string organizationId, int pageNo = 1, int pageSize = 10)
    {
        var data = _settlementService.GetReceivableSettledInvoices(organizationId, pageNo, pageSize);
        return Ok(new { data });
    }
}