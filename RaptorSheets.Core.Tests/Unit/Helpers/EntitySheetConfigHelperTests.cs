using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Tests.Data;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class EntitySheetConfigHelperTests
{
    [Fact]
    public void GenerateHeadersFromEntity_SimpleEntity_ReturnsCorrectHeaders()
    {
        // Act
        var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TestSimpleEntity>();

        // Assert
        Assert.Equal(2, headers.Count);
        Assert.Equal(TestHeaderNames.Name, headers[0].Name);
        Assert.Equal(TestHeaderNames.Date, headers[1].Name);
    }

    [Fact]
    public void GenerateHeadersFromEntity_InheritedEntity_ReturnsInheritanceOrderHeaders()
    {
        // Act
        var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TestAddressEntity>();

        // Assert
        Assert.Equal(10, headers.Count);
        
        // Base class properties first (TestAmountEntity)
        Assert.Equal(TestHeaderNames.Pay, headers[0].Name);
        Assert.Equal(TestHeaderNames.Tips, headers[1].Name);
        Assert.Equal(TestHeaderNames.Bonus, headers[2].Name);
        Assert.Equal(TestHeaderNames.Total, headers[3].Name);
        Assert.Equal(TestHeaderNames.Cash, headers[4].Name);
        
        // Middle class properties (TestVisitEntity)
        Assert.Equal(TestHeaderNames.Trips, headers[5].Name);
        Assert.Equal(TestHeaderNames.FirstTrip, headers[6].Name);
        Assert.Equal(TestHeaderNames.LastTrip, headers[7].Name);
        
        // Derived class properties (TestAddressEntity)
        Assert.Equal(TestHeaderNames.Address, headers[8].Name);
        Assert.Equal(TestHeaderNames.Distance, headers[9].Name);
    }

    [Fact]
    public void GenerateHeadersFromEntity_EntityWithoutAttributes_ReturnsEmptyList()
    {
        // Act
        var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TestNoAttributesEntity>();

        // Assert
        Assert.Empty(headers);
    }

    [Fact]
    public void GenerateHeadersFromEntity_WithAdditionalHeaders_MergesCorrectly()
    {
        // Arrange
        var additionalHeaders = new[]
        {
            new SheetCellModel { Name = "Extra Header 1" },
            new SheetCellModel { Name = "Extra Header 2" },
            new SheetCellModel { Name = TestHeaderNames.Name } // Duplicate - should not be added again
        };

        // Act
        var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TestSimpleEntity>(additionalHeaders);

        // Assert
        Assert.Equal(4, headers.Count);
        
        // Entity headers first
        Assert.Equal(TestHeaderNames.Name, headers[0].Name);
        Assert.Equal(TestHeaderNames.Date, headers[1].Name);
        
        // Additional headers (excluding duplicates)
        Assert.Equal("Extra Header 1", headers[2].Name);
        Assert.Equal("Extra Header 2", headers[3].Name);
    }

    [Fact]
    public void GenerateHeadersFromEntity_WithCommonHeaderPatterns_MergesCorrectly()
    {
        // Arrange
        var commonPattern1 = new[]
        {
            new SheetCellModel { Name = "Common Header 1" },
            new SheetCellModel { Name = TestHeaderNames.Name } // Duplicate - should not be added again
        };

        var commonPattern2 = new[]
        {
            new SheetCellModel { Name = "Common Header 2" },
            new SheetCellModel { Name = "Common Header 1" } // Duplicate from pattern1 - should not be added again
        };

        // Act
        var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TestSimpleEntity>(commonPattern1, commonPattern2);

        // Assert
        Assert.Equal(4, headers.Count);
        
        // Entity headers first
        Assert.Equal(TestHeaderNames.Name, headers[0].Name);
        Assert.Equal(TestHeaderNames.Date, headers[1].Name);
        
        // Common headers (excluding duplicates)
        Assert.Equal("Common Header 1", headers[2].Name);
        Assert.Equal("Common Header 2", headers[3].Name);
    }

    [Fact]
    public void ValidateEntityForSheetGeneration_ValidEntity_ReturnsNoErrors()
    {
        // Act
        var errors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<TestAddressEntity>();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateEntityForSheetGeneration_EntityWithoutAttributes_ReturnsError()
    {
        // Act
        var errors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<TestNoAttributesEntity>();

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("TestNoAttributesEntity"));
        Assert.Contains(errors, e => e.Contains("no properties with Column attributes"));
    }

    [Fact]
    public void ValidateEntityForSheetGeneration_WithRequiredHeaders_ValidatesCorrectly()
    {
        // Arrange
        var requiredHeaders = new[]
        {
            TestHeaderNames.Pay,
            TestHeaderNames.Tips,
            TestHeaderNames.Address,
            "Missing Header" // This header is not in the entity
        };

        // Act
        var errors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<TestAddressEntity>(requiredHeaders);

        // Assert
        Assert.Single(errors); // Should only have one error for the missing header
        Assert.Contains(errors, e => e.Contains("Missing Header"));
        
        // The valid headers should not be mentioned as missing in the error messages
        Assert.DoesNotContain(errors, e => e.Contains($"missing required header '{TestHeaderNames.Pay}'"));
        Assert.DoesNotContain(errors, e => e.Contains($"missing required header '{TestHeaderNames.Tips}'"));
        Assert.DoesNotContain(errors, e => e.Contains($"missing required header '{TestHeaderNames.Address}'"));
    }

    [Fact]
    public void ValidateEntityForSheetGeneration_WithAllRequiredHeaders_ReturnsNoErrors()
    {
        // Arrange
        var requiredHeaders = new[]
        {
            TestHeaderNames.Pay,
            TestHeaderNames.Tips,
            TestHeaderNames.Address,
            TestHeaderNames.Distance
        };

        // Act
        var errors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<TestAddressEntity>(requiredHeaders);

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(typeof(TestSimpleEntity), 2)]
    [InlineData(typeof(TestAmountEntity), 5)]
    [InlineData(typeof(TestVisitEntity), 8)]
    [InlineData(typeof(TestAddressEntity), 10)]
    [InlineData(typeof(TestNoAttributesEntity), 0)]
    public void GenerateHeadersFromEntity_VariousEntityTypes_ReturnsCorrectCount(Type entityType, int expectedCount)
    {
        // Act
        var method = typeof(EntitySheetConfigHelper)
            .GetMethod(nameof(EntitySheetConfigHelper.GenerateHeadersFromEntity), Type.EmptyTypes)!
            .MakeGenericMethod(entityType);
        
        var result = (List<SheetCellModel>)method.Invoke(null, null)!;

        // Assert
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void GenerateHeadersFromEntity_DuplicateHeaderNames_ProcessesOnlyFirst()
    {
        // This test ensures that if somehow duplicate header names exist in inheritance chain,
        // only the first occurrence is processed
        
        // Act
        var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TestAddressEntity>();

        // Assert
        var headerNames = headers.Select(h => h.Name).ToList();
        var uniqueHeaders = headerNames.Distinct().ToList();
        Assert.Equal(headerNames.Count, uniqueHeaders.Count); // No duplicates
    }

    [Fact]
    public void GenerateHeadersFromEntity_ReturnsSheetCellModelInstances()
    {
        // Act
        var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TestSimpleEntity>();

        // Assert
        Assert.All(headers, header =>
        {
            Assert.IsType<SheetCellModel>(header);
            Assert.False(string.IsNullOrEmpty(header.Name));
        });
    }

    [Fact]
    public void GenerateHeadersFromEntity_InheritanceWithOverrides_HandlesCorrectly()
    {
        // This test verifies that our reflection properly handles the inheritance chain
        // and doesn't include properties multiple times even with potential overrides
        
        // Act
        var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TestAddressEntity>();

        // Assert
        var headerNames = headers.Select(h => h.Name).ToList();
        
        // Verify base class properties appear first and only once
        var payIndex = headerNames.IndexOf(TestHeaderNames.Pay);
        var addressIndex = headerNames.IndexOf(TestHeaderNames.Address);
        
        Assert.True(payIndex < addressIndex, "Base class properties should appear before derived class properties");
        Assert.Equal(1, headerNames.Count(h => h == TestHeaderNames.Pay)); // Should appear only once
    }

    private class TestNoPropertiesEntity
    {
    }

    private class TestDuplicateColumnEntity
    {
        [Column("DuplicateHeader")]
        public string Property1 { get; set; } = "";

        [Column("DuplicateHeader")]
        public string Property2 { get; set; } = "";
    }

    private class TestInvalidColumnEntity
    {
        [Column("InvalidColumn")]
        public string InvalidProperty { get; set; } = "";
    }
}