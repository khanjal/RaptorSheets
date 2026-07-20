using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using System.Linq;
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
        Assert.Contains(result, m => m.Message.Contains("Column [Header2] should be [Header1]"));
        Assert.Contains(result, m => m.Message.Contains("Column [Header1] should be [Header2]"));
    }

    [Theory]
    [InlineData(new object[] { "Col1", "Col2", "Col3" }, 3)]
    [InlineData(new object[] { "Single" }, 1)]
    [InlineData(new object[] { }, 0)]
    public void ParserHeader_WithVariousInputs_ShouldReturnCorrectDictionary(object[] headers, int expectedCount)
    {
        // Arrange
        var sheetHeader = headers.ToList();

        // Act
        var result = HeaderHelpers.ParserHeader(sheetHeader);

        // Assert
        Assert.Equal(expectedCount, result.Count);
        for (int i = 0; i < expectedCount; i++)
        {
            Assert.Equal(headers[i].ToString(), result[i]);
        }
    }

    [Theory]
    [InlineData("TRUE", true)]
    [InlineData("true", true)]
    [InlineData("TRUE ", true)]
    [InlineData("FALSE", false)]
    [InlineData("false", false)]
    [InlineData("FALSE ", false)]
    [InlineData("", false)]
    [InlineData("invalid", false)]
    [InlineData("1", false)]
    [InlineData("0", false)]
    public void GetBoolValue_WithVariousInputs_ShouldReturnCorrectBool(string input, bool expected)
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "BoolColumn" } };
        var values = new List<object> { input };

        // Act
        var result = HeaderHelpers.GetBoolValue("BoolColumn", values, headers);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("2023-10-01", "2023-10-01")]
    [InlineData("2023/10/01", "2023/10/01")]
    [InlineData("", "")]
    [InlineData("invalid-date", "invalid-date")]
    public void GetDateValue_WithVariousFormats_ShouldReturnCorrectDate(string input, string expected)
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "DateColumn" } };
        var values = new List<object> { input };

        // Act
        var result = HeaderHelpers.GetDateValue("DateColumn", values, headers);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", 123)]
    [InlineData("0", 0)]
    [InlineData("-123", -123)]
    [InlineData("", 0)]
    [InlineData("invalid", 0)]
    [InlineData("123.45", 0)]
    [InlineData("2147483647", 2147483647)] // Max int
    [InlineData("2147483648", 0)] // Overflow
    public void GetIntValue_WithVariousInputs_ShouldReturnCorrectInt(string input, int expected)
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "IntColumn" } };
        var values = new List<object> { input };

        // Act
        var result = HeaderHelpers.GetIntValue("IntColumn", values, headers);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123.45", 123.45)]
    [InlineData("0", 0)]
    [InlineData("-123.45", -123.45)]
    [InlineData("", 0)]
    [InlineData("invalid", 0)]
    [InlineData("123", 123)]
    [InlineData("123.456789", 123.456789)]
    public void GetDecimalValue_WithVariousInputs_ShouldReturnCorrectDecimal(string input, double expected)
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "DecimalColumn" } };
        var values = new List<object> { input };

        // Act
        var result = HeaderHelpers.GetDecimalValue("DecimalColumn", values, headers);

        // Assert
        Assert.Equal((decimal)expected, result);
    }

    [Fact]
    public void GetValue_WithMissingColumn_ShouldReturnDefault()
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "ExistingColumn" } };
        var values = new List<object> { "value" };

        // Act
        var stringResult = HeaderHelpers.GetStringValue("MissingColumn", values, headers);
        var intResult = HeaderHelpers.GetIntValue("MissingColumn", values, headers);
        var boolResult = HeaderHelpers.GetBoolValue("MissingColumn", values, headers);
        var decimalResult = HeaderHelpers.GetDecimalValue("MissingColumn", values, headers);

        // Assert
        Assert.Equal("", stringResult);
        Assert.Equal(0, intResult);
        Assert.False(boolResult);
        Assert.Equal(0, decimalResult);
    }

    [Fact]
    public void GetValue_WithIndexOutOfRange_ShouldReturnDefault()
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 5, "OutOfRangeColumn" } };
        var values = new List<object> { "value1", "value2" }; // Only 2 items, index 5 is out of range

        // Act
        var result = HeaderHelpers.GetStringValue("OutOfRangeColumn", values, headers);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void CheckSheetHeaders_WithPerfectMatch_ShouldReturnNoMessages()
    {
        // Arrange
        var values = new List<object> { "Header1", "Header2", "Header3" };
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
        var result = HeaderHelpers.CheckSheetHeaders(values, sheetModel);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void CheckSheetHeaders_WithExtraHeaders_ShouldReturnWarningMessages()
    {
        // Arrange
        var values = new List<object> { "Header1", "Header2", "Header3", "ExtraHeader" };
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
        var result = HeaderHelpers.CheckSheetHeaders(values, sheetModel);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, m => m.Message.Contains("Extra column"));
    }

    [Fact]
    public void CheckSheetHeaders_WithEmptyValues_ShouldReturnErrorMessages()
    {
        // Arrange
        var values = new List<object>();
        var sheetModel = new SheetModel
        {
            Name = "TestSheet",
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = "Header1" }
            }
        };

        // Act
        var result = HeaderHelpers.CheckSheetHeaders(values, sheetModel);

        // Assert
        Assert.NotEmpty(result);
        // Accept either the detailed per-column missing messages or the single
        // 'No header row found or sheet is empty' message introduced to reduce noise.
        Assert.Contains(result, m => m.Message.Contains("Missing column") || m.Message.Contains("No header row found"));
    }    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetStringValue_WithNullOrWhitespaceInput_ShouldReturnEmpty(string input)
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "StringColumn" } };
        var values = new List<object> { input };

        // Act
        var result = HeaderHelpers.GetStringValue("StringColumn", values, headers);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void GetStringValue_WithNullInput_ShouldReturnEmpty()
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "StringColumn" } };
        var values = new List<object> { null! };

        // Act
        var result = HeaderHelpers.GetStringValue("StringColumn", values, headers);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void ParserHeader_WithNullInput_ShouldReturnEmptyDictionary()
    {
        // Act
        var result = HeaderHelpers.ParserHeader(null!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // GetHeaderKey (used internally by every GetXValue) matches columns by name, not position,
    // so a user swapping two columns in the sheet must still map to the correct values. This also
    // exercises the O(1) reverse-index cache introduced to replace the old linear scan.
    [Fact]
    public void GetStringValue_WithSwappedColumnOrder_ShouldStillMatchByName()
    {
        // Arrange - "Name" and "Date" are swapped relative to a "natural" A, B order
        var headers = new Dictionary<int, string> { { 0, "Date" }, { 1, "Name" } };
        var values = new List<object> { "2023-10-01", "Alice" };

        // Act
        var name = HeaderHelpers.GetStringValue("Name", values, headers);
        var date = HeaderHelpers.GetStringValue("Date", values, headers);

        // Assert
        Assert.Equal("Alice", name);
        Assert.Equal("2023-10-01", date);
    }

    [Fact]
    public void GetIntValue_WithReorderedColumns_ShouldStillMatchByName()
    {
        // Arrange - deliberately out of "natural" order
        var headers = new Dictionary<int, string> { { 0, "Name" }, { 1, "Amount" }, { 2, "Id" } };
        var values = new List<object> { "Bob", "42", "7" };

        // Act & Assert
        Assert.Equal(7, HeaderHelpers.GetIntValue("Id", values, headers));
        Assert.Equal(42, HeaderHelpers.GetIntValue("Amount", values, headers));
        Assert.Equal("Bob", HeaderHelpers.GetStringValue("Name", values, headers));
    }

    [Fact]
    public void GetStringValue_WithDistinctHeaderInstancesSameNames_ShouldEachMatchOwnValues()
    {
        // Arrange - two distinct Dictionary instances (as happens once per sheet in real mapping),
        // same header names but different column order, to confirm the per-instance reverse-index
        // cache doesn't leak/collide between them.
        var headersA = new Dictionary<int, string> { { 0, "Name" }, { 1, "Id" } };
        var valuesA = new List<object> { "Alice", "1" };

        var headersB = new Dictionary<int, string> { { 0, "Id" }, { 1, "Name" } };
        var valuesB = new List<object> { "2", "Bob" };

        // Act
        var nameA = HeaderHelpers.GetStringValue("Name", valuesA, headersA);
        var nameB = HeaderHelpers.GetStringValue("Name", valuesB, headersB);

        // Assert
        Assert.Equal("Alice", nameA);
        Assert.Equal("Bob", nameB);
    }

    [Fact]
    public void GetStringValue_WithUnknownColumnName_ShouldReturnEmptyNotThrow()
    {
        // Arrange
        var headers = new Dictionary<int, string> { { 0, "Known" } };
        var values = new List<object> { "value" };

        // Act
        var result = HeaderHelpers.GetStringValue("DoesNotExist", values, headers);

        // Assert
        Assert.Equal("", result);
    }

    // CheckSheetHeaders(values, sheetModel, out insertionInfo) - missing-column detection used by
    // the column auto-insertion feature (RaptorSheets.Core.Helpers.ColumnInsertionHelper).

    [Fact]
    public void CheckSheetHeaders_WithMissingColumn_ShouldReportInsertionInfoAtExpectedIndex()
    {
        // Arrange - "Header2" is missing entirely
        var values = new List<object> { "Header1", "Header3" };
        var sheetModel = new SheetModel
        {
            Name = "TestSheet",
            Headers =
            [
                new SheetCellModel { Name = "Header1" },
                new SheetCellModel { Name = "Header2" },
                new SheetCellModel { Name = "Header3" }
            ]
        };

        // Act
        var messages = HeaderHelpers.CheckSheetHeaders(values, sheetModel, out var insertionInfo);

        // Assert
        Assert.Single(insertionInfo);
        Assert.Equal("Header2", insertionInfo[0].ColumnName);
        Assert.Equal(1, insertionInfo[0].ColumnIndex);
        Assert.Equal("TestSheet", insertionInfo[0].SheetName);
        Assert.Equal(0, insertionInfo[0].SheetId); // caller fills this in from spreadsheet metadata
        Assert.Contains(messages, m => m.Message.Contains("Missing column [Header2]") && m.Message.Contains("can be inserted"));
    }

    [Fact]
    public void CheckSheetHeaders_WithNoMissingColumns_ShouldReturnEmptyInsertionInfo()
    {
        var values = new List<object> { "Header1", "Header2" };
        var sheetModel = new SheetModel
        {
            Name = "TestSheet",
            Headers = [new SheetCellModel { Name = "Header1" }, new SheetCellModel { Name = "Header2" }]
        };

        var messages = HeaderHelpers.CheckSheetHeaders(values, sheetModel, out var insertionInfo);

        Assert.Empty(insertionInfo);
        Assert.Empty(messages);
    }

    [Fact]
    public void CheckSheetHeaders_WithMultipleMissingColumns_ShouldReportEachAtItsOwnIndex()
    {
        var values = new List<object> { "Header2" };
        var sheetModel = new SheetModel
        {
            Name = "TestSheet",
            Headers =
            [
                new SheetCellModel { Name = "Header1" },
                new SheetCellModel { Name = "Header2" },
                new SheetCellModel { Name = "Header3" }
            ]
        };

        HeaderHelpers.CheckSheetHeaders(values, sheetModel, out var insertionInfo);

        Assert.Equal(2, insertionInfo.Count);
        Assert.Contains(insertionInfo, i => i.ColumnName == "Header1" && i.ColumnIndex == 0);
        Assert.Contains(insertionInfo, i => i.ColumnName == "Header3" && i.ColumnIndex == 2);
    }

    [Fact]
    public void CheckSheetHeaders_TwoArgOverload_StillWorksWithoutInsertionInfo()
    {
        // The original two-arg overload must remain unaffected by adding the out-param version
        var values = new List<object> { "Header1" };
        var sheetModel = new SheetModel
        {
            Name = "TestSheet",
            Headers = [new SheetCellModel { Name = "Header1" }, new SheetCellModel { Name = "Header2" }]
        };

        var messages = HeaderHelpers.CheckSheetHeaders(values, sheetModel);

        Assert.Contains(messages, m => m.Message.Contains("Missing column [Header2]"));
    }

    // HideHeaderName columns (e.g. Delivery/Location's "First Trip"/"Last Trip") are populated by
    // a spilling QUERY formula placed in an earlier header cell, not written as their own
    // standalone cell - they must never be treated as insertable, since a physical column insert
    // would land inside the query's contiguous spill range and break it.

    [Fact]
    public void CheckSheetHeaders_WithMissingHideHeaderNameColumn_ShouldNotAddToInsertionInfo()
    {
        // Arrange - "QueryLabel" is missing and marked HideHeaderName (populated by a spilling
        // QUERY elsewhere on the sheet), "Regular" is missing and is a normal column
        var values = new List<object> { "Header1" };
        var sheetModel = new SheetModel
        {
            Name = "TestSheet",
            Headers =
            [
                new SheetCellModel { Name = "Header1" },
                new SheetCellModel { Name = "QueryLabel", HideHeaderName = true },
                new SheetCellModel { Name = "Regular" }
            ]
        };

        // Act
        var messages = HeaderHelpers.CheckSheetHeaders(values, sheetModel, out var insertionInfo);

        // Assert - only the regular column is a candidate for insertion
        Assert.Single(insertionInfo);
        Assert.Equal("Regular", insertionInfo[0].ColumnName);
        Assert.DoesNotContain(insertionInfo, i => i.ColumnName == "QueryLabel");

        // The HideHeaderName column still gets reported as missing, but without the
        // "can be inserted" claim, since it isn't one
        Assert.Contains(messages, m => m.Message.Contains("Missing column [QueryLabel]") && !m.Message.Contains("can be inserted"));
        Assert.Contains(messages, m => m.Message.Contains("Missing column [Regular] - can be inserted"));
    }
}
