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
        var attribute = new ColumnAttribute(headerName);
        
        // Assert
        Assert.Equal(expectedJsonName, attribute.JsonPropertyName);
        Assert.Equal(headerName, attribute.HeaderName);
    }

    [Fact]
    public void ColumnAttribute_WithCustomJsonPropertyName_ShouldUseOverride()
    {
        // Act
        var attribute = new ColumnAttribute("Pay", "paymentAmount");
        
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
        var attribute = new ColumnAttribute(headerName);
        
        // Assert
        Assert.Equal(expectedJsonName, attribute.JsonPropertyName);
    }

    [Theory]
    [InlineData("Test", "@")]
    public void ColumnAttribute_ShouldUseDefaultFormatPattern(string headerName, string expectedPattern)
    {
        // Act - No format pattern specified, should use default (inferred later)
        var attribute = new ColumnAttribute(headerName);
        
        // Assert
        Assert.Equal(expectedPattern, attribute.GetEffectiveNumberFormatPattern());
        Assert.False(attribute.HasCustomFormatPattern);
        Assert.Null(attribute.NumberFormatPattern); // No custom pattern stored
    }

    [Fact]
    public void ColumnAttribute_WithCustomPattern_ShouldUseCustomFormatPattern()
    {
        // Act - Custom format pattern specified
        var customPattern = "\"?\"#,##0.00";
        var attribute = new ColumnAttribute("Pay", isInput: false, formatPattern: customPattern);
        
        // Assert
        Assert.Equal(customPattern, attribute.GetEffectiveNumberFormatPattern());
        Assert.True(attribute.HasCustomFormatPattern);
        Assert.Equal(customPattern, attribute.NumberFormatPattern);
    }
}