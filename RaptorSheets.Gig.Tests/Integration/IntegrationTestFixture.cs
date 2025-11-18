using System.Diagnostics.CodeAnalysis;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Integration.Base;

namespace RaptorSheets.Gig.Tests.Integration;

/// <summary>
/// Test fixture that runs ONCE before all integration tests in the collection.
/// Provides clean environment setup for CI/CD pipelines that can't run PowerShell scripts.
/// </summary>
[ExcludeFromCodeCoverage] 
public class IntegrationTestFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // This runs ONCE before ALL integration tests in the collection
        var testBase = new TestableIntegrationBase();
        
        if (!testBase.HasCredentials())
        {
            // Skip setup if no credentials available
            System.Diagnostics.Debug.WriteLine("?? No credentials - skipping integration test environment setup");
            return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine("?? Setting up clean integration test environment...");
            
            // Step 1: Delete all existing sheets to ensure clean slate
            var deleteResult = await testBase.GoogleSheetManager!.DeleteAllSheets();
            
            // Check if deletion was skipped due to limitations
            var cannotDeleteWarning = deleteResult.Messages.Any(m => 
                m.Message.Contains("Cannot delete all sheets") ||
                m.Message.Contains("Google Sheets requires at least one sheet"));
            
            if (cannotDeleteWarning)
            {
                System.Diagnostics.Debug.WriteLine("?? Cannot delete all sheets - will work with existing sheets");
            }
            else
            {
                // Wait for deletion to propagate
                await Task.Delay(3000);
                
                // Step 2: Create all fresh sheets
                var createResult = await testBase.GoogleSheetManager!.CreateAllSheets();
                var createErrors = createResult.Messages.Where(m => m.Level == "ERROR").ToList();
                
                if (createErrors.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"?? Sheet creation had errors: {string.Join(", ", createErrors.Select(e => e.Message))}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("? Fresh sheets created successfully");
                }
                
                // Wait for creation to complete
                await Task.Delay(2000);
            }
            
            System.Diagnostics.Debug.WriteLine("?? Integration test environment ready");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Integration test setup failed: {ex.Message}");
            // Don't fail the fixture - let individual tests handle missing setup
            // This allows tests to run even if cleanup fails
        }
    }

    public async Task DisposeAsync()
    {
        // Optional: cleanup after all tests complete
        // Usually not needed as tests should clean up after themselves
        await Task.CompletedTask;
    }
}

/// <summary>
/// Collection definition that ensures all integration tests share the same setup fixture.
/// All test classes decorated with [Collection("IntegrationCollection")] will:
/// 1. Share the same IntegrationTestFixture instance
/// 2. Have the fixture's InitializeAsync() run ONCE before any tests
/// 3. Run tests in parallel within the collection (if desired)
/// </summary>
[CollectionDefinition("IntegrationCollection")]
[ExcludeFromCodeCoverage]
public class IntegrationCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    // This class is just a marker for the collection
    // The fixture will run setup once before any tests in this collection
}

/// <summary>
/// Helper class that exposes IntegrationTestBase methods for the fixture
/// </summary>
internal class TestableIntegrationBase : IntegrationTestBase
{
    // Expose protected members for the fixture
    public bool HasCredentials() => base.GoogleSheetManager != null;
    public new IGoogleSheetManager? GoogleSheetManager => base.GoogleSheetManager;
    public new List<string> TestSheets => base.TestSheets;
    public new async Task<bool> EnsureSheetsExist(List<string> sheets) => await base.EnsureSheetsExist(sheets);
}