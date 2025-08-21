namespace GoBangladesh.Application.DTOs.Account;

public class AccountUpdateRequest
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string AccountOrganizationName { get; set; }
    public string AccountType { get; set; }
    public string AccountName { get; set; }
    public string AccountNumber { get; set; }
    public string BranchName { get; set; }
    public string BranchCode { get; set; }
    public string BankCode { get; set; }
    public string RoutingNumber { get; set; }
    public string City { get; set; }
    public string District { get; set; }
    public string OrganizationId { get; set; }
}