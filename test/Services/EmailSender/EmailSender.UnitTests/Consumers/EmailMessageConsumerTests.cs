using EmailSender.API.Services;
using EventBus.Commands;
using EventBus.Messages;
using EventBus.Results;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

namespace EmailSender.UnitTests.Consummers
{
    public class EmailMessageConsumerTests : IDisposable
    {
        private readonly Mock<IEmailSenderService> _emailSenderServiceMock = new();
        private readonly Mock<ILogger<SendEmailConsumer>> _logger = new();

        private readonly InMemoryTestHarness _harness = new();
        private readonly ConsumerTestHarness<SendEmailConsumer> _consumerHarness;
        private readonly IRequestClient<SendEmailCommand> _client;

        public EmailMessageConsumerTests()
        {
            _consumerHarness = _harness.Consumer(() => 
                new SendEmailConsumer(_emailSenderServiceMock.Object, _logger.Object));

            _harness.Start().Wait();

            _client = _harness.ConnectRequestClient<SendEmailCommand>().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task Consume_ValidData_NoExpections()
        {
            // Arrange
            var message = new SendEmailCommand
            {
                From = "me@test.com",
                To = new string[] { "test@test.com", "test1@test.com" },
            };
            
            // Act
            await _client.GetResponse<ConsumerResponse>(message);

            // Assert
            (await IsConsumed()).Should().BeTrue();

            _emailSenderServiceMock.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Exactly(2));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid")]

        public async Task Consume_InvalidFromAddress_NotSendedEmail(string from)
        {
            // Arrange
            var message = new SendEmailCommand
            {
                From = from,
                To = new string[] { "test@gmail.com" },
            };

            // Act
            Func<Task> act = () => _client.GetResponse<ConsumerResponse>(message);

            // Assert
            await act.Should().ThrowAsync<RequestFaultException>();

            (await IsConsumed()).Should().BeTrue();

            _emailSenderServiceMock.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Never);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("invalid@")]
        public async Task Consume_InvalidToAddress_NotSendedEmail(string to)
        {
            // Arrange
            var message = new SendEmailCommand
            {
                From = "test@gmail.com",
                To = new string[] { to },
            };

            // Act
            Func<Task> act = () => _client.GetResponse<ConsumerResponse>(message);

            // Assert
            await act.Should().ThrowAsync<RequestFaultException>();

            (await IsConsumed()).Should().BeTrue();

            _emailSenderServiceMock.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Never);
        }

        [Fact]
        public async Task Consume_NullTo_NotSendedEmail()
        {
            // Arrange
            var message = new SendEmailCommand
            {
                From = "test@gmail.com",
                To = null,
            };

            // Act
            Func<Task> act = () => _client.GetResponse<ConsumerResponse>(message);

            // Assert
            await act.Should().ThrowAsync<RequestFaultException>();

            (await IsConsumed()).Should().BeTrue();

            _emailSenderServiceMock.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Consume_NullOrEmptyAdressInTo_SendOnlyToValidRecipients(string to)
        {
            // Arrange
            var message = new SendEmailCommand
            {
                From = "test@gmail.com",
                To = new string[] { "valid@mail.com", to },
            };

            // Act
            await _client.GetResponse<ConsumerResponse>(message);


            // Assert
            (await IsConsumed()).Should().BeTrue();

            _emailSenderServiceMock.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Once);
        }

        public void Dispose()
        {
            _harness.Stop().Wait();

            GC.SuppressFinalize(this);
        }

        private async Task<bool> IsConsumed()
        {
            var harnessConsumed = await _harness.Consumed.Any<SendEmailCommand>();
            var consumerConsumed = await _consumerHarness.Consumed.Any<SendEmailCommand>();

            return harnessConsumed && consumerConsumed;
        }
    }
}