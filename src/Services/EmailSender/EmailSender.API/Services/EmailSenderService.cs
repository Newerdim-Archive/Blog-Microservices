using EmailSender.API.Helper;
using EmailSender.API.Wrappers;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailSender.API.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly ISmtpClientWrapper _client;

        public EmailSenderService(ISmtpClientWrapper smtpClientWrapper)
        {
            _client = smtpClientWrapper;
        }

        public async Task SendAsync(MailMessage message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message), "Received a null argument");
            }

            if (message.To.Count == 0)
            {
                throw new ArgumentException("Received a argument with empty 'To' field", nameof(message));
            }

            await _client.SendMailAsync(message);
        }
    }
}