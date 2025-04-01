using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class HeaderHelpersTests
{
    [Fact]
    public void ParserHeader_ShouldReturnCorrectDictionary()
    {
        // Arrange
        var sheetHeader = new List<object> { "Column1", "Column2", "Column3" };

        // Act
        var result = HeaderHelpers.ParserHeader(sheetHeader);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Column1", result[0]);
        Assert.Equal("Column2", result[1]);
        Assert.Equal("Column3", result[2]);
    }

    [Fact]
    public void GetBoolValue_ShouldReturnCorrectBool()
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "BoolColumn" } };
        var values = new List<object> { "TRUE" };

        // Act
        var result = HeaderHelpers.GetBoolValue("BoolColumn", values, headers);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetDateValue_ShouldReturnCorrectDate()
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "DateColumn" } };
        var values = new List<object> { "2023-10-01" };

        // Act
        var result = HeaderHelpers.GetDateValue("DateColumn", values, headers);

        // Assert
        Assert.Equal("2023-10-01", result);
    }

    [Fact]
    public void GetStringValue_ShouldReturnCorrectString()
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "StringColumn" } };
        var values = new List<object> { "TestString" };

        // Act
        var result = HeaderHelpers.GetStringValue("StringColumn", values, headers);

        // Assert
        Assert.Equal("TestString", result);
    }

    [Fact]
    public void GetIntValue_ShouldReturnCorrectInt()
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "IntColumn" } };
        var values = new List<object> { "123" };

        // Act
        var result = HeaderHelpers.GetIntValue("IntColumn", values, headers);

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public void GetDecimalValue_ShouldReturnCorrectDecimal()
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "DecimalColumn" } };
        var values = new List<object> { "123.45" };

        // Act
        var result = HeaderHelpers.GetDecimalValue("DecimalColumn", values, headers);

        // Assert
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void CheckSheetHeaders_ShouldReturnCorrectMessages()
    {
        // Arrange
        var values = new List<IList<object>> { new List<object> { "Header2", "Header1" } };
        var sheetModel = new SheetModel
        {
            Name = "TestSheet",
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = "Header1" },
                new SheetCellModel { Name = "Header2" },
                new SheetCellModel { Name = "Header3" }
            }
        };

        // Act
        var result = HeaderHelpers.CheckSheetHeaders(values[0], sheetModel);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, m => m.Message.Contains("Missing column [Header3]"));
        Assert.Contains(result, m => m.Message.Contains("Unexpected column [Header2] should be [Header1]"));
        Assert.Contains(result, m => m.Message.Contains("Unexpected column [Header1] should be [Header2]"));
    }
}
