using EmailSender.API.Services;
using EventBus.Commands;
using EventBus.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EventBus.Messages
{
    public class SendEmailConsumer : IConsumer<SendEmailCommand>
    {
        private readonly IEmailSenderService _emailSender;
        private readonly ILogger<SendEmailConsumer> _logger;

        public SendEmailConsumer(IEmailSenderService emailSender, ILogger<SendEmailConsumer> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SendEmailCommand> context)
        {
            var data = context.Message;

            if (data is null)
            {
                _logger.LogInformation($"Empty email message. {context}");
                return;
            }

            if (string.IsNullOrWhiteSpace(data.From))
            {
                throw new ArgumentException($"Email 'from' is null or empty.");
            }

            foreach (var to in data.To)
            {
                if (string.IsNullOrWhiteSpace(to))
                {
                    _logger.LogWarning($"Email target is null or empty. {context}");
                    continue;
                }

                var message = new MailMessage(data.From, to, data.Subject, data.Body);
                await _emailSender.SendAsync(message);
            }
        }
    }
}