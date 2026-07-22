using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Attributes;

public class ColumnAttributeTests
{
    [Fact]
    public void GetEffectiveNumberFormatPattern_WithCustomPattern_ReturnsCustomPattern()
    {
        // Arrange
        var customPattern = "0.000";
        var column = new ColumnAttribute("Test", isInput: true, formatPattern: customPattern, formatType: Format.DISTANCE);
        column.SetFieldTypeFromProperty(typeof(decimal?));
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(customPattern, pattern);
    }

    [Fact]
    public void GetEffectiveNumberFormatPattern_WithExplicitFormatType_ReturnsFormatTypePattern()
    {
        // Arrange
        var column = new ColumnAttribute("Distance", formatType: Format.DISTANCE);
        column.SetFieldTypeFromProperty(typeof(decimal?));
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(CellFormatPatterns.Distance, pattern);
    }

    [Fact]
    public void GetEffectiveNumberFormatPattern_WithDurationFormat_ReturnsDurationPattern()
    {
        // Arrange
        var column = new ColumnAttribute("Duration", formatType: Format.DURATION);
        column.SetFieldTypeFromProperty(typeof(string));
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(CellFormatPatterns.Duration, pattern);
    }

    [Fact]
    public void GetEffectiveNumberFormatPattern_WithTimeFormat_ReturnsTimePattern()
    {
        // Arrange
        var column = new ColumnAttribute("Time", formatType: Format.TIME);
        column.SetFieldTypeFromProperty(typeof(string));
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(CellFormatPatterns.Time, pattern);
    }

    [Fact]
    public void GetEffectiveNumberFormatPattern_WithAccountingFormat_ReturnsAccountingPattern()
    {
        // Arrange
        var column = new ColumnAttribute("Amount", formatType: Format.ACCOUNTING);
        column.SetFieldTypeFromProperty(typeof(decimal?));
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(CellFormatPatterns.Accounting, pattern);
    }

    [Fact]
    public void GetEffectiveNumberFormatPattern_WithDefaultFormat_ReturnsFieldTypePattern()
    {
        // Arrange
        var column = new ColumnAttribute("Amount"); // Format.DEFAULT
        column.SetFieldTypeFromProperty(typeof(decimal?));
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(CellFormatPatterns.NumberWithDecimals, pattern); // FieldType.Currency default
    }

    [Fact]
    public void GetEffectiveNumberFormatPattern_PriorityTest_CustomOverridesFormatType()
    {
        // Arrange
        var customPattern = "###.###";
        var column = new ColumnAttribute("Test", isInput: true, formatPattern: customPattern, formatType: Format.CURRENCY);
        column.SetFieldTypeFromProperty(typeof(decimal?));
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(customPattern, pattern);
        Assert.NotEqual(CellFormatPatterns.Currency, pattern);
    }

    [Fact]
    public void GetEffectiveNumberFormatPattern_PriorityTest_FormatTypeOverridesFieldType()
    {
        // Arrange - decimal property with DISTANCE format should use distance pattern, not currency
        var column = new ColumnAttribute("Distance", formatType: Format.DISTANCE);
        column.SetFieldTypeFromProperty(typeof(decimal?)); // Would normally be FieldType.Currency
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(CellFormatPatterns.Distance, pattern);
        Assert.NotEqual(CellFormatPatterns.Currency, pattern);
    }

    [Theory]
    [InlineData(Format.CURRENCY, typeof(decimal?))]
    [InlineData(Format.ACCOUNTING, typeof(decimal?))]
    [InlineData(Format.DATE, typeof(string))]
    [InlineData(Format.TIME, typeof(string))]
    [InlineData(Format.DURATION, typeof(string))]
    [InlineData(Format.DISTANCE, typeof(decimal?))]
    [InlineData(Format.NUMBER, typeof(double?))]
    [InlineData(Format.PERCENT, typeof(decimal?))]
    [InlineData(Format.TEXT, typeof(string))]
    public void GetEffectiveNumberFormatPattern_WithVariousFormats_ReturnsCorrectPattern(Format formatType, Type propertyType)
    {
        // Arrange
        var column = new ColumnAttribute("Test", formatType: formatType);
        column.SetFieldTypeFromProperty(propertyType);
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.NotNull(pattern);
        Assert.NotEmpty(pattern);
        
        // Verify it matches the expected pattern for the format type
        var expectedPattern = formatType switch
        {
            Format.CURRENCY => CellFormatPatterns.Currency,
            Format.ACCOUNTING => CellFormatPatterns.Accounting,
            Format.DATE => CellFormatPatterns.Date,
            Format.TIME => CellFormatPatterns.Time,
            Format.DURATION => CellFormatPatterns.Duration,
            Format.DISTANCE => CellFormatPatterns.Distance,
            Format.NUMBER => CellFormatPatterns.Number,
            Format.PERCENT => CellFormatPatterns.Percentage,
            Format.TEXT => CellFormatPatterns.Text,
            _ => CellFormatPatterns.Text
        };
        
        Assert.Equal(expectedPattern, pattern);
    }

    [Fact]
    public void GetEffectiveNumberFormatPattern_WithWeekdayFormat_ReturnsWeekdayPattern()
    {
        // Arrange
        var column = new ColumnAttribute("Day", formatType: Format.WEEKDAY);
        column.SetFieldTypeFromProperty(typeof(string));
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(CellFormatPatterns.Weekday, pattern);
    }
}