using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IOtpService
{
    PayloadResponse SendOtp(string mobileNumber);
    PayloadResponse VerifyOtp(string mobileNumber, string otp);
}