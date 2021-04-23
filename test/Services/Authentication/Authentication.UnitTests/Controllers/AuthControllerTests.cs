using Authentication.API.Controllers;
using Authentication.API.Dtos;
using Authentication.API.Enums;
using Authentication.API.Models;
using Authentication.API.Responses;
using Authentication.API.Services;
using AutoFixture;
using AutoFixture.Kernel;
using EventBus.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock = new();
        private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
        private readonly Mock<ILogger<AuthController>> _loggerMock = new();

        public AuthControllerTests() { }

        private AuthController CreateService()
        {
            return new AuthController(_authServiceMock.Object, _publishEndpointMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Register_ValidModel_ReturnsUserIdAndMessage()
        {
            // Arrange
            var fixture = new Fixture();
            var model = fixture.Create<RegisterModel>();

            _authServiceMock.Setup(x => x
                .RegisterAsync(It.IsAny<RegisterRequest>()))
                    .ReturnsAsync(new RegisterResult 
                    { 
                        Message = RegisterResultMessage.Successful, 
                        UserId = 1 
                    });

            var sut = CreateService();

            // Act
            var response = await sut.Register(model) as OkObjectResult;
            var value = response.Value as RegisterResponse;

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            value.UserId.Should().BeGreaterThan(0);
            value.Message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Register_ValidModel_PublishNewUserEvent()
        {
            // Arrange
            var fixture = new Fixture();
            var model = fixture.Create<RegisterModel>();

            _authServiceMock.Setup(x => x
                .RegisterAsync(It.IsAny<RegisterRequest>()))
                    .ReturnsAsync(new RegisterResult
                    {
                        Message = RegisterResultMessage.Successful,
                        UserId = 1
                    });

            var sut = CreateService();

            // Act
            var response = await sut.Register(model) as OkObjectResult;
            var value = response.Value as RegisterResponse;

            // Assert
            _publishEndpointMock.Verify(x => x
                .Publish(
                    It.Is<NewUserEvent>(x => 
                        x.UserId == 1 &&
                        x.Username == model.Username &&
                        x.Email == model.Email), 
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Register_UsernameAlreadyExists_ReturnsUnauthroize()
        {
            // Arrange
            var fixture = new Fixture();
            var model = fixture.Create<RegisterModel>();

            _authServiceMock.Setup(x => x
                .RegisterAsync(It.IsAny<RegisterRequest>()))
                    .ReturnsAsync(new RegisterResult
                    {
                        Message = RegisterResultMessage.UsernameAlreadyExists,
                        UserId = 0
                    });

            var sut = CreateService();

            // Act
            var response = await sut.Register(model) as UnauthorizedObjectResult;

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            response.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Register_EmailAlreadyExists_ReturnsUnauthroize()
        {
            // Arrange
            var fixture = new Fixture();
            var model = fixture.Create<RegisterModel>();

            _authServiceMock.Setup(x => x
                .RegisterAsync(It.IsAny<RegisterRequest>()))
                    .ReturnsAsync(new RegisterResult
                    {
                        Message = RegisterResultMessage.EmailAlreadyExists,
                        UserId = 0
                    });

            var sut = CreateService();

            // Act
            var response = await sut.Register(model) as UnauthorizedObjectResult;

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            response.Value.Should().NotBeNull();
        }
    }
}
