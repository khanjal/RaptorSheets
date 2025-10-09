using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Utilities;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;

namespace RaptorSheets.Gig.Tests.Integration.Mappers;

/// <summary>
/// Integration tests for the new ColumnAttribute system using AddressMapper as an example.
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
    public void AddressEntity_HasValidColumnAttributes()
    {
        // Act - Validate that AddressEntity has proper Column attributes
        var validationErrors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<AddressEntity>();

        // Assert
        Assert.Empty(validationErrors);
    }

    [Fact]
    public void AddressEntity_ColumnOrder_RespectsDefinedOrder()
    {
        // Act
        var columnProperties = TypedFieldUtils.GetColumnProperties<AddressEntity>();

        // Assert
        Assert.NotEmpty(columnProperties);
        
        // Verify that Address comes first (entity-specific property)
        Assert.Equal(SheetsConfig.HeaderNames.Address, columnProperties[0].Column.GetEffectiveHeaderName());
        
        // Verify financial headers are present
        var financialHeaders = new[]
        {
            SheetsConfig.HeaderNames.Pay,
            SheetsConfig.HeaderNames.Tips,
            SheetsConfig.HeaderNames.Bonus,
            SheetsConfig.HeaderNames.Total,
            SheetsConfig.HeaderNames.Cash
        };

        var headerNames = columnProperties.Select(p => p.Column.GetEffectiveHeaderName()).ToList();
        foreach (var financialHeader in financialHeaders)
        {
            Assert.Contains(financialHeader, headerNames);
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
    public void EntityColumnAttribute_WithFlattenedEntities_HandlesAllEntityTypes()
    {
        // Test that the new ColumnAttribute system works with flattened entities
        
        // Act - Get column properties using the new system
        var addressProperties = TypedFieldUtils.GetColumnProperties<AddressEntity>();
        var nameProperties = TypedFieldUtils.GetColumnProperties<NameEntity>();
        var tripProperties = TypedFieldUtils.GetColumnProperties<TripEntity>();

        // Assert - Verify entities have Column attributes
        Assert.NotEmpty(addressProperties); // AddressEntity should have Column attributes
        Assert.NotEmpty(nameProperties);    // NameEntity should have Column attributes  
        Assert.NotEmpty(tripProperties);    // TripEntity should have Column attributes

        // Verify that flattened entities contain the same financial headers
        var financialHeaders = new[] 
        { 
            SheetsConfig.HeaderNames.Pay, 
            SheetsConfig.HeaderNames.Tips, 
            SheetsConfig.HeaderNames.Bonus, 
            SheetsConfig.HeaderNames.Total, 
            SheetsConfig.HeaderNames.Cash 
        };
        
        var addressHeaderNames = addressProperties.Select(p => p.Column.GetEffectiveHeaderName()).ToList();
        var nameHeaderNames = nameProperties.Select(p => p.Column.GetEffectiveHeaderName()).ToList();
        var tripHeaderNames = tripProperties.Select(p => p.Column.GetEffectiveHeaderName()).ToList();
        
        foreach (var financialHeader in financialHeaders)
        {
            Assert.Contains(financialHeader, addressHeaderNames);
            Assert.Contains(financialHeader, nameHeaderNames);
            Assert.Contains(financialHeader, tripHeaderNames);
        }

        // Verify specific entity properties
        Assert.Contains(SheetsConfig.HeaderNames.Address, addressHeaderNames);
        Assert.Contains(SheetsConfig.HeaderNames.Name, nameHeaderNames);
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
        var entityProperties = TypedFieldUtils.GetColumnProperties<AddressEntity>();
        var entityHeaderNames = entityProperties.Select(p => p.Column.GetEffectiveHeaderName()).ToList();
        
        var sheet = AddressMapper.GetSheet();
        var finalHeaderNames = sheet.Headers.Select(h => h.Name).ToList();

        // Assert
        // The entity should define headers for AddressEntity (flattened design)
        foreach (var entityHeader in entityHeaderNames)
        {
            Assert.Contains(entityHeader, finalHeaderNames);
        }

        // Key headers should appear with Address first
        var addressIndex = finalHeaderNames.IndexOf(SheetsConfig.HeaderNames.Address);
        var tripsIndex = finalHeaderNames.IndexOf(SheetsConfig.HeaderNames.Trips);
        var payIndex = finalHeaderNames.IndexOf(SheetsConfig.HeaderNames.Pay);

        // Address should be first (entity-specific property)
        Assert.Equal(0, addressIndex);
        Assert.True(tripsIndex > addressIndex, "Trips should come after Address");
    }

    [Fact]
    public void EntityAttributeSystem_IsConsistentAcrossDomainEntities()
    {
        // This test verifies that the Column attribute system is consistently applied
        // across different entity types
        
        // Act
        var addressErrors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<AddressEntity>();
        var nameErrors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<NameEntity>();
        var tripErrors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<TripEntity>();

        // Assert
        Assert.Empty(addressErrors);
        Assert.Empty(nameErrors);
        Assert.Empty(tripErrors);
    }
}