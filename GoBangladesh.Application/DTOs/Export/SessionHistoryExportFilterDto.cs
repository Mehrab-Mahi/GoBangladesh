using System;

namespace GoBangladesh.Application.DTOs.Export;

public class SessionHistoryExportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string BusId { get; set; }
    public string RouteId { get; set; }
    public string OrganizationId { get; set; }
}