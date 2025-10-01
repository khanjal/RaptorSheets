using RaptorSheets.Core.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class ConstantsOrderHelperTests
{
    // Test constants class for demonstration
    public static class TestSheetNames
    {
        public const string FirstSheet = "First";
        public const string SecondSheet = "Second";
        public const string ThirdSheet = "Third";
        public const string LastSheet = "Last";
    }

    [Fact]
    public void GetOrderFromConstants_ReturnsCorrectDeclarationOrder()
    {
        // Act
        var order = ConstantsOrderHelper.GetOrderFromConstants(typeof(TestSheetNames));

        // Assert
        Assert.Equal(4, order.Count);
        Assert.Equal("First", order[0]);
        Assert.Equal("Second", order[1]);
        Assert.Equal("Third", order[2]);
        Assert.Equal("Last", order[3]);
    }

    [Fact]
    public void ValidateSheetNames_WithValidNames_ReturnsNoErrors()
    {
        // Arrange
        var validNames = new[] { "First", "Second", "Third" };

        // Act
        var errors = ConstantsOrderHelper.ValidateSheetNames(typeof(TestSheetNames), validNames);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateSheetNames_WithInvalidNames_ReturnsErrors()
    {
        // Arrange
        var mixedNames = new[] { "First", "Invalid", "Third" };

        // Act
        var errors = ConstantsOrderHelper.ValidateSheetNames(typeof(TestSheetNames), mixedNames);

        // Assert
        Assert.Single(errors);
        Assert.Contains("Invalid", errors[0]);
    }

    [Fact]
    public void GetSheetIndex_ReturnsCorrectPositions()
    {
        // Act & Assert
        Assert.Equal(0, ConstantsOrderHelper.GetSheetIndex(typeof(TestSheetNames), "First"));
        Assert.Equal(1, ConstantsOrderHelper.GetSheetIndex(typeof(TestSheetNames), "Second"));
        Assert.Equal(3, ConstantsOrderHelper.GetSheetIndex(typeof(TestSheetNames), "Last"));
        Assert.Equal(-1, ConstantsOrderHelper.GetSheetIndex(typeof(TestSheetNames), "NotFound"));
    }
}