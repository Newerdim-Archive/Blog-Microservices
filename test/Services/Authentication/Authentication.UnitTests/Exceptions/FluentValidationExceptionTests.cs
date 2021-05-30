using EmailSender.API.Exceptions;
using FluentAssertions;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using Xunit;

namespace Authentication.UnitTests.Exceptions
{
    public class FluentValidationExceptionTests
    {
        [Fact]
        public void Throw_ValidData_ValidMessage()
        {
            // Arrange
            var testedClass = typeof(FluentValidationExceptionTests);
            var failures = new List<ValidationFailure>() { new ValidationFailure("id", "null") };
            var result = new ValidationResult(failures);
            var paramName = nameof(result);
            var expectedMessage = $"{testedClass} is invalid. Errors: {string.Concat(result.Errors)}. Parameter name: {paramName}";

            try
            {
                // Act
                throw new FluentValidationException(testedClass, result, paramName);
            }
            catch (FluentValidationException ex)
            {
                // Assert
                ex.Message.Should().Be(expectedMessage);
            }
        }

        [Fact]
        public void Throw_NullTestedClass_MessageHasEmptyTestedClass()
        {
            // Arrange
            var failures = new List<ValidationFailure>() { new ValidationFailure("id", "null") };
            var result = new ValidationResult(failures);
            var paramName = nameof(result);
            var expectedMessage = $" is invalid. Errors: {string.Concat(result.Errors)}. Parameter name: {paramName}";

            try
            {
                // Act
                throw new FluentValidationException(null, result, paramName);
            }
            catch (FluentValidationException ex)
            {
                // Assert
                ex.Message.Should().Be(expectedMessage);
            }
        }

        [Fact]
        public void Throw_NullResult_ThrowsNullReferenceException()
        {
            // Arrange
            var testedClass = typeof(FluentValidationExceptionTests);
            var paramName = nameof(testedClass);

            try
            {
                // Act
                throw new FluentValidationException(testedClass, null, paramName);
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<NullReferenceException>();
            }
        }

        [Fact]
        public void Throw_EmptyErrors_MessageHasEmptyErrors()
        {
            // Arrange
            var testedClass = typeof(FluentValidationExceptionTests);
            var result = new ValidationResult();
            var paramName = nameof(result);
            var expectedMessage = $"{testedClass} is invalid. Errors: . Parameter name: {paramName}";

            try
            {
                // Act
                throw new FluentValidationException(testedClass, result, paramName);
            }
            catch (FluentValidationException ex)
            {
                // Assert
                ex.Message.Should().Be(expectedMessage);
            }
        }

        [Fact]
        public void Throw_NullParamName_MessageHasEmptyParamName()
        {
            // Arrange
            var testedClass = typeof(FluentValidationExceptionTests);
            var failures = new List<ValidationFailure>() { new ValidationFailure("id", "null") };
            var result = new ValidationResult(failures);
            var expectedMessage = $"{testedClass} is invalid. Errors: {string.Concat(result.Errors)}. Parameter name: ";

            try
            {
                // Act
                throw new FluentValidationException(testedClass, result, null);
            }
            catch (FluentValidationException ex)
            {
                // Assert
                ex.Message.Should().Be(expectedMessage);
            }
        }
    }
}