using EmailSender.API.Helper;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailSender.API.Wrappers
{
    public class SmtpClientWrapper : ISmtpClientWrapper
    {
        private readonly SmtpSettings _smtpSettings;

        public SmtpClientWrapper(IOptions<SmtpSettings> options)
        {
            _smtpSettings = options.Value;
        }

        public async Task SendMailAsync(MailMessage message)
        {
            using var client = new SmtpClient(
                _smtpSettings.Hostname,
                _smtpSettings.Port);

            await client.SendMailAsync(message);
        }
    }
}
