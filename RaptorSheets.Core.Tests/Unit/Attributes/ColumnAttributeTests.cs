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
        var column = new ColumnAttribute("Test", isInput: true, formatPattern: customPattern, formatType: FormatEnum.DISTANCE);
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
        var column = new ColumnAttribute("Distance", formatType: FormatEnum.DISTANCE);
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
        var column = new ColumnAttribute("Duration", formatType: FormatEnum.DURATION);
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
        var column = new ColumnAttribute("Time", formatType: FormatEnum.TIME);
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
        var column = new ColumnAttribute("Amount", formatType: FormatEnum.ACCOUNTING);
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
        var column = new ColumnAttribute("Amount"); // FormatEnum.DEFAULT
        column.SetFieldTypeFromProperty(typeof(decimal?));
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(CellFormatPatterns.Currency, pattern); // FieldType.Currency default
    }

    [Fact]
    public void GetEffectiveNumberFormatPattern_PriorityTest_CustomOverridesFormatType()
    {
        // Arrange
        var customPattern = "###.###";
        var column = new ColumnAttribute("Test", isInput: true, formatPattern: customPattern, formatType: FormatEnum.CURRENCY);
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
        var column = new ColumnAttribute("Distance", formatType: FormatEnum.DISTANCE);
        column.SetFieldTypeFromProperty(typeof(decimal?)); // Would normally be FieldType.Currency
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(CellFormatPatterns.Distance, pattern);
        Assert.NotEqual(CellFormatPatterns.Currency, pattern);
    }

    [Theory]
    [InlineData(FormatEnum.CURRENCY, typeof(decimal?))]
    [InlineData(FormatEnum.ACCOUNTING, typeof(decimal?))]
    [InlineData(FormatEnum.DATE, typeof(string))]
    [InlineData(FormatEnum.TIME, typeof(string))]
    [InlineData(FormatEnum.DURATION, typeof(string))]
    [InlineData(FormatEnum.DISTANCE, typeof(decimal?))]
    [InlineData(FormatEnum.NUMBER, typeof(double?))]
    [InlineData(FormatEnum.PERCENT, typeof(decimal?))]
    [InlineData(FormatEnum.TEXT, typeof(string))]
    public void GetEffectiveNumberFormatPattern_WithVariousFormats_ReturnsCorrectPattern(FormatEnum formatType, Type propertyType)
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
            FormatEnum.CURRENCY => CellFormatPatterns.Currency,
            FormatEnum.ACCOUNTING => CellFormatPatterns.Accounting,
            FormatEnum.DATE => CellFormatPatterns.Date,
            FormatEnum.TIME => CellFormatPatterns.Time,
            FormatEnum.DURATION => CellFormatPatterns.Duration,
            FormatEnum.DISTANCE => CellFormatPatterns.Distance,
            FormatEnum.NUMBER => CellFormatPatterns.Number,
            FormatEnum.PERCENT => CellFormatPatterns.Percentage,
            FormatEnum.TEXT => CellFormatPatterns.Text,
            _ => CellFormatPatterns.Text
        };
        
        Assert.Equal(expectedPattern, pattern);
    }

    [Fact]
    public void GetEffectiveNumberFormatPattern_WithWeekdayFormat_ReturnsWeekdayPattern()
    {
        // Arrange
        var column = new ColumnAttribute("Day", formatType: FormatEnum.WEEKDAY);
        column.SetFieldTypeFromProperty(typeof(string));
        
        // Act
        var pattern = column.GetEffectiveNumberFormatPattern();
        
        // Assert
        Assert.Equal(CellFormatPatterns.Weekday, pattern);
    }
}