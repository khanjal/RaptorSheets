using RaptorSheets.Core.Extensions;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Extensions;

public class RandomExtensionsTests
{
    private enum Test
    {
        Value1,
        Value2,
        Value3
    }

    private enum SingleValue
    {
        OnlyValue
    }

    [Fact]
    public void NextEnum_ShouldReturnRandomEnumValue()
    {
        // Arrange
        var random = new Random();

        // Act
        var result = random.NextEnum<Test>();

        // Assert
        Assert.IsType<Test>(result);
        Assert.Contains(result, Enum.GetValues<Test>());
    }

    [Fact]
    public void NextEnum_ShouldReturnDifferentValues()
    {
        // Arrange
        var random = new Random();
        var results = new HashSet<Test>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.NextEnum<Test>());
        }

        // Assert
        Assert.True(results.Count > 1);
    }

    [Fact]
    public void NextEnum_WithSingleValueEnum_ShouldReturnOnlyValue()
    {
        // Arrange
        var random = new Random();

        // Act
        var result = random.NextEnum<SingleValue>();

        // Assert
        Assert.Equal(SingleValue.OnlyValue, result);
    }

    [Fact]
    public void NextEnum_WithMultipleCalls_ShouldReturnValidResults()
    {
        // Arrange
        var random = new Random();
        var validValues = Enum.GetValues<Test>();

        // Act & Assert
        for (int i = 0; i < 50; i++)
        {
            var result = random.NextEnum<Test>();
            Assert.Contains(result, validValues);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(12345)]
    public void NextEnum_WithFixedSeed_ShouldBeReproducible(int seed)
    {
        // Arrange
        var random1 = new Random(seed);
        var random2 = new Random(seed);

        // Act
        var result1 = random1.NextEnum<Test>();
        var result2 = random2.NextEnum<Test>();

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void NextEnum_ShouldHandleZeroBasedEnum()
    {
        // Arrange
        var random = new Random();

        // Act
        var result = random.NextEnum<Test>();

        // Assert
        Assert.True(Enum.IsDefined(typeof(Test), result));
    }

}


