using GoBangladesh.Application.Interfaces;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using GoBangladesh.Application.ViewModels;
using Microsoft.Extensions.Options;

namespace GoBangladesh.Application.Services;

public class OtpService : IOtpService
{
    private readonly IRepository<OneTimePassword> _oneTimePasswordRepository;
    private readonly OtpSettings _otpSettings;
    private readonly IRepository<User> _userRepository;

    public OtpService(IRepository<OneTimePassword> oneTimePasswordRepository,
        IOptions<OtpSettings> otpSettings,
        IRepository<User> userRepository)
    {
        _oneTimePasswordRepository = oneTimePasswordRepository;
        _userRepository = userRepository;
        _otpSettings = otpSettings.Value;
    }

    public PayloadResponse SendOtp(string mobileNumber)
    {
        var user = _userRepository
            .GetConditional(u => u.MobileNumber == mobileNumber);

        if (user == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Otp",
                Message = "User with this mobile number is not found!"
            };
        }

        var otp = GenerateOtp();

        _oneTimePasswordRepository.Insert(new OneTimePassword()
        {
            Otp = otp,
            MobileNumber = mobileNumber,
            ValidationTime = DateTime.Now.AddMinutes(_otpSettings.ExpireTime)
        });
        _oneTimePasswordRepository.SaveChanges();

        SendOtpToUser(mobileNumber, otp);

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Otp",
            Message = "Otp has been sent!"
        };
    }

    private void SendOtpToUser(string mobileNumber, string otp)
    {

    }

    public PayloadResponse VerifyOtp(string mobileNumber, string otp)
    {
        var otpData = _oneTimePasswordRepository
            .GetConditional(o => o.MobileNumber == mobileNumber &&
                                 o.Otp == otp && o.IsValid);

        if (otpData is null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Invalid otp!"
            };
        }

        if (otpData.ValidationTime < DateTime.Now || !otpData.IsValid)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Otp has been expired!"
            };
        }

        otpData.IsValid = false;

        _oneTimePasswordRepository.Update(otpData);
        _oneTimePasswordRepository.SaveChanges();

        return new PayloadResponse()
        {
            IsSuccess = true,
            Message = "Otp has been matched"
        };
    }

    public string GenerateOtp()
    {
        //var random = new Random();
        //return random.Next(0, 1000000).ToString("D6");
        return "123456";
    }
}