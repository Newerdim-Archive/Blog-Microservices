using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailSender.API.Wrappers
{
    public interface ISmtpClientWrapper
    {
        Task SendMailAsync(MailMessage message);
    }
}
