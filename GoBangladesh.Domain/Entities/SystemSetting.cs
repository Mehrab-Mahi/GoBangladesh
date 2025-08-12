namespace GoBangladesh.Domain.Entities;

public class SystemSetting : Entity
{
    public int TokenExpireTime { get; set; }
    public bool SendRealOtp { get; set; }
    public string SystemOrganizationId { get; set; }
}