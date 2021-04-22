using Authentication.API.Data;
using Authentication.API.Dtos;
using Authentication.API.Enums;
using Authentication.API.Helpers;
using Authentication.API.Providers;
using Authentication.API.Services;
using Authentication.UnitTests.DataFixture;
using AutoFixture.Xunit2;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
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

        public AuthServiceTests(AuthSeedDataFixture fixture)
        {
            _context = fixture.Context;
            _dateProviderMock = new ();
        }

        private IAuthService CreateService()
        {
            return new AuthService(_context, _dateProviderMock.Object);
        }

        #region Register

        [Theory, AutoData]
        public async Task Register_ValidData_ReturnsSuccessfulAndUserId(string username, MailAddress email, string password)
        {
            // Arrange
            var sut = CreateService();
            var request = new RegisterRequest
            {
                Username = username,
                Email = email.Address,
                Password = password
            };

            // Act
            var response = await sut.RegisterAsync(request);

            // Assert
            response.UserId.Should().BeGreaterThan(0);
            response.Result.Should().Be(RegisterResultMessage.Successful);
        }

        [Theory, AutoData]
        public async Task Register_ValidData_CreateValidUserInDb(string username, MailAddress email, string password)
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            _dateProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

            var sut = CreateService();
            var request = new RegisterRequest
            {
                Username = username,
                Email = email.Address,
                Password = password
            };

            // Act
            var response = await sut.RegisterAsync(request);

            var userInDb = await _context.Users.FirstAsync(u => u.Id == response.UserId);

            // Assert

            userInDb.Username.Should().Be(username);
            userInDb.Email.Should().Be(email.Address);
            userInDb.PasswordHash.Should().NotBeEmpty();
            userInDb.PasswordSalt.Should().NotBeEmpty();
            userInDb.ConfirmedEmail.Should().BeFalse();
            userInDb.Created.Should().Be(now);
            userInDb.LastChange.Should().Be(now);
        }

        [Theory, AutoData]
        public async Task Register_ValidData_CreateValidPasswordHashAndSalt(string username, MailAddress email, string password)
        {
            // Arrange
            var sut = CreateService();
            var request = new RegisterRequest
            {
                Username = username,
                Email = email.Address,
                Password = password
            };

            // Act
            var response = await sut.RegisterAsync(request);

            var userInDb = await _context.Users.FirstAsync(u => u.Id == response.UserId);

            using var hmac = new HMACSHA512(userInDb.PasswordSalt);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var computedHash = hmac.ComputeHash(passwordBytes);

            var result = userInDb.PasswordHash.SequenceEqual(computedHash);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineAutoData("")]
        [InlineAutoData(null)]
        public async Task Register_NullEmptyUsername_ThrowsArgumentException(string username, MailAddress email, string password)
        {
            // Arrange
            var sut = CreateService();
            var request = new RegisterRequest
            {
                Username = username,
                Email = email.Address,
                Password = password
            };

            // Act
            Func<Task> act = async () => await sut.RegisterAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Theory]
        [InlineAutoData("")]
        [InlineAutoData(null)]
        public async Task Register_NullEmptyEmail_ThrowsArgumentException(string email, string username, string password)
        {
            // Arrange
            var sut = CreateService();
            var request = new RegisterRequest
            {
                Username = username,
                Email = email,
                Password = password
            };

            // Act
            Func<Task> act = async () => await sut.RegisterAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Theory]
        [InlineAutoData("")]
        [InlineAutoData(null)]
        public async Task Register_NullEmptyPassword_ThrowsArgumentException(string password, MailAddress email, string username)
        {
            // Arrange
            var sut = CreateService();
            var request = new RegisterRequest
            {
                Username = username,
                Email = email.Address,
                Password = password
            };

            // Act
            Func<Task> act = async () => await sut.RegisterAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Register_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var sut = CreateService();

            // Act
            Func<Task> act = async () => await sut.RegisterAsync(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineAutoData("user1")]
        [InlineAutoData("USER1")]
        public async Task Register_UsernameAlreadyExists_ReturnsSuccessfulAndUserId(string username, MailAddress email, string password)
        {
            // Arrange
            var sut = CreateService();
            var request = new RegisterRequest
            {
                Username = username,
                Email = email.Address,
                Password = password
            };

            // Act
            var response = await sut.RegisterAsync(request);

            // Assert
            response.UserId.Should().Be(0);
            response.Result.Should().Be(RegisterResultMessage.UsernameAlreadyExists);
        }

        [Theory]
        [InlineAutoData("user1@user.com")]
        [InlineAutoData("USER1@USER.COM")]
        public async Task Register_EmailAlreadyExists_ReturnsSuccessfulAndUserId(string email, string username, string password)
        {
            // Arrange
            var sut = CreateService();
            var request = new RegisterRequest
            {
                Username = username,
                Email = email,
                Password = password
            };

            // Act
            var response = await sut.RegisterAsync(request);

            // Assert
            response.UserId.Should().Be(0);
            response.Result.Should().Be(RegisterResultMessage.EmailAlreadyExists);
        }

        #endregion
    }
}
