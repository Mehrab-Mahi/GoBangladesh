using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class Account : Entity
{
    public string Type { get; set; }
    public string AccountOrganizationName { get; set; }
    public string AccountType { get; set; }
    public string AccountName { get; set; }
    public string AccountNumber { get; set; }
    public string BranchName { get; set; }
    public string BranchCode { get; set; }
    public string RoutingNumber { get; set; }
    public string District { get; set; }
    public bool IsActive { get; set; }= true;
    public string OrganizationId { get; set; }
    [ForeignKey("OrganizationId")]
    public Organization Organization { get; set; }
}