using RaptorSheets.Job.Tests.Data.Attributes;
using RaptorSheets.Job.Tests.Integration.Base;

namespace RaptorSheets.Job.Tests.Integration;

/// <summary>
/// Manual setup test - run this FIRST before other integration tests
/// </summary>
public class SetupIntegrationTest : IntegrationTestBase
{
    [FactCheckUserSecrets]
    public async Task Setup_CreateAllSheets_ShouldSucceed()
    {
        // Arrange - Clean slate
        Console.WriteLine("Deleting all existing sheets...");
        var deleteResult = await GoogleSheetManager!.DeleteAllSheets();
        foreach (var msg in deleteResult.Messages)
        {
            Console.WriteLine($"  [{msg.Level}] {msg.Message}");
        }

        // Wait for deletion
        await Task.Delay(3000);

        // Act - Create all sheets
        Console.WriteLine("\nCreating all sheets...");
        var createResult = await GoogleSheetManager!.CreateAllSheets();

        // Assert
        Console.WriteLine($"\nCreate result: {createResult.Messages.Count} messages");
        foreach (var msg in createResult.Messages)
        {
            Console.WriteLine($"  [{msg.Level}] {msg.Message}");
        }

        var errors = createResult.Messages.Where(m => m.Level == "ERROR").ToList();
        Assert.Empty(errors);

        // Wait for creation
        await Task.Delay(2000);

        // Verify sheets were created
        var properties = await GoogleSheetManager!.GetSheetProperties(TestSheets);
        var existingSheets = properties.Where(p => !string.IsNullOrEmpty(p.Id)).ToList();

        Console.WriteLine($"\nSheets created: {existingSheets.Count}");
        foreach (var sheet in existingSheets)
        {
            Console.WriteLine($"  - {sheet.Name}");
        }

        Assert.Equal(TestSheets.Count, existingSheets.Count);
    }
}
