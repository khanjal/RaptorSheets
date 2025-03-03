using Xunit;
using RaptorSheets.Core.Extensions;

namespace RaptorSheets.Tests.Extensions
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
        [InlineData("invalid-duration", null)]
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
    }
}
