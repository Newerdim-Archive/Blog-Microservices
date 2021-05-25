using Authentication.API.Models;
using Authentication.API.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Validators
{
    public class LoginModelValidatorTests
    {
        private readonly LoginModelValidator _validator = new();

        [Fact]
        public async Task ValidateAll_ValidModel_ReturnsNoErrors()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "User1",
                Password = "User1!@#"
            };

            // Act
            var result = await _validator.TestValidateAsync(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidateAll_NullOrEmptyUsername_ReturnsError(string username)
        {
            // Arrange
            var model = new LoginModel
            {
                Username = username,
            };

            // Act
            var result = await _validator.TestValidateAsync(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Username);
            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidateAll_NullOrEmptyPassword_ReturnsError(string password)
        {
            // Arrange
            var model = new LoginModel
            {
                Password = password
            };

            // Act
            var result = await _validator.TestValidateAsync(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
            result.IsValid.Should().BeFalse();
        }
    }
}