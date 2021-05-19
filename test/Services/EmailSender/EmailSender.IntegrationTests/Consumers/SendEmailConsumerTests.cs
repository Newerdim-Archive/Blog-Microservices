using EmailSender.API;
using EmailSender.API.Services;
using EmailSender.API.Wrappers;
using EmailSender.IntegrationTests.Mock;
using EmailSender.IntergrationTests;
using EventBus.Commands;
using EventBus.Messages;
using EventBus.Results;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace EmailSender.IntegrationTests.Consumers
{
    public class SendEmailConsumerTests : IClassFixture<EmailSenderWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly InMemoryTestHarness _harness = new();
        private readonly ConsumerTestHarness<SendEmailConsumer> _consumerHarness;
        private readonly IRequestClient<SendEmailCommand> _client;

        public SendEmailConsumerTests(EmailSenderWebApplicationFactory<Startup> factory)
        {
            using var scope = factory.Services.CreateScope();

            var sendEmailConsumer = scope.ServiceProvider.GetRequiredService<SendEmailConsumer>();

            _consumerHarness = _harness.Consumer(() => sendEmailConsumer);

            _harness.Start().Wait();

            _client = _harness.ConnectRequestClient<SendEmailCommand>().GetAwaiter().GetResult();
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
            await _client.GetResponse<ConsumerResponse>(command);

            // Assert
            (await IsConsumed()).Should().BeTrue();
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
            await _client.GetResponse<ConsumerResponse>(command);

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
            Func<Task> act = async () => await _client.GetResponse<ConsumerResponse>(new SendEmailCommand());

            var files = new DirectoryInfo(GetTempFolderPath()).GetFiles();

            // Assert
            await act.Should().ThrowAsync<RequestFaultException>();
            files.Count().Should().Be(0);
        }

        private static string GetTempFolderPath()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

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

            _harness.Dispose();

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
