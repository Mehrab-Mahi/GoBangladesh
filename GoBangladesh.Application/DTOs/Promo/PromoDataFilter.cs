using System;

namespace GoBangladesh.Application.DTOs.Promo;

public class PromoDataFilter
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string OrganizationId { get; set; }
    public string SearchQuery { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}