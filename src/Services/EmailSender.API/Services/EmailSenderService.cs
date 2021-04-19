using EmailSender.API.Helper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailSender.API.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailSenderService(IOptions<SmtpSettings> options)
        {
            _smtpSettings = options.Value;
        }

        public async Task SendAsync(MailMessage message)
        {
            using var client = new SmtpClient(_smtpSettings.Hostname, _smtpSettings.Port);
            await client.SendMailAsync(message);
        }
    }
}
