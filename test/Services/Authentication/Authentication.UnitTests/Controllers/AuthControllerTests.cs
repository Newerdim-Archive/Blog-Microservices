using Authentication.API.Controllers;
using Authentication.API.Dtos;
using Authentication.API.Enums;
using Authentication.API.Models;
using Authentication.API.Responses;
using Authentication.API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<ILogger<AuthController>> _loggerMock = new();

        private readonly Mock<HttpContext> _httpContextMock = new();
        private readonly Mock<HttpResponse> _httpResponseMock = new();
        private readonly Mock<IResponseCookies> _responseCookiesMock = new();

        private readonly AuthController _sut;

        public AuthControllerTests()
        {
            var controller = new AuthController(
                _authServiceMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object);

            controller.ControllerContext = CreateControllerContext();

            _sut = controller;
        }

        private ControllerContext CreateControllerContext()
        {
            _httpResponseMock.Setup(x => x.Cookies)
                .Returns(_responseCookiesMock.Object);

            _httpContextMock.Setup(x => x.Response)
                .Returns(_httpResponseMock.Object);

            var controllerContext = new ControllerContext();
            controllerContext.HttpContext = _httpContextMock.Object;

            return controllerContext;
        }

        #region Register

        [Fact]
        public async Task Register_ValidModel_ReturnsUserIdAndMessage()
        {
            // Arrange
            var model = new RegisterModel();

            _authServiceMock.Setup(x => x
                .RegisterAsync(It.IsAny<RegisterRequest>()))
                    .ReturnsAsync(new RegisterResult
                    {
                        Message = RegisterResultMessage.Successful,
                        UserId = 1
                    });

            // Act
            var response = await _sut.Register(model) as OkObjectResult;
            var value = response.Value as RegisterResponse;

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            value.UserId.Should().BeGreaterThan(0);
            value.Message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Register_UsernameAlreadyExists_ReturnsUnauthroize()
        {
            // Arrange
            var model = new RegisterModel();

            _authServiceMock.Setup(x => x
                .RegisterAsync(It.IsAny<RegisterRequest>()))
                    .ReturnsAsync(new RegisterResult
                    {
                        Message = RegisterResultMessage.UsernameAlreadyExists,
                        UserId = 0
                    });

            // Act
            var response = await _sut.Register(model) as UnauthorizedObjectResult;

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            response.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Register_EmailAlreadyExists_ReturnsUnauthroize()
        {
            // Arrange
            var model = new RegisterModel();

            _authServiceMock.Setup(x => x
                .RegisterAsync(It.IsAny<RegisterRequest>()))
                    .ReturnsAsync(new RegisterResult
                    {
                        Message = RegisterResultMessage.EmailAlreadyExists,
                        UserId = 0
                    });

            // Act
            var response = await _sut.Register(model) as UnauthorizedObjectResult;

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            response.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Register_NotImplementedLoginResultMessage_ReturnsInternalServerError()
        {
            // Arrange
            var model = new RegisterModel();

            _authServiceMock.Setup(x => x
                .RegisterAsync(It.IsAny<RegisterRequest>()))
                    .ReturnsAsync(new RegisterResult
                    {
                        Message = (RegisterResultMessage)99,
                        UserId = 0
                    });

            // Act
            Func<Task> act = async () => await _sut.Register(model);

            // Assert
            await act.Should().ThrowAsync<NotImplementedException>();
        }

        #endregion Register

        #region Login

        [Fact]
        public async Task Login_ValidModel_ReturnsOkWithMessage()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "user1",
                Password = "password1"
            };

            _authServiceMock.Setup(x => x
                .LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new LoginResult
                {
                    Message = LoginResultMessage.Successful
                });

            // Act
            var result = await _sut.Login(model) as OkObjectResult;

            var response = result.Value as LoginResponse;

            // Assert
            response.Message.Should().Contain("Logged in successfully");
        }

        [Fact]
        public async Task Login_ValidModel_ReturnsValidUserId()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "user1",
                Password = "user1!@#"
            };

            var expectedUserId = 1;

            _authServiceMock.Setup(x => x
                .LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new LoginResult
                {
                    UserId = expectedUserId
                });

            // Act
            var result = await _sut.Login(model) as OkObjectResult;

            var response = result.Value as LoginResponse;

            // Assert
            response.UserId.Should().Be(expectedUserId);
        }

        [Fact]
        public async Task Login_PasswordNotMatch_ReturnsUnauthorizeWithMessage()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "user1",
                Password = "notMatching"
            };

            _authServiceMock.Setup(x => x
                .LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new LoginResult
                {
                    Message = LoginResultMessage.PasswordNotMatch
                });

            // Act
            var result = await _sut.Login(model) as UnauthorizedObjectResult;

            var response = result.Value as string;

            // Assert
            response.Should().Be("Password does not match");
        }

        [Fact]
        public async Task Login_UserNotExist_ReturnsUnauthorizeWithMessage()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "UserNotExist",
                Password = "Password123!"
            };

            _authServiceMock.Setup(x => x
                .LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new LoginResult
                {
                    Message = LoginResultMessage.UserNotExist
                });

            // Act
            var result = await _sut.Login(model) as UnauthorizedObjectResult;

            var response = result.Value as string;

            // Assert
            response.Should().Be("User does not exist");
        }

        [Fact]
        public async Task Login_NotImplementedLoginResultMessage_ReturnsInternalServerError()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "UserNotExist",
                Password = "Password123!"
            };

            _authServiceMock.Setup(x => x
                .LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new LoginResult
                {
                    Message = (LoginResultMessage)99
                });

            // Act
            Func<Task> act = async () => await _sut.Login(model);

            // Assert
            await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Fact]
        public async Task Login_ValidModel_ReturnsAccessToken()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "User1",
                Password = "User1!@#"
            };

            var expectedToken = "access-token";

            _authServiceMock.Setup(x => x
                .LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new LoginResult
                {
                    UserId = 1,
                    Message = LoginResultMessage.Successful
                });

            _tokenServiceMock.Setup(x => x
                .CreateAccessTokenAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _sut.Login(model) as OkObjectResult;

            var response = result.Value as LoginResponse;

            // Assert
            response.AccessToken.Should().Be(expectedToken);
        }

        [Fact]
        public async Task Login_ValidModel_ReturnsRefreshTokenInCookies()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "User1",
                Password = "User1!@#"
            };

            var excpetedToken = "refresh-token";

            _authServiceMock.Setup(x => x
                .LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new LoginResult
                {
                    UserId = 1,
                    Message = LoginResultMessage.Successful
                });

            _tokenServiceMock.Setup(x => x
                .CreateRefreshTokenAsync(It.IsAny<int>()))
                .ReturnsAsync(excpetedToken);

            _responseCookiesMock.Setup(x => x
                .Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()));

            // Act
            var result = await _sut.Login(model) as OkObjectResult;

            // Assert
            _responseCookiesMock.Verify(x => x
                .Append(
                    It.Is<string>(x => x == "refresh_token"),
                    It.Is<string>(x => x == excpetedToken),
                    It.Is<CookieOptions>(x => x.HttpOnly == true)));
        }

        #endregion Login
    }
}