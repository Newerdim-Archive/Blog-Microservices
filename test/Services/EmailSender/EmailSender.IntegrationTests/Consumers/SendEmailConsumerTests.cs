using EmailSender.API;
using EmailSender.IntergrationTests;
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
    public class SendEmailConsumerTests : IClassFixture<EmailSenderWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly InMemoryTestHarness _harness = new();
        private readonly ConsumerTestHarness<SendEmailConsumer> _consumerHarness;

        public SendEmailConsumerTests(EmailSenderWebApplicationFactory<Startup> factory)
        {
            using var scope = factory.Services.CreateScope();

            var sendEmailConsumer = scope.ServiceProvider.GetRequiredService<SendEmailConsumer>();

            _consumerHarness = _harness.Consumer(() => sendEmailConsumer);

            _harness.Start().Wait();
        }

        [Fact]
        public async Task Consume_ValidCommand_NoFault()
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
            await _harness.InputQueueSendEndpoint.Send(command);

            var isConsumed = await IsConsumedAsync<SendEmailCommand>();
            var isAnyFault = await IsAnyPublishedFaultAsync<SendEmailCommand>();

            // Assert
            isConsumed.Should().BeTrue();
            isAnyFault.Should().BeFalse();
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
            await _harness.InputQueueSendEndpoint.Send(command);

            await IsConsumedAsync<SendEmailCommand>();

            var mail = await GetMailAsync();

            // Assert
            mail.Subject.Should().Be(exceptedSubject);
            mail.From.Should().Be(exceptedFrom);
            mail.To.First().Should().Be(exceptedTarget);
            mail.Body.Should().Be(exceptedBody);
        }

        [Fact]
        public async Task Consume_InvalidCommand_PublishedFault()
        {
            // Arrange
            var command = new SendEmailCommand();

            // Act
            await _harness.InputQueueSendEndpoint.Send(command);

            var isConsumed = await IsConsumedAsync<SendEmailCommand>();
            var isAnyFault = await IsAnyPublishedFaultAsync<SendEmailCommand>();

            // Assert
            isConsumed.Should().BeTrue();
            isAnyFault.Should().BeTrue();
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

        #region Private Methods

        /// <summary>
        /// Get temporary folder path
        /// </summary>
        /// <returns>Absolute path</returns>
        private static string GetTempFolderPath()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            var tempFolder = Path.Combine(Path.GetTempPath(), assemblyName);
            return Path.Combine(tempFolder, "MailMessageTemp");
        }

        /// <summary>
        /// Get email from temporary folder
        /// </summary>
        /// <returns></returns>
        private static async Task<MailMessage> GetMailAsync()
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