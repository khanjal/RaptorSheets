using System;
using System.Collections.Generic;
using System.Linq;
using RaptorSheets.Core.Validators;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using Xunit;

namespace RaptorSheets.Core.Tests.Validators
{
    public class SchemaValidatorTests
    {
        [Fact]
        public void ValidateSheet_ShouldReturnError_WhenHeaderRowIsNull()
        {
            // Act
            var result = SchemaValidator.ValidateSheet<TestEntity>(null);

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
        public void ValidateSheetStructure_ShouldReturnError_WhenSheetDataIsEmpty()
        {
            // Act
            var result = SchemaValidator.ValidateSheetStructure<TestEntity>(null);

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
        public void ValidateSheetStructure_ShouldReturnWarning_WhenColumnCountIsInconsistent()
        {
            // Arrange
            var sheetData = new List<IList<object>>
            {
                new List<object> { "Header1", "Header2" },
                new List<object> { "Data1" }
            };

            // Act
            var result = SchemaValidator.ValidateSheetStructure<TestEntity>(sheetData);

            // Assert
            Assert.True(result.HasWarnings);
            Assert.Contains("Row 2 has 1 columns, expected 2", result.Warnings);
        }

        private class TestEntity
        {
            [Column("Header1", FieldTypeEnum.String)]
            public string Header1 { get; set; } = "";

            [Column("Header2", FieldTypeEnum.String)]
            public string Header2 { get; set; } = "";

            [Column("Header3", FieldTypeEnum.String)]
            public string Header3 { get; set; } = "";
        }
    }
}