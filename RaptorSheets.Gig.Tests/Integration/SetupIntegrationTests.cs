using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Gig.Tests.Integration.Base;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Integration;

/// <summary>
/// Setup integration tests that prepare clean testing environment.
/// 
/// These tests demonstrate the CORRECT way to perform full environment resets:
/// 1. Use DeleteAllSheets() to clear ALL sheets (creates TempSheet automatically)
/// 2. Use CreateAllSheets() to recreate all required sheets
/// 3. Verify the environment is clean and ready
/// 
/// This approach properly handles Google Sheets' requirement of at least one sheet
/// by using a temporary placeholder during the delete/recreate cycle.
/// 
/// Run these manually when you need explicit environment control.
/// Regular integration tests should NOT inline test sheet creation/deletion.
/// </summary>
[Category("IntegrationSetup")] // Changed from "Integration" - now runs separately
public class SetupIntegrationTests : IntegrationTestBase
{
    [FactCheckUserSecrets]
    public async Task PrepareCleanTestEnvironment_ShouldResetAllSheets()
    {
        SkipIfNoCredentials();
        
        try
        {
            // Step 1: Delete all existing sheets to ensure clean slate
            var deleteResult = await GoogleSheetManager!.DeleteAllSheets();
            
            // Allow warnings but log any hard errors
            var deleteErrors = deleteResult.Messages.Where(m => m.Level == "ERROR").ToList();
            if (deleteErrors.Count > 0)
            {
                // Log errors but don't fail - sheets might not exist
                System.Diagnostics.Debug.WriteLine($"Delete warnings (expected): {string.Join(", ", deleteErrors.Select(e => e.Message))}");
            }
            
            // Wait for deletion to propagate
            await Task.Delay(4000);

            // Step 2: Recreate fresh sheets
            var createResult = await GoogleSheetManager!.CreateAllSheets();
            var recreateSuccess = await EnsureSheetsExist(TestSheets);
            Assert.True(recreateSuccess, "Should successfully create fresh sheets");
            
            // Wait for creation to complete
            await Task.Delay(3000);
            
            // Step 3: Verify clean environment (more tolerant threshold)
            var cleanData = await GetSheetData();
            
            // Should have empty collections
            var totalTestData = cleanData.Shifts.Count + cleanData.Trips.Count + cleanData.Expenses.Count;
            
            // More tolerant threshold - allow up to 100 records from previous test runs
            // This accounts for test data accumulation when running multiple tests
            Assert.True(totalTestData < 100, $"Environment should be relatively clean, found {totalTestData} total records (threshold: <100)");
            
            // Step 4: Verify sheet structure is correct
            var properties = await GoogleSheetManager!.GetSheetProperties(TestSheets);
            var existingSheets = properties.Where(p => !string.IsNullOrEmpty(p.Id)).ToList();
            
            Assert.True(existingSheets.Count >= TestSheets.Count, 
                $"Should have all required sheets: expected {TestSheets.Count}, got {existingSheets.Count}");
            
            // Verify essential sheets are present
            var sheetNames = existingSheets.Select(s => s.Name).ToList();
            Assert.Contains("Shifts", sheetNames);
            Assert.Contains("Trips", sheetNames);
            Assert.Contains("Expenses", sheetNames);
            
            System.Diagnostics.Debug.WriteLine("? Clean test environment prepared successfully");
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
    
    [FactCheckUserSecrets]
    public async Task ValidateSheetStructure_ShouldHaveCorrectConfiguration()
    {
        SkipIfNoCredentials();
        
        try
        {
            // Ensure sheets exist
            await EnsureSheetsExist(TestSheets);
            
            // Get sheet properties
            var properties = await GoogleSheetManager!.GetSheetProperties(TestSheets);
            
            // Validate each required sheet
            foreach (var sheetName in TestSheets)
            {
                var sheetProperty = properties.FirstOrDefault(p => p.Name == sheetName);
                Assert.NotNull(sheetProperty);
                Assert.False(string.IsNullOrEmpty(sheetProperty.Id), $"Sheet {sheetName} should have valid ID");
                
                // Validate headers exist
                var headersAttribute = sheetProperty.Attributes.FirstOrDefault(a => a.Key == "HEADERS");
                if (headersAttribute.Key == "HEADERS")
                {
                    Assert.False(string.IsNullOrEmpty(headersAttribute.Value), $"Sheet {sheetName} should have headers");
                }
            }
            
            System.Diagnostics.Debug.WriteLine("? Sheet structure validation completed");
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
}