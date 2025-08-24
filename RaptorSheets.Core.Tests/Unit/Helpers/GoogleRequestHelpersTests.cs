using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using System.Linq;
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
        Assert.Equal(FieldEnum.USER_ENTERED_VALUE_AND_FORMAT.GetDescription(), result.AppendCells.Fields);
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
        var sheet = new SheetModel { Id = 1, TabColor = ColorEnum.BLUE, CellColor = ColorEnum.GREEN };

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
            Assert.Equal(DimensionEnum.ROWS.GetDescription(), request.DeleteDimension.Range.Dimension);
            
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
        Assert.Equal(FieldEnum.USER_ENTERED_VALUE_AND_FORMAT.GetDescription(), result.Fields);
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
    public void GenerateSheetPropertes_ShouldReturnValidRequest()
    {
        // Arrange
        var sheet = new SheetModel 
        { 
            Id = 1, 
            Name = "TestSheet", 
            TabColor = ColorEnum.BLUE, 
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
}
