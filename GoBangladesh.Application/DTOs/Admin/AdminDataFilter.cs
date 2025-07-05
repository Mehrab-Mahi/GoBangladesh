namespace GoBangladesh.Application.DTOs.Admin;

public class AdminDataFilter
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string OrganizationId { get; set; }
    public string SearchQuery { get; set; }
}