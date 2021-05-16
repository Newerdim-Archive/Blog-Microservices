using EmailSender.API.Helper;
using Microsoft.Extensions.Options;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailSender.API.Wrappers
{
    public class SmtpClientWrapper : ISmtpClientWrapper, IDisposable
    {
        private readonly SmtpClient _client;

        public SmtpClientWrapper(IOptions<SmtpSettings> options)
        {
            var smtpSettings = options.Value;
            _client = new SmtpClient(smtpSettings.Hostname, smtpSettings.Port);
        }

        public void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task SendMailAsync(MailMessage message)
        {
            await _client.SendMailAsync(message);
        }
    }
}
