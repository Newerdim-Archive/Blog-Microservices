using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailSender.API.Services
{
    /// <summary>
    /// Service that sends emails
    /// </summary>
    public interface IEmailSenderService
    {
        /// <summary>
        /// Send async email.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="SmtpException">Throws when email adress is invalid.</exception>
        /// <exception cref="ArgumentNullException">Throws when message is null.</exception>
        public Task SendAsync(MailMessage message);
    }
}
