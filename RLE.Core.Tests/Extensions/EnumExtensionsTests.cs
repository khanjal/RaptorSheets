using FluentAssertions;
using RLE.Core.Extensions;
using System.ComponentModel;
using Xunit;

namespace RLE.Core.Tests.Extensions;

public class EnumExtensionsTests
{
    [Fact]
    public void GivenDescriptionEnum_ShouldReturnDescription()
    {
        string result = TestEnum.TESTING.GetDescription();

        result.Should().Be("Testing");
    }

    [Fact]
    public void GivenNoDescriptionEnum_ShouldReturnValueAsDescription()
    {
        string result = TestEnum.NO_DESCRIPTION_TEST.GetDescription();

        result.Should().Be("NO_DESCRIPTION_TEST");
    }

    [Theory]
    [InlineData("Testing", TestEnum.TESTING)]
    [InlineData("TESTING", TestEnum.TESTING)]
    [InlineData("Another Test", TestEnum.ANOTHER_TEST)]
    [InlineData("ANOTHER_TEST", TestEnum.ANOTHER_TEST)]
    public void GivenString_ShouldReturnEnum(string text, TestEnum enumValue)
    {
        var result = text.GetValueFromName<TestEnum>();

        result.Should().Be(enumValue);
    }

    [Fact]
    public void GivenEmptyString_ShouldReturnDefault()
    {
        var result = "".GetValueFromName<TestEnum>();

        result.Should().Be(TestEnum.TESTING);
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
