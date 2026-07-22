using RaptorSheets.Core.Extensions;
using System.ComponentModel;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Extensions;

public class EnumExtensionsTests
{
    [Fact]
    public void GivenDescriptionEnum_ShouldReturnDescription()
    {
        string result = Test.TESTING.GetDescription();

        Assert.Equal("Testing", result);
    }

    [Fact]
    public void GivenNoDescriptionEnum_ShouldReturnValueAsDescription()
    {
        string result = Test.NO_DESCRIPTION_TEST.GetDescription();

        Assert.Equal("NO_DESCRIPTION_TEST", result);
    }

    [Theory]
    [InlineData("Testing", Test.TESTING)]
    [InlineData("TESTING", Test.TESTING)]
    [InlineData("Another Test", Test.ANOTHER_TEST)]
    [InlineData("ANOTHER_TEST", Test.ANOTHER_TEST)]
    public void GivenString_ShouldReturnEnum(string text, Test enumValue)
    {
        var result = text.GetValueFromName<Test>();

        Assert.Equal(enumValue, result);
    }

    [Fact]
    public void GivenEmptyString_ShouldReturnDefault()
    {
        var result = "".GetValueFromName<Test>();

        Assert.Equal(Test.TESTING, result);
    }    [Fact]
    public void GetDescription_WithNullEnum_ShouldReturnToString()
    {
        // This tests edge case behavior with first enum value
        var result = ((Test)0).GetDescription(); // First enum value
        
        Assert.NotNull(result);
        Assert.Equal("Testing", result);
    }

    [Theory]
    [InlineData("testing", Test.TESTING)] // Case insensitive
    [InlineData("ANOTHER_TEST", Test.ANOTHER_TEST)]
    [InlineData("another test", Test.ANOTHER_TEST)] // Case insensitive description
    public void GetValueFromName_CaseInsensitive_ShouldWork(string input, Test expected)
    {
        var result = input.GetValueFromName<Test>();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetValueFromName_WithInvalidString_ShouldReturnFirst()
    {
        var result = "NonExistent".GetValueFromName<Test>();
        Assert.Equal(Test.TESTING, result); // Should return first enum value
    }

    [Fact]
    public void GetValueFromName_WithNullString_ShouldReturnFirst()
    {
        var result = ((string)null!).GetValueFromName<Test>();
        Assert.Equal(Test.TESTING, result);
    }

    [Theory]
    [InlineData(Test.TESTING)]
    [InlineData(Test.ANOTHER_TEST)]
    [InlineData(Test.NO_DESCRIPTION_TEST)]
    public void GetDescription_AllEnumValues_ShouldReturnValidString(Test enumValue)
    {
        var result = enumValue.GetDescription();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetValueFromName_WithMultipleEnumTypes_ShouldWorkCorrectly()
    {
        // Test with different enum type
        var result1 = "TESTING".GetValueFromName<Test>();
        var result2 = "VALUE1".GetValueFromName<AnotherTest>();
        
        Assert.Equal(Test.TESTING, result1);
        Assert.Equal(AnotherTest.VALUE1, result2);
    }
}

public enum Test
{
    [Description("Testing")]
    TESTING,

    [Description("Another Test")]
    ANOTHER_TEST,

    NO_DESCRIPTION_TEST
}

public enum AnotherTest
{
    VALUE1,
    VALUE2,
    [Description("Value Three")]
    VALUE3
}