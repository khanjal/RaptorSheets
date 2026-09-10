using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Gig.Tests.Integration.Base;
using RaptorSheets.Test.Common.Fixtures;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Integration.Load;

/// <summary>
/// The load tier: a small number of deliberately heavy tests, on their own spreadsheet, in their own
/// collection.
///
/// These are the tests that made the rest of the suite unreliable. They write bulk data into the
/// shared spreadsheet, so every other test read state they had changed, and they assert wall-clock
/// budgets - a timing assertion against a live API on a shared runner reports load, not correctness.
/// Isolated here they are free to be destructive and to assert absolutes, because nothing else
/// shares their state, and they never gate a merge.
///
/// Runs only when spreadsheets:test:gigload is configured. It deliberately does not fall back to the
/// shared Gig spreadsheet - borrowing it would restore the coupling this split removes.
/// </summary>
[Collection("GigLoadTests")]
public class GigLoadTests : IntegrationTestBase
{
    public GigLoadTests(GigLoadFixture fixture) : base(fixture)
    {
    }

    [FactCheckLoadSpreadsheet]
    public async Task LargeDataset_ShouldHandleVolumeEfficiently()
    {
        // Arrange
        var testRunId = GenerateTestRunId();
        
        var testData = CreateTestData(testRunId, shifts: 10, tripsPerShift: 5, expenses: 15);
        
        System.Diagnostics.Debug.WriteLine($"📊 Inserting large dataset: {testData.Sheets.Shifts.Count} shifts, " +
            $"{testData.Sheets.Trips.Count} trips, {testData.Sheets.Expenses.Count} expenses");
        
        // Act
        var startTime = DateTime.UtcNow;
        var insertResult = await InsertTestData(testData);
        var elapsed = DateTime.UtcNow - startTime;
        
        // Assert
        System.Diagnostics.Debug.WriteLine($"⏱️  Insert completed in {elapsed.TotalSeconds:F1}s");
        
        var criticalErrors = insertResult.Messages.Where(m => 
            m.Level == MessageLevel.ERROR.GetDescription() && 
            !IsExpectedError(m.Message)).ToList();
        
        Assert.Empty(criticalErrors);
        Assert.True(elapsed.TotalSeconds < 30, 
            $"Large insert should complete within 30 seconds, took {elapsed.TotalSeconds:F1}s");
    }

    /// <summary>
    /// Tests using production demo data system for realistic full-year scenario.
    /// This validates both the demo system and large-scale data handling.
    /// </summary>
    [FactCheckLoadSpreadsheet]
    public async Task DemoData_FullYear_ShouldUploadAndValidate()
    {
        // Arrange - Use demo system for realistic year of data
        var startDate = new DateTime(DateTime.Today.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Local);
        var endDate = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Local);
        
        System.Diagnostics.Debug.WriteLine($"📅 Generating demo data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        
        // Use production demo helpers
        var demoData = CreateDemoData(startDate, endDate);
        
        System.Diagnostics.Debug.WriteLine($"📊 Generated: {demoData.Sheets.Shifts.Count} shifts, " +
            $"{demoData.Sheets.Trips.Count} trips, {demoData.Sheets.Expenses.Count} expenses");

        // Act - Insert demo data
        var startTime = DateTime.UtcNow;
        var insertResult = await InsertTestData(demoData);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert - Verify successful insertion
        System.Diagnostics.Debug.WriteLine($"⏱️  Insert completed in {elapsed.TotalSeconds:F1}s");
        
        var criticalErrors = (insertResult.Messages ?? new List<MessageEntity>())
            .Where(m => m.Level == MessageLevel.ERROR.GetDescription() && !IsExpectedError(m.Message))
            .ToList();
        
        Assert.Empty(criticalErrors);
        Assert.True(elapsed.TotalSeconds < 120, 
            $"Full year insert should complete within 2 minutes, took {elapsed.TotalSeconds:F1}s");

        // Validate data was inserted correctly
        var readData = await GetSheetData();
        Assert.True(readData.Sheets.Shifts.Count >= demoData.Sheets.Shifts.Count * 0.95, 
            $"Should find most shifts, found {readData.Sheets.Shifts.Count} of {demoData.Sheets.Shifts.Count}");
        Assert.True(readData.Sheets.Trips.Count >= demoData.Sheets.Trips.Count * 0.95, 
            $"Should find most trips, found {readData.Sheets.Trips.Count} of {demoData.Sheets.Trips.Count}");
        Assert.True(readData.Sheets.Expenses.Count >= demoData.Sheets.Expenses.Count * 0.95,
            $"Should find most expenses, found {readData.Sheets.Expenses.Count} of {demoData.Sheets.Expenses.Count}");
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Validated {readData.Sheets.Shifts.Count} shifts, " +
            $"{readData.Sheets.Trips.Count} trips, {readData.Sheets.Expenses.Count} expenses");

