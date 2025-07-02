namespace GoBangladesh.Application.DTOs.Organization;

public class OrganizationCreateRequest
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string FocalPerson { get; set; }
    public string Email { get; set; }
    public string MobileNumber { get; set; }
    public int PerKmFare { get; set; }
    public int BaseFare { get; set; }
}