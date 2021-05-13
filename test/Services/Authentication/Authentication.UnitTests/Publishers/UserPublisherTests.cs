using Authentication.API.Dtos;
using Authentication.API.Publishers;
using EventBus.Events;
using FluentAssertions;
using MassTransit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Publishers
{
    public class UserPublisherTests
    {
        private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();

        private IUserPublisher CreatePublisher()
        {
            return new UserPublisher(_publishEndpointMock.Object);
        }

        [Fact]
        public async Task PublishNewUserAsync_ValidRequest_PublishValidEvent()
        {
            // Arrange
            var userId = 1;
            var username = "newUser";
            var email = "newUser@mail.com";

            var sut = CreatePublisher();

            var request = new PublishNewUserRequest
            {
                UserId = userId,
                Username = username,
                Email = email
            };

            // Act
            await sut.PublishNewUserAsync(request);

            // Assert
            _publishEndpointMock.Verify(x =>
                x.Publish(
                    It.Is<NewUserEvent>(m =>
                        m.UserId == userId &&
                        m.Username == username &&
                        m.Email == email),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishNewUserAsync_InvalidRequest_ThrowsArgumentException()
        {
            // Arrange
            var userId = 0;
            var username = "";
            var email = "";

            var sut = CreatePublisher();

            var request = new PublishNewUserRequest
            {
                UserId = userId,
                Username = username,
                Email = email
            };

            // Act
            Func<Task> act = async () => await sut.PublishNewUserAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task PublishNewUserAsync_NullRequest_ThrowsArgumentException()
        {
            // Arrange
            var sut = CreatePublisher();

            // Act
            Func<Task> act = async () => await sut.PublishNewUserAsync(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}