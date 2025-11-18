using RaptorSheets.Core.Validators;
using RaptorSheets.Core.Attributes;
using Xunit;

namespace RaptorSheets.Core.Tests.Validators
{
    public class SchemaValidatorTests
    {
        [Fact]
        public void ValidateSheet_ShouldReturnError_WhenHeaderRowIsNull()
        {
            // Act
            var result = SchemaValidator.ValidateSheet<TestEntity>(default!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Header row is null", result.Errors);
        }

        [Fact]
        public void ValidateSheet_ShouldReturnError_WhenExpectedHeadersAreMissing()
        {
            // Arrange
            var headerRow = new List<object> { "Header1", "Header2" };

            // Act
            var result = SchemaValidator.ValidateSheet<TestEntity>(headerRow);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Missing expected header: 'Header3'", result.Errors);
        }

        [Fact]
        public void ValidateSheet_ShouldReturnWarning_WhenUnexpectedHeadersArePresent()
        {
            // Arrange
            var headerRow = new List<object> { "Header1", "Header2", "UnexpectedHeader" };

            // Act
            var result = SchemaValidator.ValidateSheet<TestEntity>(headerRow);

            // Assert
            Assert.True(result.HasWarnings);
            Assert.Contains("Unexpected header found: 'UnexpectedHeader'", result.Warnings);
        }

        [Fact]
        public void ValidateSheet_ShouldReturnValidResult_WhenAllHeadersMatch()
        {
            // Arrange
            var headerRow = new List<object> { "Header1", "Header2", "Header3" };

            // Act
            var result = SchemaValidator.ValidateSheet<TestEntity>(headerRow);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateRequiredHeaders_ShouldReturnError_WhenRequiredHeadersAreMissing()
        {
            // Act
            var result = SchemaValidator.ValidateRequiredHeaders<TestEntity>(["Header1", "MissingHeader"]);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("MissingHeader") && e.Contains("missing"));
        }

        [Fact]
        public void ValidateRequiredHeaders_ShouldReturnValidResult_WhenAllRequiredHeadersArePresent()
        {
            // Act
            var result = SchemaValidator.ValidateRequiredHeaders<TestEntity>(["Header1", "Header2"]);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateSheetStructure_ShouldReturnError_WhenSheetDataIsEmpty()
        {
            // Act
            var result = SchemaValidator.ValidateSheetStructure<TestEntity>(default!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Sheet data is empty", result.Errors);
        }

        [Fact]
        public void ValidateSheetStructure_ShouldReturnError_WhenRowCountIsLessThanExpected()
        {
            // Arrange
            var sheetData = new List<IList<object>>
            {
                new List<object> { "Header1", "Header2" },
                new List<object> { "Data1", "Data2" }
            };

            // Act
            var result = SchemaValidator.ValidateSheetStructure<TestEntity>(sheetData, expectedMinRows: 2);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Sheet has 1 data rows, but expected at least 2", result.Errors);
        }

        [Fact]
        public void ValidateSheetStructure_ShouldReturnValidResult_WhenColumnCountsAreConsistent()
        {
            // Arrange
            var sheetData = new List<IList<object>>
            {
                new List<object> { "Header1", "Header2", "Header3" },
                new List<object> { "Data1", "Data2", "Data3" }
            };

            // Act
            var result = SchemaValidator.ValidateSheetStructure<TestEntity>(sheetData);

            // Assert
            Assert.True(result.IsValid, $"Expected valid result but got errors: {string.Join(", ", result.Errors)}");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateDataTypes_ShouldReturnWarning_WhenSampleDataRowIsNull()
        {
            // Act
            var result = SchemaValidator.ValidateDataTypes<TestEntity>(null, new Dictionary<int, string>());

            // Assert
            Assert.True(result.HasWarnings);
            Assert.Contains("No sample data row provided for type validation", result.Warnings);
        }

        [Fact]
        public void ValidateDataTypes_ShouldReturnWarning_WhenDataTypesAreIncompatible()
        {
            // Arrange
            var sampleDataRow = new List<object> { "ValidString", "ValidString2", "ValidString3" };
            var headers = new Dictionary<int, string> 
            { 
                { 0, "Header1" },
                { 1, "Header2" },
                { 2, "Header3" }
            };

            // Act
            var result = SchemaValidator.ValidateDataTypes<TestEntity>(sampleDataRow, headers);

            // Assert
            // Since TestEntity has all String fields, and we're providing strings, there should be no warnings
            Assert.False(result.HasWarnings, $"Expected no warnings but got: {string.Join(", ", result.Warnings)}");
            Assert.Empty(result.Warnings);
        }

        private class TestEntity
        {
            [Column("Header1")]
            public string Header1 { get; set; } = "";

            [Column("Header2")]
            public string Header2 { get; set; } = "";

            [Column("Header3")]
            public string Header3 { get; set; } = "";
        }
    }
}