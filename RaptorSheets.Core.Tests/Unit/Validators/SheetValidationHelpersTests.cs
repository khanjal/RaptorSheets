using RaptorSheets.Core.Validators;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Validators;

public class SheetValidationHelpersTests
{
    private static readonly (string?, string)[] RequiredParameterTestCases =
    {
        (null, "param1"),
        ("", "param2"),
        ("value", "param3")
    };

    private static readonly (List<int>?, string)[] RequiredCollectionTestCases =
    {
        (null, "col1"),
        (new List<int>(), "col2"),
        (new List<int> { 1 }, "col3")
    };

    [Fact]
    public void ValidateRequiredParameters_WithNullOrEmptyValues_ReturnsErrorMessages()
    {
        var result = SheetValidationHelpers.ValidateRequiredParameters(RequiredParameterTestCases);
        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Contains("null or empty", m.Message));
    }

    [Fact]
    public void ValidateRequiredParameters_WithAllValidValues_ReturnsEmptyList()
    {
        var result = SheetValidationHelpers.ValidateRequiredParameters(("a", "param1"), ("b", "param2"));
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateRequiredCollections_WithNullOrEmptyCollections_ReturnsErrorMessages()
    {
        var result = SheetValidationHelpers.ValidateRequiredCollections<int>(RequiredCollectionTestCases);
        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Contains("null or empty", m.Message));
    }

    [Fact]
    public void ValidateRequiredCollections_WithAllValidCollections_ReturnsEmptyList()
    {
        var result = SheetValidationHelpers.ValidateRequiredCollections<int>((new List<int> { 1 }, "col1"));
        var result2 = SheetValidationHelpers.ValidateRequiredCollections<string>((new[] { "a" }, "col2"));
        Assert.Empty(result);
        Assert.Empty(result2);
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData("short", 1)]
    [InlineData("12345678901234567890123456789012345678901234567890", 0)] // 50 chars should be valid
    [InlineData("123456789012345678901234567890123456789012345678901", 1)] // 51 chars should be invalid
    [InlineData("1234567890123456789012345678901234567890123456789", 0)] // 49 chars should be valid
    [InlineData("1234567890123456789012345678901234567890", 0)] // 40 chars should be valid
    [InlineData("123456789012345678901234567890123456789", 1)] // 39 chars should be invalid
    public void ValidateSpreadsheetId_VariousInputs_ReturnsExpectedCount(string id, int expectedCount)
    {
        var result = SheetValidationHelpers.ValidateSpreadsheetId(id);
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void ValidateSpreadsheetId_WithInvalidLength_ReturnsWarning()
    {
        var result = SheetValidationHelpers.ValidateSpreadsheetId("short");
        Assert.Single(result);
        Assert.Contains("may be invalid", result[0].Message);
    }

    [Fact]
    public void ValidateDateFormat_WithInvalidDate_ReturnsError()
    {
        var result = SheetValidationHelpers.ValidateDateFormat("notadate", "field");
        Assert.Single(result);
        Assert.Contains("Invalid date format", result[0].Message);
    }

    [Fact]
    public void ValidateDateFormat_WithValidDate_ReturnsEmptyList()
    {
        var result = SheetValidationHelpers.ValidateDateFormat("2024-01-01", "field");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateDecimalField_WithNegativeValue_ReturnsError()
    {
        var result = SheetValidationHelpers.ValidateDecimalField(-1m, "field");
        Assert.Single(result);
        Assert.Contains("cannot be negative", result[0].Message);
    }

    [Fact]
    public void ValidateDecimalField_WithNegativeAllowed_ReturnsEmptyList()
    {
        var result = SheetValidationHelpers.ValidateDecimalField(-1m, "field", allowNegative: true);
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateDecimalField_WithPositiveOrNull_ReturnsEmptyList()
    {
        var result1 = SheetValidationHelpers.ValidateDecimalField(1m, "field");
        var result2 = SheetValidationHelpers.ValidateDecimalField(null, "field");
        Assert.Empty(result1);
        Assert.Empty(result2);
    }
}
