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