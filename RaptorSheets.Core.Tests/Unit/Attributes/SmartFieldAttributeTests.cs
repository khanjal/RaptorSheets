using System.Reflection;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Attributes;

public class SmartFieldAttributeTests
{
    private class TestEntity
    {
        [SmartField]
        public string Name { get; set; } = "";

        [SmartField(FieldType.Currency)]
        public decimal Amount { get; set; }

        [SmartField("Custom Header")]
        public string CustomField { get; set; } = "";

        [SmartField(FieldType.DateTime, "Date Header")]
        public DateTime DateField { get; set; }

        [SmartField(EnableValidation = true)]
        public bool ValidatedField { get; set; }

        [SmartField(FormatPattern = "#,##0.00", Order = 1)]
        public double FormattedField { get; set; }

        public int UnsupportedField { get; set; }
        
        // Additional test properties for convention testing
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string WebsiteUrl { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public decimal Fee { get; set; }
        public decimal Salary { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TipAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal CashPayment { get; set; }
        public decimal PercentComplete { get; set; }
        public decimal RateOfReturn { get; set; }
        public decimal ScoreValue { get; set; }
        public long LongValue { get; set; }
        public short ShortValue { get; set; }
        public float FloatValue { get; set; }
        public decimal? NullableDecimal { get; set; }
        public int? NullableInt { get; set; }
    }

    [Fact]
    public void InferFieldType_ShouldReturnCorrectFieldType()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();

        // Act & Assert
        Assert.Equal(FieldType.String, FieldConventions.InferFieldType(properties.First(p => p.Name == "Name")));
        Assert.Equal(FieldType.Currency, FieldConventions.InferFieldType(properties.First(p => p.Name == "Amount")));
        Assert.Equal(FieldType.String, FieldConventions.InferFieldType(properties.First(p => p.Name == "CustomField")));
        Assert.Equal(FieldType.DateTime, FieldConventions.InferFieldType(properties.First(p => p.Name == "DateField")));
        Assert.Equal(FieldType.Boolean, FieldConventions.InferFieldType(properties.First(p => p.Name == "ValidatedField")));
        Assert.Equal(FieldType.Number, FieldConventions.InferFieldType(properties.First(p => p.Name == "FormattedField")));
        Assert.Equal(FieldType.Integer, FieldConventions.InferFieldType(properties.First(p => p.Name == "UnsupportedField"))); // int should infer as Integer
    }

    [Fact]
    public void InferFieldType_ShouldDetectEmailType()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();

        // Act & Assert
        Assert.Equal(FieldType.Email, FieldConventions.InferFieldType(properties.First(p => p.Name == "Email")));
    }

    [Fact]
    public void InferFieldType_ShouldDetectPhoneNumberType()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();

