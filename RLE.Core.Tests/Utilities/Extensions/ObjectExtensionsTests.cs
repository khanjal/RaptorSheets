using FluentAssertions;
using RLE.Core.Models.Google;
using RLE.Core.Tests.Data;
using RLE.Core.Utilities.Extensions;
using RLE.Gig.Tests.Data;
using Xunit;

namespace RLE.Core.Tests.Utilities.Extensions;

public class ObjectExtensionsTests
{
    SheetModel modelData = TestSheetData.GetModelData();

    [Theory]
    [InlineData(HeaderEnum.FIRST_COLUMN, "A")]
    [InlineData(HeaderEnum.SECOND_COLUMN, "B")]
    public void GivenHeaders_ShouldGetColumn(HeaderEnum header, string column)
    {
        modelData.GetColumn(header.GetDescription()).Should().Be(column);
    }

    [Theory]
    [InlineData(HeaderEnum.FIRST_COLUMN, "0")]
    [InlineData(HeaderEnum.SECOND_COLUMN, "1")]
    public void GivenHeaders_ShouldGetIndex(HeaderEnum header, string index)
    {
        modelData.GetIndex(header.GetDescription()).Should().Be(index);
    }

    [Theory]
    [InlineData(HeaderEnum.FIRST_COLUMN, "A1:A")]
    [InlineData(HeaderEnum.SECOND_COLUMN, "B1:B")]
    public void GivenHeaders_ShouldGetRange(HeaderEnum header, string range)
    {
        modelData.GetRange(header.GetDescription()).Should().Be($"{modelData.Name}!{range}");
    }

    [Theory]
    [InlineData(HeaderEnum.FIRST_COLUMN, "A1:A")]
    [InlineData(HeaderEnum.SECOND_COLUMN, "B1:B")]
    public void GivenHeaders_ShouldGetLocalRange(HeaderEnum header, string range)
    {
        modelData.GetLocalRange(header.GetDescription()).Should().Be(range);
    }
}