        // --- New: validate behavior when a summary sheet is missing ---
        // Delete a lightweight summary sheet (Deliveries) and re-run a read to verify
        // missing-sheet / empty-header diagnostics are produced by the manager.
        System.Diagnostics.Debug.WriteLine("➡ Deleting Deliveries to validate missing-sheet detection...");
        var deleteResult = await SheetManager!.DeleteSheets(new List<string> { SheetsConfig.SheetNames.Deliveries });
        var deleteErrors = deleteResult.Messages.Where(m => m.Level == MessageLevel.ERROR.GetDescription()).ToList();
        if (deleteErrors.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️  Deletion returned errors: {string.Join(';', deleteErrors.Select(e => e.Message))}");
        }

        // Allow time for Sheets backend to apply deletion
        await Task.Delay(10000);

        // Re-read metadata and sheet data (include all sheets to detect missing summaries)
        var readAfterDelete = await GetAllSheetData();

        // We expect either a message indicating missing header/empty sheet for Deliveries
        // or an informational creation notice that asks the caller to retry shortly.
        var allMessages = readAfterDelete.Messages ?? new List<MessageEntity>();

        var headerMessages = allMessages
            .Where(m => m.Message != null && m.Message.Contains("Deliveries", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Detect the informational creation notice produced when the manager created missing sheets
        var creationNotice = allMessages
            .FirstOrDefault(m => m.Message != null && m.Message.Contains("Sheets may take a few seconds", StringComparison.OrdinalIgnoreCase));

        var hasCreationNotice = creationNotice != null;

        // If a creation notice exists, ensure it references the Deliveries sheet
        if (creationNotice != null)
        {
            Assert.True(creationNotice.Message.Contains("Deliveries", StringComparison.OrdinalIgnoreCase),
                $"Creation notice should include Deliveries. Notice: {creationNotice.Message}");
        }

        // Require either explicit header/missing-sheet messages or the creation notice
        Assert.True(headerMessages.Count > 0 || hasCreationNotice,
            "Expected header/missing-sheet messages or creation notice for Deliveries after deletion");

        System.Diagnostics.Debug.WriteLine("   ✓ Missing-sheet detection validated (Deliveries)");
        }
}

/// <summary>Collection definition for the Gig load tier - separate from GigSheetsIntegration so the
/// two never share a fixture or a spreadsheet.</summary>
[CollectionDefinition("GigLoadTests")]
public class GigLoadCollection : ICollectionFixture<GigLoadFixture>
{
}

/// <summary>
/// Clean-slate fixture pointed at the load spreadsheet rather than the shared one.
/// </summary>
public class GigLoadFixture : CleanSlateSheetFixture<SheetEntity, SheetManager>
{
    public GigLoadFixture() : base(
        TestConfigurationHelpers.GetGigLoadSpreadsheet(),
        (credential, spreadsheetId) => new SheetManager(credential, spreadsheetId))
    {
    }
}
