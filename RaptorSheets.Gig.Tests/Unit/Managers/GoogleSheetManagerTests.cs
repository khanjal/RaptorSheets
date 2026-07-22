using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Managers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Managers;

[Category("Unit Tests")]
public class GoogleSheetManagerTests
{
    private readonly GoogleSheetManager _manager;

    public GoogleSheetManagerTests()
    {
        _manager = new GoogleSheetManager("test-token", "test-spreadsheet-id");
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithAccessToken_ShouldInitialize()
    {
        // Act & Assert - If no exception is thrown, the constructor works
        var manager = new GoogleSheetManager("test-token", "test-spreadsheet-id");
        Assert.NotNull(manager);
    }

    #endregion

    #region CheckSheetHeaders Tests

    [Fact]
    public void CheckSheetHeaders_WithNullSpreadsheet_ShouldReturnErrorMessage()
    {
        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type
        var result = GoogleSheetManager.CheckSheetHeaders(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(MessageType.GENERAL.GetDescription(), result[0].Type);
        Assert.Contains("Unable to retrieve sheet(s)", result[0].Message);
    }

    [Fact]
    public void CheckSheetHeaders_WithValidSpreadsheet_ShouldCheckAllSheets()
    {
        // Arrange
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { Title = SheetName.SHIFTS.GetDescription() },
                    Data = new List<GridData>
                    {
                        new()
                        {
                            RowData = new List<RowData>
                            {
                                new()
                                {
                                    Values = new List<CellData>
                                    {
                                        new() { FormattedValue = "Date" },
                                        new() { FormattedValue = "Number" },
                                        new() { FormattedValue = "Service" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Should have messages about header validation
        Assert.Contains(result, m => m.Type == MessageType.CHECK_SHEET.GetDescription());
    }

    [Fact]
    public void CheckSheetHeaders_WithUnknownSheet_ShouldReturnWarningMessage()
    {
        // Arrange
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { Title = "UnknownSheet" },
                    Data = new List<GridData>
                    {
                        new()
                        {
                            RowData = new List<RowData>
                            {
                                new()
                                {
                                    Values = new List<CellData>
                                    {
                                        new() { FormattedValue = "Header1" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Should have warning about unknown sheet
        Assert.Contains(result, m => m.Message.Contains("UnknownSheet") && m.Message.Contains("does not match any known sheet name"));
    }

    [Theory]
    [InlineData("Addresses")]
    [InlineData("Daily")]
    [InlineData("Expenses")]
    [InlineData("Monthly")]
    [InlineData("Names")]
    [InlineData("Places")]
    [InlineData("Regions")]
    [InlineData("Services")]
    [InlineData("Setup")]
    [InlineData("Shifts")]
    [InlineData("Trips")]
    [InlineData("Types")]
    [InlineData("Weekdays")]
    [InlineData("Weekly")]
    [InlineData("Yearly")]
    public void CheckSheetHeaders_WithKnownSheets_ShouldValidateHeaders(string sheetName)
    {
        // Arrange
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { Title = sheetName },
                    Data = new List<GridData>
                    {
                        new()
                        {
                            RowData = new List<RowData>
                            {
                                new()
                                {
                                    Values = new List<CellData>
                                    {
                                        new() { FormattedValue = "Date" },
                                        new() { FormattedValue = "Service" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Should process the known sheet without "unknown sheet" warnings
        Assert.DoesNotContain(result, m => m.Message.Contains("does not match any known sheet name"));
    }

    [Fact]
    public void CheckSheetHeaders_WithEmptySheets_ShouldHandleGracefully()
    {
        // Arrange
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>()
        };

        // Act
        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Should have the "no header issues found" message
        Assert.Contains("No sheet header issues found", result[0].Message);
    }

    [Fact]
    public void CheckSheetHeaders_WithNullData_ShouldHandleGracefully()
    {
        // Arrange - Sheet with null data
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { Title = "TestSheet" },
                    Data = null
                }
            }
        };

        // Act
        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void CheckSheetHeaders_WithEmptyGridData_ShouldHandleGracefully()
    {
        // Arrange - Sheet with empty grid data
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { Title = "TestSheet" },
                    Data = new List<GridData>()
                }
            }
        };

        // Act - Should handle gracefully and return messages rather than throwing
        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Should include a warning about the unknown sheet name
        Assert.Contains(result, m => m.Message.Contains("TestSheet") && m.Message.Contains("does not match any known sheet name"));
    }

    [Fact]
    public void CheckSheetHeaders_WithNullRowData_ShouldHandleGracefully()
    {
        // Arrange - Sheet with null row data
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { Title = "TestSheet" },
                    Data = new List<GridData>
                    {
                        new()
                        {
                            RowData = null
                        }
                    }
                }
            }
        };

        // Act
        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void CheckSheetHeaders_WithEmptyRowData_ShouldHandleGracefully()
    {
        // Arrange - Sheet with empty row data
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { Title = "TestSheet" },
                    Data = new List<GridData>
                    {
                        new()
                        {
                            RowData = new List<RowData>()
                        }
                    }
                }
            }
        };

        // Act - Should handle gracefully and return messages rather than throwing
        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Should include a warning about the unknown sheet name
        Assert.Contains(result, m => m.Message.Contains("TestSheet") && m.Message.Contains("does not match any known sheet name"));
    }

    [Fact]
    public void CheckSheetHeaders_WithNullCellValues_ShouldHandleGracefully()
    {
        // Arrange - Sheet with null cell values
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { Title = "TestSheet" },
                    Data = new List<GridData>
                    {
                        new()
                        {
                            RowData = new List<RowData>
                            {
                                new()
                                {
                                    Values = null
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region CheckUnknownSheets Tests

    // CheckUnknownSheets is the lighter-weight replacement used on GetSheets' success path -
    // it only needs sheet tab metadata (Properties.Title), not grid/cell data, so it works
    // correctly even from a Spreadsheet fetched without IncludeGridData.

    [Fact]
    public void CheckUnknownSheets_WithNullSpreadsheet_ShouldReturnErrorMessage()
    {
        // Act
#pragma warning disable CS8625
        var result = GoogleSheetManager.CheckUnknownSheets(null);
#pragma warning restore CS8625

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("Unable to retrieve sheet(s)", result[0].Message);
    }

    [Fact]
    public void CheckUnknownSheets_WithOnlyKnownSheets_ShouldReturnNoWarnings()
    {
        // Arrange - no grid Data at all, mirroring a cheap GetSheetInfo() (no ranges) response
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = "Shifts" } },
                new() { Properties = new SheetProperties { Title = "Trips" } }
            }
        };

        // Act
        var result = GoogleSheetManager.CheckUnknownSheets(spreadsheet);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void CheckUnknownSheets_WithUnknownTab_ShouldReturnWarning()
    {
        // Arrange
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = "Shifts" } },
                new() { Properties = new SheetProperties { Title = "SomeRandomTab" } }
            }
        };

        // Act
        var result = GoogleSheetManager.CheckUnknownSheets(spreadsheet);

        // Assert
        Assert.Contains(result, m => m.Message.Contains("SomeRandomTab") && m.Message.Contains("does not match any known sheet name"));
    }

    [Fact]
    public void CheckUnknownSheets_WithNoSheets_ShouldReturnEmpty()
    {
        // Arrange
        var spreadsheet = new Spreadsheet { Sheets = new List<Sheet>() };

        // Act
        var result = GoogleSheetManager.CheckUnknownSheets(spreadsheet);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Interface Coverage Tests

    [Fact]
    public async Task CreateAllSheets_ShouldNotThrowImmediately()
    {
        // Act & Assert - Method should not throw exception immediately
        var result = await _manager.CreateAllSheets();
        
        // Result should not be null, but will have error messages due to invalid credentials
        Assert.NotNull(result);
        Assert.NotEmpty(result.Messages);
    }

    [Fact]
    public async Task GetAllSheets_ShouldNotThrowImmediately()
    {
        // Act & Assert - Method should not throw exception immediately
        var result = await _manager.GetAllSheets();
        
        // Result should not be null, but will have error messages due to invalid credentials
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("Trips")]
    [InlineData("Shifts")]
    [InlineData("Expenses")]
    public async Task GetSheet_WithValidSheetName_ShouldNotThrowImmediately(string sheetName)
    {
        // Act & Assert - Method should not throw exception immediately for valid sheet names
        var result = await _manager.GetSheet(sheetName);
        
        // Result should not be null for valid sheet names
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetSheet_WithInvalidSheetName_ShouldReturnErrorMessage()
    {
        // Act
        var result = await _manager.GetSheet("InvalidSheetName");
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Messages);
        Assert.Contains("does not exist", result.Messages[0].Message);
        Assert.Equal(MessageType.GET_SHEETS.GetDescription(), result.Messages[0].Type);
    }

    [Fact]
    public async Task GetAllSheetProperties_ShouldNotThrowImmediately()
    {
        // Act
        var result = await _manager.GetAllSheetProperties();
        
        // Assert - Result should not be null, even if empty due to invalid credentials
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetSpreadsheetInfo_ShouldNotThrowImmediately()
    {
        // Act
        var result = await _manager.GetSpreadsheetInfo();
        
        // Assert - Result may be null due to invalid credentials, but method shouldn't throw immediately
        // This is expected behavior for invalid credentials
        Assert.True(result == null); // This is the expected behavior with invalid credentials
    }

    [Fact]
    public async Task GetBatchData_ShouldNotThrowImmediately()
    {
        // Arrange
        var sheets = new List<string> { "TestSheet" };
        
        // Act
        var result = await _manager.GetBatchData(sheets);
        
        // Assert - Result may be null due to invalid credentials, but method shouldn't throw immediately
        Assert.True(result == null); // This is the expected behavior with invalid credentials
    }

    [Fact]
    public async Task GetSpreadsheetInfo_WithRanges_ShouldNotThrowImmediately()
    {
        // Arrange
        var ranges = new List<string> { "Sheet1!A1:Z1000", "Sheet2!A1:Z1000" };
        
        // Act
        var result = await _manager.GetSpreadsheetInfo(ranges);
        
        // Assert - Result may be null due to invalid credentials
        Assert.True(result == null);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task ChangeSheetData_WithEmptyChanges_ShouldReturnWarningMessage()
    {
        // Arrange
        var sheets = new List<string> { "Expenses" };
        var sheetEntity = new SheetEntity(); // No data = no changes
        
        // Act
        var result = await _manager.ChangeSheetData(sheets, sheetEntity);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Messages);
        
        // Should contain the "No data to change" warning message
        Assert.Contains(result.Messages, m => m.Message.Contains("No data to change"));
        Assert.Contains(result.Messages, m => m.Level == MessageLevel.WARNING.GetDescription());
    }

    [Fact]
    public async Task DeleteSheets_WithEmptyList_ShouldReturnWarningMessage()
    {
        // Arrange
        var emptySheets = new List<string>();
        
        // Act
        var result = await _manager.DeleteSheets(emptySheets);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Messages);
        Assert.Contains("No sheets found to delete", result.Messages[0].Message);
    }

    [Fact]
    public async Task CreateSheets_WithEmptyList_ShouldNotThrowImmediately()
    {
        // Arrange
        var emptySheets = new List<string>();
        
        // Act
        var result = await _manager.CreateSheets(emptySheets);
        
        // Assert - Should handle empty list gracefully
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetSheets_WithEmptyList_ShouldNotThrowImmediately()
    {
        // Arrange
        var emptySheets = new List<string>();
        
        // Act
        var result = await _manager.GetSheets(emptySheets);
        
        // Assert - Should handle empty list gracefully
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetSheetProperties_WithEmptyList_ShouldNotThrowImmediately()
    {
        // Arrange
        var emptySheets = new List<string>();
        
        // Act
        var result = await _manager.GetSheetProperties(emptySheets);
        
        // Assert - Should handle empty list gracefully
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ChangeSheetData_WithValidData_ShouldProcessRequest()
    {
        // Arrange
        var sheets = new List<string> { "Expenses" };
        var sheetEntity = new SheetEntity
        {
            Sheets = { Expenses = { new ExpenseEntity { Name = "Test", Amount = 100 } } }
        };
        
        // Act
        var result = await _manager.ChangeSheetData(sheets, sheetEntity);
        
        // Assert
        Assert.NotNull(result);
        // Will have errors due to invalid credentials, but should process the request structure
        Assert.NotEmpty(result.Messages);
    }

    [Fact] 
    public async Task DeleteSheets_WithValidSheetNames_ShouldAttemptDeletion()
    {
        // Arrange
        var sheets = new List<string> { "TestSheet1", "TestSheet2" };
        
        // Act
        var result = await _manager.DeleteSheets(sheets);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Messages);
        // Should contain messages about the deletion attempt
    }

    [Fact]
    public async Task GetSheetProperties_WithSpecificSheets_ShouldReturnProperties()
    {
        // Arrange
        var sheets = new List<string> { "TestSheet1", "TestSheet2" };
        
        // Act
        var result = await _manager.GetSheetProperties(sheets);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Should create properties for requested sheets
        Assert.All(result, prop => 
        {
            Assert.NotNull(prop.Name);
            Assert.Contains(prop.Name, sheets);
        });
    }

    [Fact]
    public async Task CreateSheets_WithSpecificSheets_ShouldAttemptCreation()
    {
        // Arrange - Use valid sheet names
        var sheets = new List<string> { "Expenses", "Shifts" };
        
        // Act
        var result = await _manager.CreateSheets(sheets);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Messages);
        // Should have messages about creation attempts (will fail due to invalid credentials)
    }

    [Fact]
    public async Task GetSheets_WithSpecificSheets_ShouldAttemptRetrieval()
    {
        // Arrange
        var sheets = new List<string> { "Sheet1", "Sheet2" };
        
        // Act
        var result = await _manager.GetSheets(sheets);
        
        // Assert
        Assert.NotNull(result);
        // Will have messages due to credential issues, but should attempt the operation
    }

    #endregion

}