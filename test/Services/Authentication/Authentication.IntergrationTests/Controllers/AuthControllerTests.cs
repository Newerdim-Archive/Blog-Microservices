using Authentication.API;
using Authentication.API.Helpers;
using Authentication.API.Models;
using Authentication.API.Responses;
using FluentAssertions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        #region RefreshTokens

        [Fact]
        public async Task RefreshTokens_ValidModel_ReturnsOkWithMessage()
        {
            // Arrange
            (var accessToken, var refreshToken) = await GetTokensAsync();
            AddAuthorization(accessToken, refreshToken);

            var route = AuthControllerFullRoute.RefreshTokens;

            // Act
            var response = await _client.PostAsync(route, null);

            var content = await response.Content.ReadFromJsonAsync<RefreshTokensResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Message.Should().Be("Tokens refreshed successfully");
        }

        [Fact]
        public async Task RefreshTokens_ValidRefreshToken_ReplaceOldRefreshToken()
        {
            // Arrange
            (var accessToken, var refreshToken) = await GetTokensAsync();
            AddAuthorization(accessToken, refreshToken);

            var route = AuthControllerFullRoute.RefreshTokens;

            // Act
            var response = await _client.PostAsync(route, null);

            var refreshTokenFromResponse = GetRefreshTokenFromResponse(response);

            // Assert
            refreshTokenFromResponse.Should().NotBeNullOrWhiteSpace(refreshToken);
            refreshTokenFromResponse.Should().NotBe(refreshToken);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task RefreshTokens_NullOrEmptyRefreshToken_ReturnsUnauthorizedWithMessage(
            string refreshToken)
        {
            // Arrange
            AddAuthorization(null, refreshToken);

            var route = AuthControllerFullRoute.RefreshTokens;

            // Act
            var response = await _client.PostAsync(route, null);

            var content = await response.Content.ReadFromJsonAsync<string>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().Be("Refresh token is empty or does not exists");
        }

        [Fact]
        public async Task RefreshTokens_RefreshTokenNotExist_ReturnsUnauthorizedWithMessage()
        {
            // Arrange
            var route = AuthControllerFullRoute.RefreshTokens;

            // Act
            var response = await _client.PostAsync(route, null);

            var content = await response.Content.ReadFromJsonAsync<string>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().Be("Refresh token is empty or does not exists");
        }

        [Fact]
        public async Task RefreshTokens_InvalidRefreshToken_ReturnsUnauthorizedWithMessage()
        {
            // Arrange
            AddAuthorization(null, "invalidToken");

            var route = AuthControllerFullRoute.RefreshTokens;

            // Act
            var response = await _client.PostAsync(route, null);

            var content = await response.Content.ReadFromJsonAsync<string>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().Be("Refresh token is invalid");
        }

        [Theory]
        // Empty userId. Valid until 2034
        [InlineData("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIiLCJuYmYiOjE2MjE5NjkyMjgsImV4cCI6MjAyMTk2MjkyOCwiaWF0IjoxNjIxOTY5MjI4fQ.KzwB5Eb9XHp_jTCSJHG6c7y80u0rmra1UMjLFC7XYV0n0TJ0yqPh_jG4rBM6OHLP8G6GvrX0IIyv66cc4RCd3A")]
        // Null userId. Valid until 2034
        [InlineData("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOm51bGwsIm5iZiI6MTYyMTk2OTIyOCwiZXhwIjoyMDIxOTYyOTI4LCJpYXQiOjE2MjE5NjkyMjh9.dDS-C30YjEb011dRkFhQKpH_3YryGVXPvfoP3Y2RQwgfzjy3AzB4BUNR91sBhf13KNqG4Acdak27RUJvTpNQFw")]
        public async Task RefreshTokens_NullOrEmptyUserIdInRefreshToken_ReturnsUnauthorizedWithMessage(
            string token)
        {
            // Arrange
            AddAuthorization(null, token);

            var route = AuthControllerFullRoute.RefreshTokens;

            // Act
            var response = await _client.PostAsync(route, null);

            var content = await response.Content.ReadFromJsonAsync<string>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().Be("Refresh token is invalid");
        }

        [Fact]
        public async Task RefreshTokens_RefreshTokenNotHaveUserId_ReturnsUnauthorizedWithMessage()
        {
            // Arrange

            // Valid until 2034
            var token = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE2MjE5NjkyMjgsImV4cCI6MjAyMTk2MjkyOCwiaWF0IjoxNjIxOTY5MjI4fQ.df2FfrVJTYoEFo5dhy4pmoXdKIauaECUHXE7B-mIgM-V1r5tFTq1fY7ffvsHWd7oYjb0iiPJMk0GqHgO5AURcw";
            
            AddAuthorization(null, token);

            var route = AuthControllerFullRoute.RefreshTokens;

            // Act
            var response = await _client.PostAsync(route, null);

            var content = await response.Content.ReadFromJsonAsync<string>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().Be("Refresh token is invalid");
        }

        [Fact]
        public async Task RefreshTokens_ValidRefreshToken_ReturnsAccessToken()
        {
            // Arrange
            var (accessToken, refreshToken) = await GetTokensAsync();
            AddAuthorization(null, refreshToken);

            var route = AuthControllerFullRoute.RefreshTokens;

            // Act
            var response = await _client.PostAsync(route, null);

            var content = await response.Content.ReadFromJsonAsync<RefreshTokensResponse>();

            // Assert
            content.AccessToken.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Get refresh token from response
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Token if exists, otherwise null</returns>
        private static string GetRefreshTokenFromResponse(HttpResponseMessage response)
        {
            var cookies = response.Headers.GetValues("Set-Cookie");
            return cookies.FirstOrDefault(x => x.StartsWith("refresh_token"));
        }

        /// <summary>
        /// Send login request and add access token to header
        /// and refresh token to cookies in HttpClient
        /// </summary>
        /// <returns></returns>
        private async Task<(string accessToken, string refreshToken)> GetTokensAsync()
        {
            var route = AuthControllerFullRoute.Login;

            var model = new LoginModel
            {
                Username = "User1",
                Password = "User1!@#"
            };

            var response = await _client.PostAsJsonAsync(route, model);

            var cookies = response.Headers.GetValues("Set-Cookie");
            var refreshToken = cookies.FirstOrDefault(x => x.StartsWith("refresh-token"));

            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
            var accessToken = content.AccessToken;

            return (accessToken, refreshToken);
        }

        /// <summary>
        /// Add access token to header as bearer and refresh token to cookies
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="refreshToken"></param>
        private void AddAuthorization(string accessToken, string refreshToken)
        {
            _client.DefaultRequestHeaders.Add("Cookie", $"refresh_token={refreshToken};");

            var authenticationHeader = new AuthenticationHeaderValue("Bearer", accessToken);

            _client.DefaultRequestHeaders.Authorization = authenticationHeader;
        }

        #endregion
    }
}