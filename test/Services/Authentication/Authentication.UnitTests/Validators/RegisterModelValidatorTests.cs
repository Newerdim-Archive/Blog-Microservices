using Authentication.API.Models;
using Authentication.API.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Authentication.UnitTests.Validators
{
    public class RegisterModelValidatorTests
    {
        private readonly RegisterModelValidator _validator = new();

        [Fact]
        public void ValidateAll_ValidModel_ReturnsTrue()
        {
            // Arrange
            var validModel = new RegisterModel
            {
                Username = "User1234",
                Email = "User1234@mail.com",
                Password = "Password123!",
                EmailConfirmationUrl = "http://www.website.com"
            };

            // Act
            var result = _validator.TestValidate(validModel);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        #region ValidateUsername

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateUsername_NullOrEmpty_ReturnsFalseWithError(string username)
        {
            // Arrange
            var model = new RegisterModel { Username = username };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public void ValidateUsername_TooLong_ReturnsFalseWithError()
        {
            // Arrange
            var model = new RegisterModel { Username = new string('a', 33) };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public void ValidateUsername_TooShort_ReturnsFalseWithError()
        {
            // Arrange
            var model = new RegisterModel { Username = new string('a', 2) };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public void ValidateUsername_InvalidSpecialCharacters_ReturnsFalseWithError()
        {
            // Arrange
            var model = new RegisterModel { Username = "@@#!$%%%!@" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        #endregion ValidateUsername

        #region ValidateEmail

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateEmail_NullOrEmpty_ReturnsFalseWithMessage(string email)
        {
            // Arrange
            var model = new RegisterModel { Email = email };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("@invalid.com")]
        [InlineData("invalid.com")]
        public void ValidateEmail_InvalidEmail_ReturnsFalseWithMessage(string email)
        {
            // Arrange
            var model = new RegisterModel { Email = email };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        #endregion ValidateEmail

        #region ValidatePassword

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidatePassword_NullOrEmpty_ReturnsFalseWithMessage(string password)
        {
            // Arrange
            var model = new RegisterModel { Password = password };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void ValidatePassword_TooShort_ReturnsFalseWithMessage()
        {
            // Arrange
            var model = new RegisterModel { Password = "Aa1!z" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void ValidatePassword_WithoutUppercase_ReturnsFalseWithMessage()
        {
            // Arrange
            var model = new RegisterModel { Password = "aaaaaaa1111" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void ValidatePassword_WithoutLowercase_ReturnsFalseWithMessage()
        {
            // Arrange
            var model = new RegisterModel { Password = "AAAAAAA1111" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void ValidatePassword_WithoutNumber_ReturnsFalseWithMessage()
        {
            // Arrange
            var model = new RegisterModel { Password = "AAAAAAAaaaaa" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        #endregion ValidatePassword

        #region ValidateEmailConfirmationUrl

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateEmailConfirmationUrl_NullOrEmpty_ReturnsFalseWithMessage(string emailConfirmationUrl)
        {
            // Arrange
            var model = new RegisterModel { EmailConfirmationUrl = emailConfirmationUrl };
            model.EmailConfirmationUrl = emailConfirmationUrl;

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EmailConfirmationUrl);
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test.")]
        public void ValidateEmailConfirmationUrl_Invalid_ReturnsFalseWithMessage(string emailConfirmationUrl)
        {
            // Arrange
            var model = new RegisterModel { EmailConfirmationUrl = emailConfirmationUrl };
            model.EmailConfirmationUrl = emailConfirmationUrl;

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EmailConfirmationUrl);
        }

        #endregion ValidateEmailConfirmationUrl
    }
}