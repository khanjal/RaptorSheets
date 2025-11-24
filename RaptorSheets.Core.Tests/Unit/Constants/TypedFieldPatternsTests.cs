using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Constants;

public class TypedFieldPatternsTests
{
    [Theory]
    [InlineData(FieldType.String, "@")]
    [InlineData(FieldType.Number, "#,##0.00")]
    [InlineData(FieldType.Currency, "$#,##0.00")]
    [InlineData(FieldType.Accounting, "_(\"$\"* #,##0.00_);_(\"$\"* (\\(#,##0.00\\));_(\"$\"* \"-\"??_);_(@_)")]
    [InlineData(FieldType.DateTime, "yyyy-MM-dd")]
    [InlineData(FieldType.Time, "hh:mm am/pm")]
    [InlineData(FieldType.Duration, "[h]:mm")]
    [InlineData(FieldType.Distance, "#,##0.0")]
    [InlineData(FieldType.PhoneNumber, "(###) ###-####")]
    [InlineData(FieldType.Boolean, "@")]
    [InlineData(FieldType.Integer, "0")]
    [InlineData(FieldType.Email, "@")]
    [InlineData(FieldType.Url, "@")]
    [InlineData(FieldType.Percentage, "0.00%")]
    public void GetDefaultPattern_WithKnownFieldTypes_ReturnsCorrectPattern(FieldType fieldType, string expectedPattern)
    {
        // Act
        var pattern = TypedFieldPatterns.GetDefaultPattern(fieldType);
        
        // Assert
        Assert.Equal(expectedPattern, pattern);
    }

    [Fact]
    public void GetDefaultPattern_WithUnknownFieldType_ReturnsTextPattern()
    {
        // Arrange
        var unknownFieldType = (FieldType)999;
        
        // Act
        var pattern = TypedFieldPatterns.GetDefaultPattern(unknownFieldType);
        
        // Assert
        Assert.Equal("@", pattern);
    }

    [Theory]
    [InlineData(FieldType.String, "TEXT")]
    [InlineData(FieldType.Number, "NUMBER")]
    [InlineData(FieldType.Currency, "CURRENCY")]
    [InlineData(FieldType.Accounting, "NUMBER")]
    [InlineData(FieldType.DateTime, "DATE_TIME")]
    [InlineData(FieldType.Time, "DATE_TIME")]
    [InlineData(FieldType.Duration, "DATE_TIME")]
    [InlineData(FieldType.Distance, "NUMBER")]
    [InlineData(FieldType.PhoneNumber, "NUMBER")]
    [InlineData(FieldType.Boolean, "TEXT")]
    [InlineData(FieldType.Integer, "NUMBER")]
    [InlineData(FieldType.Email, "TEXT")]
    [InlineData(FieldType.Url, "TEXT")]
    [InlineData(FieldType.Percentage, "PERCENT")]
    public void GetNumberFormatType_WithKnownFieldTypes_ReturnsCorrectType(FieldType fieldType, string expectedType)
    {
        // Act
        var formatType = TypedFieldPatterns.GetNumberFormatType(fieldType);
        
        // Assert
        Assert.Equal(expectedType, formatType);
    }

    [Fact]
    public void GetNumberFormatType_WithUnknownFieldType_ReturnsText()
    {
        // Arrange
        var unknownFieldType = (FieldType)999;
        
        // Act
        var formatType = TypedFieldPatterns.GetNumberFormatType(unknownFieldType);
        
        // Assert
        Assert.Equal("TEXT", formatType);
    }

    [Theory]
    [InlineData(FieldType.Email, true)]
    [InlineData(FieldType.PhoneNumber, true)]
    [InlineData(FieldType.Url, true)]
    [InlineData(FieldType.String, false)]
    [InlineData(FieldType.Number, false)]
    [InlineData(FieldType.Currency, false)]
    [InlineData(FieldType.DateTime, false)]
    [InlineData(FieldType.Distance, false)]
    public void GetDefaultValidationPattern_WithFieldTypes_ReturnsCorrectResult(FieldType fieldType, bool hasValidation)
    {
        // Act
        var validationPattern = TypedFieldPatterns.GetDefaultValidationPattern(fieldType);
        
        // Assert
        if (hasValidation)
        {
            Assert.NotNull(validationPattern);
            Assert.NotEmpty(validationPattern);
        }
        else
        {
            Assert.Null(validationPattern);
        }
    }

    [Fact]
    public void GetDefaultValidationPattern_Email_ReturnsEmailPattern()
    {
        // Act
        var pattern = TypedFieldPatterns.GetDefaultValidationPattern(FieldType.Email);
        
        // Assert
        Assert.NotNull(pattern);
        Assert.Contains("@", pattern);
        Assert.Contains("\\.", pattern);
    }

