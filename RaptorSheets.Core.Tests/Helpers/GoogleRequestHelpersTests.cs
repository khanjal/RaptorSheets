using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using Xunit;

namespace RaptorSheets.Tests.Helpers;

public class GoogleRequestHelpersTests
{
    [Fact]
    public void GenerateAppendCells_ShouldReturnValidRequest()
    {
        // Arrange
        var sheet = new SheetModel { Id = 1, Headers = new List<SheetCellModel> { new SheetCellModel { Name = "Header1" } } };

        // Act
        var result = GoogleRequestHelpers.GenerateAppendCells(sheet);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AppendCells);
        Assert.Equal(GoogleConfig.FieldsUpdate, result.AppendCells.Fields);
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
        Assert.Single(result);
        Assert.Equal(sheet.Id, result[0].AppendDimension.SheetId);
        Assert.Equal(randomNumber, result[0].AppendDimension.Length);
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

    [Fact]
    public void GenerateDeleteRequest_ShouldReturnValidRequest()
    {
        // Arrange
        int sheetId = 1;
        int rowId = 2;

        // Act
        var result = GoogleRequestHelpers.GenerateDeleteRequest(sheetId, rowId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.DeleteDimension);
        Assert.Equal(sheetId, result.DeleteDimension.Range.SheetId);
        Assert.Equal(rowId, result.DeleteDimension.Range.StartIndex);
        Assert.Equal(rowId + 1, result.DeleteDimension.Range.EndIndex);
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
        Assert.Equal(GoogleConfig.FieldsUpdate, result.Fields);
        Assert.Equal(repeatCellModel.GridRange, result.Range);
        Assert.NotNull(result.Cell.UserEnteredFormat);
        Assert.NotNull(result.Cell.DataValidation);
    }

    [Fact]
    public void GenerateSheetPropertes_ShouldReturnValidRequest()
    {
        // Arrange
        var sheet = new SheetModel { Id = 1, Name = "TestSheet", TabColor = ColorEnum.BLUE, FreezeColumnCount = 1, FreezeRowCount = 1 };

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
        var result = GoogleRequestHelpers.GenerateUpdateRequest(sheetName, rowValues);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        Assert.Equal(sheetName + "!A1", result.Data[0].Range);
    }
}
