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

        // Seed demo data so primary sheets are not header-only
        Console.WriteLine("\nPopulating demo data...");
        var demoData = await GoogleSheetManager.PopulateDemoData();
        Console.WriteLine($"  Applications generated: {demoData.Applications.Count}");
        Console.WriteLine($"  Interviews generated: {demoData.Interviews.Count}");

        await Task.Delay(2000);

        // Verify key sheets contain data rows
        var spreadsheetInfo = await GoogleSheetManager.GetSpreadsheetInfo(new List<string>
        {
            $"{SheetsConfig.SheetNames.Applications}!A2:A",
            $"{SheetsConfig.SheetNames.Interviews}!A2:A",
            $"{SheetsConfig.SheetNames.Companies}!A2:C",
            $"{SheetsConfig.SheetNames.Positions}!A2:B"
        });

        Assert.NotNull(spreadsheetInfo);

        var appSheet = spreadsheetInfo.Sheets.FirstOrDefault(s =>
            string.Equals(s.Properties.Title, SheetsConfig.SheetNames.Applications, StringComparison.OrdinalIgnoreCase));
        var intSheet = spreadsheetInfo.Sheets.FirstOrDefault(s =>
            string.Equals(s.Properties.Title, SheetsConfig.SheetNames.Interviews, StringComparison.OrdinalIgnoreCase));
        var companySheet = spreadsheetInfo.Sheets.FirstOrDefault(s =>
            string.Equals(s.Properties.Title, SheetsConfig.SheetNames.Companies, StringComparison.OrdinalIgnoreCase));
        var positionSheet = spreadsheetInfo.Sheets.FirstOrDefault(s =>
            string.Equals(s.Properties.Title, SheetsConfig.SheetNames.Positions, StringComparison.OrdinalIgnoreCase));

        var appHasData = appSheet?.Data?
            .SelectMany(d => d.RowData ?? [])
            .SelectMany(r => r.Values ?? [])
            .Any(v => !string.IsNullOrWhiteSpace(v.FormattedValue)) == true;

        var intHasData = intSheet?.Data?
            .SelectMany(d => d.RowData ?? [])
            .SelectMany(r => r.Values ?? [])
            .Any(v => !string.IsNullOrWhiteSpace(v.FormattedValue)) == true;

        var companyHasData = companySheet?.Data?
            .SelectMany(d => d.RowData ?? [])
            .Any(r =>
            {
                var values = r.Values ?? [];
                var company = values.ElementAtOrDefault(0)?.FormattedValue;
                var appCount = values.ElementAtOrDefault(1)?.FormattedValue;
                return !string.IsNullOrWhiteSpace(company) && !string.IsNullOrWhiteSpace(appCount);
            }) == true;

        var positionHasData = positionSheet?.Data?
            .SelectMany(d => d.RowData ?? [])
            .Any(r =>
            {
                var values = r.Values ?? [];
                var position = values.ElementAtOrDefault(0)?.FormattedValue;
                var appCount = values.ElementAtOrDefault(1)?.FormattedValue;
                return !string.IsNullOrWhiteSpace(position) && !string.IsNullOrWhiteSpace(appCount);
            }) == true;

        Assert.True(appHasData, "Applications sheet should contain seeded test data.");
        Assert.True(intHasData, "Interviews sheet should contain seeded test data.");
        Assert.True(companyHasData, "Companies sheet should auto-populate company names and application counts.");
        Assert.True(positionHasData, "Positions sheet should auto-populate position names and application counts.");
    }
}
