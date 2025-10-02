using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class SheetHelperTests
{
    #region Core Column Name Tests (Keep - These Test Our Logic)
    
    [Theory]
    [InlineData(0, "A")]
    [InlineData(25, "Z")]
    [InlineData(26, "AA")]
    [InlineData(701, "ZZ")]
    public void GetColumnName_ShouldReturnCorrectLetters(int index, string expected)
    {
        // Act
        var result = SheetHelpers.GetColumnName(index);

        // Assert
        Assert.Equal(expected, result);
    }
    
    #endregion

    #region Simplified Helper Tests (Representative, Not Exhaustive)
    
    [Fact]
    public void GetColor_ShouldReturnValidColorObject()
    {
        // Arrange - Test one representative color
        var testColor = ColorEnum.BLUE;

        // Act
        var result = SheetHelpers.GetColor(testColor);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Red);
        Assert.NotNull(result.Green);  
        Assert.NotNull(result.Blue);
        // Verify it's actually blue-ish (basic sanity check)
        Assert.True(result.Blue > 0.5f);
    }

    [Theory]
    [InlineData(FormatEnum.ACCOUNTING, "NUMBER", true)]
    [InlineData(FormatEnum.DATE, "DATE", true)]
    [InlineData(FormatEnum.TEXT, "TEXT", false)]
    public void GetCellFormat_ShouldReturnValidFormat(FormatEnum format, string expectedType, bool hasPattern)
    {
        // Act
        var result = SheetHelpers.GetCellFormat(format);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.NumberFormat);
        Assert.Equal(expectedType, result.NumberFormat.Type);

        if (hasPattern)
        {
            Assert.NotNull(result.NumberFormat.Pattern);
        }
        else
        {
            Assert.Null(result.NumberFormat.Pattern);
        }
    }

    [Fact]
    public void HeadersToList_ShouldConvertHeadersCorrectly()
    {
        // Arrange
        var headers = new List<SheetCellModel>
        {
            new() { Formula = "Formula" },
            new() { Name = "Name" }
        };

        // Act
        var result = SheetHelpers.HeadersToList(headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result[0].Count);
        Assert.Equal("Formula", result[0][0]);
        Assert.Equal("Name", result[0][1]);
    }
    
    #endregion

    #region Integration Tests (Test Real Architecture)
    
    [Fact]
    public void GetSheets_ShouldReturnConfiguredSheets()
    {
        // Act
        var sheets = GigSheetHelpers.GetSheets();

        // Assert
        Assert.NotNull(sheets);
        
        Assert.True(sheets.Count >= 2, $"Expected at least 2 sheets, got {sheets.Count}");
        
        // Verify core sheets exist (order may vary)
        var sheetNames = sheets.Select(s => s.Name).ToList();
        Assert.Contains("Shifts", sheetNames);
        Assert.Contains("Trips", sheetNames);
        
        // Verify basic sheet structure
        Assert.All(sheets, sheet =>
        {
            Assert.NotNull(sheet.Name);
            Assert.NotEmpty(sheet.Name);
            Assert.NotNull(sheet.Headers);
            Assert.NotEmpty(sheet.Headers);
        });
    }
    
    #endregion
}