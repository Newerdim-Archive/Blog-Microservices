using EmailSender.API.Services;
using EmailSender.API.Wrappers;
using EmailSender.IntegrationTests.Mock;
using EventBus.Commands;
using EventBus.Messages;
using EventBus.Results;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace EmailSender.IntegrationTests.Consumers
{
    public class SendEmailConsumerTests : IDisposable
    {
        private readonly TestHarness<SendEmailConsumer, SendEmailCommand> _testHarness;

        public SendEmailConsumerTests()
        {
            var serviceCollection = new ServiceCollection()
                .AddTransient<IEmailSenderService, EmailSenderService>()
                .AddTransient<ISmtpClientWrapper, FakeSmtpClientWrapper>()
                .AddLogging();

            _testHarness = new TestHarness<SendEmailConsumer, SendEmailCommand>(serviceCollection);
        }

        [Fact]
        public async Task Consume_ValidCommand_ReturnResult()
        {
            // Arrange
            var command = new SendEmailCommand
            {
                Subject = "Test Email",
                From = "company@mail.com",
                To = new string[] { "user@mail.com" },
                Body = "Test"
            };

            // Act
            var response = await _testHarness.GetResponse<BaseResult>(command);

            // Assert
            response.Message.Successful.Should().BeTrue();
            (await _testHarness.IsHarnessConsumed()).Should().BeTrue();
            (await _testHarness.IsConsumerHarnessConsumed()).Should().BeTrue();
        }

        [Fact]
        public async Task Consume_ValidCommand_SendValidEmail()
        {
            // Arrange
            var exceptedSubject = "Test Email";
            var exceptedFrom = "company@mail.com";
            var exceptedTarget = "user@mail.com";
            var exceptedBody = "Test";

            var command = new SendEmailCommand
            {
                Subject = exceptedSubject,
                From = exceptedFrom,
                To = new string[] { exceptedTarget },
                Body = exceptedBody
            };

            // Act
            await _testHarness.GetResponse<BaseResult>(command);

            var mail = await GetMail();

            // Assert
            mail.Subject.Should().Be(exceptedSubject);
            mail.From.Should().Be(exceptedFrom);
            mail.To.First().Should().Be(exceptedTarget);
            mail.Body.Should().Be(exceptedBody);
        }

        [Fact]
        public async Task Consume_InvalidCommand_ThrowsRequestFaultException()
        {
            // Arrange

            // Act
            Func<Task> act = async () => await _testHarness.GetResponse<BaseResult>(new SendEmailCommand());

            var directoryExists = new DirectoryInfo(GetTempFolderPath()).Exists;

            // Assert
            await act.Should().ThrowAsync<RequestFaultException>();
            directoryExists.Should().BeFalse();
        }

        private static string GetTempFolderPath()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            assemblyName = assemblyName.Replace("IntegrationTests", "API");

            var tempFolder = Path.Combine(Path.GetTempPath(), assemblyName);
            return Path.Combine(tempFolder, "MailMessageTemp");
        }

        private static async Task<MailMessage> GetMail()
        {
            var directory = new DirectoryInfo(GetTempFolderPath());

            var mailFilePath = directory.GetFiles().First().FullName;
            var mailLines = await File.ReadAllLinesAsync(mailFilePath);

            var subject = mailLines[6].Split(": ")[1];
            var from = mailLines[3].Split(": ")[1];
            var to = mailLines[4].Split(": ")[1];
            var body = mailLines[10];

            return new MailMessage(from, to, subject, body);
        }

        public void Dispose()
        {
            if (Directory.Exists(GetTempFolderPath()))
            {
                new DirectoryInfo(GetTempFolderPath()).Delete(true);
            }

            _testHarness.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
