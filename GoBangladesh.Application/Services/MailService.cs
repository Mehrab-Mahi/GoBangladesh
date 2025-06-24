using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GoBangladesh.Application.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public async Task SendEmailAsync(string content, string emailAddress)
        {
            var email = new MailMessage
            {
                From = new MailAddress(_mailSettings.Mail)
            };

            email.To.Add(emailAddress);
            var ccList = _mailSettings.CcAddress.Split(',');
            for (int i = 0; i < ccList.Length; i++)
            {
                email.CC.Add(ccList[i]);
            }


            email.Subject = "Invitioan on JazeCore Web Dashboard";

            email.Body = content;
            email.IsBodyHtml = true;

            using var smtp = new SmtpClient
            {
                Host = _mailSettings.Host,
                Port = _mailSettings.Port
            };

            NetworkCredential Credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password);
            smtp.Credentials = Credentials;

            await smtp.SendMailAsync(email);
            smtp.Dispose();
        }
    }
}
