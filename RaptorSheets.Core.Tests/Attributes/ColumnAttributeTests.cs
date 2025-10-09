using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using Xunit;

namespace RaptorSheets.Core.Tests.Attributes;

public class ColumnAttributeTests
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
    [InlineData("Service Type", "serviceType")]
    [InlineData("Total Grand", "totalGrand")]
    public void ColumnAttribute_ShouldConvertHeaderNameToJsonPropertyName(string headerName, string expectedJsonName)
    {
        // Act
        var attribute = new ColumnAttribute(headerName, FieldTypeEnum.String);
        
        // Assert
        Assert.Equal(expectedJsonName, attribute.JsonPropertyName);
        Assert.Equal(headerName, attribute.HeaderName);
        Assert.Equal(headerName, attribute.GetEffectiveHeaderName());
    }

    [Theory]
    [InlineData("Multi-Word-Header", "multiWordHeader")]
    [InlineData("Under_Score_Name", "underScoreName")]
    [InlineData("Mixed-Punctuation_Name", "mixedPunctuationName")]
    [InlineData("Name.With.Dots", "nameWithDots")]
    [InlineData("  Extra   Spaces  ", "extraSpaces")]
    public void ColumnAttribute_ShouldHandleSpecialCharactersInHeaderName(string headerName, string expectedJsonName)
    {
        // Act
        var attribute = new ColumnAttribute(headerName, FieldTypeEnum.String);
        
        // Assert
        Assert.Equal(expectedJsonName, attribute.JsonPropertyName);
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

    [Fact]
    public void ColumnAttribute_WithFormatPattern_ShouldStorePattern()
    {
        // Act
        var attribute = new ColumnAttribute("Pay", FieldTypeEnum.Currency, "\"$\"#,##0.00");
        
        // Assert
        Assert.Equal("\"$\"#,##0.00", attribute.NumberFormatPattern);
        Assert.Equal("pay", attribute.JsonPropertyName); // Generated from header
        Assert.Equal("Pay", attribute.HeaderName);
    }

    [Fact]
    public void ColumnAttribute_WithAllParameters_ShouldSetAllProperties()
    {
        // Act
        var attribute = new ColumnAttribute(
            headerName: "Email Address",
            fieldType: FieldTypeEnum.Email,
            formatPattern: null,
            jsonPropertyName: "email",
            order: 5,
            enableValidation: true,
            validationPattern: @"^[^\s@]+@[^\s@]+\.[^\s@]+$");
        
        // Assert
        Assert.Equal("Email Address", attribute.HeaderName);
        Assert.Equal("email", attribute.JsonPropertyName);
        Assert.Equal(FieldTypeEnum.Email, attribute.FieldType);
        Assert.Equal(5, attribute.Order);
        Assert.True(attribute.HasExplicitOrder);
        Assert.True(attribute.EnableValidation);
        Assert.Equal(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", attribute.ValidationPattern);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData("SingleWord", "singleword")]
    [InlineData("A", "a")]
    public void ColumnAttribute_ShouldHandleEdgeCases(string headerName, string expectedJsonName)
    {
        // Act
        var attribute = new ColumnAttribute(headerName, FieldTypeEnum.String);
        
        // Assert
        Assert.Equal(expectedJsonName, attribute.JsonPropertyName);
    }

    [Fact]
    public void ColumnAttribute_WithNullHeaderName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ColumnAttribute(null!, FieldTypeEnum.String));
    }
}