using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Tests.Integration.Mappers;

/// <summary>
/// Integration tests demonstrating the complete entity-driven sheet configuration workflow.
/// Shows how SheetsConfig now generates headers directly from entity ColumnAttribute attributes.
/// </summary>
public class EntityDrivenSheetsConfigIntegrationTests
{
    [Fact]
    public void SheetsConfig_AddressSheet_GeneratesHeadersFromEntity()
    {
        // Act
        var sheet = SheetsConfig.AddressSheet;

        // Assert
        Assert.NotNull(sheet);
        Assert.NotEmpty(sheet.Headers);
        
        // Verify that headers match the entity's ColumnAttribute attributes
        var entityHeaders = EntitySheetConfigHelper.GenerateHeadersFromEntity<AddressEntity>();
        Assert.Equal(entityHeaders.Count, sheet.Headers.Count);
        
        for (int i = 0; i < entityHeaders.Count; i++)
        {
            Assert.Equal(entityHeaders[i].Name, sheet.Headers[i].Name);
        }
    }

    [Fact]
    public void AddressEntity_DefinesCorrectHeaderOrder()
    {
        // Act
        var entityHeaders = EntitySheetConfigHelper.GenerateHeadersFromEntity<AddressEntity>();

        // Assert - AddressEntity now follows flattened CommonTripSheetHeaders pattern
        Assert.Equal(12, entityHeaders.Count); // Address + 11 CommonTripSheetHeaders properties
        
        // Verify flattened order: Address + CommonTripSheetHeaders pattern
        var expectedOrder = new[]
        {
            // Entity-specific property first
            SheetsConfig.HeaderNames.Address,
            // CommonTripSheetHeaders pattern: Trips + CommonIncomeHeaders + CommonTravelHeaders + Visit properties
            SheetsConfig.HeaderNames.Trips,
            SheetsConfig.HeaderNames.Pay,
            SheetsConfig.HeaderNames.Tips,
            SheetsConfig.HeaderNames.Bonus,
            SheetsConfig.HeaderNames.Total,
            SheetsConfig.HeaderNames.Cash,
            SheetsConfig.HeaderNames.AmountPerTrip,
            SheetsConfig.HeaderNames.Distance,
            SheetsConfig.HeaderNames.AmountPerDistance,
            SheetsConfig.HeaderNames.VisitFirst,
            SheetsConfig.HeaderNames.VisitLast
        };

        for (int i = 0; i < expectedOrder.Length; i++)
        {
            Assert.Equal(expectedOrder[i], entityHeaders[i].Name);
        }
    }

    [Fact]
    public void EntityDrivenConfig_EliminatesManualHeaderDefinition()
    {
        // This test demonstrates that we no longer need manual header definition
        // in SheetsConfig - the headers are generated from entity ColumnAttribute attributes
        
        // Before: Manual header definition in SheetsConfig
        // Headers = [
        //     new SheetCellModel { Name = HeaderNames.Address },
        //     .. CommonTripSheetHeaders
        // ]
        
        // After: Entity-driven header generation
        var sheet = SheetsConfig.AddressSheet;
        
        // Verify that all entity properties with ColumnAttribute are included
        var entityHeaders = EntitySheetConfigHelper.GenerateHeadersFromEntity<AddressEntity>();
        var sheetHeaderNames = sheet.Headers.Select(h => h.Name).ToList();
        
        foreach (var entityHeader in entityHeaders)
        {
            Assert.Contains(entityHeader.Name, sheetHeaderNames);
        }
    }

    [Fact]
    public void EntityDrivenConfig_MaintainsBackwardCompatibility()
    {
        // Verify that the new system produces the same functionality as before
        
        // Act
        var sheet = SheetsConfig.AddressSheet;
        sheet.Headers.UpdateColumns();

        // Assert
        Assert.NotNull(sheet);
        Assert.Equal(SheetsConfig.SheetNames.Addresses, sheet.Name);
        Assert.True(sheet.ProtectSheet);
        Assert.Equal(1, sheet.FreezeColumnCount);
        Assert.Equal(1, sheet.FreezeRowCount);
        
        // Verify headers have proper column indexes
        Assert.All(sheet.Headers, header => 
        {
            Assert.True(header.Index >= 0, $"Header '{header.Name}' should have a valid index");
            Assert.False(string.IsNullOrEmpty(header.Column), $"Header '{header.Name}' should have a column letter");
        });
    }

