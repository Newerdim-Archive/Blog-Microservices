using EmailSender.API.Helper;
using EmailSender.API.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EmailSender.UnitTests.Services
{
    public class EmailSenderServiceTests
    {
        private Mock<IOptions<SmtpSettings>> _mockOptions;

        public EmailSenderServiceTests()
        {
            var mock = new Mock<IOptions<SmtpSettings>>();

            mock.Setup(x => x.Value).Returns(new SmtpSettings
            {
                Hostname = "localhost",
                Port = 25
            });

            _mockOptions = mock;
        }

        [Fact]
        public async Task SendAsync_ValidData_NoExceptions()
        {
            var service = new EmailSenderService(_mockOptions.Object);
            var message = new MailMessage("test@test.com", "newerdim@onet.pl");

            Func<Task> act = async () => await service.SendAsync(message);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendAsync_NullMessage_SmtpException()
        {
            var service = new EmailSenderService(_mockOptions.Object);

            Func<Task> act = async () => await service.SendAsync(null);
            await act.Should().ThrowAsync<SmtpException>();
        }

        [Fact]
        public async Task SendAsync_EmptyMessage_SmtpException()
        {
            var service = new EmailSenderService(_mockOptions.Object);

            Func<Task> act = async () => await service.SendAsync(new MailMessage());
            await act.Should().ThrowAsync<SmtpException>();
        }
    }
}
