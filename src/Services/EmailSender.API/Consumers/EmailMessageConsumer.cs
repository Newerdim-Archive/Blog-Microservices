using EmailSender.API.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailSender.API.Consumers
{
    public class EmailMessageConsumer : IConsumer<EmailMessage>
    {
        private readonly IEmailSenderService _emailSender;
        private readonly ILogger<EmailMessageConsumer> _logger;

        public EmailMessageConsumer(IEmailSenderService emailSender, ILogger<EmailMessageConsumer> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EmailMessage> context)
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

            foreach(var to in data.To)
            {
                if (string.IsNullOrWhiteSpace(to))
                {
                    _logger.LogWarning($"Email target is null or empty.");
                    continue;
                }

                var message = new MailMessage(data.From, to, data.Subject, data.Body);
                await _emailSender.SendAsync(message);
            }
        }
    }
}
