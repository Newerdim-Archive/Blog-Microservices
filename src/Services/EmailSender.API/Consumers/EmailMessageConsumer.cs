using EmailSender.API.Services;
using MassTransit;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailSender.API.Consumers
{
    public class EmailMessageConsumer : IConsumer<EmailMessage>
    {
        private readonly IEmailSenderService _emailSender;

        public EmailMessageConsumer(IEmailSenderService emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<EmailMessage> context)
        {
            var data = context.Message;
            
            if (data is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(data.From))
            {
                throw new ArgumentException($"From: {data.From} is invalid email.");
            }

            foreach(var to in data.To)
            {
                if (string.IsNullOrWhiteSpace(to))
                {
                    throw new ArgumentException($"To: {to} is invalid email.");
                }

                var message = new MailMessage(data.From, to, data.Subject, data.Body);
                await _emailSender.SendAsync(message);
            }
        }
    }
}
