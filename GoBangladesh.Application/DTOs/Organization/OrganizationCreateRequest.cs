namespace GoBangladesh.Application.DTOs.Organization;

public class OrganizationCreateRequest
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string FocalPerson { get; set; }
    public string Email { get; set; }
    public string MobileNumber { get; set; }
    public decimal PerKmFare { get; set; }
    public decimal BaseFare { get; set; }
}