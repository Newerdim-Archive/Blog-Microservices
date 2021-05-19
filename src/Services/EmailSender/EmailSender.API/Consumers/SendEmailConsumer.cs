using EmailSender.API.Services;
using EventBus.Commands;
using EventBus.Events;
using EventBus.Results;
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

            if (string.IsNullOrWhiteSpace(data.From))
            {
                throw new ArgumentException("Email 'From' is null or empty.", nameof(context));
            }

            if (data.To is null)
            {
                throw new ArgumentException("Email 'To' is null or empty.", nameof(context));
            }

            foreach (var to in data.To)
            {
                if (string.IsNullOrWhiteSpace(to))
                {
                    // throw new ArgumentException("Email 'To' is null or empty.", nameof(context));
                    _logger.LogError("Email address from 'To' is null or empty.", nameof(context));
                    continue;
                }

                var message = new MailMessage(data.From, to, data.Subject, data.Body);
                await _emailSender.SendAsync(message);
            }

            await context.RespondAsync(new ConsumerResponse());
        }
    }
}