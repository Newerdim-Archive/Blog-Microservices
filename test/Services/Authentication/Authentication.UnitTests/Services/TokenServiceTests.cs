using Authentication.API.Helpers;
using Authentication.API.Providers;
using Authentication.API.Services;
using Authentication.UnitTests.DataFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Services
{
    public class TokenServiceTests : IClassFixture<AuthSeedDataFixture>
    {
        private readonly Mock<IDateProvider> _dateProviderMock = new();
        private readonly Mock<IOptions<TokenSettings>> _tokenSettingsOptionsMock = new();

        private readonly ITokenService _sut;

        public TokenServiceTests()
        {
            _tokenSettingsOptionsMock.Setup(x => x.Value).Returns(new TokenSettings
            {
                AccessTokenSecret = "SecretForAccessToken",
                RefreshTokenSecret = "SecretForRefreshToken"
            });

            _sut = new TokenService(
                _dateProviderMock.Object,
                _tokenSettingsOptionsMock.Object);
        }

        #region GetUserIdFromToken

        [Fact]
        public void GetUserIdFromToken_ValidToken_ReturnsUserId()
        {
            // Arrange
            var token = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIxIiwicmVzIjoiZW1jIiwibmJmIjoxNjE5MjgzMzE2LCJleHAiOjE2MTkyODY5MTYsImlhdCI6MTYxOTI4MzMxNiwiaXNzIjoiQXV0aFNlcnZpY2UiLCJhdWQiOiJBdXRoU2VydmljZSJ9.M4FAkPYmMIGFEyjDUH6_Uuf36u5CV1jNEDHhW67MEUy1b-m1T0Nyj6ewLn27ASjMk_KiYU3-BoXmtL56SIenmw";

            // Act
            var userId = _sut.GetUserIdFromToken(token);

            // Assert
            userId.Should().Be(1);
        }

        [Fact]
        public void GetUserIdFromToken_InvalidToken_ReturnsUserId()
        {
            // Arrange
            var token = "invalid";

            // Act
            Action act = () => _sut.GetUserIdFromToken(token);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetUserIdFromToken_TokenWithoutUserId_ThrowsArgumentException()
        {
            // Arrange
            var token = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJyZXMiOiJlYmQiLCJuYmYiOjE2MTkyODMzMTYsImV4cCI6MTYxOTI4NjkxNiwiaWF0IjoxNjE5MjgzMzE2LCJpc3MiOiJBdXRoU2VydmljZSIsImF1ZCI6IkF1dGhTZXJ2aWNlIn0.lJ4EHFGQKc5OjFxismsLh4xgQV_IWuY1IUrZKVDFSArZ1mrUAtIA4Vdn1E05tKMzNQSxBBCg7jl3ystpBOX01w";

            // Act
            Action act = () => _sut.GetUserIdFromToken(token);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetUserIdFromToken_TokenWithEmptyUserId_ThrowsArgumentException()
        {
            // Arrange
            var token = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIiLCJyZXMiOiJlYmQiLCJuYmYiOjE2MTkyODMzMTYsImV4cCI6MTYxOTI4NjkxNiwiaWF0IjoxNjE5MjgzMzE2LCJpc3MiOiJBdXRoU2VydmljZSIsImF1ZCI6IkF1dGhTZXJ2aWNlIn0.3TRY9CSaAD3ai_NWmSteSRmu1nZy_Z0Iu_onJZpwembV32ekAKlQK2f7iR3E01sVqCeKks5hpQesmFPSZsBPcQ";

            // Act
            Action act = () => _sut.GetUserIdFromToken(token);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetUserIdFromToken_NullOrEmptyToken_ReturnsUserId(string nullOrEmptyToken)
        {
            // Arrange

            // Act
            Func<int> act = () => _sut.GetUserIdFromToken(nullOrEmptyToken);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        #endregion GetUserIdFromToken

        #region CreateAccessTokenAsync

        [Fact]
        public async Task CreateAccessTokenAsync_ValidUserId_ReturnsToken()
        {
            // Arrange
            _dateProviderMock.Setup(x => x
                .GetAfterUtcNow(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(DateTimeOffset.UtcNow.AddMinutes(15));

            // Act
            var result = await _sut.CreateAccessTokenAsync(1);

            // Assert
            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task CreateAccessTokenAsync_ValidUserId_ExpiresIn15Minutes()
        {
            // Arrange
            _dateProviderMock.Setup(x => x
                .GetAfterUtcNow(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(DateTimeOffset.UtcNow.AddMinutes(15));

            // Act
            var result = await _sut.CreateAccessTokenAsync(1);

            // Assert
            _dateProviderMock.Verify(x => x
                .GetAfterUtcNow(
                    It.Is<int>(x => x == 0), // days
                    It.Is<int>(x => x == 15))); // minutes
        }

        #endregion CreateAccessTokenAsync

        #region CreateRefreshTokenAsync

        [Fact]
        public async Task CreateRefreshTokenAsync_ValidUserId_ReturnsToken()
        {
            // Arrange

            _dateProviderMock.Setup(x => x
                .GetAfterUtcNow(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(DateTimeOffset.UtcNow.AddDays(15));

            // Act
            var result = await _sut.CreateRefreshTokenAsync(1);

            // Assert
            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task CreateRefreshTokenAsync_ValidUserId_ExpiresIn15Days()
        {
            // Arrange

            _dateProviderMock.Setup(x => x
                .GetAfterUtcNow(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(DateTimeOffset.UtcNow.AddDays(15));

            // Act
            var result = await _sut.CreateRefreshTokenAsync(1);

            // Assert
            _dateProviderMock.Verify(x => x
                .GetAfterUtcNow(
                    It.Is<int>(x => x == 15), // days
                    It.Is<int>(x => x == 0))); // minutes
        }

        #endregion CreateRefreshTokenAsync

        #region IsValidRefreshTokenAsync

        [Fact]
        public async Task IsValidRefreshTokenAsync_ValidToken_ReturnsTrue()
        {
            // Arrange
            var token = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIxIiwibmJmIjoxNjIxOTY5MjI4LCJleHAiOjIwMjE5NjI5MjgsImlhdCI6MTYyMTk2OTIyOH0.2ZvCxrG8YvSdyrdkps1lU1FS1rmi5xYprkDVYPg9I3r2aYiIAGWWOQCB7jFLLeuMvFiy97VSiuA_P7SvC7g5aQ";

            // Act
            var result = await _sut.IsValidRefreshTokenAsync(token);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsValidRefreshTokenAsync_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var token = "invalid";

            // Act
            var result = await _sut.IsValidRefreshTokenAsync(token);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        // Null userId. Valid until 2034
        [InlineData("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOm51bGwsIm5iZiI6MTYyMTk2OTIyOCwiZXhwIjoyMDIxOTYyOTI4LCJpYXQiOjE2MjE5NjkyMjh9.IKks3Oaz74CxfEcSjAo9Oyev43CMd6g3N3MVOmp9CjJtkyIbCdxkXqiSCL1fULTjCOBcVkcb-RuPiVxD2_-goA")]
        // Empty userId. Valid until 2034
        [InlineData("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIiLCJuYmYiOjE2MjE5NjkyMjgsImV4cCI6MjAyMTk2MjkyOCwiaWF0IjoxNjIxOTY5MjI4fQ.OtMrS7TKoQpSodYaschK4Sm7bKodqNR-vUptu0dY2TBGb_E5g-NJoZbnIS_R4dd9TRuqp8t9x-zwL55tGjJweA")]
        public async Task IsValidRefreshTokenAsync_NullOrEmptyUserIdInToken_ReturnsFalse(
            string token)
        {
            // Arrange

            // Act
            var result = await _sut.IsValidRefreshTokenAsync(token);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsValidRefreshTokenAsync_TokenNotHaveUserId_ReturnsFalse()
        {
            // Arrange
            // Valid until 2034
            var token = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE2MjE5NjkyMjgsImV4cCI6MjAyMTk2MjkyOCwiaWF0IjoxNjIxOTY5MjI4fQ.2OPngTHDQNhf4YuiwgOl-duBTS1BP__2orbasH74flILm9Hr9_YLjI8MTXAQbctzEixn3KUmBFy-X0aCEoW5zQ";

            // Act
            var result = await _sut.IsValidRefreshTokenAsync(token);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}