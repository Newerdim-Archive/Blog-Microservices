using Authentication.UnitTests.Mocks;
using FluentValidation.TestHelper;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Extensions
{
    public class FluentValidatorsExtensionsTests
    {
        private readonly FluentValidatorsExtensionsTestValidator _validator = new();

        [Fact]
        public async Task ValidateAll_Valid_ReturnsNoErrors()
        {
            // Arrange
            var testClass = new FluentValidatorsExtensionsTestClass
            {
                Username = "Username 123-_",
                Password = "Password123!",
                Url = "www.website.com"
            };

            // Act
            var result = await _validator.TestValidateAsync(testClass);

            // Assert
            result.ShouldHaveAnyValidationError();
        }

        #region Username

        [Fact]
        public async Task Username_TooShort_ReturnsError()
        {
            // Arrange
            var testClass = new FluentValidatorsExtensionsTestClass
            {
                Username = "Us"
            };

            // Act
            var result = await _validator.TestValidateAsync(testClass);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public async Task Username_TooLong_ReturnsError()
        {
            // Arrange
            var testClass = new FluentValidatorsExtensionsTestClass
            {
                Username = new string('a', 33)
            };

            // Act
            var result = await _validator.TestValidateAsync(testClass);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public async Task Username_SpecialCharacters_ReturnsError()
        {
            // Arrange
            var testClass = new FluentValidatorsExtensionsTestClass
            {
                Username = "Username123$"
            };

            // Act
            var result = await _validator.TestValidateAsync(testClass);

            // Assert
            result.ShouldHaveAnyValidationError();
        }

        #endregion

        #region Password

        [Fact]
        public async Task Password_TooShort_ReturnsError()
        {
            // Arrange
            var testClass = new FluentValidatorsExtensionsTestClass
            {
                Password = "Psw1!"
            };

            // Act
            var result = await _validator.TestValidateAsync(testClass);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public async Task Password_NoUppercase_ReturnsError()
        {
            // Arrange
            var testClass = new FluentValidatorsExtensionsTestClass
            {
                Password = "password 123-_!"
            };

            // Act
            var result = await _validator.TestValidateAsync(testClass);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public async Task Password_NoLowercase_ReturnsError()
        {
            // Arrange
            var testClass = new FluentValidatorsExtensionsTestClass
            {
                Password = "PASSWORD 123-_!"
            };

            // Act
            var result = await _validator.TestValidateAsync(testClass);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public async Task Password_NoNumber_ReturnsError()
        {
            // Arrange
            var testClass = new FluentValidatorsExtensionsTestClass
            {
                Password = "PASSWORD-_!"
            };

            // Act
            var result = await _validator.TestValidateAsync(testClass);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public async Task Password_NoSpecialCharacter_ReturnsError()
        {
            // Arrange
            var testClass = new FluentValidatorsExtensionsTestClass
            {
                Password = "PASSWORD12234"
            };

            // Act
            var result = await _validator.TestValidateAsync(testClass);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        #endregion

        #region Url

        [Theory]
        [InlineData("website")]
        [InlineData("website.")]
        [InlineData("/website")]
        [InlineData("@website.")]
        public async Task Url_Invalid_ReturnsError(string url)
        {
            // Arrange
            var testClass = new FluentValidatorsExtensionsTestClass
            {
                Url = url
            };

            // Act
            var result = await _validator.TestValidateAsync(testClass);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Url);
        } 

        #endregion
    }
}
