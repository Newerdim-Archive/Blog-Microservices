using Authentication.API.Dtos;
using Authentication.API.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Validators
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator = new();

        [Fact]
        public async Task ValidateAll_ValidRequest_IsValidAndNotHaveAnyErrors()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "User123",
                Password = "Password123"
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
            result.IsValid.Should().BeTrue();
        }

        #region Username

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task ValidateUsername_NullOrEmpty_IsInvalidAndHaveUsernameError(
            string username)
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = username,
                Password = "Password123"
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Username);
            result.IsValid.Should().BeFalse();
        }

        #endregion

        #region Password

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task ValidatePassword_NullOrEmpty_IsInvalidAndHavePasswordError(
            string password)
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "User123",
                Password = password
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
            result.IsValid.Should().BeFalse();
        }

        #endregion
    }
}
