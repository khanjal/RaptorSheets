using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Gig.Tests.Integration.Base;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Integration;

/// <summary>
/// Setup integration tests that prepare clean testing environment.
/// 
/// These tests demonstrate the CORRECT way to perform full environment resets:
/// 1. Use DeleteAllSheets() to clear ALL sheets
/// 2. Use CreateAllSheets() to recreate all required sheets
/// 3. Verify all sheets exist using GetAllSheetTabNames()
/// 
/// IMPORTANT:
/// - ALL sheets in the spreadsheet are REQUIRED (formulas depend on each other)
/// - Only 3 sheets are used for test data: Trips, Shifts, Expenses
/// - Other sheets (Addresses, Names, Places, Regions, Services, Daily, Weekly, etc.) 
///   contain formulas that aggregate/analyze data from the 3 primary sheets
/// - The TempSheet persists as a safety net (similar to default "Sheet1")
/// 
/// This approach properly handles Google Sheets' requirement of at least one sheet.
/// 
/// Run these manually when you need explicit environment control.
/// Regular integration tests should NOT inline test sheet creation/deletion.
/// </summary>
[Category("IntegrationSetup")]
public class SetupIntegrationTests : IntegrationTestBase
{
    [FactCheckUserSecrets]
    public async Task PrepareCleanTestEnvironment_ShouldResetAllSheets()
    {
        SkipIfNoCredentials();
        
        try
        {
            System.Diagnostics.Debug.WriteLine("🧹 Starting clean environment setup...");
            
            // Step 1: Delete all existing sheets
            System.Diagnostics.Debug.WriteLine("  📌 Deleting all existing sheets...");
            var deleteResult = await GoogleSheetManager!.DeleteAllSheets();
            
            var deleteErrors = deleteResult.Messages.Where(m => 
                m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
            
            if (deleteErrors.Count > 0)
            {
                var errorDetails = string.Join("; ", deleteErrors.Select(e => e.Message));
                throw new SkipException($"Delete failed - environment issue: {errorDetails}");
            }
            
            System.Diagnostics.Debug.WriteLine($"  ✓ Deletion completed");
            
            // Wait for deletion to propagate
            await Task.Delay(3000);
            
            // Step 2: CRITICAL - Verify ALL sheets were deleted before creating new ones
            System.Diagnostics.Debug.WriteLine("  📌 Verifying deletion completed...");
            var sheetsAfterDeletion = await GoogleSheetManager!.GetAllSheetTabNames();
            
            // Should only have TempSheet remaining (safety net)
            var unexpectedSheets = sheetsAfterDeletion
                .Where(sheet => !sheet.Equals("TempSheet", StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            if (unexpectedSheets.Count > 0)
            {
                var remaining = string.Join(", ", unexpectedSheets);
                Assert.Fail($"STOP: Sheets still exist after deletion! Found: {remaining}. " +
                           $"Cannot proceed with CreateAllSheets as it will cause conflicts. " +
                           $"Manual intervention required.");
            }
            
            System.Diagnostics.Debug.WriteLine($"  ✓ Verified only TempSheet remains ({sheetsAfterDeletion.Count} total sheets)");
            
            // Step 3: Create all required sheets
            System.Diagnostics.Debug.WriteLine("  📌 Creating all required sheets...");
            var createResult = await GoogleSheetManager!.CreateAllSheets();
            
            var createErrors = createResult.Messages.Where(m => 
                m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
            
            if (createErrors.Count > 0)
            {
                var errorDetails = string.Join("; ", createErrors.Select(e => e.Message));
                throw new SkipException($"Create failed - environment issue: {errorDetails}");
            }
            
            System.Diagnostics.Debug.WriteLine($"  ✓ Creation completed");
            
            // Wait for creation to complete
            await Task.Delay(3000);
            
            // Step 4: Verify ALL required sheets exist (not just test sheets)
            System.Diagnostics.Debug.WriteLine("  📌 Verifying ALL sheets exist...");
            var allTabNames = await GoogleSheetManager!.GetAllSheetTabNames();
            var allProperties = await GoogleSheetManager!.GetAllSheetProperties();
            
            // Get list of ALL required sheets (from GetAllSheetProperties)
            var requiredSheets = allProperties.Select(p => p.Name).ToList();
            
            System.Diagnostics.Debug.WriteLine($"  📊 Found {allTabNames.Count} total sheets");
            System.Diagnostics.Debug.WriteLine($"  📊 Expected {requiredSheets.Count} required sheets");
            
            // Check that ALL required sheets exist
            var missingSheets = requiredSheets.Where(required => 
                !allTabNames.Any(tab => string.Equals(tab, required, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            
            if (missingSheets.Count > 0)
            {
                var missing = string.Join(", ", missingSheets);
                Assert.Fail($"Missing required sheets: {missing}");
            }
            
            System.Diagnostics.Debug.WriteLine($"  ✓ All {requiredSheets.Count} required sheets exist");
            
            // Log sheet inventory with categorization
            System.Diagnostics.Debug.WriteLine($"\n  📋 Sheet inventory:");
            System.Diagnostics.Debug.WriteLine($"     ✍️  Test Data Sheets (written to by tests):");
            foreach (var testSheet in TestSheets.OrderBy(s => s))
            {
                if (allTabNames.Contains(testSheet, StringComparer.OrdinalIgnoreCase))
                    System.Diagnostics.Debug.WriteLine($"        ✓ {testSheet}");
            }
            
            System.Diagnostics.Debug.WriteLine($"\n     📊 Formula/Analysis Sheets (read-only, contain formulas):");
            var formulaSheets = allTabNames
                .Where(tab => !TestSheets.Contains(tab, StringComparer.OrdinalIgnoreCase) && 
                             !tab.Equals("TempSheet", StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s);
            
            foreach (var formulaSheet in formulaSheets)
            {
                System.Diagnostics.Debug.WriteLine($"        • {formulaSheet}");
            }
            
            if (allTabNames.Any(t => t.Equals("TempSheet", StringComparison.OrdinalIgnoreCase)))
            {
                System.Diagnostics.Debug.WriteLine($"\n     🛡️  Safety Sheet:");
                System.Diagnostics.Debug.WriteLine($"        • TempSheet (persistent safety net)");
            }
            
            System.Diagnostics.Debug.WriteLine("\n✅ Clean test environment prepared successfully");
        }
        catch (SkipException)
        {
            throw;
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
            System.Diagnostics.Debug.WriteLine("🔍 Validating sheet structure...");
            
            // Get ALL sheet properties from the spreadsheet
            var allProperties = await GoogleSheetManager!.GetAllSheetProperties();
            
            System.Diagnostics.Debug.WriteLine($"  📊 Found {allProperties.Count} total sheets in spreadsheet");
            
            // Validate each required test sheet exists and has proper configuration
            foreach (var sheetName in TestSheets)
            {
                var sheetProperty = allProperties.FirstOrDefault(p => 
                    string.Equals(p.Name, sheetName, StringComparison.OrdinalIgnoreCase));
                
                Assert.NotNull(sheetProperty);
                Assert.False(string.IsNullOrEmpty(sheetProperty.Id), 
                    $"Sheet '{sheetName}' should have valid ID");
                
                // Validate headers exist
                if (sheetProperty.Attributes.TryGetValue("Headers", out var headers))
                {
                    Assert.False(string.IsNullOrEmpty(headers), 
                        $"Sheet '{sheetName}' should have headers");
                }
                
                System.Diagnostics.Debug.WriteLine($"  ✓ {sheetName} - ID: {sheetProperty.Id}");
            }
            
            // Log any extra sheets for informational purposes
            var extraSheets = allProperties
                .Where(p => !TestSheets.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .Select(p => p.Name)
                .ToList();
            
            if (extraSheets.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"\n  📋 Additional sheets in spreadsheet:");
                foreach (var extraSheet in extraSheets.OrderBy(s => s))
                {
                    System.Diagnostics.Debug.WriteLine($"     • {extraSheet}");
                }
            }
            
            System.Diagnostics.Debug.WriteLine("\n✅ Sheet structure validation completed");
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
}