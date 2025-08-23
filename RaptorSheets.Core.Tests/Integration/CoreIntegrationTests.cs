using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using Xunit;

namespace RaptorSheets.Core.Tests.Integration;

/// <summary>
/// Comprehensive integration tests to ensure components work together correctly
/// </summary>
public class CoreIntegrationTests
{
    [Fact]
    public void ColumnFormulas_WithSheetModel_ShouldGenerateValidFormulas()
    {
        // Arrange
        var sheet = new SheetModel
        {
            Name = "TestSheet",
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = "ID", Column = "A" },
                new SheetCellModel { Name = "Total", Column = "B" },
                new SheetCellModel { Name = "Count", Column = "C" }
            }
        };
        sheet.Headers.UpdateColumns();

        // Act
        var sumFormula = ColumnFormulas.SumIf("Total Amount", GoogleConfig.KeyRange, "B:B", "A1", "C:C");
        var countFormula = ColumnFormulas.CountIf("Count Total", GoogleConfig.KeyRange, "B:B", "A1");

        // Assert
        Assert.Contains("SUMIF", sumFormula);
        Assert.Contains("COUNTIF", countFormula);
        Assert.Contains("ARRAYFORMULA", sumFormula);
        Assert.Contains("ARRAYFORMULA", countFormula);
    }

    [Fact]
    public void HeaderHelpers_WithVariousDataTypes_ShouldParseCorrectly()
    {
        // Arrange
        var headers = new Dictionary<int, string>
        {
            { 0, "StringCol" },
            { 1, "IntCol" },
            { 2, "DecimalCol" },
            { 3, "BoolCol" },
            { 4, "DateCol" }
        };

        var values = new List<object> 
        { 
            "Test String", 
            "123", 
            "45.67", 
            "TRUE", 
            "2023-12-25" 
        };

        // Act
        var stringVal = HeaderHelpers.GetStringValue("StringCol", values, headers);
        var intVal = HeaderHelpers.GetIntValue("IntCol", values, headers);
        var decimalVal = HeaderHelpers.GetDecimalValue("DecimalCol", values, headers);
        var boolVal = HeaderHelpers.GetBoolValue("BoolCol", values, headers);
        var dateVal = HeaderHelpers.GetDateValue("DateCol", values, headers);

        // Assert
        Assert.Equal("Test String", stringVal);
        Assert.Equal(123, intVal);
        Assert.Equal(45.67m, decimalVal);
        Assert.True(boolVal);
        Assert.Equal("2023-12-25", dateVal);
    }

    [Fact]
    public void MessageHelpers_WithMultipleTypes_ShouldCreateCorrectMessages()
    {
        // Arrange
        var testMessage = "Test operation completed";

        // Act
        var infoMessage = MessageHelpers.CreateInfoMessage(testMessage, MessageTypeEnum.GET_SHEETS);
        var warningMessage = MessageHelpers.CreateWarningMessage(testMessage, MessageTypeEnum.CHECK_SHEET);
        var errorMessage = MessageHelpers.CreateErrorMessage(testMessage, MessageTypeEnum.ADD_DATA);

        // Assert
        Assert.Equal(MessageLevelEnum.INFO.UpperName(), infoMessage.Level);
        Assert.Equal(MessageLevelEnum.WARNING.UpperName(), warningMessage.Level);
        Assert.Equal(MessageLevelEnum.ERROR.UpperName(), errorMessage.Level);
        
        Assert.All(new[] { infoMessage, warningMessage, errorMessage }, 
            msg => Assert.Equal(testMessage, msg.Message));
    }

    [Fact]
    public void SheetHelpers_WithCompleteSheet_ShouldGenerateCorrectStructure()
    {
        // Arrange
        var sheet = new SheetModel
        {
            Name = "IntegrationTestSheet",
            TabColor = ColorEnum.BLUE,
            CellColor = ColorEnum.LIGHT_GRAY,
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = "ID", Format = FormatEnum.NUMBER },
                new SheetCellModel { Name = "Name", Format = FormatEnum.TEXT },
                new SheetCellModel { Name = "Date", Format = FormatEnum.DATE },
                new SheetCellModel { Name = "Amount", Format = FormatEnum.ACCOUNTING }
            }
        };

        // Act
        sheet.Headers.UpdateColumns();
        var headersList = SheetHelpers.HeadersToList(sheet.Headers);
        var rowData = SheetHelpers.HeadersToRowData(sheet);

        // Assert
        Assert.Equal(4, sheet.Headers.Count);
        Assert.Equal("A", sheet.Headers[0].Column);
        Assert.Equal("D", sheet.Headers[3].Column);
        Assert.Single(headersList);
        Assert.Single(rowData);
        Assert.Equal(4, headersList[0].Count);
    }

    [Theory]
    [InlineData(ColorEnum.RED)]
    [InlineData(ColorEnum.GREEN)]
    [InlineData(ColorEnum.BLUE)]
    [InlineData(ColorEnum.YELLOW)]
    public void SheetHelpers_GetColor_ShouldReturnValidColors(ColorEnum color)
    {
        // Act
        var result = SheetHelpers.GetColor(color);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Red >= 0 && result.Red <= 1);
        Assert.True(result.Green >= 0 && result.Green <= 1);
        Assert.True(result.Blue >= 0 && result.Blue <= 1);
    }

    [Theory]
    [InlineData(FormatEnum.ACCOUNTING)]
    [InlineData(FormatEnum.DATE)]
    [InlineData(FormatEnum.TIME)]
    [InlineData(FormatEnum.NUMBER)]
    [InlineData(FormatEnum.TEXT)]
    public void SheetHelpers_GetCellFormat_ShouldReturnValidFormats(FormatEnum format)
    {
        // Act
        var result = SheetHelpers.GetCellFormat(format);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.NumberFormat);
        Assert.NotEmpty(result.NumberFormat.Type);
    }

    [Fact]
    public void StringExtensions_WithComplexData_ShouldHandleCorrectly()
    {
        // Arrange
        var testCases = new Dictionary<string, object?>
        {
            { "2023-12-25", "valid date" },
            { "invalid-date", null },
            { "12:30:45", "valid time" },
            { "25:00:00", null },
            { "48:30:15", "valid duration" }
        };

        // Act & Assert
        foreach (var testCase in testCases)
        {
            var dateResult = testCase.Key.ToSerialDate();
            var timeResult = testCase.Key.ToSerialTime();
            var durationResult = testCase.Key.ToSerialDuration();

            if (testCase.Value == null)
            {
                // Should handle invalid formats gracefully
                Assert.True(dateResult == null || timeResult == null || durationResult == null);
            }
            else
            {
                // At least one should succeed for valid formats
                Assert.True(dateResult != null || timeResult != null || durationResult != null);
            }
        }
    }

    [Fact]
    public void EnumExtensions_WithVariousEnums_ShouldWorkConsistently()
    {
        // Arrange & Act
        var messageLevel = MessageLevelEnum.INFO.UpperName();
        var messageType = MessageTypeEnum.GET_SHEETS.GetDescription();
        var colorDescription = ColorEnum.BLUE.GetDescription();
        var formatDescription = FormatEnum.DATE.GetDescription();

        // Assert
        Assert.Equal("INFO", messageLevel);
        Assert.NotEmpty(messageType);
        Assert.NotEmpty(colorDescription);
        Assert.NotEmpty(formatDescription);
    }

    [Fact]
    public void ComplexWorkflow_CreateSheetWithDataValidation_ShouldWork()
    {
        // Arrange
        var sheet = new SheetModel
        {
            Id = 100,
            Name = "ComplexSheet",
            ProtectSheet = false,
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel 
                { 
                    Name = "Status", 
                    Format = FormatEnum.TEXT,
                    Formula = ColumnFormulas.SortUnique("Status", "B:B")
                }
            }
        };

        // Act
        sheet.Headers.UpdateColumns();
        var appendRequest = GoogleRequestHelpers.GenerateAppendCells(sheet);
        var bandingRequest = GoogleRequestHelpers.GenerateBandingRequest(sheet);
        var protectionRequest = GoogleRequestHelpers.GenerateProtectedRangeForHeaderOrSheet(sheet);

        // Assert
        Assert.NotNull(appendRequest);
        Assert.NotNull(bandingRequest);
        Assert.NotNull(protectionRequest);
        Assert.Equal(sheet.Id, appendRequest.AppendCells.SheetId);
        Assert.Equal(sheet.Id, bandingRequest.AddBanding.BandedRange.BandedRangeId);
        Assert.Equal(sheet.Id, protectionRequest.AddProtectedRange.ProtectedRange.Range.SheetId);
    }
}