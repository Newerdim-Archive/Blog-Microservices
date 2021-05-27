using Authentication.API.Providers;
using FluentAssertions;
using System;
using Xunit;

namespace Authentication.UnitTests.Providers
{
    public class DateProviderTests
    {
        private readonly IDateProvider _sut;

        public DateProviderTests()
        {
            _sut = new DateProvider();
        }

        [Fact]
        public void GetUtcNow_ReturnsDateNow()
        {
            // Arrange
            var exceptedDate = DateTimeOffset.UtcNow;

            // Act
            var result = _sut.GetUtcNow();

            // Assert
            result.Should().BeCloseTo(exceptedDate);
        }

        [Fact]
        public void GetAfterUtcNow_ValidData_ReturnsExpectedDate()
        {
            // Arrange
            var exceptedDate = DateTimeOffset.UtcNow
                .AddDays(1)
                .AddMinutes(4);

            // Act
            var result = _sut.GetAfterUtcNow(1, 4);

            // Assert
            result.Should().BeCloseTo(exceptedDate);
        }

        [Fact]
        public void GetAfterUtcNow_TooBigNumberOfDays_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var exceptedDate = DateTimeOffset.UtcNow
                .AddDays(1)
                .AddMinutes(4);

            // Act
            Action act = () => _sut.GetAfterUtcNow(int.MaxValue, 4);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}