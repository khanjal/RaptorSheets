using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using Xunit;

namespace RaptorSheets.Core.Tests.Attributes;

public class ColumnOptionsTests
{
    [Fact]
    public void ColumnOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new ColumnOptions();

        // Assert
        Assert.Null(options.FormatPattern);
        Assert.Null(options.JsonPropertyName);
        Assert.Equal(-1, options.Order);
        Assert.False(options.IsInput);
        Assert.False(options.EnableValidation);
        Assert.Null(options.ValidationPattern);
        Assert.Null(options.Note);
        Assert.Equal(FormatEnum.DEFAULT, options.FormatType);
    }

    [Fact]
    public void ColumnOptionsBuilder_FluentAPI_ShouldSetAllProperties()
    {
        // Arrange & Act
        var options = ColumnOptions.Builder()
            .WithFormatPattern("\"$\"#,##0.00")
            .WithJsonPropertyName("customName")
            .WithOrder(5)
            .AsInput()
            .WithValidation("RangeService")
            .WithNote("Test note")
            .WithFormatType(FormatEnum.CURRENCY)
            .Build();

        // Assert
        Assert.Equal("\"$\"#,##0.00", options.FormatPattern);
        Assert.Equal("customName", options.JsonPropertyName);
        Assert.Equal(5, options.Order);
        Assert.True(options.IsInput);
        Assert.True(options.EnableValidation);
        Assert.Equal("RangeService", options.ValidationPattern);
        Assert.Equal("Test note", options.Note);
        Assert.Equal(FormatEnum.CURRENCY, options.FormatType);
    }

    [Fact]
    public void ColumnOptionsBuilder_AsOutput_ShouldSetIsInputFalse()
    {
        // Arrange & Act
        var options = ColumnOptions.Builder()
            .AsOutput()
            .Build();

        // Assert
        Assert.False(options.IsInput);
    }

    [Fact]
    public void ColumnOptionsBuilder_WithValidation_NoPattern_ShouldEnableValidation()
    {
        // Arrange & Act
        var options = ColumnOptions.Builder()
            .WithValidation()
            .Build();

        // Assert
        Assert.True(options.EnableValidation);
        Assert.Null(options.ValidationPattern);
    }

    [Fact]
    public void ColumnAttribute_WithOptions_ShouldUseAllSettings()
    {
        // Arrange
        var options = new ColumnOptions
        {
            FormatPattern = "\"£\"#,##0.00",
            JsonPropertyName = "payAmount",
            Order = 3,
            IsInput = true,
            EnableValidation = true,
            ValidationPattern = "RangeService",
            Note = "Payment field",
            FormatType = FormatEnum.ACCOUNTING
        };

        // Act
        var attribute = new ColumnAttribute("Pay", options);

        // Assert
        Assert.Equal("Pay", attribute.HeaderName);
        Assert.Equal(FormatEnum.ACCOUNTING, attribute.FormatType);
        Assert.Equal("\"£\"#,##0.00", attribute.NumberFormatPattern);
        Assert.Equal(3, attribute.Order);
        Assert.True(attribute.IsInput);
        Assert.True(attribute.EnableValidation);
        Assert.Equal("RangeService", attribute.ValidationPattern);
        Assert.Equal("Payment field", attribute.Note);
    }

    [Fact]
    public void ColumnAttribute_WithOptionsBuilder_ShouldWork()
    {
        // Act
        var attribute = new ColumnAttribute("Pay",
            ColumnOptions.Builder()
                .AsInput()
                .WithNote("Payment amount")
                .WithValidation("RangeService"));

        // Assert
        Assert.Equal("Pay", attribute.HeaderName);
        Assert.True(attribute.IsInput);
        Assert.Equal("Payment amount", attribute.Note);
        Assert.True(attribute.EnableValidation);
        Assert.Equal("RangeService", attribute.ValidationPattern);
    }

    [Fact]
    public void ColumnAttribute_WithMinimalOptions_ShouldUseDefaults()
    {
        // Arrange
        var options = new ColumnOptions { IsInput = true };

        // Act
        var attribute = new ColumnAttribute("Test", options);

        // Assert
        Assert.Equal("Test", attribute.HeaderName);
        Assert.True(attribute.IsInput);
        Assert.Equal(-1, attribute.Order);
        Assert.False(attribute.EnableValidation);
        Assert.Equal(FormatEnum.DEFAULT, attribute.FormatType);
    }

    [Fact]
    public void ColumnAttribute_WithOptionsNullJsonPropertyName_ShouldAutoGenerate()
    {
        // Arrange
        var options = new ColumnOptions
        {
            JsonPropertyName = null,
            IsInput = true
        };

        // Act
        var attribute = new ColumnAttribute("Start Address", options);

        // Assert
        Assert.Equal("Start Address", attribute.HeaderName);
    }

    [Fact]
    public void ColumnOptionsBuilder_ChainMultipleCalls_ShouldReturnSameInstance()
    {
        // Arrange & Act
        var builder = ColumnOptions.Builder();
        var result1 = builder.AsInput();
        var result2 = result1.WithNote("Test");

        // Assert
        Assert.Same(builder, result1);
        Assert.Same(builder, result2);
    }

    [Fact]
    public void ColumnAttribute_WithOptions_ThrowsIfOptionsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ColumnAttribute("Test", (ColumnOptions)null!));
    }
}
