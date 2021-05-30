using Authentication.API.Controllers;
using Authentication.API.Dtos;
using Authentication.API.Enums;
using Authentication.API.Helpers;
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

        private readonly Mock<IResponseCookies> _responseCookiesMock = new();
        private readonly Mock<IRequestCookieCollection> _requestCookieCollectionMock = new();

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
                    It.Is<string>(key => key == CookieName.RefreshToken),
                    It.Is<string>(value => value == excpetedToken),
                    It.Is<CookieOptions>(opt => opt.HttpOnly == true)));
        }

        #endregion Login

        #region RefreshTokens

        [Fact]
        public async Task RefreshTokens_ValidRefreshToken_ReturnsOkWithMessage()
        {
            // Arrange
            var refreshToken = "valid";

            _requestCookieCollectionMock.Setup(x => x
                .TryGetValue(It.IsAny<string>(), out refreshToken))
                .Returns(true);

            _tokenServiceMock.Setup(x => x
                .IsValidRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _tokenServiceMock.Setup(x => x
                .CreateRefreshTokenAsync(It.IsAny<int>()))
                .ReturnsAsync("token");

            // Act
            var result = await _sut.RefreshTokens() as OkObjectResult;
            
            var response = result.Value as RefreshTokensResponse;

            // Assert
            result.Should().NotBeNull();
            response.Message.Should().Be("Tokens refreshed successfully");
        }

        [Fact]
        public async Task RefreshTokens_ValidRefreshToken_RefreshOldRefreshToken()
        {
            // Arrange
            var expectedRefreshToken = "newToken";

            _requestCookieCollectionMock.Setup(x => x
                .TryGetValue(It.IsAny<string>(), out expectedRefreshToken))
                .Returns(true);

            _tokenServiceMock.Setup(x => x
                .IsValidRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _tokenServiceMock.Setup(x => x
                .CreateRefreshTokenAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedRefreshToken);

            // Act
            var result = await _sut.RefreshTokens() as OkObjectResult;

            var response = result.Value as RefreshTokensResponse;

            // Assert
            _responseCookiesMock.Verify(x => x
                .Append(
                    It.Is<string>(key => key == "refresh_token"),
                    It.Is<string>(value => value == expectedRefreshToken),
                    It.Is<CookieOptions>(opt => opt.HttpOnly == true)));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task RefreshTokens_NullOrEmptyRefreshToken_ReturnsUnauthorizedWithMessage(
            string refreshToken)
        {
            // Arrange
            // Without this visual studio show that refreshToken isn't used
            var outRefreshToken = refreshToken;

            _requestCookieCollectionMock.Setup(x => x
                .TryGetValue(It.IsAny<string>(), out outRefreshToken))
                .Returns(true);

            // Act
            var result = await _sut.RefreshTokens() as UnauthorizedObjectResult;

            var response = result.Value as string;

            // Assert
            result.Should().NotBeNull();
            response.Should().Be("Refresh token is empty or does not exists");
        }

        [Fact]
        public async Task RefreshTokens_RefreshTokenNotExist_ReturnsUnauthorizedWithMessage()
        {
            // Arrange
            string outRefreshToken = null;

            _requestCookieCollectionMock.Setup(x => x
                .TryGetValue(It.IsAny<string>(), out outRefreshToken))
                .Returns(false);

            // Act
            var result = await _sut.RefreshTokens() as UnauthorizedObjectResult;

            var response = result.Value as string;

            // Assert
            result.Should().NotBeNull();
            response.Should().Be("Refresh token is empty or does not exists");
        }

        [Fact]
        public async Task RefreshTokens_InvalidRefreshToken_ReturnsUnauthorizedWithMessage()
        {
            // Arrange
            string refreshToken = "invalid";

            _requestCookieCollectionMock.Setup(x => x
                .TryGetValue(It.IsAny<string>(), out refreshToken))
                .Returns(true);

            _tokenServiceMock.Setup(x => x
                .IsValidRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.RefreshTokens() as UnauthorizedObjectResult;

            var response = result.Value as string;

            // Assert
            result.Should().NotBeNull();
            response.Should().Be("Refresh token is invalid");
        }

        [Fact]
        public async Task RefreshTokens_NullOrEmptyUserIdInRefreshToken_ReturnsUnauthorizedWithMessage()
        {
            // Arrange
            string refreshToken = "invalid";

            _requestCookieCollectionMock.Setup(x => x
                .TryGetValue(It.IsAny<string>(), out refreshToken))
                .Returns(true);

            _tokenServiceMock.Setup(x => x
                .IsValidRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.RefreshTokens() as UnauthorizedObjectResult;

            var response = result.Value as string;

            // Assert
            result.Should().NotBeNull();
            response.Should().Be("Refresh token is invalid");
        }

        [Fact]
        public async Task RefreshTokens_RefreshTokenNotHaveUserId_ReturnsUnauthorizedWithMessage()
        {
            // Arrange
            string refreshToken = "invalid";

            _requestCookieCollectionMock.Setup(x => x
                .TryGetValue(It.IsAny<string>(), out refreshToken))
                .Returns(true);

            _tokenServiceMock.Setup(x => x
                .IsValidRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.RefreshTokens() as UnauthorizedObjectResult;

            var response = result.Value as string;

            // Assert
            result.Should().NotBeNull();
            response.Should().Be("Refresh token is invalid");
        }

        [Fact]
        public async Task RefreshTokens_ValidRefreshToken_ReturnsAccessToken()
        {
            // Arrange
            var refreshToken = "token";
            var expectedAccessToken = "token";

            _requestCookieCollectionMock.Setup(x => x
                .TryGetValue(It.IsAny<string>(), out refreshToken))
                .Returns(true);

            _tokenServiceMock.Setup(x => x
                .IsValidRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _tokenServiceMock.Setup(x => x
                .CreateRefreshTokenAsync(It.IsAny<int>()))
                .ReturnsAsync("refreshToken");

            _tokenServiceMock.Setup(x => x
                .CreateAccessTokenAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedAccessToken);

            // Act
            var result = await _sut.RefreshTokens() as OkObjectResult;

            var response = result.Value as RefreshTokensResponse;

            // Assert
            response.AccessToken.Should().NotBeNullOrWhiteSpace();
        }

        #endregion

        #region Private Methods

        private ControllerContext CreateControllerContext()
        {
            var httpContextMock = new Mock<HttpContext>();
            var httpResponseMock = new Mock<HttpResponse>();
            var httpRequestMock = new Mock<HttpRequest>();

            httpResponseMock.Setup(x => x.Cookies)
                .Returns(_responseCookiesMock.Object);

            httpRequestMock.Setup(x => x.Cookies)
                .Returns(_requestCookieCollectionMock.Object);

            httpContextMock.Setup(x => x.Response)
                .Returns(httpResponseMock.Object);

            httpContextMock.Setup(x => x.Request)
                .Returns(httpRequestMock.Object);

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            return controllerContext;
        }

        #endregion Private Methods
    }
}