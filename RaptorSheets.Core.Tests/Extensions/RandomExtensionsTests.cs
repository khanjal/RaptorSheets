using RaptorSheets.Core.Extensions;
using System;
using Xunit;

namespace RaptorSheets.Core.Tests.Extensions;

public class RandomExtensionsTests
{
    private enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }

    [Fact]
    public void NextEnum_ShouldReturnRandomEnumValue()
    {
        // Arrange
        var random = new Random();

        // Act
        var result = random.NextEnum<TestEnum>();

        // Assert
        Assert.IsType<TestEnum>(result);
        Assert.Contains(result, Enum.GetValues<TestEnum>());
    }

    [Fact]
    public void NextEnum_ShouldReturnDifferentValues()
    {
        // Arrange
        var random = new Random();
        var values = Enum.GetValues<TestEnum>();
        var results = new HashSet<TestEnum>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.NextEnum<TestEnum>());
        }

        // Assert
        Assert.True(results.Count > 1);
    }
}


