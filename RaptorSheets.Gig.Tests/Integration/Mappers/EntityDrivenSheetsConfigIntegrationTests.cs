using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using Xunit;

namespace RaptorSheets.Gig.Tests.Integration.Mappers;

/// <summary>
/// Integration tests demonstrating the complete entity-driven sheet configuration workflow.
/// Shows how SheetsConfig now generates headers directly from entity SheetOrder attributes.
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
        
        // Verify that headers match the entity's SheetOrder attributes
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

        // Assert
        Assert.Equal(10, entityHeaders.Count); // 5 AmountEntity + 3 VisitEntity + 2 AddressEntity
        
        // Verify inheritance order: AmountEntity ? VisitEntity ? AddressEntity
        var expectedOrder = new[]
        {
            // AmountEntity (base class)
            SheetsConfig.HeaderNames.Pay,
            SheetsConfig.HeaderNames.Tips,
            SheetsConfig.HeaderNames.Bonus,
            SheetsConfig.HeaderNames.Total,
            SheetsConfig.HeaderNames.Cash,
            // VisitEntity (middle class)  
            SheetsConfig.HeaderNames.Trips,
            SheetsConfig.HeaderNames.VisitFirst,
            SheetsConfig.HeaderNames.VisitLast,
            // AddressEntity (derived class)
            SheetsConfig.HeaderNames.Address,
            SheetsConfig.HeaderNames.Distance
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
        // in SheetsConfig - the headers are generated from entity attributes
        
        // Before: Manual header definition in SheetsConfig
        // Headers = [
        //     new SheetCellModel { Name = HeaderNames.Address },
        //     .. CommonTripSheetHeaders
        // ]
        
        // After: Entity-driven header generation
        var sheet = SheetsConfig.AddressSheet;
        
        // Verify that all entity properties with SheetOrder attributes are included
        var entityOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<AddressEntity>();
        var sheetHeaderNames = sheet.Headers.Select(h => h.Name).ToList();
        
        foreach (var entityHeader in entityOrder)
        {
            Assert.Contains(entityHeader, sheetHeaderNames);
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
        Assert.True(headers.Count >= 10); // At least the entity headers
        Assert.Contains(headers, h => h.Name == "Extra Header 1");
        Assert.Contains(headers, h => h.Name == "Extra Header 2");
        
        // Entity headers should come first
        Assert.Equal(SheetsConfig.HeaderNames.Pay, headers[0].Name);
    }

    [Fact]
    public void NewApproach_SimplifiesMaintenanceWorkflow()
    {
        // This test documents the simplified workflow:
        // 1. Add SheetOrder attributes to entity properties
        // 2. SheetsConfig automatically generates headers
        // 3. No manual header maintenance needed
        
        // Verify entity has SheetOrder attributes
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
}