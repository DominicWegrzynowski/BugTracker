using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MailKit.Security;
using Microsoft.Extensions.Configuration;

namespace BugTracker.Services
{
    public class BTEmailService : IEmailSender
    {
        #region Fields
        private readonly IConfiguration _config;
        #endregion

        #region Constructor
        public BTEmailService(IConfiguration config)
        {
            _config = config;
        }
        #endregion

        #region Send Email
        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            MimeMessage email = new();

            email.Sender = MailboxAddress.Parse(_config["Mail"]);
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;

            var builder = new BodyBuilder()
            {
                HtmlBody = htmlMessage
            };

            email.Body = builder.ToMessageBody();

            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync(_config["MailHost"], int.Parse(_config["MailPort"]), SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["Mail"], _config["MailPassword"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception)
            {
                throw;
            }
        }
    } 
    #endregion
}
