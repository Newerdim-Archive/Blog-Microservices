using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailSender.API.Services
{
    public interface IEmailSenderService
    {
        public Task SendAsync(MailMessage message);
    }
}
