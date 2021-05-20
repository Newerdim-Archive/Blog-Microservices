using EmailSender.API.Dtos;
using EmailSender.API.Exceptions;
using EmailSender.API.Services;
using EmailSender.API.Wrappers;
using FluentAssertions;
using Moq;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

namespace EmailSender.UnitTests.Services
{
    public class EmailSenderServiceTests
    {
        private readonly Mock<ISmtpClientWrapper> _client = new();

        private IEmailSenderService CreateService()
        {
            return new EmailSenderService(_client.Object);
        }

        [Fact]
        public async Task SendAsync_ValidRequest_NoExceptions()
        {
            // Arrange
            var sut = CreateService();

            var message = new SendRequest
            {
                To = "user1@mail.com", 
                From = "company@mail.com"
            };

            // Act
            Func<Task> act = async () => await sut.SendAsync(message);

            // Assert
            await act.Should().NotThrowAsync();

            _client.Verify(x => 
                x.SendMailAsync(It.IsAny<MailMessage>()),
                Times.Once);
        }

        [Fact]
        public async Task SendAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var sut = CreateService();

            // Act
            Func<Task> act = async () => await sut.SendAsync(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();

            _client.Verify(x =>
                x.SendMailAsync(It.IsAny<MailMessage>()),
                Times.Never);
        }

        [Fact]
        public async Task SendAsync_InvalidRequest_ThrowsFluentValidationException()
        {
            // Arrange
            var sut = CreateService();

            // Act
            Func<Task> act = async () => await sut.SendAsync(new SendRequest
            {
                To = null,
                From = null
            });

            // Assert
            await act.Should().ThrowAsync<FluentValidationException>();

            _client.Verify(x =>
                x.SendMailAsync(It.IsAny<MailMessage>()),
                Times.Never);
        }
    }
}