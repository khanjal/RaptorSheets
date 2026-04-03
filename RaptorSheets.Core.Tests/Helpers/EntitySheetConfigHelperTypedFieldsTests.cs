using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using Xunit;

namespace RaptorSheets.Core.Tests.Helpers;

public class EntitySheetConfigHelperColumnTests
{
    [Fact]
    public void GenerateHeadersFromEntity_WithColumnAttributes_ShouldApplyFormats()
    {
        // Act
        var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TestEntityWithColumns>();
        
        // Assert
        Assert.NotEmpty(headers);
        
        var currencyHeader = headers.FirstOrDefault(h => h.Name == "Currency");
        Assert.NotNull(currencyHeader);
        Assert.Equal(FormatEnum.CURRENCY, currencyHeader.Format);
        
        var dateHeader = headers.FirstOrDefault(h => h.Name == "Date");
        Assert.NotNull(dateHeader);
        Assert.Equal(FormatEnum.DATE, dateHeader.Format);
        
        var numberHeader = headers.FirstOrDefault(h => h.Name == "Number");
        Assert.NotNull(numberHeader);
        Assert.Equal(FormatEnum.NUMBER, numberHeader.Format);
    }

    [Fact]
    public void GenerateHeadersFromEntity_WithCustomPatterns_ShouldStorePatterns()
    {
        // Act
        var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TestEntityWithColumns>();
        
        // Assert
        var customCurrencyHeader = headers.FirstOrDefault(h => h.Name == "CustomCurrency");
        Assert.NotNull(customCurrencyHeader);
        Assert.Contains("Cell Format:", customCurrencyHeader.Note ?? "");
        Assert.Contains("\"�\"#,##0.00", customCurrencyHeader.Note ?? "");
    }

    [Fact]
    public void ValidateEntityForSheetGeneration_WithValidColumns_ShouldPassValidation()
    {
        // Act
        var errors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<TestEntityWithColumns>();
        
        // Assert
        Assert.Empty(errors);
    }

    // Test entities for validation
    private class TestEntityWithColumns
    {
        [Header("Currency")]
        [Format(FormatEnum.CURRENCY)]
        public decimal? Currency { get; set; }

        [Header("Date")]
        [Format(FormatEnum.DATE)]
        public DateTime? Date { get; set; }

        [Header("Number")]
        [Format(FormatEnum.NUMBER)]
        public double? Number { get; set; }

        [Header("CustomCurrency")]
        [Format(FormatEnum.CURRENCY, pattern: "\"�\"#,##0.00")]
        public decimal? CustomCurrency { get; set; }

        [Header("String")]
        [Format(FormatEnum.TEXT)]
        public string StringField { get; set; } = "";
    }
}