        // Act & Assert
        Assert.Equal(FieldType.PhoneNumber, FieldConventions.InferFieldType(properties.First(p => p.Name == "PhoneNumber")));
    }

    [Fact]
    public void InferFieldType_ShouldDetectUrlType()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();

        // Act & Assert
        Assert.Equal(FieldType.Url, FieldConventions.InferFieldType(properties.First(p => p.Name == "WebsiteUrl")));
    }

    [Theory]
    [InlineData("Price", FieldType.Currency)]
    [InlineData("Cost", FieldType.Currency)]
    [InlineData("Fee", FieldType.Currency)]
    [InlineData("Salary", FieldType.Currency)]
    [InlineData("TotalAmount", FieldType.Currency)]
    [InlineData("TipAmount", FieldType.Currency)]
    [InlineData("BonusAmount", FieldType.Currency)]
    [InlineData("CashPayment", FieldType.Currency)]
    public void InferFieldType_ShouldDetectCurrencyType(string propertyName, FieldType expectedType)
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();
        var property = properties.First(p => p.Name == propertyName);

        // Act
        var result = FieldConventions.InferFieldType(property);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Theory]
    [InlineData("PercentComplete", FieldType.Percentage)]
    [InlineData("RateOfReturn", FieldType.Percentage)]
    [InlineData("ScoreValue", FieldType.Percentage)]
    public void InferFieldType_ShouldDetectPercentageType(string propertyName, FieldType expectedType)
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();
        var property = properties.First(p => p.Name == propertyName);

        // Act
        var result = FieldConventions.InferFieldType(property);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Theory]
    [InlineData("LongValue", FieldType.Integer)]
    [InlineData("ShortValue", FieldType.Integer)]
    public void InferFieldType_ShouldDetectIntegerTypes(string propertyName, FieldType expectedType)
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();
        var property = properties.First(p => p.Name == propertyName);

        // Act
        var result = FieldConventions.InferFieldType(property);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Fact]
    public void InferFieldType_ShouldHandleFloatAsNumber()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();
        var property = properties.First(p => p.Name == "FloatValue");

        // Act
        var result = FieldConventions.InferFieldType(property);

        // Assert
        Assert.Equal(FieldType.Number, result);
    }

    [Theory]
    [InlineData("NullableDecimal", FieldType.Number)]
    [InlineData("NullableInt", FieldType.Integer)]
    public void InferFieldType_ShouldHandleNullableTypes(string propertyName, FieldType expectedType)
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();
        var property = properties.First(p => p.Name == propertyName);

        // Act
        var result = FieldConventions.InferFieldType(property);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Fact]
    public void InferHeaderName_ShouldReturnCorrectHeaderName()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();

        // Act & Assert
        Assert.Equal("Name", FieldConventions.InferHeaderName(properties.First(p => p.Name == "Name")));
        Assert.Equal("Amount", FieldConventions.InferHeaderName(properties.First(p => p.Name == "Amount")));
        Assert.Equal("Custom Header", FieldConventions.InferHeaderName(properties.First(p => p.Name == "CustomField")));
        Assert.Equal("Date Header", FieldConventions.InferHeaderName(properties.First(p => p.Name == "DateField")));
        Assert.Equal("Validated Field", FieldConventions.InferHeaderName(properties.First(p => p.Name == "ValidatedField")));
        Assert.Equal("Formatted Field", FieldConventions.InferHeaderName(properties.First(p => p.Name == "FormattedField")));
        Assert.Equal("Unsupported Field", FieldConventions.InferHeaderName(properties.First(p => p.Name == "UnsupportedField")));
    }

    [Theory]
    [InlineData("PhoneNumber", "Phone Number")]
    [InlineData("WebsiteUrl", "Website Url")]
    [InlineData("TotalAmount", "Total Amount")]
    [InlineData("NullableDecimal", "Nullable Decimal")]
    public void InferHeaderName_ShouldConvertPascalCaseToTitleCase(string propertyName, string expectedHeader)
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();
        var property = properties.First(p => p.Name == propertyName);

        // Act
        var result = FieldConventions.InferHeaderName(property);

        // Assert
        Assert.Equal(expectedHeader, result);
    }

    [Fact]
    public void InferJsonPropertyName_ShouldReturnCorrectJsonPropertyName()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();

        // Act & Assert
        Assert.Equal("name", FieldConventions.InferJsonPropertyName(properties.First(p => p.Name == "Name")));
        Assert.Equal("amount", FieldConventions.InferJsonPropertyName(properties.First(p => p.Name == "Amount")));
        Assert.Equal("customField", FieldConventions.InferJsonPropertyName(properties.First(p => p.Name == "CustomField")));
        Assert.Equal("dateField", FieldConventions.InferJsonPropertyName(properties.First(p => p.Name == "DateField")));
        Assert.Equal("validatedField", FieldConventions.InferJsonPropertyName(properties.First(p => p.Name == "ValidatedField")));
        Assert.Equal("formattedField", FieldConventions.InferJsonPropertyName(properties.First(p => p.Name == "FormattedField")));
        Assert.Equal("unsupportedField", FieldConventions.InferJsonPropertyName(properties.First(p => p.Name == "UnsupportedField")));
    }

    [Theory]
    [InlineData("Email", "email")]
    [InlineData("PhoneNumber", "phoneNumber")]
    [InlineData("WebsiteUrl", "websiteUrl")]
    [InlineData("TotalAmount", "totalAmount")]
    public void InferJsonPropertyName_ShouldConvertPascalCaseToCamelCase(string propertyName, string expectedJson)
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();
        var property = properties.First(p => p.Name == propertyName);

        // Act
        var result = FieldConventions.InferJsonPropertyName(property);

        // Assert
        Assert.Equal(expectedJson, result);
    }

    [Fact]
    public void SmartFieldAttribute_ShouldApplyCustomConfigurations()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();

        // Act
        var nameAttribute = properties.First(p => p.Name == "Name").GetCustomAttribute<SmartFieldAttribute>();
        var amountAttribute = properties.First(p => p.Name == "Amount").GetCustomAttribute<SmartFieldAttribute>();
        var customFieldAttribute = properties.First(p => p.Name == "CustomField").GetCustomAttribute<SmartFieldAttribute>();
        var dateFieldAttribute = properties.First(p => p.Name == "DateField").GetCustomAttribute<SmartFieldAttribute>();
        var validatedFieldAttribute = properties.First(p => p.Name == "ValidatedField").GetCustomAttribute<SmartFieldAttribute>();
        var formattedFieldAttribute = properties.First(p => p.Name == "FormattedField").GetCustomAttribute<SmartFieldAttribute>();

        // Assert
        Assert.NotNull(nameAttribute);
        Assert.Null(nameAttribute.FieldType);
        Assert.Null(nameAttribute.HeaderName);
        Assert.Null(nameAttribute.FormatPattern);
        Assert.Equal(0, nameAttribute.Order);
        Assert.False(nameAttribute.EnableValidation);

        Assert.NotNull(amountAttribute);
        Assert.Equal(FieldType.Currency, amountAttribute.FieldType);
        Assert.Null(amountAttribute.HeaderName);

        Assert.NotNull(customFieldAttribute);
        Assert.Null(customFieldAttribute.FieldType);
        Assert.Equal("Custom Header", customFieldAttribute.HeaderName);

        Assert.NotNull(dateFieldAttribute);
        Assert.Equal(FieldType.DateTime, dateFieldAttribute.FieldType);
        Assert.Equal("Date Header", dateFieldAttribute.HeaderName);

        Assert.NotNull(validatedFieldAttribute);
        Assert.Null(validatedFieldAttribute.FieldType);
        Assert.Null(validatedFieldAttribute.HeaderName);
        Assert.True(validatedFieldAttribute.EnableValidation);

        Assert.NotNull(formattedFieldAttribute);
        Assert.Null(formattedFieldAttribute.FieldType);
        Assert.Null(formattedFieldAttribute.HeaderName);
        Assert.Equal("#,##0.00", formattedFieldAttribute.FormatPattern);
        Assert.Equal(1, formattedFieldAttribute.Order);
    }

    [Fact]
    public void SmartFieldAttribute_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var attribute = new SmartFieldAttribute();

        // Assert
        Assert.Null(attribute.FieldType);
        Assert.Null(attribute.HeaderName);
        Assert.Null(attribute.JsonPropertyName);
        Assert.Null(attribute.FormatPattern);
        Assert.Equal(0, attribute.Order);
        Assert.False(attribute.EnableValidation);
    }

    [Fact]
    public void SmartFieldAttribute_FieldTypeConstructor_ShouldSetFieldType()
    {
        // Act
        var attribute = new SmartFieldAttribute(FieldType.String);

        // Assert
        Assert.Equal(FieldType.String, attribute.FieldType);
        Assert.Null(attribute.HeaderName);
    }

    [Fact]
    public void SmartFieldAttribute_HeaderNameConstructor_ShouldSetHeaderName()
    {
        // Act
        var attribute = new SmartFieldAttribute("Test Header");

        // Assert
        Assert.Null(attribute.FieldType);
        Assert.Equal("Test Header", attribute.HeaderName);
    }

    [Fact]
    public void SmartFieldAttribute_BothParametersConstructor_ShouldSetBoth()
    {
        // Act
        var attribute = new SmartFieldAttribute(FieldType.Email, "Email Address");

        // Assert
        Assert.Equal(FieldType.Email, attribute.FieldType);
        Assert.Equal("Email Address", attribute.HeaderName);
    }

    [Fact]
    public void FieldConventions_ShouldHandleNullPropertyNamesGracefully()
    {
        // Arrange
        PropertyInfo? nullProperty = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FieldConventions.InferFieldType(nullProperty!));
        Assert.Throws<ArgumentNullException>(() => FieldConventions.InferJsonPropertyName(nullProperty!));
        Assert.Throws<ArgumentNullException>(() => FieldConventions.InferHeaderName(nullProperty!));
    }
}