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

    #region GetRandomItem Tests

    [Fact]
    public void GetRandomItem_WithValidList_ShouldReturnItemFromList()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3", "item4", "item5" };

        // Act
        var result = list.GetRandomItem();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result, list);
    }

    [Fact]
    public void GetRandomItem_WithSingleItem_ShouldReturnThatItem()
    {
        // Arrange
        var list = new List<int> { 42 };

        // Act
        var result = list.GetRandomItem();

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetRandomItem_WithEmptyList_ShouldThrowArgumentException()
    {
        // Arrange
        var list = new List<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => list.GetRandomItem());
        Assert.Equal("The list cannot be null or empty. (Parameter 'list')", exception.Message);
        Assert.Equal("list", exception.ParamName);
    }

    [Fact]
    public void GetRandomItem_WithNullList_ShouldThrowArgumentException()
    {
        // Arrange
        List<string>? list = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => list!.GetRandomItem());
        Assert.Equal("The list cannot be null or empty. (Parameter 'list')", exception.Message);
        Assert.Equal("list", exception.ParamName);
    }

    [Fact]
    public void GetRandomItem_WithMultipleCalls_ShouldReturnValidResults()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // Act & Assert
        for (int i = 0; i < 50; i++)
        {
            var result = list.GetRandomItem();
            Assert.Contains(result, list);
        }
    }

    [Fact]
    public void GetRandomItem_ShouldReturnDifferentValues()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var results = new HashSet<int>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            results.Add(list.GetRandomItem());
        }

        // Assert - With enough iterations, we should get multiple different values
        Assert.True(results.Count > 1, "GetRandomItem should return different values over multiple calls");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void GetRandomItem_WithVariousListSizes_ShouldWorkCorrectly(int listSize)
    {
        // Arrange
        var list = Enumerable.Range(1, listSize).ToList();

        // Act
        var result = list.GetRandomItem();

        // Assert
        Assert.InRange(result, 1, listSize);
        Assert.Contains(result, list);
    }

    [Fact]
    public void GetRandomItem_WithDifferentTypes_ShouldWorkCorrectly()
    {
        // Arrange & Act & Assert
        
        // String list
        var stringList = new List<string> { "alpha", "beta", "gamma" };
        var stringResult = stringList.GetRandomItem();
        Assert.Contains(stringResult, stringList);

        // Double list
        var doubleList = new List<double> { 1.1, 2.2, 3.3 };
        var doubleResult = doubleList.GetRandomItem();
        Assert.Contains(doubleResult, doubleList);

        // Boolean list
        var boolList = new List<bool> { true, false };
        var boolResult = boolList.GetRandomItem();
        Assert.Contains(boolResult, boolList);

        // Object list
        var objectList = new List<object> { new object(), "string", 123 };
        var objectResult = objectList.GetRandomItem();
        Assert.Contains(objectResult, objectList);
    }

    [Fact]
    public void GetRandomItem_WithNullableTypes_ShouldHandleNulls()
    {
        // Arrange
        var list = new List<string?> { "item1", null, "item3" };

        // Act
        var result = list.GetRandomItem();

        // Assert
        Assert.Contains(result, list);
    }

    [Fact]
    public void GetRandomItem_WithComplexObjects_ShouldWorkCorrectly()
    {
        // Arrange
        var list = new List<SheetCellModel>
        {
            new SheetCellModel { Name = "Header1", Index = 0 },
            new SheetCellModel { Name = "Header2", Index = 1 },
            new SheetCellModel { Name = "Header3", Index = 2 }
        };

        // Act
        var result = list.GetRandomItem();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result, list);
        Assert.NotNull(result.Name);
    }

    [Fact]
    public void GetRandomItem_WithListContainingDuplicates_ShouldWorkCorrectly()
    {
        // Arrange
        var list = new List<int> { 1, 2, 2, 3, 3, 3 };

        // Act
        var result = list.GetRandomItem();

        // Assert
        Assert.Contains(result, list);
        Assert.InRange(result, 1, 3);
    }

    [Fact]
    public void GetRandomItem_ShouldUseNewRandomInstanceEachTime()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var results = new List<int>();

        // Act - Call multiple times to see if we get variety
        for (int i = 0; i < 20; i++)
        {
            results.Add(list.GetRandomItem());
        }

        // Assert - Should get some variety in results (not all the same)
        var distinctResults = results.Distinct().Count();
        Assert.True(distinctResults > 1, "Should get variety in random results");
    }

    #endregion
}