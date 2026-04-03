using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Mappers;

namespace RaptorSheets.Gig.Tests.Integration.Managers;

/// <summary>
/// Tests for automatic missing column insertion functionality.
/// Addresses Issue #17: Automated Insertion of Missing Headers in Existing Sheets
/// </summary>
public class GoogleSheetManager_MissingColumns_Tests
{
    [Fact]
    public void CheckSheetHeaders_DetectsMissingColumns()
    {
        // Arrange
        var spreadsheet = TestHelpers.CreateMockSpreadsheet(
            sheetName: "Trips",
            sheetId: 123,
            headers: new[] { "Date", "Service", "Pay" } // Missing many columns
        );

        // Act
        var messages = GoogleSheetManager.CheckSheetHeaders(
            spreadsheet, 
            out var missingColumns);

        // Assert
        Assert.NotEmpty(missingColumns);
        Assert.True(missingColumns.ContainsKey("Trips"));
        
        var tripsMissingColumns = missingColumns["Trips"];
        Assert.NotEmpty(tripsMissingColumns);
        
        // Verify SheetId is set
        Assert.All(tripsMissingColumns, c => Assert.Equal(123, c.SheetId));
        
        // Verify messages indicate missing columns
        Assert.Contains(messages, m => m.Message.Contains("Missing column"));
    }

    [Fact]
    public void CheckSheetHeaders_NoMissingColumns_ReturnsEmptyDictionary()
    {
        // Arrange
        var spreadsheet = TestHelpers.CreateMockSpreadsheetWithAllHeaders("Trips", 123);

        // Act
        var messages = GoogleSheetManager.CheckSheetHeaders(
            spreadsheet, 
            out var missingColumns);

        // Assert
        Assert.Empty(missingColumns);
        Assert.Contains(messages, m => m.Message.Contains("No sheet header issues found"));
    }

    [Fact]
    public void CheckSheetHeaders_OverloadWithoutOutParam_StillWorks()
    {
        // Arrange
        var spreadsheet = TestHelpers.CreateMockSpreadsheet(
            sheetName: "Trips",
            sheetId: 123,
            headers: new[] { "Date", "Service" }
        );

        // Act
        var messages = GoogleSheetManager.CheckSheetHeaders(spreadsheet);

        // Assert
        Assert.NotEmpty(messages);
        Assert.Contains(messages, m => m.Message.Contains("Missing column"));
    }

    [Fact]
    public void GenerateInsertColumnDimension_CreatesValidRequest()
    {
        // Arrange
        int sheetId = 456;
        int startIndex = 5;
        int endIndex = 6;

        // Act
        var request = GoogleRequestHelpers.GenerateInsertColumnDimension(sheetId, startIndex, endIndex);

        // Assert
        Assert.NotNull(request);
        Assert.NotNull(request.InsertDimension);
        Assert.Equal("COLUMNS", request.InsertDimension.Range.Dimension);
        Assert.Equal(sheetId, request.InsertDimension.Range.SheetId);
        Assert.Equal(startIndex, request.InsertDimension.Range.StartIndex);
        Assert.Equal(endIndex, request.InsertDimension.Range.EndIndex);
        Assert.True(request.InsertDimension.InheritFromBefore);
    }

    [Fact]
    public void ColumnInsertionInfo_CorrectlyStoresData()
    {
        // Arrange & Act
        var info = new ColumnInsertionInfo
        {
            SheetName = "TestSheet",
            SheetId = 789,
            ColumnIndex = 2,
            ColumnName = "TestColumn",
            ColumnLetter = SheetHelpers.GetColumnName(2)
        };

        // Assert
        Assert.Equal("TestSheet", info.SheetName);
        Assert.Equal(789, info.SheetId);
        Assert.Equal(2, info.ColumnIndex);
        Assert.Equal("TestColumn", info.ColumnName);
        Assert.Equal("C", info.ColumnLetter); // Column index 2 = letter "C"
    }
}

/// <summary>
/// Test helpers for creating mock spreadsheet data
/// </summary>
internal static class TestHelpers
{
    public static Google.Apis.Sheets.v4.Data.Spreadsheet CreateMockSpreadsheet(
        string sheetName, 
        int sheetId, 
        string[] headers)
    {
        var cellValues = headers.Select(h => new Google.Apis.Sheets.v4.Data.CellData
        {
            FormattedValue = h
        }).ToList();

        return new Google.Apis.Sheets.v4.Data.Spreadsheet
        {
            Sheets = new List<Google.Apis.Sheets.v4.Data.Sheet>
            {
                new()
                {
                    Properties = new Google.Apis.Sheets.v4.Data.SheetProperties
                    {
                        Title = sheetName,
                        SheetId = sheetId
                    },
                    Data = new List<Google.Apis.Sheets.v4.Data.GridData>
                    {
                        new()
                        {
                            RowData = new List<Google.Apis.Sheets.v4.Data.RowData>
                            {
                                new()
                                {
                                    Values = cellValues
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    public static Google.Apis.Sheets.v4.Data.Spreadsheet CreateMockSpreadsheetWithAllHeaders(
        string sheetName, 
        int sheetId)
    {
        // Get actual headers dynamically from the mapper to ensure test stays synchronized
        var sheetModel = TripMapper.GetSheet();
        var headers = sheetModel.Headers.Select(h => h.Name).ToArray();

        return CreateMockSpreadsheet(sheetName, sheetId, headers);
    }
}
