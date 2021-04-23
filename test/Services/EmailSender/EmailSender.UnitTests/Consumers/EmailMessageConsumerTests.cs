using EmailSender.API.Services;
using EventBus.Events;
using EventBus.Messages;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

namespace EmailSender.UnitTests
{
    public class EmailMessageConsumerTests
    {
        private readonly Mock<IEmailSenderService> _emailSenderServiceMock = new();
        private readonly ILogger<SendEmailConsumer> _logger;

        public EmailMessageConsumerTests()
        {
            var loggerMock = new Mock<ILogger<SendEmailConsumer>>();
            _logger = loggerMock.Object;
        }

        private ConsumerTestHarness<SendEmailConsumer> GetConsumer(InMemoryTestHarness harness)
        {
            return harness.Consumer(
                () => new SendEmailConsumer(_emailSenderServiceMock.Object, _logger));
        }

        [Fact]
        public async Task Consume_ValidData_NoExpections()
        {
            var message = new SendEmailEvent
            {
                From = "me@test.com",
                To = new string[] { "test@test.com", "test1@test.com" },
            };

            var harness = new InMemoryTestHarness();

            var consumerHarness = GetConsumer(harness);

            await harness.Start();
            try
            {
                await harness.InputQueueSendEndpoint.Send(message);

                await harness.Consumed.Any<SendEmailEvent>();
                var act = await consumerHarness.Consumed.Any<SendEmailEvent>();

                act.Should().BeTrue();
            }
            finally
            {
                await harness.Stop();
            }

            _emailSenderServiceMock.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Exactly(2));
        }

        [Theory]
        [InlineData(null, new string[] { "test@test.com" })]
        [InlineData("", new string[] { "test@test.com" })]
        [InlineData("invalid", new string[] { "test@test.com" })]

        [InlineData("test@gmail.com", new string[] { "" })]
        [InlineData("test@gmail.com", new string[] { null })]
        [InlineData("test@gmail.com", null)]
        public async Task Consume_InvalidFromAddress_NotSendEmail(string from, string[] to)
        {
            var message = new SendEmailEvent
            {
                From = from,
                To = to,
            };

            var harness = new InMemoryTestHarness();

            var consumerHarness = GetConsumer(harness);

            await harness.Start();
            try
            {
                await harness.InputQueueSendEndpoint.Send(message);

                await harness.Consumed.Any<SendEmailEvent>();
                var act = await consumerHarness.Consumed.Any<SendEmailEvent>();

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
