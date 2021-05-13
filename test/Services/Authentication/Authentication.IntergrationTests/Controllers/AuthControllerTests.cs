using Authentication.API;
using Authentication.API.Helpers;
using Authentication.API.Models;
using Authentication.API.Responses;
using FluentAssertions;
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

        private static RegisterModel CreateValidRegisterModel()
        {
            return new RegisterModel
            {
                Username = "User1234",
                Email = "User1234@mail.com",
                Password = "Password123!",
                EmailConfirmationUrl = "http://www.website.com"
            };
        }

        [Fact]
        public async Task Register_ValidModel_ReturnsOkWithUserIdAndMessage()
        {
            // Arrange
            var route = AuthControllerRoutes.Controller + "/" + AuthControllerRoutes.Register;
            var model = CreateValidRegisterModel();

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
            var model = CreateValidRegisterModel();
            model.Email = "User1@mail.com";

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
            var model = CreateValidRegisterModel();
            model.Username = "User1";

            // Act
            var response = await _client.PostAsJsonAsync(route, model);

            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            responseContent.Should().Contain("username already exists");
        }
    }
}