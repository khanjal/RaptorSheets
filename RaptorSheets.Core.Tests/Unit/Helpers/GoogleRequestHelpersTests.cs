using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
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
    //[InlineData(new int[] { 1 }, 1)]
    [InlineData(new int[] { 2 }, 1)]
    [InlineData(new int[] { 1, 2, 3 }, 1)]
    [InlineData(new int[] { 5, 10, 15 }, 3)]
    public void GenerateDeleteRequest_ShouldReturnValidRequest(int[] rowIds, int expected)
    {
        // Arrange
        int sheetId = 1;
        var rowList = rowIds.ToList();

        // Act
        var requests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, rowList);

        // Assert
        Assert.NotNull(requests);
        Assert.Equal(expected, requests.Count);
        
        // Verify all requests have correct sheet ID
        foreach (var request in requests)
        {
            Assert.NotNull(request.DeleteDimension);
            Assert.Equal(sheetId, request.DeleteDimension.Range.SheetId);
        }

        // For specific test cases, verify ranges
        if (rowIds.SequenceEqual(new int[] { 1, 2, 3 }))
        {
            Assert.Equal(0, requests[0].DeleteDimension.Range.StartIndex); // 1 - 1
            Assert.Equal(3, requests[0].DeleteDimension.Range.EndIndex); // last row ID
        }
        else if (rowIds.SequenceEqual(new int[] { 5, 10, 15 }))
        {
            Assert.Equal(4, requests[0].DeleteDimension.Range.StartIndex); // 5 - 1
            Assert.Equal(5, requests[0].DeleteDimension.Range.EndIndex); // 5
            Assert.Equal(9, requests[1].DeleteDimension.Range.StartIndex); // 10 - 1
            Assert.Equal(10, requests[1].DeleteDimension.Range.EndIndex); // 10
            Assert.Equal(14, requests[2].DeleteDimension.Range.StartIndex); // 15 - 1
            Assert.Equal(15, requests[2].DeleteDimension.Range.EndIndex); // 15
        }
        else if (rowIds.SequenceEqual(new int[] { 2 }))
        {
            Assert.Equal(1, requests[0].DeleteDimension.Range.StartIndex); // 2 - 1
            Assert.Equal(2, requests[0].DeleteDimension.Range.EndIndex); // 2
        }
    }

    [Fact]
    public void GenerateDeleteRequest_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        int sheetId = 1;
        var rowList = new List<int>();

        // Act
        var requests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, rowList);

        // Assert
        Assert.NotNull(requests);
        Assert.Empty(requests);
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
        Assert.Equal(start - 1, result[0].Item1);
        Assert.Equal(end, result[0].Item2);
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
        for (int i = 0; i < result.Count; i++)
        {
            Assert.Equal(rowIds[i] - 1, result[i].Item1);
            Assert.Equal(rowIds[i], result[i].Item2);
        }
    }
}
