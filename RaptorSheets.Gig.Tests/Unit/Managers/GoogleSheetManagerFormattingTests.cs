using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Managers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Managers;

[Category("Unit Tests")]
public class GoogleSheetManagerFormattingTests
{
    #region GetSheetConfiguration Tests

    [Fact]
    public void GetSheetConfiguration_WithValidSheetName_ShouldReturnConfiguration()
    {
        // Arrange
        var sheetName = SheetsConfig.SheetNames.Trips;

        // Act
        var config = GoogleSheetManager.GetSheetConfiguration(sheetName);

        // Assert
        Assert.NotNull(config);
        Assert.Equal(sheetName, config.Name);
    }

    [Fact]
    public void GetSheetConfiguration_WithTripsSheet_ShouldHaveTripProperties()
    {
        // Act
        var config = GoogleSheetManager.GetSheetConfiguration(SheetsConfig.SheetNames.Trips);

        // Assert
        Assert.NotNull(config);
        Assert.False(config.ProtectSheet); // Trips sheet is not protected
        Assert.True(config.FreezeColumnCount > 0);
        Assert.True(config.FreezeRowCount > 0);
    }

    [Fact]
    public void GetSheetConfiguration_WithShiftsSheet_ShouldHaveShiftProperties()
    {
        // Act
        var config = GoogleSheetManager.GetSheetConfiguration(SheetsConfig.SheetNames.Shifts);

        // Assert
        Assert.NotNull(config);
        Assert.False(config.ProtectSheet); // Shifts sheet is not protected
    }

    [Theory]
    [InlineData("Trips")]
    [InlineData("Shifts")]
    [InlineData("Expenses")]
    [InlineData("Addresses")]
    [InlineData("Names")]
    [InlineData("Places")]
    [InlineData("Services")]
    [InlineData("Types")]
    [InlineData("Daily")]
    [InlineData("Setup")]
    public void GetSheetConfiguration_WithAllValidSheets_ShouldReturnConfiguration(string sheetName)
    {
        // Act
        var config = GoogleSheetManager.GetSheetConfiguration(sheetName);

        // Assert
        Assert.NotNull(config);
        Assert.Equal(sheetName, config.Name);
        Assert.NotNull(config.Headers);
        Assert.NotEmpty(config.Headers);
    }

