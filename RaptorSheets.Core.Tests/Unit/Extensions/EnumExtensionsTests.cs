using RaptorSheets.Core.Extensions;
using System.ComponentModel;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Extensions;

public class EnumExtensionsTests
{
    [Fact]
    public void GivenDescriptionEnum_ShouldReturnDescription()
    {
        string result = TestEnum.TESTING.GetDescription();

        Assert.Equal("Testing", result);
    }

    [Fact]
    public void GivenNoDescriptionEnum_ShouldReturnValueAsDescription()
    {
        string result = TestEnum.NO_DESCRIPTION_TEST.GetDescription();

        Assert.Equal("NO_DESCRIPTION_TEST", result);
    }

    [Theory]
    [InlineData("Testing", TestEnum.TESTING)]
    [InlineData("TESTING", TestEnum.TESTING)]
    [InlineData("Another Test", TestEnum.ANOTHER_TEST)]
    [InlineData("ANOTHER_TEST", TestEnum.ANOTHER_TEST)]
    public void GivenString_ShouldReturnEnum(string text, TestEnum enumValue)
    {
        var result = text.GetValueFromName<TestEnum>();

        Assert.Equal(enumValue, result);
    }

    [Fact]
    public void GivenEmptyString_ShouldReturnDefault()
    {
        var result = "".GetValueFromName<TestEnum>();

        Assert.Equal(TestEnum.TESTING, result);
    }    [Fact]
    public void GetDescription_WithNullEnum_ShouldReturnToString()
    {
        // This tests edge case behavior with first enum value
        var result = ((TestEnum)0).GetDescription(); // First enum value
        
        Assert.NotNull(result);
        Assert.Equal("Testing", result);
    }

    [Theory]
    [InlineData("testing", TestEnum.TESTING)] // Case insensitive
    [InlineData("ANOTHER_TEST", TestEnum.ANOTHER_TEST)]
    [InlineData("another test", TestEnum.ANOTHER_TEST)] // Case insensitive description
    public void GetValueFromName_CaseInsensitive_ShouldWork(string input, TestEnum expected)
    {
        var result = input.GetValueFromName<TestEnum>();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetValueFromName_WithInvalidString_ShouldReturnFirst()
    {
        var result = "NonExistent".GetValueFromName<TestEnum>();
        Assert.Equal(TestEnum.TESTING, result); // Should return first enum value
    }

    [Fact]
    public void GetValueFromName_WithNullString_ShouldReturnFirst()
    {
        var result = ((string)null!).GetValueFromName<TestEnum>();
        Assert.Equal(TestEnum.TESTING, result);
    }

    [Theory]
    [InlineData(TestEnum.TESTING)]
    [InlineData(TestEnum.ANOTHER_TEST)]
    [InlineData(TestEnum.NO_DESCRIPTION_TEST)]
    public void GetDescription_AllEnumValues_ShouldReturnValidString(TestEnum enumValue)
    {
        var result = enumValue.GetDescription();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetValueFromName_WithMultipleEnumTypes_ShouldWorkCorrectly()
    {
        // Test with different enum type
        var result1 = "TESTING".GetValueFromName<TestEnum>();
        var result2 = "VALUE1".GetValueFromName<AnotherTestEnum>();
        
        Assert.Equal(TestEnum.TESTING, result1);
        Assert.Equal(AnotherTestEnum.VALUE1, result2);
    }
}

public enum TestEnum
{
    [Description("Testing")]
    TESTING,

    [Description("Another Test")]
    ANOTHER_TEST,

    NO_DESCRIPTION_TEST
}

public enum AnotherTestEnum
{
    VALUE1,
    VALUE2,
    [Description("Value Three")]
    VALUE3
}