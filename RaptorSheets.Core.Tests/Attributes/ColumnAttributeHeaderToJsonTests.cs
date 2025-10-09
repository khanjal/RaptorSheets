using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using Xunit;

namespace RaptorSheets.Core.Tests.Attributes;

public class ColumnAttributeHeaderToJsonTests
{
    [Theory]
    [InlineData("Pay", "pay")]
    [InlineData("Date", "date")]
    [InlineData("Start Address", "startAddress")]
    [InlineData("Address End", "addressEnd")]
    [InlineData("Order Number", "orderNumber")]
    [InlineData("Amount Per Time", "amountPerTime")]
    [InlineData("Odometer Start", "odometerStart")]
    [InlineData("Unit End", "unitEnd")]
    [InlineData("Tips", "tips")]
    public void ColumnAttribute_ShouldConvertHeaderNameToJsonPropertyName(string headerName, string expectedJsonName)
    {
        // Act
        var attribute = new ColumnAttribute(headerName, FieldTypeEnum.String);
        
        // Assert
        Assert.Equal(expectedJsonName, attribute.JsonPropertyName);
        Assert.Equal(headerName, attribute.HeaderName);
    }

    [Fact]
    public void ColumnAttribute_WithCustomJsonPropertyName_ShouldUseOverride()
    {
        // Act
        var attribute = new ColumnAttribute("Pay", FieldTypeEnum.Currency, jsonPropertyName: "paymentAmount");
        
        // Assert
        Assert.Equal("paymentAmount", attribute.JsonPropertyName);
        Assert.Equal("Pay", attribute.HeaderName);
    }

    [Theory]
    [InlineData("Multi-Word-Header", "multiWordHeader")]
    [InlineData("Under_Score_Name", "underScoreName")]
    [InlineData("Name.With.Dots", "nameWithDots")]
    public void ColumnAttribute_ShouldHandleSpecialCharacters(string headerName, string expectedJsonName)
    {
        // Act
        var attribute = new ColumnAttribute(headerName, FieldTypeEnum.String);
        
        // Assert
        Assert.Equal(expectedJsonName, attribute.JsonPropertyName);
    }

    [Theory]
    [InlineData(FieldTypeEnum.Currency, "\"$\"#,##0.00")]
    [InlineData(FieldTypeEnum.Number, "#,##0.00")]
    [InlineData(FieldTypeEnum.Integer, "0")]
    [InlineData(FieldTypeEnum.DateTime, "M/d/yyyy H:mm:ss")]
    [InlineData(FieldTypeEnum.String, "@")]
    [InlineData(FieldTypeEnum.Percentage, "0.00%")]
    public void ColumnAttribute_ShouldUseDefaultFormatPattern(FieldTypeEnum fieldType, string expectedPattern)
    {
        // Act - No format pattern specified, should use default
        var attribute = new ColumnAttribute("Test", fieldType);
        
        // Assert
        Assert.Equal(expectedPattern, attribute.GetEffectiveNumberFormatPattern());
        Assert.False(attribute.HasCustomFormatPattern);
        Assert.Null(attribute.NumberFormatPattern); // No custom pattern stored
    }

    [Fact]
    public void ColumnAttribute_WithCustomPattern_ShouldUseCustomFormatPattern()
    {
        // Act - Custom format pattern specified
        var customPattern = "\"£\"#,##0.00";
        var attribute = new ColumnAttribute("Pay", FieldTypeEnum.Currency, customPattern);
        
        // Assert
        Assert.Equal(customPattern, attribute.GetEffectiveNumberFormatPattern());
        Assert.True(attribute.HasCustomFormatPattern);
        Assert.Equal(customPattern, attribute.NumberFormatPattern);
    }

    [Theory]
    [InlineData(FieldTypeEnum.Currency, null, "\"$\"#,##0.00", false)] // Default case
    [InlineData(FieldTypeEnum.Currency, "\"$\"#,##0.00", "\"$\"#,##0.00", true)] // Explicit default (redundant but allowed)
    [InlineData(FieldTypeEnum.Currency, "\"£\"#,##0.00", "\"£\"#,##0.00", true)] // Custom pattern
    [InlineData(FieldTypeEnum.Number, null, "#,##0.00", false)] // Default case
    [InlineData(FieldTypeEnum.Number, "#,##0.0", "#,##0.0", true)] // Custom pattern with 1 decimal
    public void ColumnAttribute_ShouldDistinguishCustomFromDefault(
        FieldTypeEnum fieldType, 
        string? specifiedPattern, 
        string expectedEffectivePattern, 
        bool expectedHasCustom)
    {
        // Act
        var attribute = specifiedPattern != null 
            ? new ColumnAttribute("Test", fieldType, specifiedPattern)
            : new ColumnAttribute("Test", fieldType);
        
        // Assert
        Assert.Equal(expectedEffectivePattern, attribute.GetEffectiveNumberFormatPattern());
        Assert.Equal(expectedHasCustom, attribute.HasCustomFormatPattern);
        Assert.Equal(specifiedPattern, attribute.NumberFormatPattern);
    }
}