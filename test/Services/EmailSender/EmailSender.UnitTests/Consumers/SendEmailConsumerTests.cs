using EmailSender.API.Dtos;
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
using System.Threading.Tasks;
using Xunit;

namespace EmailSender.UnitTests.Consummers
{
    public class SendEmailConsumerTests : IDisposable
    {
        private readonly Mock<IEmailSenderService> _emailSenderServiceMock = new();
        private readonly Mock<ILogger<SendEmailConsumer>> _logger = new();

        private readonly InMemoryTestHarness _harness = new();
        private readonly ConsumerTestHarness<SendEmailConsumer> _consumerHarness;

        public SendEmailConsumerTests()
        {
            _consumerHarness = _harness.Consumer(() =>
                new SendEmailConsumer(_emailSenderServiceMock.Object, _logger.Object));

            _harness.Start().Wait();
        }

        [Fact]
        public async Task Consume_ValidData_EmailSended()
        {
            // Arrange
            var command = new SendEmailCommand
            {
                From = "me@test.com",
                To = new string[] { "test@test.com", "test1@test.com" },
            };

            // Act
            await _harness.InputQueueSendEndpoint.Send(command);

            var isConsumed = await IsConsumedAsync<SendEmailCommand>();

            // Assert
            isConsumed.Should().BeTrue();

            _emailSenderServiceMock.Verify(x => x
                .SendAsync(It.IsAny<SendRequest>()), 
                Times.Exactly(2));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("invalid@")]
        public async Task Consume_InvalidFromAddress_NotSendedAnyEmail(string from)
        {
            // Arrange
            var command = new SendEmailCommand
            {
                From = from,
                To = new string[] { "test@gmail.com" },
            };

            // Act
            await _harness.InputQueueSendEndpoint.Send(command);

            var isConsumed = await IsConsumedAsync<SendEmailCommand>();
            var isAnyPublishedFault = await IsAnyPublishedFaultAsync<SendEmailCommand>();

            // Assert
            isConsumed.Should().BeTrue();
            isAnyPublishedFault.Should().BeTrue();

            _emailSenderServiceMock.Verify(x => x.SendAsync(It.IsAny<SendRequest>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("invalid@")]
        public async Task Consume_InvalidToAddress_NotSendedAnyEmail(string to)
        {
            // Arrange
            var command = new SendEmailCommand
            {
                From = "test@gmail.com",
                To = new string[] { to },
            };

            // Act
            await _harness.InputQueueSendEndpoint.Send(command);

            var isConsumed = await IsConsumedAsync<SendEmailCommand>();
            var isAnyPublishedFault = await IsAnyPublishedFaultAsync<SendEmailCommand>();

            // Assert
            isConsumed.Should().BeTrue();
            isAnyPublishedFault.Should().BeTrue();

            _emailSenderServiceMock.Verify(x => x.SendAsync(It.IsAny<SendRequest>()), Times.Never);
        }

        [Fact]
        public async Task Consume_NullTo_NotSendedAnyEmail()
        {
            // Arrange
            var command = new SendEmailCommand
            {
                From = "test@gmail.com",
                To = null,
            };

            // Act
            await _harness.InputQueueSendEndpoint.Send(command);

            var isConsumed = await IsConsumedAsync<SendEmailCommand>();
            var isAnyPublishedFault = await IsAnyPublishedFaultAsync<SendEmailCommand>();

            // Assert
            isConsumed.Should().BeTrue();
            isAnyPublishedFault.Should().BeTrue();

            _emailSenderServiceMock.Verify(x => x
                .SendAsync(It.IsAny<SendRequest>()), 
                Times.Never);
        }

        public void Dispose()
        {
            _harness.Stop().Wait();
            GC.SuppressFinalize(this);
        }

        #region Private Methods

        /// <summary>
        /// Check if harness and consumer harness consumed SendEmailCommand
        /// </summary>
        /// <typeparam name="TMessage">Message type</typeparam>
        /// <returns>True if consumed, otherwise false</returns>
        private async Task<bool> IsConsumedAsync<TMessage>() where TMessage : class
        {
            var harnessConsumed = await _harness.Consumed.Any<TMessage>();
            var consumerConsumed = await _consumerHarness.Consumed.Any<TMessage>();

            return harnessConsumed && consumerConsumed;
        }

        /// <summary>
        /// Check if consumer published fault of type <typeparamref name="TMessage"/>
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <returns>True if published fault, otherwise false</returns>
        private async Task<bool> IsAnyPublishedFaultAsync<TMessage>() where TMessage : class
        {
            return await _harness.Published.Any<Fault<SendEmailCommand>>();
        }

        #endregion
    }
}