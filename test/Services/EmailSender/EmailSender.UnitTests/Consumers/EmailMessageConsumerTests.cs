using EmailSender.API.Consumers;
using EmailSender.API.Services;
using FluentAssertions;
using MassTransit.Testing;
using Moq;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

namespace EmailSender.UnitTests
{
    public class EmailMessageConsumerTests
    {
        private Mock<IEmailSenderService> _emailSenderServiceMock = new Mock<IEmailSenderService>();

        public EmailMessageConsumerTests()
        {

        }

        [Fact]
        public async Task Consume_ValidData_NoExpections()
        {
            var message = new EmailMessage
            {
                From = "me@test.com",
                To = new string[] { "test@test.com", "test1@test.com" },
                Subject = "test",
                Body = "body"
            };

            _emailSenderServiceMock.Setup(x => x
                .SendAsync(It.IsAny<MailMessage>()));

            var harness = new InMemoryTestHarness();
            var consumerHarness = harness.Consumer(() => new EmailMessageConsumer(_emailSenderServiceMock.Object));

            await harness.Start();
            try
            {
                await harness.InputQueueSendEndpoint.Send(message);

                await harness.Consumed.Any<EmailMessage>();
                var act = await consumerHarness.Consumed.Any<EmailMessage>();

                act.Should().BeTrue();
            }
            finally
            {
                await harness.Stop();
            }

            _emailSenderServiceMock.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Consume_InvalidFromAddress_NotSendEmail()
        {
            var message = new EmailMessage
            {
                From = "me",
                To = new string[] { "test@test.com", "test1@test.com" },
                Subject = "test",
                Body = "body"
            };

            _emailSenderServiceMock.Setup(x => x
                .SendAsync(It.IsAny<MailMessage>()));

            var harness = new InMemoryTestHarness();
            var consumerHarness = harness.Consumer(() => new EmailMessageConsumer(_emailSenderServiceMock.Object));

            await harness.Start();
            try
            {
                await harness.InputQueueSendEndpoint.Send(message);

                await harness.Consumed.Any<EmailMessage>();
                var act = await consumerHarness.Consumed.Any<EmailMessage>();

                act.Should().BeTrue();
            }
            finally
            {
                await harness.Stop();
            }

            _emailSenderServiceMock.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Never);
        }

        [Fact]
        public async Task Consume_InvalidToAddress_NotSendEmail()
        {
            var message = new EmailMessage
            {
                From = "me@test.com",
                To = new string[] { "test" },
                Subject = "test",
                Body = "body"
            };

            _emailSenderServiceMock.Setup(x => x
                .SendAsync(It.IsAny<MailMessage>()));

            var harness = new InMemoryTestHarness();
            var consumerHarness = harness.Consumer(() => new EmailMessageConsumer(_emailSenderServiceMock.Object));

            await harness.Start();
            try
            {
                await harness.InputQueueSendEndpoint.Send(message);

                await harness.Consumed.Any<EmailMessage>();
                var act = await consumerHarness.Consumed.Any<EmailMessage>();

                act.Should().BeTrue();
            }
            finally
            {
                await harness.Stop();
            }

            _emailSenderServiceMock.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Never);
        }
    }
}
