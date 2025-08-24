using RaptorSheets.Core.Extensions;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Extensions;

public class RandomExtensionsTests
{
    private enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }

    private enum EmptyEnum
    {
    }

    private enum SingleValueEnum
    {
        OnlyValue
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

    [Fact]
    public void NextEnum_WithSingleValueEnum_ShouldReturnOnlyValue()
    {
        // Arrange
        var random = new Random();

        // Act
        var result = random.NextEnum<SingleValueEnum>();

        // Assert
        Assert.Equal(SingleValueEnum.OnlyValue, result);
    }

    [Fact]
    public void NextEnum_WithMultipleCalls_ShouldReturnValidResults()
    {
        // Arrange
        var random = new Random();
        var validValues = Enum.GetValues<TestEnum>();

        // Act & Assert
        for (int i = 0; i < 50; i++)
        {
            var result = random.NextEnum<TestEnum>();
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
        var result1 = random1.NextEnum<TestEnum>();
        var result2 = random2.NextEnum<TestEnum>();

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void NextEnum_ShouldHandleZeroBasedEnum()
    {
        // Arrange
        var random = new Random();

        // Act
        var result = random.NextEnum<TestEnum>();

        // Assert
        Assert.True(Enum.IsDefined(typeof(TestEnum), result));
    }

}


