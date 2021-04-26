using Authentication.API.Providers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.UnitTests.Providers
{
    public class DateProviderTests
    {
        private static IDateProvider CreateProvider()
        {
            return new DateProvider();
        }

        [Fact]
        public void GetUtcNow_ReturnsDateNow()
        {
            // Arrange
            var sut = CreateProvider();
            var exceptedDate = DateTimeOffset.UtcNow;

            // Act
            var result = sut.GetUtcNow();

            // Assert
            result.Should().BeCloseTo(exceptedDate);
        }

        [Fact]
        public void GetAfterUtcNow_ValidData_ReturnsDateNow()
        {
            // Arrange
            var sut = CreateProvider();
            var exceptedDate = DateTimeOffset.UtcNow
                .AddDays(1)
                .AddMinutes(4);

            // Act
            var result = sut.GetAfterUtcNow(1, 4);

            // Assert
            result.Should().BeCloseTo(exceptedDate);
        }

        [Fact]
        public void GetAfterUtcNow_TooBigNumberOfDays_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var sut = CreateProvider();
            var exceptedDate = DateTimeOffset.UtcNow
                .AddDays(1)
                .AddMinutes(4);

            // Act
            Action act = () => sut.GetAfterUtcNow(int.MaxValue, 4);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
