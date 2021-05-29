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
        private readonly Mock<ISmtpClientWrapper> _smtpClient = new();
        private readonly IEmailSenderService _sut;

        public EmailSenderServiceTests()
        {
            _sut = new EmailSenderService(_smtpClient.Object);
        }

        [Fact]
        public async Task SendAsync_ValidRequest_NoExceptions()
        {
            // Arrange
            var request = new SendRequest
            {
                To = "user1@mail.com",
                From = "company@mail.com"
            };

            // Act
            Func<Task> act = async () => await _sut.SendAsync(request);

            // Assert
            await act.Should().NotThrowAsync();

            _smtpClient.Verify(x =>
                x.SendMailAsync(It.IsAny<MailMessage>()),
                Times.Once);
        }

        [Fact]
        public async Task SendAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            Func<Task> act = async () => await _sut.SendAsync(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();

            _smtpClient.Verify(x => x
                .SendMailAsync(It.IsAny<MailMessage>()),
                Times.Never);
        }

        [Fact]
        public async Task SendAsync_InvalidRequest_ThrowsFluentValidationException()
        {
            // Arrange
            var request = new SendRequest
            {
                To = null,
                From = null
            };

            // Act
            Func<Task> act = async () => await _sut.SendAsync(request);

            // Assert
            await act.Should().ThrowAsync<FluentValidationException>();

            _smtpClient.Verify(x => x
                .SendMailAsync(It.IsAny<MailMessage>()),
                Times.Never);
        }
    }
}