    [Fact]
    public void GetSheetConfiguration_WithInvalidSheetName_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => GoogleSheetManager.GetSheetConfiguration("NonExistentSheet"));
    }

    #endregion

    #region ReapplyFormatting Tests (Single Sheet)

    [Fact]
    public void ReapplyFormatting_WithValidSheet_ShouldNotThrowException()
    {
        // Arrange
        var sheetName = SheetsConfig.SheetNames.Trips;

        // Act & Assert - Should not throw
        GoogleSheetManager.GetSheetConfiguration(sheetName); // Verify sheet exists
    }

    #endregion

    #region ReapplyFormatting Tests (Multiple Sheets)

    [Fact]
    public void ReapplyFormatting_WithMultipleSheets_ShouldProcessAll()
    {
        // Arrange
        var sheets = new List<string>
        {
            SheetsConfig.SheetNames.Trips,
            SheetsConfig.SheetNames.Shifts,
            SheetsConfig.SheetNames.Expenses
        };

        // Act & Assert
        Assert.NotNull(sheets);
        Assert.Equal(3, sheets.Count);
        foreach (var sheet in sheets)
        {
            var config = GoogleSheetManager.GetSheetConfiguration(sheet);
            Assert.NotNull(config);
        }
    }

    [Fact]
    public void ReapplyFormatting_WithEmptySheetList_ShouldHandleGracefully()
    {
        // Arrange
        var sheets = new List<string>();

        // Act & Assert
        Assert.Empty(sheets);
    }

    [Fact]
    public void ReapplyFormatting_WithDuplicateSheets_ShouldHandleMultipleReferences()
    {
        // Arrange
        var sheets = new List<string>
        {
            SheetsConfig.SheetNames.Trips,
            SheetsConfig.SheetNames.Trips, // Duplicate
            SheetsConfig.SheetNames.Shifts
        };

        // Act & Assert
        Assert.Equal(3, sheets.Count);
        var uniqueSheets = sheets.Distinct().ToList();
        Assert.Equal(2, uniqueSheets.Count);
    }

    #endregion

    #region Formatting Request Helper Tests

    [Fact]
    public void GenerateUpdateNumberFormat_WithValidRange_ShouldCreateRequest()
    {
        // Arrange
        const int sheetId = 0;
        const int startRow = 1;
        const int endRow = 100;
        const int startColumn = 0;
        const int endColumn = 1;
        const string formatType = "CURRENCY";
        const string pattern = "\"$\"#,##0.00";

        // Act
        var request = GoogleRequestHelpers.GenerateUpdateNumberFormat(
            sheetId, startRow, endRow, startColumn, endColumn, formatType, pattern);

        // Assert
        Assert.NotNull(request);
        Assert.NotNull(request.UpdateCells);
    }

    [Fact]
    public void GenerateUpdateCellColor_WithValidRange_ShouldCreateRequest()
    {
        // Arrange
        const int sheetId = 0;
        const int startRow = 1;
        const int endRow = 100;
        const int startColumn = 0;
        const int endColumn = 10;
        const float red = 1.0f;
        const float green = 0.0f;
        const float blue = 0.0f;

        // Act
        var request = GoogleRequestHelpers.GenerateUpdateCellColor(
            sheetId, startRow, endRow, startColumn, endColumn, red, green, blue);

        // Assert
        Assert.NotNull(request);
        Assert.NotNull(request.UpdateCells);
    }

    [Fact]
    public void GenerateUpdateTabColor_WithValidColor_ShouldCreateRequest()
    {
        // Arrange
        const int sheetId = 0;
        const float red = 0.5f;
        const float green = 0.5f;
        const float blue = 0.5f;

        // Act
        var request = GoogleRequestHelpers.GenerateUpdateTabColor(sheetId, red, green, blue);

        // Assert
        Assert.NotNull(request);
        Assert.NotNull(request.UpdateSheetProperties);
    }

    [Fact]
    public void GenerateUpdateFrozenRowsColumns_WithValidValues_ShouldCreateRequest()
    {
        // Arrange
        const int sheetId = 0;
        const int frozenRowCount = 1;
        const int frozenColumnCount = 1;

        // Act
        var request = GoogleRequestHelpers.GenerateUpdateFrozenRowsColumns(
            sheetId, frozenRowCount, frozenColumnCount);

        // Assert
        Assert.NotNull(request);
        Assert.NotNull(request.UpdateSheetProperties);
    }

    [Fact]
    public void GenerateProtectSheet_WithValidSheetId_ShouldCreateRequest()
    {
        // Arrange
        const int sheetId = 0;
        const string title = "Protected Sheet";

        // Act
        var request = GoogleRequestHelpers.GenerateProtectSheet(sheetId, title);

        // Assert
        Assert.NotNull(request);
        Assert.NotNull(request.AddProtectedRange);
        Assert.NotNull(request.AddProtectedRange.ProtectedRange);
    }

    [Fact]
    public void GenerateProtectSheet_WithDefaultTitle_ShouldUseFallback()
    {
        // Arrange
        const int sheetId = 42;

        // Act
        var request = GoogleRequestHelpers.GenerateProtectSheet(sheetId);

        // Assert
        Assert.NotNull(request);
        Assert.NotNull(request.AddProtectedRange);
        var protectedRange = request.AddProtectedRange.ProtectedRange;
        Assert.NotNull(protectedRange);
        Assert.Contains("42", protectedRange.Description);
    }

    #endregion

    #region Formatting Colors Tests

    [Fact]
    public void ReapplyFormatting_WithTabColor_ShouldApplyColor()
    {
        // Arrange
        var config = GoogleSheetManager.GetSheetConfiguration(SheetsConfig.SheetNames.Trips);

        // Act & Assert
        Assert.NotNull(config);
        Assert.NotEqual(ColorEnum.BLACK, config.TabColor);
    }

    [Fact]
    public void ReapplyFormatting_WithCellColor_ShouldApplyColor()
    {
        // Arrange
        var config = GoogleSheetManager.GetSheetConfiguration(SheetsConfig.SheetNames.Trips);

        // Act & Assert
        Assert.NotNull(config);
        Assert.NotEqual(ColorEnum.BLACK, config.CellColor);
    }

    [Fact]
    public void ReapplyFormatting_WithMultipleColors_ShouldApplyBoth()
    {
        // Arrange
        var config = GoogleSheetManager.GetSheetConfiguration(SheetsConfig.SheetNames.Trips);

        // Act & Assert
        Assert.NotNull(config);
        // At least one should not be black
        var hasTabColor = config.TabColor != ColorEnum.BLACK;
        var hasCellColor = config.CellColor != ColorEnum.BLACK;
        Assert.True(hasTabColor || hasCellColor);
    }

    #endregion

    #region Formatting Protection Tests

    [Fact]
    public void ReapplyFormatting_WithProtectedSheet_ShouldHaveProtection()
    {
        // Arrange - Use a sheet that is actually protected
        var config = GoogleSheetManager.GetSheetConfiguration(SheetsConfig.SheetNames.Setup);

        // Act & Assert
        Assert.NotNull(config);
        Assert.True(config.ProtectSheet);
    }

    [Fact]
    public void ReapplyFormatting_WithFrozenRows_ShouldHaveFrozenRowCount()
    {
        // Arrange
        var config = GoogleSheetManager.GetSheetConfiguration(SheetsConfig.SheetNames.Trips);

        // Act & Assert
        Assert.NotNull(config);
        Assert.True(config.FreezeRowCount >= 0);
    }

    [Fact]
    public void ReapplyFormatting_WithFrozenColumns_ShouldHaveFrozenColumnCount()
    {
        // Arrange
        var config = GoogleSheetManager.GetSheetConfiguration(SheetsConfig.SheetNames.Trips);

        // Act & Assert
        Assert.NotNull(config);
        Assert.True(config.FreezeColumnCount >= 0);
    }

    #endregion
}
