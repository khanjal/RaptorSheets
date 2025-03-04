using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class SheetHelpersTests
{
    [Fact]
    public void GetSpreadsheetTitle_ShouldReturnTitle()
    {
        // Arrange
        var sheet = new Spreadsheet
        {
            Properties = new SpreadsheetProperties { Title = "TestTitle" }
        };

        // Act
        var result = SheetHelpers.GetSpreadsheetTitle(sheet);

        // Assert
        Assert.Equal("TestTitle", result);
    }

    [Fact]
    public void GetSpreadsheetSheets_ShouldReturnSheetTitles()
    {
        // Arrange
        var sheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new Sheet { Properties = new SheetProperties { Title = "Sheet1" } },
                new Sheet { Properties = new SheetProperties { Title = "Sheet2" } }
            }
        };

        // Act
        var result = SheetHelpers.GetSpreadsheetSheets(sheet);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("SHEET1", result);
        Assert.Contains("SHEET2", result);
    }

    [Fact]
    public void GetColor_ShouldReturnCorrectColor()
    {
        // Act & Assert
        Assert.Equivalent(Colors.Black, SheetHelpers.GetColor(ColorEnum.BLACK));
        Assert.Equivalent(Colors.Blue, SheetHelpers.GetColor(ColorEnum.BLUE));
        Assert.Equivalent(Colors.Cyan, SheetHelpers.GetColor(ColorEnum.CYAN));
        // Add more assertions for other colors as needed
    }

    [Fact]
    public void GetColumnName_ShouldReturnCorrectColumnName()
    {
        // Act & Assert
        Assert.Equal("A", SheetHelpers.GetColumnName(0));
        Assert.Equal("B", SheetHelpers.GetColumnName(1));
        Assert.Equal("Z", SheetHelpers.GetColumnName(25));
        Assert.Equal("AA", SheetHelpers.GetColumnName(26));
        Assert.Equal("AB", SheetHelpers.GetColumnName(27));
    }

    [Fact]
    public void HeadersToList_ShouldReturnCorrectList()
    {
        // Arrange
        var headers = new List<SheetCellModel>
        {
            new SheetCellModel { Name = "Header1" },
            new SheetCellModel { Name = "Header2", Formula = "=SUM(A1:A10)" }
        };

        // Act
        var result = SheetHelpers.HeadersToList(headers);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Count);
        Assert.Equal("Header1", result[0][0]);
        Assert.Equal("=SUM(A1:A10)", result[0][1]);
    }

    [Fact]
    public void HeadersToRowData_ShouldReturnCorrectRowData()
    {
        // Arrange
        var sheet = new SheetModel
        {
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = "Header1" },
                new SheetCellModel { Name = "Header2", Formula = "=SUM(A1:A10)" }
            }
        };

        // Act
        var result = SheetHelpers.HeadersToRowData(sheet);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Values.Count);
        Assert.Equal("Header1", result[0].Values[0].UserEnteredValue.StringValue);
        Assert.Equal("=SUM(A1:A10)", result[0].Values[1].UserEnteredValue.FormulaValue);
    }

    [Fact]
    public void GetCellFormat_ShouldReturnCorrectCellFormat()
    {
        // Act & Assert
        Assert.Equal("NUMBER", SheetHelpers.GetCellFormat(FormatEnum.ACCOUNTING).NumberFormat.Type);
        Assert.Equal("DATE", SheetHelpers.GetCellFormat(FormatEnum.DATE).NumberFormat.Type);
        Assert.Equal("NUMBER", SheetHelpers.GetCellFormat(FormatEnum.DISTANCE).NumberFormat.Type);
        Assert.Equal("DATE", SheetHelpers.GetCellFormat(FormatEnum.DURATION).NumberFormat.Type);
        Assert.Equal("NUMBER", SheetHelpers.GetCellFormat(FormatEnum.NUMBER).NumberFormat.Type);
        Assert.Equal("TEXT", SheetHelpers.GetCellFormat(FormatEnum.TEXT).NumberFormat.Type);
        Assert.Equal("DATE", SheetHelpers.GetCellFormat(FormatEnum.TIME).NumberFormat.Type);
        Assert.Equal("DATE", SheetHelpers.GetCellFormat(FormatEnum.WEEKDAY).NumberFormat.Type);
    }
}