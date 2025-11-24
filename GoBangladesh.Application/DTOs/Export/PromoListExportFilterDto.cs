using System;

namespace GoBangladesh.Application.DTOs.Export;

public class PromoListExportFilterDto
{
    public string OrganizationId { get; set; }
    public string SearchQuery { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}