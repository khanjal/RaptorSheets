using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Tests.Data;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class EntityColumnOrderHelperTests
{
    [Fact]
    public void GetColumnOrderFromEntity_SimpleEntity_ReturnsCorrectOrder()
    {
        // Act
        var columnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<TestSimpleEntity>();

        // Assert
        Assert.Equal(2, columnOrder.Count);
        Assert.Equal(TestHeaderNames.Name, columnOrder[0]);
        Assert.Equal(TestHeaderNames.Date, columnOrder[1]);
    }

    [Fact]
    public void GetColumnOrderFromEntity_InheritedEntity_ReturnsInheritanceOrder()
    {
        // Act
        var columnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<TestAddressEntity>();

        // Assert
        Assert.Equal(10, columnOrder.Count);
        
        // Base class properties first (TestAmountEntity)
        Assert.Equal(TestHeaderNames.Pay, columnOrder[0]);
        Assert.Equal(TestHeaderNames.Tips, columnOrder[1]);
        Assert.Equal(TestHeaderNames.Bonus, columnOrder[2]);
        Assert.Equal(TestHeaderNames.Total, columnOrder[3]);
        Assert.Equal(TestHeaderNames.Cash, columnOrder[4]);
        
        // Middle class properties (TestVisitEntity)
        Assert.Equal(TestHeaderNames.Trips, columnOrder[5]);
        Assert.Equal(TestHeaderNames.FirstTrip, columnOrder[6]);
        Assert.Equal(TestHeaderNames.LastTrip, columnOrder[7]);
        
        // Derived class properties (TestAddressEntity)
        Assert.Equal(TestHeaderNames.Address, columnOrder[8]);
        Assert.Equal(TestHeaderNames.Distance, columnOrder[9]);
    }

    [Fact]
    public void GetColumnOrderFromEntity_EntityWithoutAttributes_ReturnsEmptyList()
    {
        // Act
        var columnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<TestNoAttributesEntity>();

        // Assert
        Assert.Empty(columnOrder);
    }

    [Fact]
    public void GetColumnOrderFromEntity_WithSheetHeaders_ReordersHeaders()
    {
        // Arrange - Create headers in "wrong" order
        var sheetHeaders = new List<SheetCellModel>
        {
            new() { Name = TestHeaderNames.Distance },    // Should be last
            new() { Name = TestHeaderNames.Address },     // Should be 9th
            new() { Name = TestHeaderNames.Pay },         // Should be first
            new() { Name = TestHeaderNames.Total },       // Should be 4th
            new() { Name = TestHeaderNames.Tips },        // Should be 2nd
        };

        // Act - Apply entity column order to reorder the headers
        EntityColumnOrderHelper.ApplyEntityColumnOrder<TestAddressEntity>(sheetHeaders);

        // Assert - The headers should be reordered to match entity order
        Assert.Equal(5, sheetHeaders.Count);
        Assert.Equal(TestHeaderNames.Pay, sheetHeaders[0].Name);
        Assert.Equal(TestHeaderNames.Tips, sheetHeaders[1].Name);
        Assert.Equal(TestHeaderNames.Total, sheetHeaders[2].Name);
        Assert.Equal(TestHeaderNames.Address, sheetHeaders[3].Name);
        Assert.Equal(TestHeaderNames.Distance, sheetHeaders[4].Name);
        
        // Also test that GetColumnOrderFromEntity returns the full entity order
        var columnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<TestAddressEntity>();
        Assert.Equal(10, columnOrder.Count); // All entity properties with ColumnOrder attributes
    }

    [Fact]
    public void GetColumnOrderFromEntity_WithExtraHeaders_PreservesUnmappedHeaders()
    {
        // Arrange - Include headers not in entity
        var sheetHeaders = new List<SheetCellModel>
        {
            new() { Name = TestHeaderNames.Pay },
            new() { Name = "Unmapped Header 1" },  // Not in entity
            new() { Name = TestHeaderNames.Tips },
            new() { Name = "Unmapped Header 2" },  // Not in entity
        };

        // Act - Apply entity column order
        EntityColumnOrderHelper.ApplyEntityColumnOrder<TestAmountEntity>(sheetHeaders);

        // Assert - ApplyEntityColumnOrder includes all headers but reorders them
        // Entity headers come first in entity order, unmapped headers are included but may be filtered
        Assert.Equal(4, sheetHeaders.Count); // All headers should be preserved
        Assert.Equal(TestHeaderNames.Pay, sheetHeaders[0].Name);
        Assert.Equal(TestHeaderNames.Tips, sheetHeaders[1].Name);
        // Unmapped headers should still be there but order may vary
        Assert.Contains(sheetHeaders, h => h.Name == "Unmapped Header 1");
        Assert.Contains(sheetHeaders, h => h.Name == "Unmapped Header 2");
        
        // Test GetColumnOrderFromEntity separately to see how it handles unmapped headers
        var originalHeaders = new List<SheetCellModel>
        {
            new() { Name = TestHeaderNames.Pay },
            new() { Name = "Unmapped Header 1" },
            new() { Name = TestHeaderNames.Tips },
            new() { Name = "Unmapped Header 2" },
        };
        
        var columnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<TestAmountEntity>(originalHeaders);
        
        // GetColumnOrderFromEntity includes entity headers first, then unmapped headers in original order
        Assert.Equal(7, columnOrder.Count); // 5 entity headers + 2 unmapped
        Assert.Equal(TestHeaderNames.Pay, columnOrder[0]);
        Assert.Equal(TestHeaderNames.Tips, columnOrder[1]);
        Assert.Contains("Unmapped Header 1", columnOrder);
        Assert.Contains("Unmapped Header 2", columnOrder);
    }

    [Fact]
    public void GetColumnOrderFromEntity_WithFallbackOrder_UsesProvidedFallback()
    {
        // Arrange
        var sheetHeaders = new List<SheetCellModel>
        {
            new() { Name = TestHeaderNames.Pay },
            new() { Name = "Unmapped 2" },
            new() { Name = TestHeaderNames.Tips },
            new() { Name = "Unmapped 1" },
        };
        
        var additionalHeaders = new List<SheetCellModel>
        {
            new() { Name = "Unmapped 1" },
            new() { Name = "Unmapped 2" }
        };

        // Act
        var columnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<TestAmountEntity>(sheetHeaders, additionalHeaders);

        // Assert - Entity order first, then additional headers
        Assert.Contains(TestHeaderNames.Pay, columnOrder);
        Assert.Contains(TestHeaderNames.Tips, columnOrder);
        Assert.Contains("Unmapped 1", columnOrder);
        Assert.Contains("Unmapped 2", columnOrder);
        
        var payIndex = columnOrder.IndexOf(TestHeaderNames.Pay);
        var tipsIndex = columnOrder.IndexOf(TestHeaderNames.Tips);
        var unmapped1Index = columnOrder.IndexOf("Unmapped 1");
        
        Assert.True(payIndex < tipsIndex, "Entity headers should be in entity order");
        Assert.True(tipsIndex < unmapped1Index, "Entity headers should come before additional headers");
    }

    [Fact]
    public void ValidateEntityHeaderMapping_ValidEntity_ReturnsNoErrors()
    {
        // Arrange
        var availableHeaders = new[]
        {
            TestHeaderNames.Pay,
            TestHeaderNames.Tips,
            TestHeaderNames.Bonus,
            TestHeaderNames.Total,
            TestHeaderNames.Cash,
            TestHeaderNames.Trips,
            TestHeaderNames.FirstTrip,
            TestHeaderNames.LastTrip,
            TestHeaderNames.Address,
            TestHeaderNames.Distance
        };

        // Act
        var errors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<TestAddressEntity>(availableHeaders);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateEntityHeaderMapping_InvalidHeaders_ReturnsErrors()
    {
        // Arrange - Limited available headers (missing some that entity uses)
        var availableHeaders = new[]
        {
            TestHeaderNames.Pay,
            TestHeaderNames.Tips
            // Missing other headers that TestAddressEntity uses
        };

        // Act
        var errors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<TestAddressEntity>(availableHeaders);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Bonus"));
        Assert.Contains(errors, e => e.Contains("Total"));
        Assert.Contains(errors, e => e.Contains("Cash"));
        Assert.Contains(errors, e => e.Contains("Trips"));
        Assert.Contains(errors, e => e.Contains("Address"));
        Assert.Contains(errors, e => e.Contains("Distance"));
    }

    [Fact]
    public void ValidateEntityHeaderMapping_EntityWithInvalidReference_ReturnsError()
    {
        // Arrange
        var availableHeaders = new[]
        {
            TestHeaderNames.Name
            // "Invalid Header Name" is not in available headers
        };

        // Act
        var errors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<TestInvalidEntity>(availableHeaders);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("InvalidProperty"));
        Assert.Contains(errors, e => e.Contains("Invalid Header Name"));
        Assert.Contains(errors, e => e.Contains("SheetsConfig.HeaderNames"));
    }

    [Fact]
    public void ValidateEntityHeaderMapping_EntityWithoutAttributes_ReturnsNoErrors()
    {
        // Arrange
        var availableHeaders = new[] { "Any Header" };

        // Act
        var errors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<TestNoAttributesEntity>(availableHeaders);

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(typeof(TestSimpleEntity), 2)]
    [InlineData(typeof(TestAmountEntity), 5)]
    [InlineData(typeof(TestVisitEntity), 8)]
    [InlineData(typeof(TestAddressEntity), 10)]
    [InlineData(typeof(TestNoAttributesEntity), 0)]
    public void GetColumnOrderFromEntity_VariousEntityTypes_ReturnsCorrectCount(Type entityType, int expectedCount)
    {
        // Act
        var method = typeof(EntityColumnOrderHelper)
            .GetMethod(nameof(EntityColumnOrderHelper.GetColumnOrderFromEntity))!
            .MakeGenericMethod(entityType);
        
        var result = (List<string>)method.Invoke(null, new object?[] { null, null })!;

        // Assert
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void GetColumnOrderFromEntity_DuplicateHeaderNames_ProcessesOnlyFirst()
    {
        // This test ensures that if somehow duplicate header names exist in inheritance chain,
        // only the first occurrence is processed
        
        // Act
        var columnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<TestAddressEntity>();

        // Assert
        var uniqueHeaders = columnOrder.Distinct().ToList();
        Assert.Equal(columnOrder.Count, uniqueHeaders.Count); // No duplicates
    }

    [Fact]
    public void GetColumnOrderFromEntity_NullSheetHeaders_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        var columnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<TestSimpleEntity>(null, null);
        
        Assert.Equal(2, columnOrder.Count);
    }

    [Fact]
    public void GetColumnOrderFromEntity_EmptySheetHeaders_ReturnsEntityOrder()
    {
        // Arrange
        var emptyHeaders = new List<SheetCellModel>();

        // Act
        var columnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<TestSimpleEntity>(emptyHeaders);

        // Assert
        Assert.Equal(2, columnOrder.Count);
        Assert.Equal(TestHeaderNames.Name, columnOrder[0]);
        Assert.Equal(TestHeaderNames.Date, columnOrder[1]);
        Assert.Empty(emptyHeaders); // Should remain empty
    }

    [Fact]
    public void ValidateEntityHeaderMapping_EmptyAvailableHeaders_ReturnsErrorsForAllEntityHeaders()
    {
        // Arrange
        var emptyHeaders = Array.Empty<string>();

        // Act
        var errors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<TestSimpleEntity>(emptyHeaders);

        // Assert
        Assert.Equal(2, errors.Count); // One for each entity property with ColumnOrder
        Assert.Contains(errors, e => e.Contains("Name"));
        Assert.Contains(errors, e => e.Contains("Date"));
    }

    [Fact]
    public void GetColumnOrderFromEntity_InheritanceWithOverrides_HandlesCorrectly()
    {
        // This test verifies that our reflection properly handles the inheritance chain
        // and doesn't include properties multiple times even with potential overrides
        
        // Act
        var columnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<TestAddressEntity>();

        // Assert
        // Verify base class properties appear first and only once
        var payIndex = columnOrder.IndexOf(TestHeaderNames.Pay);
        var addressIndex = columnOrder.IndexOf(TestHeaderNames.Address);
        
        Assert.True(payIndex < addressIndex, "Base class properties should appear before derived class properties");
        Assert.Equal(1, columnOrder.Count(h => h == TestHeaderNames.Pay)); // Should appear only once
    }
}