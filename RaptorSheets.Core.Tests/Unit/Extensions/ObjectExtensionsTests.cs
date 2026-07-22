using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Tests.Data;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Extensions;

public class ObjectExtensionsTests
{
    readonly SheetModel modelData = TestSheetData.GetModelData();

    [Theory]
    [InlineData(Header.FIRST_COLUMN, "A")]
    [InlineData(Header.SECOND_COLUMN, "B")]
    public void GivenHeaders_ShouldGetColumn(Header header, string column)
    {
        Assert.Equal(column, modelData.GetColumn(header.GetDescription()));
    }

    [Theory]
    [InlineData(Header.FIRST_COLUMN, "0")]
    [InlineData(Header.SECOND_COLUMN, "1")]
    public void GivenHeaders_ShouldGetIndex(Header header, string index)
    {
        Assert.Equal(index, modelData.GetIndex(header.GetDescription()));
    }

    [Theory]
    [InlineData(Header.FIRST_COLUMN, "A1:A")]
    [InlineData(Header.SECOND_COLUMN, "B1:B")]
    public void GivenHeaders_ShouldGetRange(Header header, string range)
    {
        Assert.Equal($"'{modelData.Name}'!{range}", modelData.GetRange(header.GetDescription()));
    }

    [Theory]
    [InlineData(Header.FIRST_COLUMN, "A1:A")]
    [InlineData(Header.SECOND_COLUMN, "B1:B")]
    public void GivenHeaders_ShouldGetLocalRange(Header header, string range)
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
        Assert.Equal($"'{modelData.Name}'!", result);
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
    public void ObjectExtensions_WithNullSheetModel_ShouldThrowArgumentNullException()
    {
        // Arrange
        SheetModel? nullModel = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullModel!.GetColumn("Test"));
        Assert.Throws<ArgumentNullException>(() => nullModel!.GetIndex("Test"));
        Assert.Throws<ArgumentNullException>(() => nullModel!.GetRange("Test"));
        Assert.Throws<ArgumentNullException>(() => nullModel!.GetLocalRange("Test"));
        Assert.Throws<ArgumentNullException>(() => nullModel!.GetRangeBetweenColumns("Start", "End"));
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
        Assert.Equal("'EmptySheet'!", range);
        Assert.Equal("", localRange);
    }

    [Fact]
    public void ObjectExtensions_CaseSensitivity_ShouldWork()
    {
        // Act
        var lowerResult = modelData.GetColumn(Header.FIRST_COLUMN.GetDescription().ToLower());
        var upperResult = modelData.GetColumn(Header.FIRST_COLUMN.GetDescription().ToUpper());
        var correctResult = modelData.GetColumn(Header.FIRST_COLUMN.GetDescription());

        // Assert - GetColumn matches header names case-sensitively, so mismatched case
        // does not match the actual "First Column" header and returns empty.
        Assert.Equal("A", correctResult);
        Assert.Equal("", lowerResult);
        Assert.Equal("", upperResult);
    }
}