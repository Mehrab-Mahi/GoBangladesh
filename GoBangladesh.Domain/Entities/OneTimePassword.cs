using System;

namespace GoBangladesh.Domain.Entities;

public class OneTimePassword : Entity
{
    public string MobileNumber { get; set; }
    public string Otp { get; set; }
    public bool IsValid { get; set; } = true;
    public DateTime ValidationTime { get; set; }
}