using Authentication.API.Dtos;
using Authentication.API.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Validators
{
    public class RegisterRequestValidatorTests
    {
        private readonly RegisterRequestValidator _validator = new();

        [Fact]
        public async Task ValidateAll_ValidRequest_IsValidAndNotHaveAnyErrors()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "User123",
                Password = "Password123",
                Email = "User123@mail.com"
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
            var request = new RegisterRequest
            {
                Username = username,
                Password = "Password123",
                Email = "User123@mail.com"
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
            var request = new RegisterRequest
            {
                Username = "User123",
                Password = password,
                Email = "User123@mail.com"
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
            result.IsValid.Should().BeFalse();
        }

        #endregion

        #region Email

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task ValidateEmail_NullOrEmpty_IsInvalidAndHavePasswordError(
            string email)
        {
            var request = new RegisterRequest
            {
                Username = "User123",
                Password = "Password123",
                Email = email
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
            result.IsValid.Should().BeFalse();
        }

        #endregion
    }
}