    [Fact]
    public void EntitySheetConfigHelper_ValidatesConsistency()
    {
        // Verify that the helper can validate entity consistency
        
        // Act
        var validationErrors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<AddressEntity>();

        // Assert
        Assert.Empty(validationErrors); // AddressEntity should be valid
    }

    [Fact]
    public void EntitySheetConfigHelper_SupportsCommonPatterns()
    {
        // Demonstrate that the helper can merge with common patterns if needed
        
        // Arrange
        var commonHeaders = new[]
        {
            new RaptorSheets.Core.Models.Google.SheetCellModel { Name = "Extra Header 1" },
            new RaptorSheets.Core.Models.Google.SheetCellModel { Name = "Extra Header 2" }
        };

        // Act
        var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<AddressEntity>(commonHeaders);

        // Assert
        Assert.True(headers.Count >= 11); // At least the entity headers
        Assert.Contains(headers, h => h.Name == "Extra Header 1");
        Assert.Contains(headers, h => h.Name == "Extra Header 2");
        
        // Entity headers should come first - Address is now the first property
        Assert.Equal(SheetsConfig.HeaderNames.Address, headers[0].Name);
    }

    [Fact]
    public void NewApproach_SimplifiesMaintenanceWorkflow()
    {
        // This test documents the simplified workflow:
        // 1. Add ColumnAttribute to entity properties
        // 2. SheetsConfig automatically generates headers
        // 3. No manual header maintenance needed
        
        // Verify entity has ColumnAttribute attributes
        var entityHeaders = EntitySheetConfigHelper.GenerateHeadersFromEntity<AddressEntity>();
        Assert.NotEmpty(entityHeaders);
        
        // Verify SheetsConfig uses these headers
        var configHeaders = SheetsConfig.AddressSheet.Headers;
        Assert.Equal(entityHeaders.Count, configHeaders.Count);
        
        // Verify no manual intervention needed
        for (int i = 0; i < entityHeaders.Count; i++)
        {
            Assert.Equal(entityHeaders[i].Name, configHeaders[i].Name);
        }
    }

    [Fact]
    public void FlattenedEntity_EliminatesInheritanceOrderingIssues()
    {
        // This test demonstrates that flattening entities eliminates inheritance ordering problems
        // where base class properties would appear first regardless of desired column order
        
        // Act
        var entityHeaders = EntitySheetConfigHelper.GenerateHeadersFromEntity<AddressEntity>();
        
        // Assert - Address appears first (entity-specific property)
        Assert.Equal(SheetsConfig.HeaderNames.Address, entityHeaders[0].Name);
        
        // Financial properties appear in the correct position (after Trips, not first)
        var tripsIndex = entityHeaders.FindIndex(h => h.Name == SheetsConfig.HeaderNames.Trips);
        var payIndex = entityHeaders.FindIndex(h => h.Name == SheetsConfig.HeaderNames.Pay);
        
        Assert.True(tripsIndex < payIndex, "Trips should come before Pay in CommonTripSheetHeaders pattern");
        Assert.True(tripsIndex >= 0 && payIndex >= 0, "Both Trips and Pay should be present");
        
        // Verify the pattern: Address, Trips, [financial properties], [travel properties], [visit properties]
        Assert.Equal(SheetsConfig.HeaderNames.Address, entityHeaders[0].Name);
        Assert.Equal(SheetsConfig.HeaderNames.Trips, entityHeaders[1].Name);
        Assert.Equal(SheetsConfig.HeaderNames.Pay, entityHeaders[2].Name);
    }
}