namespace GoBangladesh.Application.DTOs.Organization;

public class OrganizationUpdateRequest
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string FocalPerson { get; set; }
    public string Email { get; set; }
    public string Designation { get; set; }
    public string MobileNumber { get; set; }
    public string OrganizationType { get; set; }
}