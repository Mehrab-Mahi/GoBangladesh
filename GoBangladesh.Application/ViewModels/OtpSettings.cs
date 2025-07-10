namespace GoBangladesh.Application.ViewModels;

public class OtpSettings
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public string SecretKey { get; set; }
    public int ExpireTime { get; set; }
}