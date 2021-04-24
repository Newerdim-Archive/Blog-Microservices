using Authentication.API.Data;
using Authentication.API.Helpers;
using Authentication.API.Providers;
using Authentication.API.Services;
using Authentication.UnitTests.DataFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Services
{
    public class TokenServiceTests : IClassFixture<AuthSeedDataFixture>
    {
        private readonly AuthDataContext _context;
        private readonly Mock<IDateProvider> _dateProviderMock = new();
        private readonly Mock<IOptions<TokenSettings>> _tokenSettingsOptionsMock = new();

        public TokenServiceTests(AuthSeedDataFixture fixture)
        {
            _context = fixture.Context;
            _tokenSettingsOptionsMock.Setup(x => x.Value).Returns(new TokenSettings
            {
                EmailConfirmationSecret = "SecretForEmailConfirmation"
            });
        }

        private ITokenService CreateService()
        {

            return new TokenService(
                _context,
                _dateProviderMock.Object,
                _tokenSettingsOptionsMock.Object);
        }

        #region CreateEmailConfirmationTokenAsync

        [Fact]
        public async Task CreateEmailConfirmationTokenAsync_UserExists_ReturnsToken()
        {
            // Arrange
            var sut = CreateService();

            // Act
            var result = await sut.CreateEmailConfirmationTokenAsync(1);

            // Assert
            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task CreateEmailConfirmationTokenAsync_UserIdIs0_ThrowsArgumentException()
        {
            // Arrange
            var sut = CreateService();

            // Act
            Func<Task<string>> act = async () => await sut.CreateEmailConfirmationTokenAsync(0);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateEmailConfirmationTokenAsync_UserNotExists_ThrowsArgumentException()
        {
            // Arrange
            var sut = CreateService();

            // Act
            Func<Task<string>> act = async () => await sut.CreateEmailConfirmationTokenAsync(999);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        #endregion

        #region IsValidEmailConfirmationTokenAsync

        [Fact]
        public async Task IsValidEmailConfirmationTokenAsync_ValidToken_ReturnsTrue()
        {
            // Arrange
            var validToken = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIxIiwicmVzIjoiZW1jIiwibmJmIjoxNjE5MjgzMzE2LCJleHAiOjE2MTkyODY5MTYsImlhdCI6MTYxOTI4MzMxNiwiaXNzIjoiQXV0aFNlcnZpY2UiLCJhdWQiOiJBdXRoU2VydmljZSJ9.M4FAkPYmMIGFEyjDUH6_Uuf36u5CV1jNEDHhW67MEUy1b-m1T0Nyj6ewLn27ASjMk_KiYU3-BoXmtL56SIenmw";
            var sut = CreateService();

            // Act
            var result = await sut.IsValidEmailConfirmationTokenAsync(validToken);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsValidEmailConfirmationTokenAsync_WrongReason_ReturnsFalse()
        {
            // Arrange
            var invalidToken = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIxIiwicmVzIjoiZWJkIiwibmJmIjoxNjE5MjgzMzE2LCJleHAiOjE2MTkyODY5MTYsImlhdCI6MTYxOTI4MzMxNiwiaXNzIjoiQXV0aFNlcnZpY2UiLCJhdWQiOiJBdXRoU2VydmljZSJ9.D0R2vVet6lz7OvTZ6bj6RKmyVi_q1IwfrL1PmFalHoJxO18R56FfM__gCHg4o_cKAN__CLs9k_GuduiTWZ1kFA";

            var sut = CreateService();

            // Act
            var result = await sut.IsValidEmailConfirmationTokenAsync(invalidToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsValidEmailConfirmationTokenAsync_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var invalidToken = "invalid";

            var sut = CreateService();

            // Act
            var result = await sut.IsValidEmailConfirmationTokenAsync(invalidToken);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task IsValidEmailConfirmationTokenAsync_EmptyAndNullToken_ThrowsArgumentException(string emptyOrNullToken)
        {
            // Arrange
            var sut = CreateService();

            // Act
            Func<Task<bool>> act = async () => await sut.IsValidEmailConfirmationTokenAsync(emptyOrNullToken);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        #endregion

        #region GetUserIdFromToken

        [Fact]
        public void GetUserIdFromToken_ValidToken_ReturnsUserId()
        {
            // Arrange
            var validToken = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIxIiwicmVzIjoiZW1jIiwibmJmIjoxNjE5MjgzMzE2LCJleHAiOjE2MTkyODY5MTYsImlhdCI6MTYxOTI4MzMxNiwiaXNzIjoiQXV0aFNlcnZpY2UiLCJhdWQiOiJBdXRoU2VydmljZSJ9.M4FAkPYmMIGFEyjDUH6_Uuf36u5CV1jNEDHhW67MEUy1b-m1T0Nyj6ewLn27ASjMk_KiYU3-BoXmtL56SIenmw";

            var sut = CreateService();

            // Act
            var userId = sut.GetUserIdFromToken(validToken);

            // Assert
            userId.Should().Be(1);
        }

        [Fact]
        public void GetUserIdFromToken_InvalidToken_ReturnsUserId()
        {
            // Arrange
            var validToken = "invalid";

            var sut = CreateService();

            // Act
            var userId = sut.GetUserIdFromToken(validToken);

            // Assert
            userId.Should().Be(0);
        }

        [Fact]
        public void GetUserIdFromToken_TokenWithoutUserId_ReturnsUserId()
        {
            // Arrange
            var validToken = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJyZXMiOiJlYmQiLCJuYmYiOjE2MTkyODMzMTYsImV4cCI6MTYxOTI4NjkxNiwiaWF0IjoxNjE5MjgzMzE2LCJpc3MiOiJBdXRoU2VydmljZSIsImF1ZCI6IkF1dGhTZXJ2aWNlIn0.lJ4EHFGQKc5OjFxismsLh4xgQV_IWuY1IUrZKVDFSArZ1mrUAtIA4Vdn1E05tKMzNQSxBBCg7jl3ystpBOX01w";

            var sut = CreateService();

            // Act
            var userId = sut.GetUserIdFromToken(validToken);

            // Assert
            userId.Should().Be(0);
        }

        [Fact]
        public void GetUserIdFromToken_TokenWithEmptyUserId_ReturnsUserId()
        {
            // Arrange
            var validToken = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIiLCJyZXMiOiJlYmQiLCJuYmYiOjE2MTkyODMzMTYsImV4cCI6MTYxOTI4NjkxNiwiaWF0IjoxNjE5MjgzMzE2LCJpc3MiOiJBdXRoU2VydmljZSIsImF1ZCI6IkF1dGhTZXJ2aWNlIn0.3TRY9CSaAD3ai_NWmSteSRmu1nZy_Z0Iu_onJZpwembV32ekAKlQK2f7iR3E01sVqCeKks5hpQesmFPSZsBPcQ";

            var sut = CreateService();

            // Act
            var userId = sut.GetUserIdFromToken(validToken);

            // Assert
            userId.Should().Be(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetUserIdFromToken_NullOrEmptyToken_ReturnsUserId(string nullOrEmptyToken)
        {
            // Arrange
            var sut = CreateService();

            // Act
            Func<int> act = () => sut.GetUserIdFromToken(nullOrEmptyToken);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        #endregion
    }
}
