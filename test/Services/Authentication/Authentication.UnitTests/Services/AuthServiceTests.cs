using Authentication.API.Data;
using Authentication.API.Dtos;
using Authentication.API.Enums;
using Authentication.API.Providers;
using Authentication.API.Services;
using Authentication.UnitTests.DataFixture;
using AutoFixture.Xunit2;
using EmailSender.API.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Services
{
    public class AuthServiceTests : IClassFixture<AuthSeedDataFixture>
    {
        private readonly AuthDataContext _context;
        private readonly Mock<IDateProvider> _dateProviderMock;
        private readonly IAuthService _sut;

        public AuthServiceTests(AuthSeedDataFixture fixture)
        {
            _context = fixture.Context;
            _dateProviderMock = new();
            _sut = new AuthService(_context, _dateProviderMock.Object);
        }

        #region Register

        [Theory, AutoData]
        public async Task Register_ValidData_ReturnsSuccessfulAndUserId(
            string username,
            MailAddress email)
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = username,
                Email = email.Address,
                Password = "Password123!@#"
            };

            // Act
            var response = await _sut.RegisterAsync(request);

            // Assert
            response.UserId.Should().BeGreaterThan(0);
            response.Message.Should().Be(RegisterResultMessage.Successful);
        }

        [Theory, AutoData]
        public async Task Register_ValidData_CreateValidUserInDb(
            string username,
            MailAddress email)
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;

            var request = new RegisterRequest
            {
                Username = username,
                Email = email.Address,
                Password = "Password123!@#"
            };

            _dateProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

            // Act
            var response = await _sut.RegisterAsync(request);

            var userInDb = await _context.Users
                .FirstAsync(u => u.Id == response.UserId);

            // Assert
            userInDb.Username.Should().Be(username);
            userInDb.Email.Should().Be(email.Address);

            userInDb.PasswordHash.Should().NotBeEmpty();
            userInDb.PasswordSalt.Should().NotBeEmpty();

            userInDb.Created.Should().Be(now);
            userInDb.LastChange.Should().Be(now);
        }

        [Theory, AutoData]
        public async Task Register_ValidData_CreateValidPasswordHashAndSalt(
            string username,
            MailAddress email)
        {
            // Arrange
            var password = "Password123!@#";
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var request = new RegisterRequest
            {
                Username = username,
                Email = email.Address,
                Password = password
            };

            // Act
            var response = await _sut.RegisterAsync(request);

            var userInDb = await _context.Users.FirstAsync(u => u.Id == response.UserId);

            using var hmac = new HMACSHA512(userInDb.PasswordSalt);

            var computedPasswordHash = hmac.ComputeHash(passwordBytes);

            var isPasswordEquel = userInDb.PasswordHash.SequenceEqual(computedPasswordHash);

            // Assert
            isPasswordEquel.Should().BeTrue();
        }

        [Fact]
        public async Task Register_InvalidRequest_FluentValidationException()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = null,
                Email = null,
                Password = null
            };

            // Act
            Func<Task> act = async () => await _sut.RegisterAsync(request);

            // Assert
            await act.Should().ThrowAsync<FluentValidationException>();
        }

        [Fact]
        public async Task Register_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            Func<Task> act = async () => await _sut.RegisterAsync(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineAutoData("user1")]
        [InlineAutoData("USER1")]
        public async Task Register_UsernameAlreadyExists_ReturnsUsernameAlreadyExists(
            string username,
            MailAddress email)
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = username,
                Email = email.Address,
                Password = "Password123!@#"
            };

            // Act
            var response = await _sut.RegisterAsync(request);

            // Assert
            response.UserId.Should().Be(0);
            response.Message.Should().Be(RegisterResultMessage.UsernameAlreadyExists);
        }

        [Theory]
        [InlineAutoData("User1@mail.com")]
        [InlineAutoData("USER1@MAIL.COM")]
        public async Task Register_EmailAlreadyExists_ReturnsEmailAlreadyExists(
            string email,
            string username)
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = username,
                Email = email,
                Password = "Password123!@#"
            };

            // Act
            var response = await _sut.RegisterAsync(request);

            // Assert
            response.UserId.Should().Be(0);
            response.Message.Should().Be(RegisterResultMessage.EmailAlreadyExists);
        }

        #endregion Register

        #region Login

        [Fact]
        public async Task Login_ValidRequest_ReturnsSuccessfulMessage()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "User1",
                Password = "User1!@#"
            };

            // Act
            var result = await _sut.LoginAsync(request);

            // Assert
            result.Message.Should().Be(LoginResultMessage.Successful);
        }

        [Theory]
        [InlineData("User1", "User1!@#", 1)]
        [InlineData("User2", "User2!@#", 2)]
        public async Task Login_ValidRequest_ReturnsValidUserId(
            string username, 
            string password, 
            int expectedUserId)
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = username,
                Password = password
            };

            // Act
            var result = await _sut.LoginAsync(request);

            // Assert
            result.UserId.Should().Be(expectedUserId);
        }

        [Theory]
        [InlineData("uSER1", "User1!@#", 1)]
        [InlineData("uSER2", "User2!@#", 2)]
        public async Task Login_DifferentCaseOfLettersInUsername_ReturnsValidUserId(
            string username,
            string password,
            int expectedUserId)
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = username,
                Password = password
            };

            // Act
            var result = await _sut.LoginAsync(request);

            // Assert
            result.UserId.Should().Be(expectedUserId);
        }

        [Fact]
        public async Task Login_PasswordNotMatch_ReturnsPasswordNotMatchMessage()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "User1",
                Password = "notMatching"
            };

            // Act
            var result = await _sut.LoginAsync(request);

            // Assert
            result.UserId.Should().Be(0);
            result.Message.Should().Be(LoginResultMessage.PasswordNotMatch);
        }

        [Fact]
        public async Task Login_UserNotExist_ReturnsUserNotExistMessage()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "UserNotExist",
                Password = "Password123!"
            };

            // Act
            var result = await _sut.LoginAsync(request);

            // Assert
            result.UserId.Should().Be(0);
            result.Message.Should().Be(LoginResultMessage.UserNotExist);
        }

        [Fact]
        public async Task Login_InvalidRequest_ThrowsFluentValidationException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = null,
                Password = null
            };

            // Act
            Func<Task> act = async () => await _sut.LoginAsync(request);

            // Assert
            await act.Should().ThrowAsync<FluentValidationException>();
        }

        [Fact]
        public async Task Login_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            Func<Task> act = async () => await _sut.LoginAsync(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        #endregion Login
    }
}