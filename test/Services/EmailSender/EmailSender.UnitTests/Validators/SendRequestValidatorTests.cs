using EmailSender.API.Dtos;
using EmailSender.API.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EmailSender.UnitTests.Validators
{
    public class SendRequestValidatorTests
    {
        private readonly SendRequestValidator _validator = new();

        [Fact]
        public async Task ValidateAll_ValidRequest_ReturnsValidResult()
        {
            // Arrange
            var request = new SendRequest
            {
                To = "valid@mail.com",
                From = "valid@mail.com",
                Subject = "",
                Body = ""
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAll_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            Func<Task> act = () => _validator.TestValidateAsync(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        #region Validate To

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ValidateTo_NullOrEmpty_ReturnsValidationError(string to)
        {
            // Arrange
            var request = new SendRequest
            {
                To = to,
                From = "valid@mail.com",
                Subject = "",
                Body = ""
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.To);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("invalid@")]
        public async Task ValidateTo_InvalidEmail_ReturnsValidationError(string to)
        {
            // Arrange
            var request = new SendRequest
            {
                To = to,
                From = "valid@mail.com",
                Subject = "",
                Body = ""
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.To);
        }

        #endregion Validate To

        #region Validate From

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ValidateFrom_NullOrEmpty_ReturnsValidationError(string from)
        {
            // Arrange
            var request = new SendRequest
            {
                To = "valid@mail.com",
                From = from,
                Subject = "",
                Body = ""
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.From);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("invalid@")]
        public async Task ValidateFrom_InvalidEmail_ReturnsValidationError(string from)
        {
            // Arrange
            var request = new SendRequest
            {
                To = "valid@mail.com",
                From = from,
                Subject = "",
                Body = ""
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.From);
        }

        #endregion Validate From
    }
}