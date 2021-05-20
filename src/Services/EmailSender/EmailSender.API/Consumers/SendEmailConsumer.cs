using EmailSender.API.Dtos;
using EmailSender.API.Exceptions;
using EmailSender.API.Services;
using EmailSender.API.Validators;
using EventBus.Commands;
using EventBus.Events;
using EventBus.Results;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
            var command = context.Message;

            var validator = new SendEmailCommandValidator();
            var result = await validator.ValidateAsync(command);

            if (!result.IsValid)
            {
                throw new FluentValidationException(typeof(SendEmailCommand), result, nameof(context));
            }

            foreach (var to in command.To)
            {
                await _emailSender.SendAsync(new SendRequest
                {
                    To = to,
                    From = command.From,
                    Subject = command.Subject,
                    Body = command.Body
                });
            }

            await context.RespondAsync(new ConsumerBaseResult());
        }
    }
}