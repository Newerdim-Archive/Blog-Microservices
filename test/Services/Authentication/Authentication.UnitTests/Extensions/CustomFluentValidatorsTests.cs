using Authentication.UnitTests.Mocks;
using FluentValidation.TestHelper;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Extensions
{
    public class CustomFluentValidatorsTests
    {
        private readonly CustomFluentValidatorsTestValidator _validator = new();

        [Fact]
        public async Task ValidateAll_Valid_ReturnsNoErrors()
        {
            // Arrange
            var testClass = new CustomFluentValidatorsTestClass
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
            var testClass = new CustomFluentValidatorsTestClass
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
            var testClass = new CustomFluentValidatorsTestClass
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
            var testClass = new CustomFluentValidatorsTestClass
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
            var testClass = new CustomFluentValidatorsTestClass
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
            var testClass = new CustomFluentValidatorsTestClass
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
            var testClass = new CustomFluentValidatorsTestClass
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
            var testClass = new CustomFluentValidatorsTestClass
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
            var testClass = new CustomFluentValidatorsTestClass
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
            var testClass = new CustomFluentValidatorsTestClass
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
