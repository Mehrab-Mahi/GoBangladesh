using GoBangladesh.Application.Interfaces;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using GoBangladesh.Application.ViewModels;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace GoBangladesh.Application.Services;

public class OtpService : IOtpService
{
    private readonly IRepository<OneTimePassword> _oneTimePasswordRepository;
    private readonly OtpSettings _otpSettings;

    public OtpService(IRepository<OneTimePassword> oneTimePasswordRepository,
        IOptions<OtpSettings> otpSettings)
    {
        _oneTimePasswordRepository = oneTimePasswordRepository;
        _otpSettings = otpSettings.Value;
    }

    public PayloadResponse SendOtp(string mobileNumber)
    {
        try
        {
            var otp = GenerateOtp();

            _oneTimePasswordRepository.Insert(new OneTimePassword()
            {
                Otp = otp,
                MobileNumber = mobileNumber,
                ValidationTime = DateTime.UtcNow.AddMinutes(_otpSettings.ExpireTime)
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
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Otp",
                Message = $"Otp sending has been failed because {ex.Message}, {ex.InnerException}!"
            };
        }
    }

    private void SendOtpToUser(string mobileNumber, string otp)
    {
        using var httpClient = new HttpClient();

        var smsUrl = $"{_otpSettings.BaseUrl}/sendtext?apikey={_otpSettings.ApiKey}&secretkey={_otpSettings.SecretKey}&callerID=8801847&toUser={mobileNumber}&messageContent=Your OTP is {otp}. This code is valid for the next 10 minutes. For your safety, do not disclose it to anyone.";

        var response = httpClient.GetAsync(smsUrl).Result;

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("OTP sending has been failed!");
        }
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

        if (otpData.ValidationTime < DateTime.UtcNow || !otpData.IsValid)
        {
            otpData.IsValid = false;
            _oneTimePasswordRepository.Update(otpData);
            _oneTimePasswordRepository.SaveChanges();

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
        var random = new Random();
        return random.Next(0, 1000000).ToString("D6");
    }
}