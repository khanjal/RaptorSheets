using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Integration.Base;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Integration;

/// <summary>
/// Test fixture that runs ONCE before all integration tests in the collection.
/// Provides clean environment setup for CI/CD pipelines that can't run PowerShell scripts.
/// </summary>
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
            var deleteResult = await testBase.GoogleSheetManager!.DeleteSheets(testBase.TestSheets);
            
            // Allow warnings but log any hard errors
            var deleteErrors = deleteResult.Messages.Where(m => m.Level == "ERROR").ToList();
            if (deleteErrors.Count > 0)
            {
                // Log but don't fail - sheets might not exist or deletion might not be supported
                System.Diagnostics.Debug.WriteLine($"Delete warnings (expected): {string.Join(", ", deleteErrors.Select(e => e.Message))}");
            }

            // Wait for deletion to propagate
            await Task.Delay(4000);

            // Step 2: Recreate fresh sheets
            var recreateSuccess = await testBase.EnsureSheetsExist(testBase.TestSheets);
            
            if (recreateSuccess)
            {
                System.Diagnostics.Debug.WriteLine("? Clean integration test environment prepared successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("?? Sheet setup had issues, but continuing with tests");
            }

            // Wait for creation to complete
            await Task.Delay(3000);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"?? Integration test setup failed: {ex.Message}");
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
    public new bool HasCredentials() => base.GoogleSheetManager != null;
    public new IGoogleSheetManager? GoogleSheetManager => base.GoogleSheetManager;
    public new List<string> TestSheets => base.TestSheets;
    public new async Task<bool> EnsureSheetsExist(List<string> sheets) => await base.EnsureSheetsExist(sheets);
}