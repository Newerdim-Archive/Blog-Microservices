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
        public RegisterModelValidatorTests()
        {

        }

        private static RegisterModel CreateValidModel()
        {
            var fixture = new Fixture();

            var usernamePattern = @"^[A-Za-z0-9]{4}(?:[ _-][A-Za-z0-9]{1}){14}$";
            var username = new SpecimenContext(fixture)
                .Resolve(new RegularExpressionRequest(usernamePattern));

            var passwordPattern = @"[A-Z]{4}[a-z]{4}[0-9]{4}";
            var password = new SpecimenContext(fixture)
                .Resolve(new RegularExpressionRequest(passwordPattern));

            var email = fixture.Create<MailAddress>().Address;

            fixture.Customize<RegisterModel>(c => c
                .With(x => x.Username, username)
                .With(x => x.Email, email)
                .With(x => x.Password, password));

            return fixture.Create<RegisterModel>();
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
            var invalidModel = CreateValidModel();
            invalidModel.Username = username;

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(invalidModel);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyUsernameError = result.Errors.Any(e => e.PropertyName == nameof(invalidModel.Username));
            isAnyUsernameError.Should().BeTrue();
        }

        [Fact]
        public void ValidateUsername_TooLong_ReturnsFalseWithError()
        {
            // Arrange
            var invalidModel = CreateValidModel();
            invalidModel.Username = new string('a', 33);

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(invalidModel);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyUsernameError = result.Errors.Any(e => e.PropertyName == nameof(invalidModel.Username));
            isAnyUsernameError.Should().BeTrue();
        }

        [Fact]
        public void ValidateUsername_TooShort_ReturnsFalseWithError()
        {
            // Arrange
            var invalidModel = CreateValidModel();
            invalidModel.Username = new string('a', 2);

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(invalidModel);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyUsernameError = result.Errors.Any(e => e.PropertyName == nameof(invalidModel.Username));
            isAnyUsernameError.Should().BeTrue();
        }

        [Fact]
        public void ValidateUsername_InvalidSpecialCharacters_ReturnsFalseWithError()
        {
            // Arrange
            var invalidModel = CreateValidModel();
            invalidModel.Username = "Invalid !@#?";

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(invalidModel);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyUsernameError = result.Errors.Any(e => e.PropertyName == nameof(invalidModel.Username));
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
            var invalidModel = CreateValidModel();
            invalidModel.Email = email;

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(invalidModel);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyEmailError = result.Errors.Any(e => e.PropertyName == nameof(invalidModel.Email));
            isAnyEmailError.Should().BeTrue();
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("@invalid.com")]
        [InlineData("invalid.com")]
        public void ValidateEmail_InvalidEmail_ReturnsFalseWithMessage(string email)
        {
            // Arrange
            var invalidModel = CreateValidModel();
            invalidModel.Email = email;

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(invalidModel);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyEmailError = result.Errors.Any(e => e.PropertyName == nameof(invalidModel.Email));
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
            var invalidModel = CreateValidModel();
            invalidModel.Password = password;

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(invalidModel);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyPasswordError = result.Errors.Any(e => e.PropertyName == nameof(invalidModel.Password));
            isAnyPasswordError.Should().BeTrue();
        }

        [Fact]
        public void ValidatePassword_TooShort_ReturnsFalseWithMessage()
        {
            // Arrange
            var invalidModel = CreateValidModel();
            invalidModel.Password = "Aa1!z";

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(invalidModel);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyPasswordError = result.Errors.Any(e => e.PropertyName == nameof(invalidModel.Password));
            isAnyPasswordError.Should().BeTrue();
        }

        [Fact]
        public void ValidatePassword_WithoutUppercase_ReturnsFalseWithMessage()
        {
            // Arrange
            var invalidModel = CreateValidModel();
            invalidModel.Password = "aaaaaaa1111";

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(invalidModel);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyPasswordError = result.Errors.Any(e => e.PropertyName == nameof(invalidModel.Password));
            isAnyPasswordError.Should().BeTrue();
        }

        [Fact]
        public void ValidatePassword_WithoutLowercase_ReturnsFalseWithMessage()
        {
            // Arrange
            var invalidModel = CreateValidModel();
            invalidModel.Password = "AAAAAAA1111";

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(invalidModel);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyPasswordError = result.Errors.Any(e => e.PropertyName == nameof(invalidModel.Password));
            isAnyPasswordError.Should().BeTrue();
        }

        [Fact]
        public void ValidatePassword_WithoutNumber_ReturnsFalseWithMessage()
        {
            // Arrange
            var invalidModel = CreateValidModel();
            invalidModel.Password = "AAAAAAAaaaaa";

            var sut = new RegisterModelValidator();

            // Act
            var result = sut.Validate(invalidModel);

            // Assert
            result.IsValid.Should().BeFalse();

            var isAnyPasswordError = result.Errors.Any(e => e.PropertyName == nameof(invalidModel.Password));
            isAnyPasswordError.Should().BeTrue();
        }

        #endregion
    }
}
