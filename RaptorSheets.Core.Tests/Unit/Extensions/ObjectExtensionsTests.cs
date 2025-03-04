using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Tests.Data;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Extensions;

public class ObjectExtensionsTests
{
    SheetModel modelData = TestSheetData.GetModelData();

    [Theory]
    [InlineData(HeaderEnum.FIRST_COLUMN, "A")]
    [InlineData(HeaderEnum.SECOND_COLUMN, "B")]
    public void GivenHeaders_ShouldGetColumn(HeaderEnum header, string column)
    {
        Assert.Equal(column, modelData.GetColumn(header.GetDescription()));
    }

    [Theory]
    [InlineData(HeaderEnum.FIRST_COLUMN, "0")]
    [InlineData(HeaderEnum.SECOND_COLUMN, "1")]
    public void GivenHeaders_ShouldGetIndex(HeaderEnum header, string index)
    {
        Assert.Equal(index, modelData.GetIndex(header.GetDescription()));
    }

    [Theory]
    [InlineData(HeaderEnum.FIRST_COLUMN, "A1:A")]
    [InlineData(HeaderEnum.SECOND_COLUMN, "B1:B")]
    public void GivenHeaders_ShouldGetRange(HeaderEnum header, string range)
    {
        Assert.Equal($"{modelData.Name}!{range}", modelData.GetRange(header.GetDescription()));
    }

    [Theory]
    [InlineData(HeaderEnum.FIRST_COLUMN, "A1:A")]
    [InlineData(HeaderEnum.SECOND_COLUMN, "B1:B")]
    public void GivenHeaders_ShouldGetLocalRange(HeaderEnum header, string range)
    {
        Assert.Equal(range, modelData.GetLocalRange(header.GetDescription()));
    }
}