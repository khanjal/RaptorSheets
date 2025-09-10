using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class SheetHelpersTests
{
    #region CheckSheets Tests

    public enum TestSheetEnum
    {
        SHEET1, // Use uppercase to match the actual behavior
        SHEET2,
        MISSINGSHEET
    }

    [Fact]
    public void CheckSheets_WithAllSheetsPresent_ShouldReturnEmptyList()
    {
        // Arrange
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new Sheet { Properties = new SheetProperties { Title = "SHEET1" } },
                new Sheet { Properties = new SheetProperties { Title = "SHEET2" } },
                new Sheet { Properties = new SheetProperties { Title = "MISSINGSHEET" } }
            }
        };

        // Act
        var result = SheetHelpers.CheckSheets<TestSheetEnum>(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CheckSheets_WithMissingSheets_ShouldReturnMissingSheetNames()
    {
        // Arrange
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new Sheet { Properties = new SheetProperties { Title = "SHEET1" } },
                new Sheet { Properties = new SheetProperties { Title = "SHEET2" } }
                // MISSINGSHEET is not present
            }
        };

        // Act
        var result = SheetHelpers.CheckSheets<TestSheetEnum>(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("MISSINGSHEET", result);
    }

    [Fact]
    public void CheckSheets_WithNullSpreadsheet_ShouldReturnAllSheetNames()
    {
        // Arrange - Test with empty spreadsheet to verify handling behavior
        Spreadsheet spreadsheet = new Spreadsheet();

        // Act
        var result = SheetHelpers.CheckSheets<TestSheetEnum>(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains("SHEET1", result);
        Assert.Contains("SHEET2", result);
        Assert.Contains("MISSINGSHEET", result);
    }

    [Fact]
    public void CheckSheets_WithActuallyNullSpreadsheet_ShouldReturnAllSheetNames()
    {
        // Arrange - Test with actual null to verify null handling behavior
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Spreadsheet? spreadsheet = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var result = SheetHelpers.CheckSheets<TestSheetEnum>(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains("SHEET1", result);
        Assert.Contains("SHEET2", result);
        Assert.Contains("MISSINGSHEET", result);
    }

    [Fact]
    public void CheckSheets_WithEmptySpreadsheet_ShouldReturnAllSheetNames()
    {
        // Arrange
        var spreadsheet = new Spreadsheet { Sheets = new List<Sheet>() };

        // Act
        var result = SheetHelpers.CheckSheets<TestSheetEnum>(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains("SHEET1", result);
        Assert.Contains("SHEET2", result);
        Assert.Contains("MISSINGSHEET", result);
    }

    [Fact]
    public void CheckSheets_WithCaseMismatch_ShouldReturnMissingSheets()
    {
        // Arrange - The method compares enum names (uppercase) with spreadsheet titles (converted to uppercase)
        // So case actually doesn't matter because GetSpreadsheetSheets converts to uppercase
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new Sheet { Properties = new SheetProperties { Title = "sheet1" } }, // Will be converted to SHEET1
                new Sheet { Properties = new SheetProperties { Title = "Sheet2" } }, // Will be converted to SHEET2
            }
        };

        // Act
        var result = SheetHelpers.CheckSheets<TestSheetEnum>(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Only MISSINGSHEET should be missing
        Assert.Contains("MISSINGSHEET", result);
    }

    [Fact]
    public void CheckSheets_WithStringList_WithMissingSheets_ShouldReturnErrorMessages()
    {
        // Arrange
        var missingSheets = new List<string> { "Sheet1", "Sheet2" };

        // Act
        var result = SheetHelpers.CheckSheets(missingSheets);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, message => 
        {
            Assert.Equal(MessageLevelEnum.ERROR.UpperName(), message.Level);
            Assert.Equal(MessageTypeEnum.CHECK_SHEET.GetDescription(), message.Type);
            Assert.Contains("Unable to find sheet", message.Message);
        });
    }

    [Fact]
    public void CheckSheets_WithStringList_WithEmptyList_ShouldReturnSuccessMessage()
    {
        // Arrange
        var missingSheets = new List<string>();

        // Act
        var result = SheetHelpers.CheckSheets(missingSheets);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(MessageLevelEnum.INFO.UpperName(), result[0].Level);
        Assert.Equal(MessageTypeEnum.CHECK_SHEET.GetDescription(), result[0].Type);
        Assert.Equal("All sheets found", result[0].Message);
    }

    #endregion

    #region GetSpreadsheetTitle Tests

    [Fact]
    public void GetSpreadsheetTitle_WithValidSpreadsheet_ShouldReturnTitle()
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
    public void GetSpreadsheetTitle_WithNullSpreadsheet_ShouldReturnEmptyString()
    {
        // Arrange
        Spreadsheet? sheet = null;

        // Act
        var result = SheetHelpers.GetSpreadsheetTitle(sheet);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetSpreadsheetTitle_WithNullProperties_ShouldThrow()
    {
        // Arrange
        var sheet = new Spreadsheet { Properties = null };

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => SheetHelpers.GetSpreadsheetTitle(sheet));
    }

    [Theory]
    [InlineData("My Spreadsheet")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Special Characters: !@#$%^&*()")]
    public void GetSpreadsheetTitle_WithVariousTitles_ShouldReturnTitle(string title)
    {
        // Arrange
        var sheet = new Spreadsheet
        {
            Properties = new SpreadsheetProperties { Title = title }
        };

        // Act
        var result = SheetHelpers.GetSpreadsheetTitle(sheet);

        // Assert
        Assert.Equal(title, result);
    }

    #endregion

    #region GetSpreadsheetSheets Tests

    [Fact]
    public void GetSpreadsheetSheets_WithValidSpreadsheet_ShouldReturnSheetTitles()
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
    public void GetSpreadsheetSheets_WithNullSpreadsheet_ShouldReturnEmptyList()
    {
        // Arrange - Test with empty spreadsheet to verify handling behavior
        Spreadsheet sheet = new Spreadsheet();

        // Act
        var result = SheetHelpers.GetSpreadsheetSheets(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetSpreadsheetSheets_WithActuallyNullSpreadsheet_ShouldReturnEmptyList()
    {
        // Arrange - Test with actual null to verify null handling behavior
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Spreadsheet? sheet = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type;

        // Act
        var result = SheetHelpers.GetSpreadsheetSheets(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetSpreadsheetSheets_WithEmptySheets_ShouldReturnEmptyList()
    {
        // Arrange
        var sheet = new Spreadsheet { Sheets = new List<Sheet>() };

        // Act
        var result = SheetHelpers.GetSpreadsheetSheets(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetSpreadsheetSheets_ShouldReturnUppercaseTitles()
    {
        // Arrange
        var sheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new Sheet { Properties = new SheetProperties { Title = "lowercase" } },
                new Sheet { Properties = new SheetProperties { Title = "MixedCase" } },
                new Sheet { Properties = new SheetProperties { Title = "UPPERCASE" } }
            }
        };

        // Act
        var result = SheetHelpers.GetSpreadsheetSheets(sheet);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("LOWERCASE", result);
        Assert.Contains("MIXEDCASE", result);
        Assert.Contains("UPPERCASE", result);
    }

    #endregion

    #region GetSheetValues Tests

    [Fact]
    public void GetSheetValues_WithValidData_ShouldReturnSheetValues()
    {
        // Arrange
        var sheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new Sheet
                {
                    Properties = new SheetProperties { Title = "TestSheet" },
                    Data = new List<GridData>
                    {
                        new GridData
                        {
                            RowData = new List<RowData>
                            {
                                new RowData
                                {
                                    Values = new List<CellData>
                                    {
                                        new CellData { FormattedValue = "Header1" },
                                        new CellData { FormattedValue = "Header2" }
                                    }
                                },
                                new RowData
                                {
                                    Values = new List<CellData>
                                    {
                                        new CellData { FormattedValue = "Value1" },
                                        new CellData { FormattedValue = "Value2" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = SheetHelpers.GetSheetValues(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.ContainsKey("TestSheet"));
        Assert.Equal(2, result["TestSheet"].Count);
        Assert.Equal("Header1", result["TestSheet"][0][0]);
        Assert.Equal("Value1", result["TestSheet"][1][0]);
    }

    [Fact]
    public void GetSheetValues_WithNullRowData_ShouldReturnEmptyValues()
    {
        // Arrange
        var sheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new Sheet
                {
                    Properties = new SheetProperties { Title = "EmptySheet" },
                    Data = new List<GridData>
                    {
                        new GridData { RowData = null }
                    }
                }
            }
        };

        // Act
        var result = SheetHelpers.GetSheetValues(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.ContainsKey("EmptySheet"));
        Assert.Empty(result["EmptySheet"]);
    }

    [Fact]
    public void GetSheetValues_ShouldFilterEmptyRows()
    {
        // Arrange
        var sheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new Sheet
                {
                    Properties = new SheetProperties { Title = "TestSheet" },
                    Data = new List<GridData>
                    {
                        new GridData
                        {
                            RowData = new List<RowData>
                            {
                                new RowData
                                {
                                    Values = new List<CellData>
                                    {
                                        new CellData { FormattedValue = "ValidRow" }
                                    }
                                },
                                new RowData
                                {
                                    Values = new List<CellData>
                                    {
                                        new CellData { FormattedValue = "" } // Empty first cell
                                    }
                                },
                                new RowData
                                {
                                    Values = new List<CellData>() // No values
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = SheetHelpers.GetSheetValues(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.ContainsKey("TestSheet"));
        Assert.Single(result["TestSheet"]); // Only one valid row
        Assert.Equal("ValidRow", result["TestSheet"][0][0]);
    }

    #endregion

    #region GetColor Tests

    [Theory]
    [InlineData(ColorEnum.BLACK)]
    [InlineData(ColorEnum.BLUE)]
    [InlineData(ColorEnum.CYAN)]
    [InlineData(ColorEnum.DARK_YELLOW)]
    [InlineData(ColorEnum.GREEN)]
    [InlineData(ColorEnum.LIGHT_CYAN)]
    [InlineData(ColorEnum.LIGHT_GRAY)]
    [InlineData(ColorEnum.LIGHT_GREEN)]
    [InlineData(ColorEnum.LIGHT_PURPLE)]
    [InlineData(ColorEnum.LIGHT_RED)]
    [InlineData(ColorEnum.LIGHT_YELLOW)]
    [InlineData(ColorEnum.LIME)]
    [InlineData(ColorEnum.ORANGE)]
    [InlineData(ColorEnum.MAGENTA)]
    [InlineData(ColorEnum.PINK)]
    [InlineData(ColorEnum.PURPLE)]
    [InlineData(ColorEnum.RED)]
    [InlineData(ColorEnum.WHITE)]
    [InlineData(ColorEnum.YELLOW)]
    public void GetColor_WithValidColorEnum_ShouldReturnColor(ColorEnum colorEnum)
    {
        // Act
        var result = SheetHelpers.GetColor(colorEnum);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetColor_WithSpecificColors_ShouldReturnCorrectColors()
    {
        // Act & Assert
        Assert.Equivalent(Colors.Black, SheetHelpers.GetColor(ColorEnum.BLACK));
        Assert.Equivalent(Colors.Blue, SheetHelpers.GetColor(ColorEnum.BLUE));
        Assert.Equivalent(Colors.Cyan, SheetHelpers.GetColor(ColorEnum.CYAN));
        Assert.Equivalent(Colors.Magenta, SheetHelpers.GetColor(ColorEnum.MAGENTA));
        Assert.Equivalent(Colors.Magenta, SheetHelpers.GetColor(ColorEnum.PINK));
        Assert.Equivalent(Colors.White, SheetHelpers.GetColor(ColorEnum.WHITE));
    }

    [Fact]
    public void GetColor_WithInvalidColorEnum_ShouldReturnWhite()
    {
        // Act
        var result = SheetHelpers.GetColor((ColorEnum)999);

        // Assert
        Assert.Equivalent(Colors.White, result);
    }

    #endregion

    #region GetColumnName Tests

    [Theory]
    [InlineData(0, "A")]
    [InlineData(1, "B")]
    [InlineData(25, "Z")]
    [InlineData(26, "AA")]
    [InlineData(27, "AB")]
    [InlineData(51, "AZ")]
    [InlineData(52, "BA")]
    [InlineData(675, "YZ")]
    [InlineData(676, "ZA")]
    [InlineData(677, "ZB")]
    [InlineData(701, "ZZ")]
    public void GetColumnName_WithVariousIndexes_ShouldReturnCorrectColumnName(int index, string expected)
    {
        // Act
        var result = SheetHelpers.GetColumnName(index);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region HeadersToList Tests

    [Fact]
    public void HeadersToList_WithValidHeaders_ShouldReturnCorrectList()
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
    public void HeadersToList_WithEmptyHeaders_ShouldReturnEmptyRow()
    {
        // Arrange
        var headers = new List<SheetCellModel>();

        // Act
        var result = SheetHelpers.HeadersToList(headers);

        // Assert
        Assert.Single(result);
        Assert.Empty(result[0]);
    }

    [Fact]
    public void HeadersToList_WithMixedFormulasAndNames_ShouldPrioritizeFormulas()
    {
        // Arrange
        var headers = new List<SheetCellModel>
        {
            new SheetCellModel { Name = "Name1", Formula = "" },
            new SheetCellModel { Name = "Name2", Formula = "Formula2" },
            new SheetCellModel { Name = "Name3" }
        };

        // Act
        var result = SheetHelpers.HeadersToList(headers);

        // Assert
        Assert.Single(result);
        Assert.Equal(3, result[0].Count);
        Assert.Equal("Name1", result[0][0]); // Empty formula, use name
        Assert.Equal("Formula2", result[0][1]); // Has formula, use formula
        Assert.Equal("Name3", result[0][2]); // No formula, use name
    }

    [Fact]
    public void HeadersToList_WithNullNames_ShouldHandleGracefully()
    {
        // Arrange
        var headers = new List<SheetCellModel>
        {
            new SheetCellModel { Name = null, Formula = "Formula1" },
            new SheetCellModel { Name = null, Formula = null }
        };

        // Act
        var result = SheetHelpers.HeadersToList(headers);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Count);
        Assert.Equal("Formula1", result[0][0]);
        Assert.Null(result[0][1]);
    }

    #endregion

    #region HeadersToRowData Tests

    [Fact]
    public void HeadersToRowData_WithValidHeaders_ShouldReturnCorrectRowData()
    {
        // Arrange
        var sheet = new SheetModel
        {
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = "Header1" },
                new SheetCellModel { Name = "Header2", Formula = "=SUM(A1:A10)" }
            },
            FontColor = ColorEnum.BLACK
        };

        // Act
        var result = SheetHelpers.HeadersToRowData(sheet);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Values.Count);
        Assert.Equal("Header1", result[0].Values[0].UserEnteredValue.StringValue);
        Assert.Equal("=SUM(A1:A10)", result[0].Values[1].UserEnteredValue.FormulaValue);
        Assert.True(result[0].Values[0].UserEnteredFormat.TextFormat.Bold);
        Assert.True(result[0].Values[1].UserEnteredFormat.TextFormat.Bold);
    }

    [Fact]
    public void HeadersToRowData_WithProtectedHeaders_ShouldAddBorders()
    {
        // Arrange
        var sheet = new SheetModel
        {
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = "Protected", Protect = true } // Formula defaults to empty string
            },
            ProtectSheet = false,
            FontColor = ColorEnum.RED
        };

        // Act
        var result = SheetHelpers.HeadersToRowData(sheet);

        // Assert
        Assert.Single(result);
        Assert.Single(result[0].Values);
        // When Protect is true but Formula is empty, the condition (!string.IsNullOrEmpty(header.Formula) || header.Protect) is true
        // But since header.Formula is empty string (not null), header.Formula != null is true
        // So it should use FormulaValue = header.Formula (which is empty string)
        // Actually looking at the logic: if Formula is empty string but not null, it uses header.Formula
        Assert.Equal("", result[0].Values[0].UserEnteredValue.FormulaValue); // Empty formula
        Assert.NotNull(result[0].Values[0].UserEnteredFormat.Borders);
        Assert.Equal("SOLID_THICK", result[0].Values[0].UserEnteredFormat.Borders.Bottom.Style);
    }

    [Fact]
    public void HeadersToRowData_WithProtectedHeadersAndNullFormula_ShouldUseNameAsFormula()
    {
        // Arrange
        var sheet = new SheetModel
        {
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = "Protected", Protect = true, Formula = null! } // Explicitly null
            },
            ProtectSheet = false,
            FontColor = ColorEnum.RED
        };

        // Act
        var result = SheetHelpers.HeadersToRowData(sheet);

        // Assert
        Assert.Single(result);
        Assert.Single(result[0].Values);
        // When Protect is true and Formula is null, it uses Name as FormulaValue
        Assert.Equal("Protected", result[0].Values[0].UserEnteredValue.FormulaValue);
        Assert.NotNull(result[0].Values[0].UserEnteredFormat.Borders);
        Assert.Equal("SOLID_THICK", result[0].Values[0].UserEnteredFormat.Borders.Bottom.Style);
    }

    [Fact]
    public void HeadersToRowData_WithNotes_ShouldIncludeNotes()
    {
        // Arrange
        var sheet = new SheetModel
        {
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = "Header1", Note = "This is a note" }
            }
        };

        // Act
        var result = SheetHelpers.HeadersToRowData(sheet);

        // Assert
        Assert.Single(result);
        Assert.Single(result[0].Values);
        Assert.Equal("This is a note", result[0].Values[0].Note);
    }

    [Fact]
    public void HeadersToRowData_WithEmptyHeaders_ShouldReturnEmptyRowData()
    {
        // Arrange
        var sheet = new SheetModel
        {
            Headers = new List<SheetCellModel>()
        };

        // Act
        var result = SheetHelpers.HeadersToRowData(sheet);

        // Assert
        Assert.Single(result);
        Assert.Empty(result[0].Values);
    }

    #endregion

    #region GetCellFormat Tests

    [Theory]
    [InlineData(FormatEnum.ACCOUNTING, "NUMBER")]
    [InlineData(FormatEnum.DATE, "DATE")]
    [InlineData(FormatEnum.DISTANCE, "NUMBER")]
    [InlineData(FormatEnum.DURATION, "DATE")]
    [InlineData(FormatEnum.NUMBER, "NUMBER")]
    [InlineData(FormatEnum.TEXT, "TEXT")]
    [InlineData(FormatEnum.TIME, "DATE")]
    [InlineData(FormatEnum.WEEKDAY, "DATE")]
    public void GetCellFormat_WithValidFormatEnum_ShouldReturnCorrectType(FormatEnum format, string expectedType)
    {
        // Act
        var result = SheetHelpers.GetCellFormat(format);

        // Assert
        Assert.Equal(expectedType, result.NumberFormat.Type);
        // For TEXT format, Pattern is null, for others it should not be null
        if (format == FormatEnum.TEXT)
        {
            Assert.Null(result.NumberFormat.Pattern);
        }
        else
        {
            Assert.NotNull(result.NumberFormat.Pattern);
        }
    }

    [Fact]
    public void GetCellFormat_WithSpecificPatterns_ShouldReturnCorrectPatterns()
    {
        // Act & Assert
        var accountingFormat = SheetHelpers.GetCellFormat(FormatEnum.ACCOUNTING);
        Assert.Equal(CellFormatPatterns.Accounting, accountingFormat.NumberFormat.Pattern);

        var dateFormat = SheetHelpers.GetCellFormat(FormatEnum.DATE);
        Assert.Equal(CellFormatPatterns.Date, dateFormat.NumberFormat.Pattern);

        var distanceFormat = SheetHelpers.GetCellFormat(FormatEnum.DISTANCE);
        Assert.Equal(CellFormatPatterns.Distance, distanceFormat.NumberFormat.Pattern);

        var textFormat = SheetHelpers.GetCellFormat(FormatEnum.TEXT);
        Assert.Null(textFormat.NumberFormat.Pattern); // TEXT format doesn't have a pattern
    }

    [Fact]
    public void GetCellFormat_WithDefaultFormat_ShouldReturnTextFormat()
    {
        // Act
        var result = SheetHelpers.GetCellFormat(FormatEnum.DEFAULT);

        // Assert
        Assert.Equal("TEXT", result.NumberFormat.Type);
    }

    [Fact]
    public void GetCellFormat_WithInvalidFormat_ShouldReturnTextFormat()
    {
        // Act
        var result = SheetHelpers.GetCellFormat((FormatEnum)999);

        // Assert
        Assert.Equal("TEXT", result.NumberFormat.Type);
    }

    #endregion
}