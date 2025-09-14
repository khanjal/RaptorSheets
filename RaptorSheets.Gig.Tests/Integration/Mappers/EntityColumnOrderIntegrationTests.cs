using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Integration.Mappers;

/// <summary>
/// Integration tests for the new SheetOrder attribute system using AddressMapper as an example.
/// These tests verify the complete workflow from entity attributes to sheet configuration.
/// </summary>
public class EntityColumnOrderIntegrationTests
{
    [Fact]
    public void AddressMapper_GetSheet_AppliesEntityDrivenOrdering()
    {
        // Act
        var sheet = AddressMapper.GetSheet();

        // Assert
        Assert.NotNull(sheet);
        Assert.NotEmpty(sheet.Headers);
        
        // Verify headers were updated with column indexes
        Assert.All(sheet.Headers, header => 
        {
            Assert.True(header.Index >= 0, $"Header '{header.Name}' should have a valid index");
            Assert.False(string.IsNullOrEmpty(header.Column), $"Header '{header.Name}' should have a column letter");
        });
    }

    [Fact]
    public void AddressEntity_HasValidSheetOrderAttributes()
    {
        // Arrange - Get all available header constants from SheetsConfig
        var availableHeaders = typeof(SheetsConfig.HeaderNames)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToList();

        // Act
        var validationErrors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<AddressEntity>(availableHeaders);

        // Assert
        Assert.Empty(validationErrors);
    }

    [Fact]
    public void AddressEntity_ColumnOrder_RespectsInheritanceHierarchy()
    {
        // Act
        var columnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<AddressEntity>();

        // Assert
        Assert.NotEmpty(columnOrder);
        
        // Verify inheritance order: AmountEntity ? VisitEntity ? AddressEntity
        var expectedBaseOrder = new[]
        {
            SheetsConfig.HeaderNames.Pay,       // AmountEntity
            SheetsConfig.HeaderNames.Tips,      // AmountEntity
            SheetsConfig.HeaderNames.Bonus,     // AmountEntity
            SheetsConfig.HeaderNames.Total,     // AmountEntity
            SheetsConfig.HeaderNames.Cash,      // AmountEntity
            SheetsConfig.HeaderNames.Trips,     // VisitEntity
            SheetsConfig.HeaderNames.VisitFirst, // VisitEntity
            SheetsConfig.HeaderNames.VisitLast,  // VisitEntity
            SheetsConfig.HeaderNames.Address,   // AddressEntity
            SheetsConfig.HeaderNames.Distance   // AddressEntity
        };

        Assert.Equal(expectedBaseOrder.Length, columnOrder.Count);
        for (int i = 0; i < expectedBaseOrder.Length; i++)
        {
            Assert.Equal(expectedBaseOrder[i], columnOrder[i]);
        }
    }

    [Fact]
    public void AddressMapper_GetSheet_WithEntityOrdering_DoesNotThrow()
    {
        // This test ensures the integration works without exceptions
        
        // Act & Assert - Should not throw any exceptions
        var exception = Record.Exception(() => AddressMapper.GetSheet());
        
        Assert.Null(exception);
    }

    [Fact]
    public void AddressMapper_GetSheet_PreservesExistingFunctionality()
    {
        // This test ensures that the new entity ordering doesn't break existing mapper functionality
        
        // Act
        var sheet = AddressMapper.GetSheet();

        // Assert
        Assert.NotNull(sheet);
        Assert.Equal(SheetsConfig.SheetNames.Addresses, sheet.Name);
        Assert.True(sheet.ProtectSheet);
        Assert.Equal(1, sheet.FreezeColumnCount);
        Assert.Equal(1, sheet.FreezeRowCount);
        
        // Verify some headers have formulas configured (existing functionality)
        var headersWithFormulas = sheet.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).ToList();
        Assert.NotEmpty(headersWithFormulas);
        
        // Verify specific formula configuration for address-specific headers
        var addressHeader = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.Address);
        Assert.NotNull(addressHeader);
        
        var tripsHeader = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.Trips);
        Assert.NotNull(tripsHeader);
        
        var firstTripHeader = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.VisitFirst);
        Assert.NotNull(firstTripHeader);
        Assert.NotEmpty(firstTripHeader.Formula);
    }

    [Fact]
    public void EntityColumnOrderHelper_WithComplexInheritance_HandlesAllEntityTypes()
    {
        // Test that the helper works with various entity types in the inheritance chain
        
        // Act & Assert - None of these should throw
        var amountOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<AmountEntity>();
        var visitOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<VisitEntity>();
        var addressOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<AddressEntity>();

        // Verify inheritance progression
        Assert.Equal(5, amountOrder.Count);  // 5 financial properties
        Assert.Equal(8, visitOrder.Count);   // 5 financial + 3 visit properties  
        Assert.Equal(10, addressOrder.Count); // 5 financial + 3 visit + 2 address properties

        // Verify base properties appear in all derived types
        foreach (var baseHeader in amountOrder)
        {
            Assert.Contains(baseHeader, visitOrder);
            Assert.Contains(baseHeader, addressOrder);
        }

        foreach (var visitHeader in visitOrder)
        {
            Assert.Contains(visitHeader, addressOrder);
        }
    }

    [Fact]
    public void AddressMapper_ColumnOrder_MatchesSheetConfiguration()
    {
        // This test verifies that the entity-driven ordering produces consistent results
        // with the sheet configuration
        
        // Arrange
        var originalSheet = SheetsConfig.AddressSheet;
        var originalHeaderNames = originalSheet.Headers.Select(h => h.Name).ToList();

        // Act
        var entityOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<AddressEntity>();
        var sheet = AddressMapper.GetSheet();
        var finalHeaderNames = sheet.Headers.Select(h => h.Name).ToList();

        // Assert
        // The entity should define at least the core headers, but the sheet may have additional ones
        foreach (var entityHeader in entityOrder)
        {
            Assert.Contains(entityHeader, finalHeaderNames);
        }

        // Key entity headers should appear in the expected order
        var addressIndex = finalHeaderNames.IndexOf(SheetsConfig.HeaderNames.Address);
        var payIndex = finalHeaderNames.IndexOf(SheetsConfig.HeaderNames.Pay);
        var distanceIndex = finalHeaderNames.IndexOf(SheetsConfig.HeaderNames.Distance);

        Assert.True(payIndex < addressIndex, "AmountEntity properties should come before AddressEntity properties");
        Assert.True(addressIndex < distanceIndex, "Address should come before Distance within AddressEntity properties");
    }

    [Fact]
    public void EntityAttributeSystem_IsConsistentAcrossDomainEntities()
    {
        // This test verifies that the attribute system is consistently applied
        // across different entity types that might be used in other mappers
        
        // Act
        var amountErrors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<AmountEntity>(
            GetAllAvailableHeaders());
        var visitErrors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<VisitEntity>(
            GetAllAvailableHeaders());
        var addressErrors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<AddressEntity>(
            GetAllAvailableHeaders());

        // Assert
        Assert.Empty(amountErrors);
        Assert.Empty(visitErrors); 
        Assert.Empty(addressErrors);
    }

    private static List<string> GetAllAvailableHeaders()
    {
        return typeof(SheetsConfig.HeaderNames)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToList();
    }
}