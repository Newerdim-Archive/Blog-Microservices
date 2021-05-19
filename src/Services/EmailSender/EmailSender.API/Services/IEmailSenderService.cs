using System.Net.Mail;
using System.Threading.Tasks;
using System;

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
        /// <exception cref="ArgumentException">Throws when message is null.</exception>
        public Task SendAsync(MailMessage message);
    }
}