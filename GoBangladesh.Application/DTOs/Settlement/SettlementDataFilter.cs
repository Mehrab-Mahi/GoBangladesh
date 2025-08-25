using System;

namespace GoBangladesh.Application.DTOs.Settlement;

public class SettlementDataFilter
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string FromOrganizationId { get; set; }
    public string ToOrganizationId { get; set; }
}