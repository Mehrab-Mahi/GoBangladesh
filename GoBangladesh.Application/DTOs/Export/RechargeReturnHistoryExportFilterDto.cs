using System;

namespace GoBangladesh.Application.DTOs.Export;

public class RechargeReturnHistoryExportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string AgentId { get; set; }
    public string OrganizationId { get; set; }
}