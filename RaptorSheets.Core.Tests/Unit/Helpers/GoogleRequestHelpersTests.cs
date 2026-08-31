using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class GoogleRequestHelpersTests
{
    [Fact]
    public void GenerateAppendCells_ShouldReturnValidRequest()
    {
        // Arrange
        var sheet = new SheetModel { Id = 1, Headers = [new SheetCellModel { Name = "Header1" }] };

        // Act
        var result = GoogleRequestHelpers.GenerateAppendCells(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AppendCells);
        Assert.Equal(Field.USER_ENTERED_VALUE_AND_FORMAT.GetDescription(), result.AppendCells.Fields);
        Assert.Equal(sheet.Id, result.AppendCells.SheetId);
    }

    [Fact]
    public void GenerateAppendDimension_ShouldReturnValidRequests()
    {
        // Arrange
        var random = new Random();
        var randomNumber = random.Next(1, 10);
        var defaultColumns = GoogleConfig.DefaultColumnCount;
        var totalColumns = randomNumber + defaultColumns;
        var headers = new List<SheetCellModel>();

        for (var i = 0; i < totalColumns; i++)
        {
            var header = new SheetCellModel { Name = $"Header{i}" };
            headers.Add(header);
        }
        var sheet = new SheetModel { Id = 1, Headers = headers };

        // Act
        var result = GoogleRequestHelpers.GenerateAppendDimension(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sheet.Id, result.AppendDimension.SheetId);
        Assert.Equal(randomNumber, result.AppendDimension.Length);
    }

    [Fact]
    public void GenerateAppendDimension_WithDefaultColumns_ShouldReturnNull()
    {
        // Arrange
        var headers = new List<SheetCellModel>();
        for (var i = 0; i < GoogleConfig.DefaultColumnCount; i++)
        {
            headers.Add(new SheetCellModel { Name = $"Header{i}" });
        }
        var sheet = new SheetModel { Id = 1, Headers = headers };

        // Act
        var result = GoogleRequestHelpers.GenerateAppendDimension(sheet);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GenerateBandingRequest_ShouldReturnValidRequest()
    {
        // Arrange
        var sheet = new SheetModel { Id = 1, TabColor = SheetColor.BLUE, CellColor = SheetColor.GREEN };

        // Act
        var result = GoogleRequestHelpers.GenerateBandingRequest(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AddBanding);
        Assert.Equal(sheet.Id, result.AddBanding.BandedRange.BandedRangeId);
    }

    [Theory]
    [InlineData(new int[] { 1 })]
    [InlineData(new int[] { 2 })]
    [InlineData(new int[] { 1, 2, 3 })]
    [InlineData(new int[] { 5, 10, 15 })]
    [InlineData(new int[] { 1, 3, 5, 7, 9 })]
    [InlineData(new int[] { 10, 11, 12, 20, 21, 25 })]
    public void GenerateDeleteRequest_ShouldReturnValidRequest(int[] rowIds)
    {
        // Arrange
        int sheetId = 1;
        var rowList = rowIds.ToList();

        // Act - Test the range-based method
        var indexRanges = GoogleRequestHelpers.GenerateIndexRanges(rowList);
        var requests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, indexRanges);

        // Assert - General validations based on input
        Assert.NotNull(requests);
        Assert.True(requests.Count > 0, "Should generate at least one request");
        
        // Verify all requests have correct sheet ID and are valid delete requests
        foreach (var request in requests)
        {
            Assert.NotNull(request.DeleteDimension);
            Assert.Equal(sheetId, request.DeleteDimension.Range.SheetId);
            Assert.Equal(Dimension.ROWS.GetDescription(), request.DeleteDimension.Range.Dimension);
            
            // Verify that start index is less than end index
            Assert.True(request.DeleteDimension.Range.StartIndex < request.DeleteDimension.Range.EndIndex,
                "StartIndex should be less than EndIndex");
            
            // Verify that the range is within reasonable bounds (0-based indexing)
            Assert.True(request.DeleteDimension.Range.StartIndex >= 0, "StartIndex should be non-negative");
        }

        // Verify that all original row IDs are covered by the generated ranges
        var coveredRowIds = new List<int>();
        foreach (var request in requests)
        {
            for (int i = request.DeleteDimension.Range.StartIndex!.Value; 
                 i < request.DeleteDimension.Range.EndIndex!.Value; i++)
            {
                coveredRowIds.Add(i + 1); // Convert back to 1-based row ID
            }
        }
        
        // All original row IDs should be covered
        foreach (var originalRowId in rowIds)
        {
            Assert.Contains(originalRowId, coveredRowIds);
        }
        
        // No extra row IDs should be covered
        Assert.Equal(rowIds.Length, coveredRowIds.Count);

        // Verify requests are in descending order (for safe deletion)
        for (int i = 0; i < requests.Count - 1; i++)
        {
            Assert.True(requests[i].DeleteDimension.Range.StartIndex >= requests[i + 1].DeleteDimension.Range.EndIndex,
                "Delete requests should be ordered from highest to lowest row numbers to prevent index shifting issues");
        }
    }

    [Fact]
    public void GenerateDeleteRequest_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        int sheetId = 1;
        var rowList = new List<int>();

        // Act - Test both methods
        var individualRequests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, rowList);
        var indexRanges = GoogleRequestHelpers.GenerateIndexRanges(rowList);
        var rangeRequests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, indexRanges);

        // Assert
        Assert.NotNull(individualRequests);
        Assert.Empty(individualRequests);
        Assert.NotNull(rangeRequests);
        Assert.Empty(rangeRequests);
    }

    [Fact]
    public void GenerateDeleteRequests_IndividualVsRange_ShouldShowEfficiencyDifference()
    {
        // Arrange - Test with consecutive rows
        int sheetId = 1;
        var consecutiveRowIds = new List<int> { 5, 6, 7, 8, 9 }; // 5 consecutive rows

        // Act - Compare both approaches
        var individualRequests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, consecutiveRowIds);
        var indexRanges = GoogleRequestHelpers.GenerateIndexRanges(consecutiveRowIds);
        var rangeRequests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, indexRanges);

        // Assert - Range-based approach should be more efficient
        Assert.Equal(5, individualRequests.Count); // Inefficient: 5 individual requests
        Assert.Single(rangeRequests);      // Efficient: 1 range request

        // Verify the range request covers all rows correctly
        Assert.Equal(4, rangeRequests[0].DeleteDimension.Range.StartIndex);  // Row 5 -> index 4
        Assert.Equal(9, rangeRequests[0].DeleteDimension.Range.EndIndex);    // Row 9 -> end index 9 (exclusive)
    }

    [Fact]
    public void GenerateDeleteRequests_MixedConsecutiveRanges_ShouldOptimizeCorrectly()
    {
        // Arrange - Test with mixed consecutive and non-consecutive rows
        int sheetId = 1;
        var mixedRowIds = new List<int> { 1, 2, 3, 10, 15, 16, 17 }; // Two ranges: 1-3 and 15-17, plus isolated 10

        // Act
        var indexRanges = GoogleRequestHelpers.GenerateIndexRanges(mixedRowIds);
        var rangeRequests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, indexRanges);

        // Assert - Should optimize to 3 requests: [15-17], [10], [1-3] (in descending order)
        Assert.Equal(3, rangeRequests.Count);

        // Verify ranges are processed in descending order
        // Range 1: 15-17
        Assert.Equal(14, rangeRequests[0].DeleteDimension.Range.StartIndex); // Row 15 -> index 14
        Assert.Equal(17, rangeRequests[0].DeleteDimension.Range.EndIndex);   // Row 17 -> end index 17

        // Range 2: 10 (isolated)
        Assert.Equal(9, rangeRequests[1].DeleteDimension.Range.StartIndex);  // Row 10 -> index 9
        Assert.Equal(10, rangeRequests[1].DeleteDimension.Range.EndIndex);   // Row 10 -> end index 10

        // Range 3: 1-3  
        Assert.Equal(0, rangeRequests[2].DeleteDimension.Range.StartIndex);  // Row 1 -> index 0
        Assert.Equal(3, rangeRequests[2].DeleteDimension.Range.EndIndex);    // Row 3 -> end index 3
    }

    [Fact]
    public void GenerateProtectedRangeForHeaderOrSheet_ShouldReturnValidRequest()
    {
        // Arrange
        var sheet = new SheetModel { Id = 1, ProtectSheet = true };

        // Act
        var result = GoogleRequestHelpers.GenerateProtectedRangeForHeaderOrSheet(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AddProtectedRange);
        Assert.Equal(sheet.Id, result.AddProtectedRange.ProtectedRange.Range.SheetId);
    }

    [Fact]
    public void GenerateProtectedRangeForHeaderOrSheet_WithoutProtection_ShouldReturnHeaderProtection()
    {
        // Arrange
        var sheet = new SheetModel { Id = 1, ProtectSheet = false };

        // Act
        var result = GoogleRequestHelpers.GenerateProtectedRangeForHeaderOrSheet(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AddProtectedRange);
        Assert.Equal(sheet.Id, result.AddProtectedRange.ProtectedRange.Range.SheetId);
        Assert.Equal(0, result.AddProtectedRange.ProtectedRange.Range.StartRowIndex);
        Assert.Equal(1, result.AddProtectedRange.ProtectedRange.Range.EndRowIndex);
    }

    [Fact]
    public void GenerateColumnProtection_ShouldReturnValidRequest()
    {
        // Arrange
        var range = new GridRange { SheetId = 1, StartColumnIndex = 0, EndColumnIndex = 1 };

        // Act
        var result = GoogleRequestHelpers.GenerateColumnProtection(range);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AddProtectedRange);
        Assert.Equal(range.SheetId, result.AddProtectedRange.ProtectedRange.Range.SheetId);
    }

    [Fact]
    public void GenerateRepeatCellRequest_ShouldReturnValidRequest()
    {
        // Arrange
        var repeatCellModel = new RepeatCellModel
        {
            GridRange = new GridRange { SheetId = 1 },
            CellFormat = new CellFormat(),
            DataValidation = new DataValidationRule()
        };

        // Act
        var result = GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Field.USER_ENTERED_VALUE_AND_FORMAT.GetDescription(), result.Fields);
        Assert.Equal(repeatCellModel.GridRange, result.Range);
        Assert.NotNull(result.Cell.UserEnteredFormat);
        Assert.NotNull(result.Cell.DataValidation);
    }

    [Fact]
    public void GenerateRepeatCellRequest_WithNullValidation_ShouldHandleGracefully()
    {
        // Arrange
        var repeatCellModel = new RepeatCellModel
        {
            GridRange = new GridRange { SheetId = 1 },
            CellFormat = new CellFormat(),
            DataValidation = null
        };

        // Act
        var result = GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Cell.UserEnteredFormat);
        Assert.Null(result.Cell.DataValidation);
    }

    [Fact]
    public void GenerateColumnFormatRequest_WithFormat_ShouldReturnRepeatCellRequestForThatColumn()
    {
        // Act
        var result = GoogleRequestHelpers.GenerateColumnFormatRequest(sheetId: 7, columnIndex: 3, format: Format.ACCOUNTING, formatPattern: null);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.RepeatCell);
        Assert.Equal(7, result.RepeatCell.Range.SheetId);
        Assert.Equal(3, result.RepeatCell.Range.StartColumnIndex);
        Assert.Equal(4, result.RepeatCell.Range.EndColumnIndex);
        Assert.NotNull(result.RepeatCell.Cell.UserEnteredFormat);
    }

    [Fact]
    public void GenerateColumnFormatRequest_ShouldScopeFieldsMaskToNumberFormatOnly()
    {
        // Regression guard: this request must never use a field mask (like "*" or
        // "userEnteredFormat") that could clear existing values, notes, validation, or other
        // userEnteredFormat sub-fields on a live, already-populated column - see ReapplyFormatting.
        var result = GoogleRequestHelpers.GenerateColumnFormatRequest(sheetId: 1, columnIndex: 0, format: Format.ACCOUNTING, formatPattern: null);

        Assert.Equal(Field.NUMBER_FORMAT.GetDescription(), result!.RepeatCell.Fields);
        Assert.Null(result.RepeatCell.Cell.UserEnteredValue);
    }

    [Fact]
    public void GenerateColumnFormatRequest_WithFormatPatternOnly_ShouldReturnRequest()
    {
        // Act
        var result = GoogleRequestHelpers.GenerateColumnFormatRequest(sheetId: 1, columnIndex: 0, format: null, formatPattern: "0.00%");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.RepeatCell?.Cell.UserEnteredFormat);
    }

    [Fact]
    public void GenerateColumnFormatRequest_WithNeitherFormatNorPattern_ShouldReturnNull()
    {
        // Act
        var result = GoogleRequestHelpers.GenerateColumnFormatRequest(sheetId: 1, columnIndex: 0, format: null, formatPattern: null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GenerateColumnValidationRequest_WithValidation_ShouldReturnRepeatCellRequestForThatColumn()
    {
        // Act
        var result = GoogleRequestHelpers.GenerateColumnValidationRequest(sheetId: 7, columnIndex: 3, validation: new DataValidationRule());

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.RepeatCell);
        Assert.Equal(7, result.RepeatCell.Range.SheetId);
        Assert.Equal(3, result.RepeatCell.Range.StartColumnIndex);
        Assert.Equal(4, result.RepeatCell.Range.EndColumnIndex);
        Assert.Equal(1, result.RepeatCell.Range.StartRowIndex);
        Assert.NotNull(result.RepeatCell.Cell.DataValidation);
    }

    [Fact]
    public void GenerateColumnValidationRequest_ShouldScopeFieldsMaskToDataValidationOnly()
    {
        // Regression guard: mirrors GenerateColumnFormatRequest's own guard - this request must
        // never use a broad field mask that could clear existing values, format, or notes on a
        // live, already-populated column.
        var result = GoogleRequestHelpers.GenerateColumnValidationRequest(sheetId: 1, columnIndex: 0, validation: new DataValidationRule());

        Assert.Equal(Field.DATA_VALIDATION.GetDescription(), result!.RepeatCell.Fields);
        Assert.Null(result.RepeatCell.Cell.UserEnteredValue);
        Assert.Null(result.RepeatCell.Cell.UserEnteredFormat);
    }

    [Fact]
    public void GenerateColumnValidationRequest_WithNullValidation_ShouldReturnNull()
    {
        // Act
        var result = GoogleRequestHelpers.GenerateColumnValidationRequest(sheetId: 1, columnIndex: 0, validation: null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GenerateNamedRangeRequest_ShouldCoverOnlyTheColumnsDataExcludingTheHeaderRow()
    {
        // Arrange
        var sheet = new SheetModel { Id = 5, Name = "Shifts" };
        var header = new SheetCellModel { Name = "Total Pay", Index = 3 };

        // Act
        var result = GoogleRequestHelpers.GenerateNamedRangeRequest(sheet, header);

        // Assert
        Assert.NotNull(result.AddNamedRange);
        var range = result.AddNamedRange.NamedRange.Range;
        Assert.Equal(5, range.SheetId);
        Assert.Equal(3, range.StartColumnIndex);
        Assert.Equal(4, range.EndColumnIndex);
        Assert.Equal(1, range.StartRowIndex); // row 0 (the header) is excluded
    }

    [Theory]
    [InlineData("Shifts", "Total Pay", "Shifts_Total_Pay")]
    [InlineData("Deliveries", "Amt/Trip", "Deliveries_Amt_Trip")]
    [InlineData("Stocks", "P/E Ratio", "Stocks_P_E_Ratio")]
    public void GenerateNamedRangeRequest_ShouldSanitizeInvalidIdentifierCharacters(string sheetName, string headerName, string expectedName)
    {
        // Arrange
        var sheet = new SheetModel { Id = 1, Name = sheetName };
        var header = new SheetCellModel { Name = headerName, Index = 0 };

        // Act
        var result = GoogleRequestHelpers.GenerateNamedRangeRequest(sheet, header);

        // Assert
        Assert.Equal(expectedName, result.AddNamedRange.NamedRange.Name);
    }

    [Fact]
    public void GenerateNamedRangeRequest_WithHeaderStartingWithDigit_ShouldPrefixUnderscore()
    {
        // Google rejects named ranges that look like a cell reference (e.g. "2024_Total") -
        // guard against a header/sheet name pair that would sanitize down to start with a digit
        var sheet = new SheetModel { Id = 1, Name = "2024" };
        var header = new SheetCellModel { Name = "Total", Index = 0 };

        var result = GoogleRequestHelpers.GenerateNamedRangeRequest(sheet, header);

        Assert.Equal("_2024_Total", result.AddNamedRange.NamedRange.Name);
    }

    [Fact]
    public void GenerateBasicFilterRequest_ShouldCoverHeaderRowThroughDeclaredColumns()
    {
        // Arrange
        var sheet = new SheetModel
        {
            Id = 5,
            Headers = [new SheetCellModel { Name = "Date" }, new SheetCellModel { Name = "Amount" }]
        };

        // Act
        var result = GoogleRequestHelpers.GenerateBasicFilterRequest(sheet);

        // Assert
        Assert.NotNull(result.SetBasicFilter);
        var range = result.SetBasicFilter.Filter.Range;
        Assert.Equal(5, range.SheetId);
        Assert.Equal(0, range.StartRowIndex); // includes the header row (filter dropdowns live there)
        Assert.Equal(0, range.StartColumnIndex);
        Assert.Equal(2, range.EndColumnIndex);
        Assert.Null(range.EndRowIndex); // open-ended: keeps applying as data rows are added
    }

    [Fact]
    public void GenerateConditionalFormatRequest_WithRule_ShouldCoverOnlyTheColumnsDataExcludingTheHeaderRow()
    {
        // Arrange
        var rule = new BooleanRule
        {
            Condition = new BooleanCondition { Type = "NUMBER_LESS", Values = [new ConditionValue { UserEnteredValue = "0" }] },
            Format = new CellFormat { BackgroundColor = new Color { Red = 1 } }
        };

        // Act
        var result = GoogleRequestHelpers.GenerateConditionalFormatRequest(sheetId: 5, columnIndex: 3, rule: rule, ruleIndex: 2);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.AddConditionalFormatRule);
        Assert.Equal(2, result.AddConditionalFormatRule.Index);
        Assert.Same(rule, result.AddConditionalFormatRule.Rule.BooleanRule);
        var range = Assert.Single(result.AddConditionalFormatRule.Rule.Ranges);
        Assert.Equal(5, range.SheetId);
        Assert.Equal(3, range.StartColumnIndex);
        Assert.Equal(4, range.EndColumnIndex);
        Assert.Equal(1, range.StartRowIndex); // row 0 (the header) is excluded
    }

    [Fact]
    public void GenerateConditionalFormatRequest_WithNullRule_ShouldReturnNull()
    {
        // Act
        var result = GoogleRequestHelpers.GenerateConditionalFormatRequest(sheetId: 1, columnIndex: 0, rule: null, ruleIndex: 0);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GenerateSheetPropertes_ShouldReturnValidRequest()
    {
        // Arrange
        var sheet = new SheetModel 
        { 
            Id = 1, 
            Name = "TestSheet", 
            TabColor = SheetColor.BLUE, 
            FreezeColumnCount = 1, 
            FreezeRowCount = 1 
        };

        // Act
        var result = GoogleRequestHelpers.GenerateSheetPropertes(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AddSheet);
        Assert.Equal(sheet.Id, result.AddSheet.Properties.SheetId);
        Assert.Equal(sheet.Name, result.AddSheet.Properties.Title);
    }

    [Fact]
    public void GenerateUpdateRequest_ShouldReturnValidRequest()
    {
        // Arrange
        var sheetName = "TestSheet";
        var rowValues = new Dictionary<int, IList<IList<object?>>>
        {
            { 1, new List<IList<object?>> { new List<object?> { "Value1" } } }
        };

        // Act
        var result = GoogleRequestHelpers.GenerateUpdateValueRequest(sheetName, rowValues);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        Assert.Equal(sheetName + "!A1", result.Data[0].Range);
    }

    [Fact]
    public void GenerateUpdateRequest_WithMultipleRows_ShouldReturnValidRequest()
    {
        // Arrange
        var sheetName = "TestSheet";
        var rowValues = new Dictionary<int, IList<IList<object?>>>
        {
            { 1, new List<IList<object?>> { new List<object?> { "Value1" } } },
            { 2, new List<IList<object?>> { new List<object?> { "Value2" } } },
            { 5, new List<IList<object?>> { new List<object?> { "Value5" } } }
        };

        // Act
        var result = GoogleRequestHelpers.GenerateUpdateValueRequest(sheetName, rowValues);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Data.Count);
        Assert.Equal(sheetName + "!A1", result.Data[0].Range);
        Assert.Equal(sheetName + "!A2", result.Data[1].Range);
        Assert.Equal(sheetName + "!A5", result.Data[2].Range);
    }

    [Fact]
    public void GenerateUpdateRequest_WithEmptyValues_ShouldReturnEmptyRequest()
    {
        // Arrange
        var sheetName = "TestSheet";
        var rowValues = new Dictionary<int, IList<IList<object?>>>();

        // Act
        var result = GoogleRequestHelpers.GenerateUpdateValueRequest(sheetName, rowValues);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Data);
    }

    [Theory]
    [InlineData(1, 2, 1)]
    [InlineData(1, 5, 1)]
    [InlineData(10, 15, 1)]
    [InlineData(1, 10, 1)]
    public void GenerateIndexRanges_WithConsecutiveNumbers_ShouldReturnSingleRange(int start, int end, int expectedRanges)
    {
        // Arrange
        var rowIds = Enumerable.Range(start, end - start + 1).ToList();

        // Act
        var result = GoogleRequestHelpers.GenerateIndexRanges(rowIds);

        // Assert
        Assert.Equal(expectedRanges, result.Count);
        // Note: With the new descending order logic, consecutive ranges are still grouped
        // but the range covers from the lowest to highest in the sequence
        Assert.Equal(start - 1, result[0].Item1); // Start of range (0-based)
        Assert.Equal(end, result[0].Item2); // End of range (exclusive)
    }

    [Fact]
    public void GenerateIndexRanges_WithNonConsecutiveNumbers_ShouldReturnMultipleRanges()
    {
        // Arrange
        var rowIds = new List<int> { 1, 3, 5, 7, 9 };

        // Act
        var result = GoogleRequestHelpers.GenerateIndexRanges(rowIds);

        // Assert
        Assert.Equal(5, result.Count);
        
        // Verify ranges are in descending order (highest row ID first)
        var sortedRowIds = rowIds.OrderByDescending(x => x).ToList(); // [9, 7, 5, 3, 1]
        for (int i = 0; i < result.Count; i++)
        {
            Assert.Equal(sortedRowIds[i] - 1, result[i].Item1);
            Assert.Equal(sortedRowIds[i], result[i].Item2);
        }
    }

    [Fact]
    public void GenerateDeleteSheetRequests_WithValidSheetProperties_ShouldReturnDeleteRequests()
    {
        // Arrange
        var sheetProperties = new List<PropertyEntity>
        {
            new PropertyEntity { Id = "100", Name = "TestSheet1" },
            new PropertyEntity { Id = "200", Name = "TestSheet2" },
            new PropertyEntity { Id = "", Name = "EmptyIdSheet" }, // Should be skipped
            new PropertyEntity { Id = "invalid", Name = "InvalidIdSheet" } // Should be skipped
        };

        // Act
        var result = GoogleRequestHelpers.GenerateDeleteSheetRequests(sheetProperties).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Only sheets with valid IDs should be processed

        // Verify first delete request
        Assert.NotNull(result[0].DeleteSheet);
        Assert.Equal(100, result[0].DeleteSheet.SheetId);

        // Verify second delete request
        Assert.NotNull(result[1].DeleteSheet);
        Assert.Equal(200, result[1].DeleteSheet.SheetId);
    }

    [Fact]
    public void GenerateDeleteSheetRequests_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var sheetProperties = new List<PropertyEntity>();

        // Act
        var result = GoogleRequestHelpers.GenerateDeleteSheetRequests(sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GenerateDeleteSheetRequests_WithNullInput_ShouldReturnEmptyList()
    {
        // Arrange
        List<PropertyEntity> sheetProperties = null!;

        // Act
        var result = GoogleRequestHelpers.GenerateDeleteSheetRequests(sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ComputeEndIndex_ReturnsCorrectEndPosition()
    {
        // Arrange
        var existingSheetCount = 4;
        var newSheetCount = 10;

        // Act
        var endIndex = GoogleRequestHelpers.ComputeEndIndex(existingSheetCount, newSheetCount);

        // Assert
        Assert.Equal(existingSheetCount + newSheetCount, endIndex);
    }

    [Fact]
    public void GenerateUpdateSheetIndex_BuildsRequestWithIndexField()
    {
        // Arrange
        var sheetId = 42;
        var index = 10;

        // Act
        var request = GoogleRequestHelpers.GenerateUpdateSheetIndex(sheetId, index);

        // Assert
        Assert.NotNull(request.UpdateSheetProperties);
        Assert.Equal("index", request.UpdateSheetProperties.Fields);
        Assert.NotNull(request.UpdateSheetProperties.Properties);
        Assert.Equal(sheetId, request.UpdateSheetProperties.Properties.SheetId);
        Assert.Equal(index, request.UpdateSheetProperties.Properties.Index);
    }

    [Fact]
    public void GenerateInsertColumnDimension_BuildsColumnsInsertRequest()
    {
        // Arrange
        var sheetId = 7;
        var startIndex = 3;
        var endIndex = 4;

        // Act
        var request = GoogleRequestHelpers.GenerateInsertColumnDimension(sheetId, startIndex, endIndex);

        // Assert
        Assert.NotNull(request.InsertDimension);
        Assert.Equal("COLUMNS", request.InsertDimension.Range.Dimension);
        Assert.Equal(sheetId, request.InsertDimension.Range.SheetId);
        Assert.Equal(startIndex, request.InsertDimension.Range.StartIndex);
        Assert.Equal(endIndex, request.InsertDimension.Range.EndIndex);
        Assert.True(request.InsertDimension.InheritFromBefore);
    }

    [Fact]
    public void GenerateInsertColumnDimension_WithInheritFromBeforeFalse_SetsFlagFalse()
    {
        var request = GoogleRequestHelpers.GenerateInsertColumnDimension(1, 0, 1, inheritFromBefore: false);

        Assert.False(request.InsertDimension.InheritFromBefore);
    }

    [Fact]
    public void GenerateUpdateCellsRequest_DefaultStartColumn_TargetsColumnZero()
    {
        // Arrange
        var rows = new List<RowData> { new() { Values = [new CellData(), new CellData()] } };

        // Act
        var request = GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId: 5, rowIndex: 2, rows: rows);

        // Assert
        Assert.NotNull(request.UpdateCells);
        Assert.Equal(0, request.UpdateCells.Range.StartColumnIndex);
        Assert.Equal(2, request.UpdateCells.Range.EndColumnIndex);
        Assert.Equal(2, request.UpdateCells.Range.StartRowIndex);
        Assert.Equal(3, request.UpdateCells.Range.EndRowIndex);
    }

    [Fact]
    public void GenerateUpdateCellsRequest_WithoutFieldsArgument_DefaultsToUserEnteredValueOnly()
    {
        // Every pre-existing caller relies on this default staying exactly what it was before the
        // optional `fields` parameter was added.
        var rows = new List<RowData> { new() { Values = [new CellData()] } };

        var request = GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId: 5, rowIndex: 0, rows: rows);

        Assert.Equal(Field.USER_ENTERED_VALUE.GetDescription(), request.UpdateCells.Fields);
    }

    [Fact]
    public void GenerateUpdateCellsRequest_WithFieldsArgument_UsesProvidedMask()
    {
        var rows = new List<RowData> { new() { Values = [new CellData()] } };

        var request = GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId: 5, rowIndex: 0, rows: rows, fields: Field.USER_ENTERED_VALUE_AND_NOTE.GetDescription());

        Assert.Equal("userEnteredValue,note", request.UpdateCells.Fields);
    }

    [Fact]
    public void GenerateUpdateCellsRequest_WithStartColumnIndex_TargetsThatColumn()
    {
        // Arrange - a single-cell row being written at a specific inserted column position
        var rows = new List<RowData> { new() { Values = [new CellData()] } };

        // Act
        var request = GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId: 5, rowIndex: 0, rows: rows, startColumnIndex: 4);

        // Assert - this is the fix for the original ported implementation, which always wrote to
        // column 0 regardless of where the column was actually inserted
        Assert.Equal(4, request.UpdateCells.Range.StartColumnIndex);
        Assert.Equal(5, request.UpdateCells.Range.EndColumnIndex);
    }

    #region CreateUpdateCellRequests / CreateDeleteRequests sheetId resolution (GH #114)

    // GH #114: sheetId used to be resolved as `int.TryParse(...) ? id : 0`, then checked via
    // `sheetId == 0` as an "invalid" sentinel - which also fires for a tab whose real gid genuinely
    // is 0 (a spreadsheet's first-ever tab, not an edge case). These assert the fixed behavior:
    // TryParse's own success flag distinguishes "invalid id" from "id is legitimately zero".

    private static PropertyEntity BuildSheetProperties(string id, int maxRowValue = 0) => new()
    {
        Id = id,
        Attributes = new Dictionary<string, string>
        {
            [Property.HEADERS.GetDescription()] = "Header1",
            [Property.MAX_ROW_VALUE.GetDescription()] = maxRowValue.ToString(),
        },
    };

    private static readonly Func<List<TestRow>, IList<object>, IList<RowData>> NoOpMapToRowData =
        (rows, _) => rows.Select(_ => new RowData()).ToList();

    [Fact]
    public void CreateUpdateCellRequests_WithSheetIdZero_AppendsNewRow()
    {
        // Arrange - RowId 1 with MaxRowValue 0 means this row is past the real data extent, so it
        // should append, not update.
        var sheetProperties = BuildSheetProperties(id: "0", maxRowValue: 0);
        var entities = new List<TestRow> { new() { RowId = 1 } };

        // Act
        var requests = GoogleRequestHelpers.CreateUpdateCellRequests(entities, sheetProperties, NoOpMapToRowData).ToList();

        // Assert - appended rows are written at an explicit index now, not via AppendCells.
        var request = Assert.Single(requests);
        Assert.NotNull(request.UpdateCells);
        Assert.Equal(0, request.UpdateCells.Range.SheetId);
        Assert.Equal(0, request.UpdateCells.Range.StartRowIndex);
    }

    [Fact]
    public void CreateUpdateCellRequests_WithSheetIdZero_UpdatesExistingRow()
    {
        // Arrange - RowId 2 within MaxRowValue 5 means this targets an already-populated row.
        var sheetProperties = BuildSheetProperties(id: "0", maxRowValue: 5);
        var entities = new List<TestRow> { new() { RowId = 2 } };

        // Act
        var requests = GoogleRequestHelpers.CreateUpdateCellRequests(entities, sheetProperties, NoOpMapToRowData).ToList();

        // Assert
        var request = Assert.Single(requests);
        Assert.NotNull(request.UpdateCells);
        Assert.Equal(0, request.UpdateCells.Range.SheetId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-number")]
    public void CreateUpdateCellRequests_WithUnparseableSheetId_ReturnsEmpty(string id)
    {
        var sheetProperties = BuildSheetProperties(id);
        var entities = new List<TestRow> { new() { RowId = 1 } };

        var requests = GoogleRequestHelpers.CreateUpdateCellRequests(entities, sheetProperties, NoOpMapToRowData);

        Assert.Empty(requests);
    }

    [Fact]
    public void CreateDeleteRequests_WithSheetIdZero_ReturnsDeleteRequest()
    {
        var sheetProperties = BuildSheetProperties(id: "0");

        var requests = GoogleRequestHelpers.CreateDeleteRequests([3], sheetProperties).ToList();

        var request = Assert.Single(requests);
        Assert.NotNull(request.DeleteDimension);
        Assert.Equal(0, request.DeleteDimension.Range.SheetId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-number")]
    public void CreateDeleteRequests_WithUnparseableSheetId_ReturnsEmpty(string id)
    {
        var sheetProperties = BuildSheetProperties(id);

        var requests = GoogleRequestHelpers.CreateDeleteRequests([3], sheetProperties);

        Assert.Empty(requests);
    }

    #endregion

    #region ChangeSheetData dispatch (ResolveSheetsWithData / BuildChangeRequests)

    private sealed class TestRow : SheetRowEntityBase { }

    private sealed class TestEntity
    {
        public List<TestRow> Alpha { get; set; } = [];
        public List<TestRow> Beta { get; set; } = [];
    }

    private static Dictionary<string, GoogleRequestHelpers.SheetChangeAccessor<TestEntity>> BuildAccessors(int alphaRequestCount = 1) =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Alpha"] = new(
                e => e.Alpha.Count,
                e => e.Alpha,
                (_, _) => Enumerable.Range(0, alphaRequestCount).Select(_ => new Request())),
            ["Beta"] = new(
                e => e.Beta.Count,
                e => e.Beta,
                (_, _) => [new Request()])
        };

    [Fact]
    public void ResolveSheetsWithData_ReturnsOnlySheetsWithData_AndErrorsForUnknown()
    {
        var entity = new TestEntity { Alpha = [new TestRow()] }; // Beta empty
        var accessors = BuildAccessors();

        var (withData, messages) = GoogleRequestHelpers.ResolveSheetsWithData(
            ["Alpha", "Beta", "Unknown"], entity, accessors);

        Assert.Equal(["Alpha"], withData); // Beta recognized-but-empty -> excluded, no error
        Assert.Single(messages);
        Assert.Contains("Unknown", messages[0].Message);
        Assert.Equal(MessageType.GENERAL.GetDescription(), messages[0].Type);
    }

    [Fact]
    public void BuildChangeRequests_BuildsRequestsAndInfoMessagePerSheet()
    {
        var entity = new TestEntity { Alpha = [new TestRow()] };
        var accessors = BuildAccessors(alphaRequestCount: 3);
        var sheetInfo = new List<PropertyEntity> { new() { Name = "Alpha", Id = "1" } };

        var (requests, messages) = GoogleRequestHelpers.BuildChangeRequests(
            ["Alpha"], entity, accessors, sheetInfo);

        Assert.Equal(3, requests.Count);
        Assert.Single(messages);
        Assert.Contains("Saving data: ALPHA", messages[0].Message);
        Assert.Equal(MessageType.SAVE_DATA.GetDescription(), messages[0].Type);
    }

    #endregion
}
