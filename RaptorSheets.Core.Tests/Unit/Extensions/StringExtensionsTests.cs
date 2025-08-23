using Xunit;
using RaptorSheets.Core.Extensions;

namespace RaptorSheets.Core.Tests.Unit.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("2023-10-01", 45200.0)] // Example date
        [InlineData("2024-09-29", 45564.0)] // Example date
        [InlineData("2025-03-03", 45719.0)] // Example date
        [InlineData("1900-01-01", 2.0)] // Base date
        [InlineData("1899-12-30", 0.0)] // Base date
        [InlineData("invalid-date", null)]
        public void ToSerialDate_ShouldReturnExpectedResult(string input, double? expected)
        {
            // Act
            var result = input.ToSerialDate();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("00:00:00.0000", 0.0)]
        [InlineData("01:00:00.0000", 0.041666666666666664)]
        [InlineData("12:00:00.0000", 0.5)]
        [InlineData("18:00:00.0000", 0.75)]
        [InlineData("24:00:00.0000", 1.0)]
        [InlineData("36:00:00", 1.5)]
        [InlineData("-01:00:00", -0.041666666666666664)] // Negative time
        [InlineData("99:99:99", 4.1948958333333337)] // Time format for duration
        [InlineData("invalid-duration", null)]
        [InlineData("abc:def:ghi", null)]
        public void ToSerialDuration_ShouldReturnExpectedResult(string input, double? expected)
        {
            // Act
            var result = input.ToSerialDuration();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("12:00:00 am", 0.0)]
        [InlineData("00:00:00", 0.0)]
        [InlineData("01:00:00 am", 0.041666666666666664)]
        [InlineData("06:00:00", 0.25)]
        [InlineData("12:00:00 pm", 0.5)]
        [InlineData("18:00:00", 0.75)]
        [InlineData("24:00:00", null)]
        [InlineData("invalid-time", null)]
        public void ToSerialTime_ShouldReturnExpectedResult(string input, double? expected)
        {
            // Act
            var result = input.ToSerialTime();

            // Assert
            Assert.Equal(expected, result);
        }

        // Additional edge cases and error handling tests
        [Fact]
        public void ToSerialDate_WithNullString_ShouldReturnNull()
        {
            // Act
            var result = ((string)null!).ToSerialDate();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToSerialDuration_WithNullString_ShouldReturnNull()
        {
            // Act
            var result = ((string)null!).ToSerialDuration();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToSerialTime_WithNullString_ShouldReturnNull()
        {
            // Act
            var result = ((string)null!).ToSerialTime();

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void ToSerialDate_WithEmptyOrWhitespace_ShouldReturnNull(string input)
        {
            // Act
            var result = input.ToSerialDate();

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void ToSerialDuration_WithEmptyOrWhitespace_ShouldReturnNull(string input)
        {
            // Act
            var result = input.ToSerialDuration();

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void ToSerialTime_WithEmptyOrWhitespace_ShouldReturnNull(string input)
        {
            // Act
            var result = input.ToSerialTime();

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("2023-13-01")] // Invalid month
        [InlineData("2023-02-30")] // Invalid day for February
        [InlineData("not-a-date")]
        [InlineData("2023/99/99")]
        public void ToSerialDate_WithInvalidDates_ShouldReturnNull(string input)
        {
            // Act
            var result = input.ToSerialDate();

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("25:00:00")] // Invalid hour
        [InlineData("12:60:00")] // Invalid minute
        [InlineData("12:30:60")] // Invalid second
        [InlineData("abc:def:ghi")]
        public void ToSerialTime_WithInvalidTimes_ShouldReturnNull(string input)
        {
            // Act
            var result = input.ToSerialTime();

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("1900-01-01T00:00:00")]
        [InlineData("2023-12-25T15:30:45")]
        public void ToSerialDate_WithISO8601Format_ShouldParse(string input)
        {
            // Act
            var result = input.ToSerialDate();

            // Assert
            Assert.NotNull(result);
            Assert.True(result > 0);
        }

        [Theory]
        [InlineData("23:59:59")]
        [InlineData("00:00:01")]
        public void ToSerialTime_WithBoundaryValues_ShouldReturnValidResult(string input)
        {
            // Act
            var result = input.ToSerialTime();

            // Assert
            Assert.NotNull(result);
            Assert.InRange(result.Value, 0.0, 1.0);
        }

        [Theory]
        [InlineData("100:00:00", 4.166666666666667)] // 100 hours
        [InlineData("168:00:00", 7.0)] // 1 week
        public void ToSerialDuration_WithLargeDurations_ShouldReturnValidResult(string input, double expected)
        {
            // Act
            var result = input.ToSerialDuration();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result.Value, 10);
        }
    }
}
