using System.Diagnostics.CodeAnalysis;
using RaptorSheets.Job.Tests.Integration.Base;

namespace RaptorSheets.Job.Tests.Integration;

/// <summary>
/// Test fixture that runs ONCE before all integration tests in the collection.
/// Provides clean environment setup for integration testing.
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
            Console.WriteLine("WARNING: No credentials - skipping integration test environment setup");
            return;
        }

        try
        {
            Console.WriteLine("Setting up clean integration test environment...");

            // Step 1: Delete all existing sheets to ensure clean slate
            var deleteResult = await testBase.GoogleSheetManager!.DeleteAllSheets();
            Console.WriteLine($"Delete result: {deleteResult.Messages.Count} messages");
            foreach (var msg in deleteResult.Messages)
            {
                Console.WriteLine($"  [{msg.Level}] {msg.Message}");
            }

            // Check if deletion was skipped due to limitations
            var cannotDeleteWarning = deleteResult.Messages.Any(m => 
                m.Message.Contains("Cannot delete all sheets") ||
                m.Message.Contains("Google Sheets requires at least one sheet"));

            if (cannotDeleteWarning)
            {
                Console.WriteLine("WARNING: Cannot delete all sheets - will work with existing sheets");
            }
            else
            {
                // Wait for deletion to propagate
                Console.WriteLine("Waiting 3s for deletion to propagate...");
                await Task.Delay(3000);

                // Step 2: Create all fresh sheets and populate demo data (SetupDemo handles both)
                try
                {
                    Console.WriteLine("Creating sheets and populating demo data (SetupDemo)...");
                    var demoResult = await testBase.GoogleSheetManager!.SetupDemo();
                    Console.WriteLine($"SetupDemo complete: Applications={demoResult.Applications.Count}, Interviews={demoResult.Interviews.Count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: SetupDemo failed: {ex.Message}");
                }

                // Populate demo data so calculated columns and sample rows are visible for integration tests
                try
                {
                    Console.WriteLine("Populating demo data into sheets...");
                    var demo = await testBase.GoogleSheetManager!.PopulateDemoData();
                    Console.WriteLine($"Demo population complete: Applications={demo.Applications.Count}, Interviews={demo.Interviews.Count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Demo data population failed: {ex.Message}");
                }
            }

            Console.WriteLine("SUCCESS: Integration test environment ready");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Integration test setup failed: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            // Don't fail the fixture - let individual tests handle missing setup
        }
    }

    public async Task DisposeAsync()
    {
        // Optional: cleanup after all tests complete
        await Task.CompletedTask;
    }
}

/// <summary>
/// Collection definition that ensures all integration tests share the same setup fixture.
/// All test classes decorated with [Collection("JobIntegrationCollection")] will:
/// 1. Share the same IntegrationTestFixture instance
/// 2. Have the fixture's InitializeAsync() run ONCE before any tests
/// 3. Run tests in sequence within the collection
/// </summary>
[CollectionDefinition("JobIntegrationCollection")]
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
    // Exposes protected members for fixture use
}
