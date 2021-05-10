using Authentication.API.Dtos;
using Authentication.API.Helpers;
using Authentication.API.Publishers;
using EventBus.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Publishers
{
    public class EmailPublisherTests
    {
        private readonly Mock<IOptions<CompanySettings>> _companySettingsOptionsMock = new();
        private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();

        public EmailPublisherTests()
        {
            var companySettings = new CompanySettings
            {
                Email = "test@company.com",
                Name = "company"
            };

            _companySettingsOptionsMock.Setup(x => x.Value).Returns(companySettings);
        }

        private IEmailPublisher CreatePublisher()
        {
            return new EmailPublisher(
                _publishEndpointMock.Object, 
                _companySettingsOptionsMock.Object);
        }


        [Fact]
        public async Task PublishEmailConfirmationAsync_ValidRequest_PublishEvent()
        {
            // Arrange
            var request = new PublishEmailConfirmationRequest
            {
                TargetEmail = "target@mail.com",
                Token = "secretToken",
                ReturnUrl = "www.website.com/email-confrimation"
            };

            var sut = CreatePublisher();

            // Act
            await sut.PublishEmailConfirmationAsync(request);

            // Assert
            _publishEndpointMock.Verify(x => x
                .Publish(It.IsAny<SendEmailEvent>(), It.IsAny<CancellationToken>()), 
                    Times.Once);
        }

        [Fact]
        public async Task PublishEmailConfirmationAsync_ValidRequest_PublishValidEmail()
        {
            // Arrange
            var targetEmail = "target@mail.com";
            var token = "secretToken";
            var returnUrl = "www.website.com/email-confrimation";

            var sut = CreatePublisher();

            // Act
            await sut.PublishEmailConfirmationAsync(new PublishEmailConfirmationRequest
            {
                TargetEmail = targetEmail,
                Token = token,
                ReturnUrl = returnUrl
            });

            // Assert
            _publishEndpointMock.Verify(x => 
                x.Publish(
                    It.Is<SendEmailEvent>(e => 
                        e.To.Contains(targetEmail) &&
                        e.Subject.Contains("email verification") &&
                        e.Body.Contains(token) &&
                        e.From.Contains("test@company.com")), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishEmailConfirmationAsync_InvalidRequest_ThrowsArgumentException()
        {
            // Arrange
            var request = new PublishEmailConfirmationRequest
            {
                TargetEmail = "",
                Token = "",
                ReturnUrl = ""
            };

            var sut = CreatePublisher();

            // Act
            Func<Task> act = () => sut.PublishEmailConfirmationAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task PublishEmailConfirmationAsync_NullRequest_ThrowsArgumentException()
        {
            // Arrange
            var sut = CreatePublisher();

            // Act
            Func<Task> act = () => sut.PublishEmailConfirmationAsync(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
