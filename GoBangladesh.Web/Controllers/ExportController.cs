using GoBangladesh.Application.DTOs.Export;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers;

[Route("api/export/")]
[GoBangladeshAuth]
public class ExportController : Controller
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpPost("BusList")]
    public IActionResult ExportBusList([FromBody] BusListExportFilterDto model)
    {
        var data = _exportService.ExportBusList(model);
        return Ok(new { data });
    }

    [HttpPost("SessionHistoryData")]
    public IActionResult ExportSessionHistoryData([FromBody] SessionHistoryExportFilterDto model)
    {
        var data = _exportService.ExportSessionHistoryData(model);
        return Ok(new { data });
    }

    [HttpPost("CardList")]
    public IActionResult ExportCardList([FromBody] CardListExportFilterDto model)
    {
        var data = _exportService.ExportCardList(model);
        return Ok(new { data });
    }

    [HttpPost("PassengerList")]
    public IActionResult ExportPassengerList([FromBody] PassengerListExportFilterDto model)
    {
        var data = _exportService.ExportPassengerList(model);
        return Ok(new { data });
    }

    [HttpPost("PromoList")]
    public IActionResult ExportPromoList([FromBody] PromoListExportFilterDto model)
    {
        var data = _exportService.ExportPromoList(model);
        return Ok(new { data });
    }

    [HttpPost("CardStatement")]
    public IActionResult ExportCardStatement([FromBody] RechargeReturnHistoryExportFilterDto model)
    {
        var data = _exportService.ExportCardStatement(model);
        return Ok(new { data });
    }

    [HttpPost("TripHistoryData")]
    public IActionResult ExportTripHistoryData([FromBody] TripHistoryExportFilterDto model)
    {
        var data = _exportService.ExportTripHistoryData(model);
        return Ok(new { data });
    }

    [HttpDelete("Delete")]
    public IActionResult DeleteExportedFile(string filePath)
    {
        var data = _exportService.DeleteExportedFile(filePath);
        return Ok(new { data });
    }
}