    [Fact]
    public void GetDefaultValidationPattern_PhoneNumber_ReturnsPhonePattern()
    {
        // Act
        var pattern = TypedFieldPatterns.GetDefaultValidationPattern(FieldType.PhoneNumber);
        
        // Assert
        Assert.NotNull(pattern);
        Assert.Contains("\\d", pattern);
    }

    [Fact]
    public void GetDefaultValidationPattern_Url_ReturnsUrlPattern()
    {
        // Act
        var pattern = TypedFieldPatterns.GetDefaultValidationPattern(FieldType.Url);
        
        // Assert
        Assert.NotNull(pattern);
        Assert.Contains("http", pattern);
    }

    [Fact]
    public void NewFieldTypes_Distance_HasCorrectMappings()
    {
        // Act & Assert
        Assert.Equal(CellFormatPatterns.Distance, TypedFieldPatterns.GetDefaultPattern(FieldType.Distance));
        Assert.Equal(CellFormatPatterns.CellFormatNumber, TypedFieldPatterns.GetNumberFormatType(FieldType.Distance));
        Assert.Null(TypedFieldPatterns.GetDefaultValidationPattern(FieldType.Distance));
    }

    [Fact]
    public void NewFieldTypes_Time_HasCorrectMappings()
    {
        // Act & Assert
        Assert.Equal(CellFormatPatterns.Time, TypedFieldPatterns.GetDefaultPattern(FieldType.Time));
        Assert.Equal(CellFormatPatterns.CellFormatDateTime, TypedFieldPatterns.GetNumberFormatType(FieldType.Time));
        Assert.Null(TypedFieldPatterns.GetDefaultValidationPattern(FieldType.Time));
    }

    [Fact]
    public void NewFieldTypes_Duration_HasCorrectMappings()
    {
        // Act & Assert
        Assert.Equal(CellFormatPatterns.Duration, TypedFieldPatterns.GetDefaultPattern(FieldType.Duration));
        Assert.Equal(CellFormatPatterns.CellFormatDateTime, TypedFieldPatterns.GetNumberFormatType(FieldType.Duration));
        Assert.Null(TypedFieldPatterns.GetDefaultValidationPattern(FieldType.Duration));
    }

    [Fact]
    public void NewFieldTypes_Accounting_HasCorrectMappings()
    {
        // Act & Assert
        Assert.Equal(CellFormatPatterns.Accounting, TypedFieldPatterns.GetDefaultPattern(FieldType.Accounting));
        Assert.Equal(CellFormatPatterns.CellFormatNumber, TypedFieldPatterns.GetNumberFormatType(FieldType.Accounting));
        Assert.Null(TypedFieldPatterns.GetDefaultValidationPattern(FieldType.Accounting));
    }

    [Fact]
    public void AllFieldTypes_HaveDefaultPatterns()
    {
        // Arrange
        var allFieldTypes = Enum.GetValues<FieldType>();
        
        // Act & Assert
        foreach (var fieldType in allFieldTypes)
        {
            var pattern = TypedFieldPatterns.GetDefaultPattern(fieldType);
            var formatType = TypedFieldPatterns.GetNumberFormatType(fieldType);
            
            Assert.NotNull(pattern);
            Assert.NotEmpty(pattern);
            Assert.NotNull(formatType);
            Assert.NotEmpty(formatType);
        }
    }

    [Fact]
    public void PatternsUseConstants_NotHardcodedStrings()
    {
        // Verify that patterns use CellFormatPatterns constants
        Assert.Equal(CellFormatPatterns.Text, TypedFieldPatterns.GetDefaultPattern(FieldType.String));
        Assert.Equal(CellFormatPatterns.Currency, TypedFieldPatterns.GetDefaultPattern(FieldType.Currency));
        Assert.Equal(CellFormatPatterns.Date, TypedFieldPatterns.GetDefaultPattern(FieldType.DateTime));
        Assert.Equal(CellFormatPatterns.Time, TypedFieldPatterns.GetDefaultPattern(FieldType.Time));
        Assert.Equal(CellFormatPatterns.Duration, TypedFieldPatterns.GetDefaultPattern(FieldType.Duration));
        Assert.Equal(CellFormatPatterns.Distance, TypedFieldPatterns.GetDefaultPattern(FieldType.Distance));
        Assert.Equal(CellFormatPatterns.Accounting, TypedFieldPatterns.GetDefaultPattern(FieldType.Accounting));
        Assert.Equal(CellFormatPatterns.Percentage, TypedFieldPatterns.GetDefaultPattern(FieldType.Percentage));
        Assert.Equal(CellFormatPatterns.Phone, TypedFieldPatterns.GetDefaultPattern(FieldType.PhoneNumber));
        Assert.Equal(CellFormatPatterns.Integer, TypedFieldPatterns.GetDefaultPattern(FieldType.Integer));
        Assert.Equal(CellFormatPatterns.NumberWithDecimals, TypedFieldPatterns.GetDefaultPattern(FieldType.Number));
    }
}