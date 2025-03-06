using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Tests.Data;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Extensions;

public class ListExtensionsTests
{
    [Theory]
    [InlineData("A", 0)]
    [InlineData("B", 1)]
    public void GivenHeaders_ShouldAddColumnAndIndex(string column, int index)
    {
        var result = TestSheetData.GetModelData();

        Assert.Equal(column, result.Headers[index].Column);
        Assert.Equal(index, result.Headers[index].Index);
    }

    [Fact]
    public void AddColumn_ShouldAddHeaderWithCorrectColumnAndIndex()
    {
        // Arrange
        var headers = new List<SheetCellModel>();
        var header = new SheetCellModel { Name = "Header1" };

        // Act
        headers.AddColumn(header);

        // Assert
        Assert.Single(headers);
        Assert.Equal("A", headers[0].Column);
        Assert.Equal(0, headers[0].Index);
    }

    [Fact]
    public void UpdateColumns_ShouldUpdateHeadersWithCorrectColumnsAndIndexes()
    {
        // Arrange
        var headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = "Header1" },
                new SheetCellModel { Name = "Header2" }
            };

        // Act
        headers.UpdateColumns();

        // Assert
        Assert.Equal(2, headers.Count);
        Assert.Equal("A", headers[0].Column);
        Assert.Equal(0, headers[0].Index);
        Assert.Equal("B", headers[1].Column);
        Assert.Equal(1, headers[1].Index);
    }

    [Fact]
    public void AddItems_ShouldAddDefaultItemsToList()
    {
        // Arrange
        var list = new List<int>();

        // Act
        list.AddItems(3);

        // Assert
        Assert.Equal(3, list.Count);
        Assert.All(list, item => Assert.Equal(default, item));
    }
}