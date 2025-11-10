using RaptorSheets.Core.Models;
using Xunit;

namespace RaptorSheets.Core.Tests.Models;

public class SchemaValidationResultTests
{
    [Fact]
    public void AddError_ShouldSetIsValidToFalse()
    {
        // Arrange
        var result = new SchemaValidationResult();

        // Act
        result.AddError("Test error");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Test error", result.Errors);
    }

    [Fact]
    public void AddWarning_ShouldAddWarningToList()
    {
        // Arrange
        var result = new SchemaValidationResult();

        // Act
        result.AddWarning("Test warning");

        // Assert
        Assert.True(result.HasWarnings);
        Assert.Contains("Test warning", result.Warnings);
    }

    [Fact]
    public void Merge_ShouldCombineErrorsAndWarnings()
    {
        // Arrange
        var result1 = new SchemaValidationResult();
        result1.AddError("Error1");
        result1.AddWarning("Warning1");

        var result2 = new SchemaValidationResult();
        result2.AddError("Error2");
        result2.AddWarning("Warning2");

        // Act
        result1.Merge(result2);

        // Assert
        Assert.False(result1.IsValid);
        Assert.Contains("Error1", result1.Errors);
        Assert.Contains("Error2", result1.Errors);
        Assert.Contains("Warning1", result1.Warnings);
        Assert.Contains("Warning2", result1.Warnings);
    }

    [Fact]
    public void Success_ShouldReturnValidResult()
    {
        // Act
        var result = SchemaValidationResult.Success();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Failure_ShouldReturnInvalidResult()
    {
        // Act
        var result = SchemaValidationResult.Failure("Test failure");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Test failure", result.Errors);
    }

    [Fact]
    public void ErrorMessage_ShouldCombineAllErrors()
    {
        // Arrange
        var result = new SchemaValidationResult();
        result.AddError("Error1");
        result.AddError("Error2");

        // Act
        var errorMessage = result.ErrorMessage;

        // Assert
        Assert.Equal("Error1; Error2", errorMessage);
    }

    [Fact]
    public void WarningMessage_ShouldCombineAllWarnings()
    {
        // Arrange
        var result = new SchemaValidationResult();
        result.AddWarning("Warning1");
        result.AddWarning("Warning2");

        // Act
        var warningMessage = result.WarningMessage;

        // Assert
        Assert.Equal("Warning1; Warning2", warningMessage);
    }
}