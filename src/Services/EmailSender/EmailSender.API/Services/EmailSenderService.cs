using EmailSender.API.Dtos;
using EmailSender.API.Helper;
using EmailSender.API.Validators;
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

        public async Task SendAsync(SendRequest request)
        {
            var validator = new SendRequestValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                throw new ArgumentException($"{typeof(SendRequest)} is invalid. Errors: {string.Concat(result.Errors)}");
            }

            var message = new MailMessage(
                request.From,
                request.To,
                request.Subject,
                request.Body);

            await _client.SendMailAsync(message);
        }
    }
}