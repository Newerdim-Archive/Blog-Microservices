using EmailSender.API.Dtos;
using EmailSender.API.Exceptions;
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
                throw new FluentValidationException(typeof(SendRequest), result, nameof(request));
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