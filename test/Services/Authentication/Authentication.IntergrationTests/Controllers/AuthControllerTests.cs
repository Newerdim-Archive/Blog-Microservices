using Authentication.API;
using Authentication.API.Helpers;
using Authentication.API.Models;
using Authentication.API.Responses;
using FluentAssertions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.IntergrationTests.Controllers
{
    public class AuthControllerTests : IClassFixture<AuthWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(AuthWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        #region Register

        [Fact]
        public async Task Register_ValidModel_ReturnsOkWithUserIdAndMessage()
        {
            // Arrange
            var route = AuthControllerRoutes.Controller + "/" + AuthControllerRoutes.Register;
            var model = new RegisterModel
            {
                Username = "User1234",
                Email = "User1234@mail.com",
                Password = "Password123!",
                EmailConfirmationUrl = "http://www.website.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            var responseContent = await response.Content.ReadFromJsonAsync<RegisterResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Message.Should().Contain("successful");
            responseContent.UserId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Register_InvalidModel_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var route = AuthControllerRoutes.Controller + "/" + AuthControllerRoutes.Register;
            var model = new RegisterModel
            {
                Username = null,
                Password = null,
                Email = null,
                EmailConfirmationUrl = null
            };

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseContent.Should().Contain("validation errors occurred.");
        }

        [Fact]
        public async Task Register_EmailAlreadyExists_ReturnsUnauthorizedWithMessage()
        {
            // Arrange
            var route = AuthControllerRoutes.Controller + "/" + AuthControllerRoutes.Register;
            var model = new RegisterModel
            {
                Username = "User1234",
                Email = "User1@mail.com",
                Password = "Password123!",
                EmailConfirmationUrl = "http://www.website.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            responseContent.Should().Contain("email already exists");
        }

        [Fact]
        public async Task Register_UsernameAlreadyExists_ReturnsUnauthorizedWithMessage()
        {
            // Arrange
            var route = AuthControllerRoutes.Controller + "/" + AuthControllerRoutes.Register;
            var model = new RegisterModel
            {
                Username = "User1",
                Email = "User1345@mail.com",
                Password = "Password123!",
                EmailConfirmationUrl = "http://www.website.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            responseContent.Should().Contain("username already exists");
        }

        #endregion Register

        #region Login

        [Fact]
        public async Task Login_ValidModel_ReturnsOkWithMessage()
        {
            // Arrange
            var route = AuthControllerFullRoute.Login;

            var model = new LoginModel
            {
                Username = "User1",
                Password = "User1!@#"
            };

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Message.Should().Be("Logged in successfully");
        }

        [Fact]
        public async Task Login_InvalidModel_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var route = AuthControllerFullRoute.Login;

            var model = new LoginModel
            {
                Username = null,
                Password = null
            };

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("User1", "User1!@#", 1)]
        [InlineData("User2", "User2!@#", 2)]
        public async Task Login_ValidModel_ReturnsUserId(
            string username,
            string password,
            int expectedUserId)
        {
            // Arrange
            var route = AuthControllerFullRoute.Login;

            var model = new LoginModel
            {
                Username = username,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();

            // Assert
            content.UserId.Should().Be(expectedUserId);
        }

        [Fact]
        public async Task Login_PasswordNotMatch_ReturnsUnauthorizedWithMessage()
        {
            // Arrange
            var route = AuthControllerFullRoute.Login;

            var model = new LoginModel
            {
                Username = "User1",
                Password = "notMatchingPassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            var content = await response.Content.ReadFromJsonAsync<string>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().Be("Password does not match");
        }

        [Fact]
        public async Task Login_UserNotExist_ReturnsUnauthorizedWithMessage()
        {
            // Arrange
            var route = AuthControllerFullRoute.Login;

            var model = new LoginModel
            {
                Username = "UserNotExist123",
                Password = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            var content = await response.Content.ReadFromJsonAsync<string>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().Be("User does not exist");
        }

        [Fact]
        public async Task Login_ValidModel_ReturnsAccessToken()
        {
            // Arrange
            var route = AuthControllerFullRoute.Login;

            var model = new LoginModel
            {
                Username = "User1",
                Password = "User1!@#"
            };

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();

            // Assert
            content.AccessToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_ValidModel_ReturnsRefreshTokenInCookies()
        {
            // Arrange
            var route = AuthControllerFullRoute.Login;

            var model = new LoginModel
            {
                Username = "User1",
                Password = "User1!@#"
            };

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            var cookies = response.Headers.GetValues("Set-Cookie");

            var refreshToken = cookies.FirstOrDefault(c => c.Contains("refresh_token"));

            // Assert
            refreshToken.Should().NotBeNullOrWhiteSpace();
        }

        #endregion Login
    }
}