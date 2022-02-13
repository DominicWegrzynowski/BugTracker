using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using BugTracker.Models;
using MimeKit;
using System.Net.Mail;
using MailKit.Security;
using MailKit.Net.Smtp;


namespace BugTracker.Services
{
    public class BTEmailService : IEmailSender
    {
        #region Fields
        private readonly MailSettings _mailSettings;
        #endregion

        #region Constructor
        public BTEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        #endregion

        #region Send Email
        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            MimeMessage email = new();

            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
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
                smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

                await smtp.SendAsync(email);

                smtp.Disconnect(true);
            }
            catch (Exception)
            {
                throw;
            }
        }
    } 
    #endregion
}
