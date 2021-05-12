using Authentication.API.Models;
using Authentication.API.Validators;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Validators
{
    public class RegisterModelValidatorTests
    {
        private static RegisterModel CreateValidModel()
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
        public void ValidateAll_ValidModel_ReturnsTrue()
        {
            // Arrange
            var validModel = CreateValidModel();
            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(validModel);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #region ValidateUsername

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateUsername_NullOrEmpty_ReturnsFalseWithError(string username)
        {
            // Arrange
            var model = CreateValidModel();
            model.Username = username;

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyUsernameError = result.Errors.Any(e => e.PropertyName == nameof(model.Username));
            isAnyUsernameError.Should().BeTrue();
        }

        [Fact]
        public void ValidateUsername_TooLong_ReturnsFalseWithError()
        {
            // Arrange
            var model = CreateValidModel();
            model.Username = new string('a', 33);

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyUsernameError = result.Errors.Any(e => e.PropertyName == nameof(model.Username));
            isAnyUsernameError.Should().BeTrue();
        }

        [Fact]
        public void ValidateUsername_TooShort_ReturnsFalseWithError()
        {
            // Arrange
            var model = CreateValidModel();
            model.Username = new string('a', 2);

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyUsernameError = result.Errors.Any(e => e.PropertyName == nameof(model.Username));
            isAnyUsernameError.Should().BeTrue();
        }

        [Fact]
        public void ValidateUsername_InvalidSpecialCharacters_ReturnsFalseWithError()
        {
            // Arrange
            var model = CreateValidModel();
            model.Username = "Invalid !@#?";

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyUsernameError = result.Errors.Any(e => e.PropertyName == nameof(model.Username));
            isAnyUsernameError.Should().BeTrue();
        }

        #endregion

        #region ValidateEmail

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateEmail_NullOrEmpty_ReturnsFalseWithMessage(string email)
        {
            // Arrange
            var model = CreateValidModel();
            model.Email = email;

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyEmailError = result.Errors.Any(e => e.PropertyName == nameof(model.Email));
            isAnyEmailError.Should().BeTrue();
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("@invalid.com")]
        [InlineData("invalid.com")]
        public void ValidateEmail_InvalidEmail_ReturnsFalseWithMessage(string email)
        {
            // Arrange
            var model = CreateValidModel();
            model.Email = email;

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyEmailError = result.Errors.Any(e => e.PropertyName == nameof(model.Email));
            isAnyEmailError.Should().BeTrue();
        }

        #endregion

        #region ValidatePassword

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidatePassword_NullOrEmpty_ReturnsFalseWithMessage(string password)
        {
            // Arrange
            var model = CreateValidModel();
            model.Password = password;

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyPasswordError = result.Errors.Any(e => e.PropertyName == nameof(model.Password));
            isAnyPasswordError.Should().BeTrue();
        }

        [Fact]
        public void ValidatePassword_TooShort_ReturnsFalseWithMessage()
        {
            // Arrange
            var model = CreateValidModel();
            model.Password = "Aa1!z";

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyPasswordError = result.Errors.Any(e => e.PropertyName == nameof(model.Password));
            isAnyPasswordError.Should().BeTrue();
        }

        [Fact]
        public void ValidatePassword_WithoutUppercase_ReturnsFalseWithMessage()
        {
            // Arrange
            var model = CreateValidModel();
            model.Password = "aaaaaaa1111";

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyPasswordError = result.Errors.Any(e => e.PropertyName == nameof(model.Password));
            isAnyPasswordError.Should().BeTrue();
        }

        [Fact]
        public void ValidatePassword_WithoutLowercase_ReturnsFalseWithMessage()
        {
            // Arrange
            var model = CreateValidModel();
            model.Password = "AAAAAAA1111";

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyPasswordError = result.Errors.Any(e => e.PropertyName == nameof(model.Password));
            isAnyPasswordError.Should().BeTrue();
        }

        [Fact]
        public void ValidatePassword_WithoutNumber_ReturnsFalseWithMessage()
        {
            // Arrange
            var model = CreateValidModel();
            model.Password = "AAAAAAAaaaaa";

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyPasswordError = result.Errors.Any(e => e.PropertyName == nameof(model.Password));
            isAnyPasswordError.Should().BeTrue();
        }

        #endregion

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateEmailConfirmationUrl_NullOrEmpty_ReturnsFalseWithMessage(string emailConfirmationUrl)
        {
            // Arrange
            var model = CreateValidModel();
            model.EmailConfirmationUrl = emailConfirmationUrl;

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyEmailConfirmationError = result.Errors
                .Any(e => e.PropertyName == nameof(model.EmailConfirmationUrl));

            isAnyEmailConfirmationError.Should().BeTrue();
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test.")]
        public void ValidateEmailConfirmationUrl_Invalid_ReturnsFalseWithMessage(string emailConfirmationUrl)
        {
            // Arrange
            var model = CreateValidModel();
            model.EmailConfirmationUrl = emailConfirmationUrl;

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyEmailConfirmationError = result.Errors
                .Any(e => e.PropertyName == nameof(model.EmailConfirmationUrl));

            isAnyEmailConfirmationError.Should().BeTrue();
        }
    }
}
