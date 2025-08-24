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

    [Fact]
    public void AddColumn_WithMultipleHeaders_ShouldMaintainOrder()
    {
        // Arrange
        var headers = new List<SheetCellModel>();
        var header1 = new SheetCellModel { Name = "Header1" };
        var header2 = new SheetCellModel { Name = "Header2" };
        var header3 = new SheetCellModel { Name = "Header3" };

        // Act
        headers.AddColumn(header1);
        headers.AddColumn(header2);
        headers.AddColumn(header3);

        // Assert
        Assert.Equal(3, headers.Count);
        Assert.Equal("A", headers[0].Column);
        Assert.Equal("B", headers[1].Column);
        Assert.Equal("C", headers[2].Column);
        Assert.Equal(0, headers[0].Index);
        Assert.Equal(1, headers[1].Index);
        Assert.Equal(2, headers[2].Index);
    }

    [Fact]
    public void UpdateColumns_WithEmptyList_ShouldNotThrow()
    {
        // Arrange
        var headers = new List<SheetCellModel>();

        // Act
        var exception = Record.Exception(() => headers.UpdateColumns());

        // Assert
        Assert.Null(exception);
        Assert.Empty(headers);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void AddItems_WithVariousCounts_ShouldAddCorrectNumber(int count)
    {
        // Arrange
        var list = new List<string>();

        // Act
        list.AddItems(count);

        // Assert
        Assert.Equal(count, list.Count);
        Assert.All(list, item => Assert.Equal(default(string), item));
    }

    [Fact]
    public void AddItems_WithNegativeCount_ShouldNotAddItems()
    {
        // Arrange
        var list = new List<int>();

        // Act
        list.AddItems(-5);

        // Assert
        Assert.Empty(list);
    }

    [Fact]
    public void UpdateColumns_WithLargeList_ShouldHandleCorrectly()
    {
        // Arrange
        var headers = new List<SheetCellModel>();
        for (int i = 0; i < 100; i++)
        {
            headers.Add(new SheetCellModel { Name = $"Header{i}" });
        }

        // Act
        headers.UpdateColumns();

        // Assert
        Assert.Equal(100, headers.Count);
        Assert.Equal("A", headers[0].Column);
        Assert.Equal("Z", headers[25].Column);
        Assert.Equal("AA", headers[26].Column);
        Assert.Equal("AB", headers[27].Column);
        Assert.Equal("CV", headers[99].Column);
    }

    [Fact]
    public void AddColumn_WithNullHeader_ShouldStillWork()
    {
        // Arrange
        var headers = new List<SheetCellModel>();

        // Act
        headers.AddColumn(null!);

        // Assert
        Assert.Single(headers);
        Assert.Null(headers[0]);
    }

    [Fact]
    public void UpdateColumns_ShouldPreserveExistingProperties()
    {
        // Arrange
        var headers = new List<SheetCellModel>
        {
            new SheetCellModel { Name = "Header1", Formula = "Formula1" },
            new SheetCellModel { Name = "Header2", Formula = "Formula2" }
        };

        // Act
        headers.UpdateColumns();

        // Assert
        Assert.Equal("Header1", headers[0].Name);
        Assert.Equal("Formula1", headers[0].Formula);
        Assert.Equal("Header2", headers[1].Name);
        Assert.Equal("Formula2", headers[1].Formula);
    }
}