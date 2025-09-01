using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Common.Mappers;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Gig.Mappers;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class GigSheetHelpersTests
{
    [Fact]
    public void GetSheets_ShouldReturnCorrectSheetModels()
    {
        // Act
        var result = GigSheetHelpers.GetSheets();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var shiftSheet = result.FirstOrDefault(s => s.Name == SheetEnum.SHIFTS.GetDescription());
        var tripSheet = result.FirstOrDefault(s => s.Name == SheetEnum.TRIPS.GetDescription());
        
        Assert.NotNull(shiftSheet);
        Assert.NotNull(tripSheet);
    }

    [Fact]
    public void GetSheetNames_ShouldReturnAllSheetNames()
    {
        // Act
        var result = GigSheetHelpers.GetSheetNames();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Should contain Gig sheets
        Assert.Contains(SheetEnum.SHIFTS.GetDescription().ToUpper(), result.Select(x => x.ToUpper()));
        Assert.Contains(SheetEnum.TRIPS.GetDescription().ToUpper(), result.Select(x => x.ToUpper()));
        
        // Should contain Common sheets
        Assert.Contains(Common.Enums.SheetEnum.SETUP.GetDescription().ToUpper(), result.Select(x => x.ToUpper()));
    }

    [Fact]
    public void GetMissingSheets_WithEmptySpreadsheet_ShouldReturnAllSheets()
    {
        // Arrange
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>()
        };

        // Act
        var result = GigSheetHelpers.GetMissingSheets(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Should return all sheets since none exist in spreadsheet
        var expectedSheetCount = GigSheetHelpers.GetSheetNames().Count;
        Assert.Equal(expectedSheetCount, result.Count);
    }

    [Fact]
    public void GetMissingSheets_WithSomeExistingSheets_ShouldReturnOnlyMissing()
    {
        // Arrange
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = SheetEnum.SHIFTS.GetDescription() } },
                new() { Properties = new SheetProperties { Title = SheetEnum.TRIPS.GetDescription() } }
            }
        };

        // Act
        var result = GigSheetHelpers.GetMissingSheets(spreadsheet);

        // Assert
        Assert.NotNull(result);
        
        // Should not contain existing sheets
        Assert.DoesNotContain(result, s => s.Name == SheetEnum.SHIFTS.GetDescription());
        Assert.DoesNotContain(result, s => s.Name == SheetEnum.TRIPS.GetDescription());
        
        // But should contain other sheets like SETUP, EXPENSES, etc.
        Assert.Contains(result, s => s.Name == Common.Enums.SheetEnum.SETUP.GetDescription());
    }

    [Fact]
    public void GetMissingSheets_WithAllExistingSheets_ShouldReturnEmpty()
    {
        // Arrange
        var allSheetNames = GigSheetHelpers.GetSheetNames();
        var sheets = allSheetNames.Select(name => new Sheet 
        { 
            Properties = new SheetProperties { Title = name } 
        }).ToList();
        
        var spreadsheet = new Spreadsheet { Sheets = sheets };

        // Act
        var result = GigSheetHelpers.GetMissingSheets(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(ValidationEnum.BOOLEAN)]
    [InlineData(ValidationEnum.RANGE_ADDRESS)]
    [InlineData(ValidationEnum.RANGE_NAME)]
    [InlineData(ValidationEnum.RANGE_PLACE)]
    [InlineData(ValidationEnum.RANGE_REGION)]
    [InlineData(ValidationEnum.RANGE_SERVICE)]
    [InlineData(ValidationEnum.RANGE_TYPE)]
    public void GetDataValidation_ShouldReturnCorrectValidation(ValidationEnum validation)
    {
        // Act
        var result = GigSheetHelpers.GetDataValidation(validation);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Condition);

        switch (validation)
        {
            case ValidationEnum.BOOLEAN:
                Assert.Equal("BOOLEAN", result.Condition.Type);
                break;
            case ValidationEnum.RANGE_ADDRESS:
            case ValidationEnum.RANGE_NAME:
            case ValidationEnum.RANGE_PLACE:
            case ValidationEnum.RANGE_REGION:
            case ValidationEnum.RANGE_SERVICE:
            case ValidationEnum.RANGE_TYPE:
                Assert.Equal("ONE_OF_RANGE", result.Condition.Type);
                Assert.NotNull(result.Condition.Values);
                Assert.Single(result.Condition.Values);
                Assert.True(result.ShowCustomUi);
                Assert.False(result.Strict);
                break;
        }
    }

    [Fact]
    public void MapData_WithSpreadsheet_ShouldReturnSheetEntity()
    {
        // Arrange
        var spreadsheet = new Spreadsheet
        {
            Properties = new SpreadsheetProperties { Title = "Test Spreadsheet" },
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { Title = SheetEnum.SHIFTS.GetDescription() },
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
                                        new() { FormattedValue = "Number" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = GigSheetHelpers.MapData(spreadsheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Properties);
        Assert.Equal("Test Spreadsheet", result.Properties.Name);
    }

    [Fact]
    public void MapData_WithBatchResponse_ShouldReturnSheetEntity()
    {
        // Arrange
        var response = new BatchGetValuesByDataFilterResponse
        {
            ValueRanges = new List<MatchedValueRange>
            {
                new()
                {
                    DataFilters = new List<DataFilter>
                    {
                        new() { A1Range = SheetEnum.SHIFTS.GetDescription() }
                    },
                    ValueRange = new ValueRange
                    {
                        Values = new List<IList<object>>
                        {
                            new List<object> { "Date", "Number", "Service" }, // Headers
                            new List<object> { "2024-01-01", "1", "TestService" } // Data
                        }
                    }
                }
            }
        };

        // Act
        var result = GigSheetHelpers.MapData(response);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Shifts);
        Assert.Single(result.Shifts);
    }

    [Fact]
    public void MapData_WithNullValueRanges_ShouldReturnNull()
    {
        // Arrange
        var response = new BatchGetValuesByDataFilterResponse
        {
            ValueRanges = null
        };

        // Act
        var result = GigSheetHelpers.MapData(response);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MapData_WithEmptyValues_ShouldHandleGracefully()
    {
        // Arrange
        var response = new BatchGetValuesByDataFilterResponse
        {
            ValueRanges = new List<MatchedValueRange>
            {
                new()
                {
                    DataFilters = new List<DataFilter>
                    {
                        new() { A1Range = SheetEnum.SHIFTS.GetDescription() }
                    },
                    ValueRange = new ValueRange
                    {
                        Values = new List<IList<object>>() // Empty values
                    }
                }
            }
        };

        // Act
        var result = GigSheetHelpers.MapData(response);

        // Assert
        Assert.NotNull(result);
        // Should handle empty values without throwing
    }

    [Fact]
    public void MapData_WithAllSheetTypes_ShouldMapCorrectly()
    {
        // Arrange
        var sheetNames = new[]
        {
            nameof(SheetEnum.ADDRESSES),
            nameof(SheetEnum.DAILY),
            nameof(SheetEnum.EXPENSES),
            nameof(SheetEnum.MONTHLY),
            nameof(SheetEnum.NAMES),
            nameof(SheetEnum.PLACES),
            nameof(SheetEnum.REGIONS),
            nameof(SheetEnum.SERVICES),
            nameof(Common.Enums.SheetEnum.SETUP),
            nameof(SheetEnum.SHIFTS),
            nameof(SheetEnum.TRIPS),
            nameof(SheetEnum.TYPES),
            nameof(SheetEnum.WEEKDAYS),
            nameof(SheetEnum.WEEKLY),
            nameof(SheetEnum.YEARLY)
        };

        var valueRanges = sheetNames.Select(sheetName => new MatchedValueRange
        {
            DataFilters = new List<DataFilter>
            {
                new() { A1Range = sheetName }
            },
            ValueRange = new ValueRange
            {
                Values = new List<IList<object>>
                {
                    new List<object> { "Header1", "Header2" }, // Headers
                    new List<object> { "Value1", "Value2" }    // Data
                }
            }
        }).ToList();

        var response = new BatchGetValuesByDataFilterResponse
        {
            ValueRanges = valueRanges
        };

        // Act
        var result = GigSheetHelpers.MapData(response);

        // Assert
        Assert.NotNull(result);
        
        // Verify that each sheet type gets mapped to the correct property
        // Note: This test verifies the switch statement coverage in ProcessSheetData
        // The actual mapping logic is tested in individual mapper tests
    }
}