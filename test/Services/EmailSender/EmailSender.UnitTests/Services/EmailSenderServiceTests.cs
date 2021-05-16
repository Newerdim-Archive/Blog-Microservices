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
        public async Task SendAsync_ValidData_NoExceptions()
        {
            var sut = CreateService();
            var message = new MailMessage("test@test.com", "test2@test.com");

            Func<Task> act = async () => await sut.SendAsync(message);

            await act.Should().NotThrowAsync();
            _client.Verify(x =>
                x.SendMailAsync(It.IsAny<MailMessage>()),
                Times.Once);
        }

        [Fact]
        public async Task SendAsync_NullMessage_ThrowsArgumentNullException()
        {
            var sut = CreateService();

            Func<Task> act = async () => await sut.SendAsync(null);

            await act.Should().ThrowAsync<ArgumentNullException>();
            _client.Verify(x =>
                x.SendMailAsync(It.IsAny<MailMessage>()),
                Times.Never);
        }

        [Fact]
        public async Task SendAsync_EmptyMessage_ArgumentException()
        {
            var sut = CreateService();

            Func<Task> act = async () => await sut.SendAsync(new MailMessage());

            await act.Should().ThrowAsync<ArgumentException>();
            _client.Verify(x =>
                x.SendMailAsync(It.IsAny<MailMessage>()),
                Times.Never);
        }

        [Fact]
        public async Task SendAsync_NoRecipients_ArgumentException()
        {
            var sut = CreateService();

            Func<Task> act = async () => await sut.SendAsync(new MailMessage());

            await act.Should().ThrowAsync<ArgumentException>();
            _client.Verify(x =>
                x.SendMailAsync(It.IsAny<MailMessage>()),
                Times.Never);
        }
    }
}