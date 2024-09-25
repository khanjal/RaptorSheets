using FluentAssertions;
using RaptorLoggerEngine.Enums;
using RaptorLoggerEngine.Tests.Data;
using RaptorLoggerEngine.Utilities.Extensions;
using RLE.Core.Models.Google;

namespace RaptorLoggerEngine.Tests.Utilities.Extensions;

public class ObjectExtensionsTests
{
    SheetModel modelData = TestSheetData.GetModelData();

    [Theory]
    [InlineData(HeaderEnum.WEEK, "A")]
    [InlineData(HeaderEnum.DATE, "B")]
    public void GivenHeaders_ShouldGetColumn(HeaderEnum header, string column)
    {
        modelData.GetColumn(header).Should().Be(column);
    }

    [Theory]
    [InlineData(HeaderEnum.WEEK, "0")]
    [InlineData(HeaderEnum.DATE, "1")]
    public void GivenHeaders_ShouldGetIndex(HeaderEnum header, string index)
    {
        modelData.GetIndex(header).Should().Be(index);
    }

    [Theory]
    [InlineData(HeaderEnum.WEEK, "A1:A")]
    [InlineData(HeaderEnum.DATE, "B1:B")]
    public void GivenHeaders_ShouldGetRange(HeaderEnum header, string range)
    {
        modelData.GetRange(header).Should().Be($"{modelData.Name}!{range}");
    }

    [Theory]
    [InlineData(HeaderEnum.WEEK, "A1:A")]
    [InlineData(HeaderEnum.DATE, "B1:B")]
    public void GivenHeaders_ShouldGetLocalRange(HeaderEnum header, string range)
    {
        modelData.GetLocalRange(header).Should().Be(range);
    }
}
