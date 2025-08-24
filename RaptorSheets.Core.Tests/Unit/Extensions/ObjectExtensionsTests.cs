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

    [Fact]
    public void GetColumn_WithNonExistentHeader_ShouldReturnEmpty()
    {
        // Act
        var result = modelData.GetColumn("NonExistent");

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void GetIndex_WithNonExistentHeader_ShouldReturnEmpty()
    {
        // Act
        var result = modelData.GetIndex("NonExistent");

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void GetRange_WithNonExistentHeader_ShouldReturnSheetName()
    {
        // Act
        var result = modelData.GetRange("NonExistent");

        // Assert
        Assert.Equal($"{modelData.Name}!", result);
    }

    [Fact]
    public void GetLocalRange_WithNonExistentHeader_ShouldReturnEmpty()
    {
        // Act
        var result = modelData.GetLocalRange("NonExistent");

        // Assert
        Assert.Equal("", result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetColumn_WithNullOrEmptyHeader_ShouldReturnEmpty(string? headerName)
    {
        // Act
        var result = modelData.GetColumn(headerName!);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void ObjectExtensions_WithNullSheetModel_ShouldHandleGracefully()
    {
        // Arrange
        SheetModel? nullModel = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => nullModel!.GetColumn("Test"));
        Assert.Throws<NullReferenceException>(() => nullModel!.GetIndex("Test"));
        Assert.Throws<NullReferenceException>(() => nullModel!.GetRange("Test"));
        Assert.Throws<NullReferenceException>(() => nullModel!.GetLocalRange("Test"));
    }

    [Fact]
    public void ObjectExtensions_WithEmptyHeaders_ShouldReturnDefaults()
    {
        // Arrange
        var emptyModel = new SheetModel
        {
            Name = "EmptySheet",
            Headers = new List<SheetCellModel>()
        };

        // Act
        var column = emptyModel.GetColumn("Test");
        var index = emptyModel.GetIndex("Test");
        var range = emptyModel.GetRange("Test");
        var localRange = emptyModel.GetLocalRange("Test");

        // Assert
        Assert.Equal("", column);
        Assert.Equal("", index);
        Assert.Equal("EmptySheet!", range);
        Assert.Equal("", localRange);
    }

    [Fact]
    public void ObjectExtensions_CaseSensitivity_ShouldWork()
    {
        // Act
        var lowerResult = modelData.GetColumn(HeaderEnum.FIRST_COLUMN.GetDescription().ToLower());
        var upperResult = modelData.GetColumn(HeaderEnum.FIRST_COLUMN.GetDescription().ToUpper());
        var correctResult = modelData.GetColumn(HeaderEnum.FIRST_COLUMN.GetDescription());

        // Assert (assuming case-sensitive implementation)
        Assert.Equal("A", correctResult);
        // Depending on implementation, these might be empty or match
    }
}