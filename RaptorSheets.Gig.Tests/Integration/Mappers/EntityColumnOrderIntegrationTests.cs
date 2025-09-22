using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;

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
    public void AddressEntity_HasValidColumnOrderAttributes()
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
        
        // Verify flattened order: Address + CommonTripSheetHeaders pattern
        // AddressEntity no longer uses inheritance - it defines all properties directly
        var expectedOrder = new[]
        {
            SheetsConfig.HeaderNames.Address,    // Entity-specific property first
            SheetsConfig.HeaderNames.Trips,     // CommonTripSheetHeaders start
            SheetsConfig.HeaderNames.Pay,       // CommonIncomeHeaders
            SheetsConfig.HeaderNames.Tips,      // CommonIncomeHeaders
            SheetsConfig.HeaderNames.Bonus,     // CommonIncomeHeaders
            SheetsConfig.HeaderNames.Total,     // CommonIncomeHeaders
            SheetsConfig.HeaderNames.Cash,      // CommonIncomeHeaders
            SheetsConfig.HeaderNames.AmountPerTrip,     // CommonTravelHeaders
            SheetsConfig.HeaderNames.Distance,          // CommonTravelHeaders
            SheetsConfig.HeaderNames.AmountPerDistance, // CommonTravelHeaders
            SheetsConfig.HeaderNames.VisitFirst,        // Visit properties
            SheetsConfig.HeaderNames.VisitLast          // Visit properties
        };

        Assert.Equal(expectedOrder.Length, columnOrder.Count);
        for (int i = 0; i < expectedOrder.Length; i++)
        {
            Assert.Equal(expectedOrder[i], columnOrder[i]);
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
    public void EntityColumnOrderHelper_WithFlattenedEntities_HandlesAllEntityTypes()
    {
        // Test that the helper works with flattened entities (no more inheritance)
        
        // Act & Assert - None of these should throw
        var addressOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<AddressEntity>();
        var nameOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<NameEntity>();
        var tripOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<TripEntity>();

        // Verify entity property counts for flattened entities
        Assert.Equal(12, addressOrder.Count); // AddressEntity: 1 address + 11 other properties
        Assert.Equal(12, nameOrder.Count);    // NameEntity: 1 name + 11 other properties (same pattern)
        Assert.True(tripOrder.Count >= 20);   // TripEntity has many properties

        // Verify that flattened entities contain the same financial headers
        var financialHeaders = new[] 
        { 
            SheetsConfig.HeaderNames.Pay, 
            SheetsConfig.HeaderNames.Tips, 
            SheetsConfig.HeaderNames.Bonus, 
            SheetsConfig.HeaderNames.Total, 
            SheetsConfig.HeaderNames.Cash 
        };
        
        foreach (var financialHeader in financialHeaders)
        {
            Assert.Contains(financialHeader, addressOrder);
            Assert.Contains(financialHeader, nameOrder);
            Assert.Contains(financialHeader, tripOrder);
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
        // The entity should define all the headers for AddressEntity (flattened design)
        foreach (var entityHeader in entityOrder)
        {
            Assert.Contains(entityHeader, finalHeaderNames);
        }

        // Key headers should appear in the correct flattened order
        var addressIndex = finalHeaderNames.IndexOf(SheetsConfig.HeaderNames.Address);
        var tripsIndex = finalHeaderNames.IndexOf(SheetsConfig.HeaderNames.Trips);
        var payIndex = finalHeaderNames.IndexOf(SheetsConfig.HeaderNames.Pay);
        var distanceIndex = finalHeaderNames.IndexOf(SheetsConfig.HeaderNames.Distance);

        // AddressEntity follows: Address, Trips, Pay..., Distance...
        Assert.True(addressIndex < tripsIndex, "Address should come before Trips");
        Assert.True(tripsIndex < payIndex, "Trips should come before Pay in CommonTripSheetHeaders pattern");
        Assert.True(payIndex < distanceIndex, "Pay should come before Distance");
    }

    [Fact]
    public void EntityAttributeSystem_IsConsistentAcrossDomainEntities()
    {
        // This test verifies that the attribute system is consistently applied
        // across different entity types that might be used in other mappers
        
        // Act
        var addressErrors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<AddressEntity>(
            GetAllAvailableHeaders());

        // Assert
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