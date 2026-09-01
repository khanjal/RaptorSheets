using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Gig.Tests.Integration.Base;
using RaptorSheets.Test.Common.Fixtures;
using RaptorSheets.Test.Common.Helpers;
using System.ComponentModel;
using RaptorSheets.Core.Constants;

namespace RaptorSheets.Gig.Tests.Integration;

/// <summary>
/// Integration tests for Google Sheets operations.
///
/// Test Organization:
/// - Single orchestrated flow to minimize API calls
/// - Each test validates a specific aspect during the flow
/// - Shared test data across related validations
/// - Collection fixture (<see cref="GigCleanSlateFixture"/>) deletes/recreates every sheet before
///   tests run
/// </summary>
[Collection("GigSheetsIntegration")]
[Category("Integration")]
[Trait("TestType", "Comprehensive")]
public class GoogleSheetsIntegrationTests : IntegrationTestBase
{
    public GoogleSheetsIntegrationTests(GigCleanSlateFixture fixture) : base(fixture)
    {
    }

    #region 1. Environment Setup & Validation

    // Environment_ShouldHaveAllRequiredSheets, Environment_SheetProperties_ShouldHaveValidStructure,
    // and CreatedSheets_ShouldHaveCorrectHeaders were removed here - all now covered by the shared
    // GigPlumbingTests (SheetPlumbingTestsBase in RaptorSheets.Test.Common), which proves the exact
    // same sheet-creation/header-generation mechanism this file used to re-verify per sheet. The
    // three tests below stay: they check things the shared suite doesn't (formula well-formedness on
    // Shifts/Trips/Expenses' own computed columns, tab visual properties, sheet tab order).


    [FactCheckUserSecrets]
    public async Task CreatedSheets_ShouldHaveCorrectVisualProperties()
    {
        // This test validates that sheets have correct colors, protection, etc.
        
        // Act - Get spreadsheet info to check visual properties
        var spreadsheetInfo = await SheetManager!.GetSpreadsheetInfo();
        
        Assert.NotNull(spreadsheetInfo);
        Assert.NotNull(spreadsheetInfo.Sheets);
        
        // Assert
        foreach (var sheet in spreadsheetInfo.Sheets)
        {
            var sheetName = sheet.Properties.Title;
            var properties = sheet.Properties;
            
            // Get expected layout
            var expectedLayout = SheetManager.GetSheetLayout(sheetName);
            
            if (expectedLayout != null)
            {
                System.Diagnostics.Debug.WriteLine($"  🔍 Validating {sheetName} visual properties");
                
                // Should have tab color
                Assert.NotNull(properties.TabColor);
                
                // Should have frozen rows/columns if specified
                if (expectedLayout.FreezeRowCount > 0)
                {
                    Assert.NotNull(properties.GridProperties);
                    Assert.True(properties.GridProperties.FrozenRowCount >= expectedLayout.FreezeRowCount,
                        $"{sheetName} should have at least {expectedLayout.FreezeRowCount} frozen rows");
                }
                
                if (expectedLayout.FreezeColumnCount > 0)
                {
                    Assert.NotNull(properties.GridProperties);
                    Assert.True(properties.GridProperties.FrozenColumnCount >= expectedLayout.FreezeColumnCount,
                        $"{sheetName} should have at least {expectedLayout.FreezeColumnCount} frozen columns");
                }
            }
        }
    }

    [FactCheckUserSecrets]
    public async Task CreatedSheets_ShouldBeInCorrectOrder()
    {
        // This test validates that sheets in TestSheets are created in the correct order
        // as defined by the constants declaration in SheetsConfig.SheetNames
        
        // Get expected order directly from constants reflection (source of truth)
        var expectedOrder = typeof(SheetsConfig.SheetNames)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(fi => fi.GetValue(null)?.ToString() ?? "")
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();
        
        // Act - Get all sheet properties which includes sheet IDs for ordering
        var allProperties = await SheetManager!.GetAllSheetProperties();
        var existingSheets = allProperties.Where(p => !string.IsNullOrEmpty(p.Id)).ToList();
        
        // Sort by sheet ID (Google Sheets internal ordering) to get actual tab order
        var actualOrder = existingSheets
            .Select(p => p.Name)
            .ToList();
        
        System.Diagnostics.Debug.WriteLine($"  📋 Actual order (from GetAllSheetProperties): {string.Join(", ", actualOrder)}");
        System.Diagnostics.Debug.WriteLine($"  📋 Expected order (from constants): {string.Join(", ", expectedOrder)}");
        
        // Assert - Orders should match
        Assert.Equal(expectedOrder.Count, actualOrder.Count);
        
        for (int i = 0; i < Math.Min(expectedOrder.Count, actualOrder.Count); i++)
        {
            Assert.True(
                string.Equals(expectedOrder[i], actualOrder[i], StringComparison.OrdinalIgnoreCase),
                $"Sheet at position {i} should be '{expectedOrder[i]}' but was '{actualOrder[i]}'");
        }
    }

    #endregion

    #region 2. Orchestrated CRUD Workflow Tests

    /// <summary>
    /// Main orchestrated test that performs a complete CRUD workflow in sequence:
    /// 1. Insert test data
    /// 2. Read and validate inserted data
    /// 3. Update specific records
    /// 4. Read and validate updates
    /// 5. Validate cross-entity relationships
    /// 
    /// This approach minimizes API calls and maintains test data consistency.
    /// </summary>
    [FactCheckUserSecrets]
    public async Task FullWorkflow_InsertReadUpdate_ShouldSucceedWithConsistentData()
    {
        // Arrange
        var testRunId = GenerateTestRunId();
        System.Diagnostics.Debug.WriteLine($"🚀 Starting orchestrated workflow test: {testRunId}");
        
        // Step 1: INSERT - Create comprehensive test dataset
        System.Diagnostics.Debug.WriteLine("📝 Step 1: Inserting test data...");
        var testData = CreateTestData(testRunId, shifts: 5, tripsPerShift: 3, expenses: 4);
        await InsertTestData(testData);
        await Task.Delay(2000); // Allow propagation
        
        ValidateInsertResult(testRunId, testData);
        
        // Step 2: READ - Retrieve and validate inserted data
        System.Diagnostics.Debug.WriteLine("📖 Step 2: Reading and validating inserted data...");
        var readData = await GetSheetData();
        
        var insertedShifts = ValidateInsertedShifts(testRunId, readData, testData);
        var insertedTrips = ValidateInsertedTrips(testRunId, readData, testData);
        var insertedExpenses = ValidateInsertedExpenses(testRunId, readData, testData);
        
        ValidateEntityStructures(insertedShifts, insertedTrips, insertedExpenses);
        ValidateCrossEntityRelationships(insertedShifts, insertedTrips);
        ValidateDateRanges(insertedShifts, insertedExpenses);
        
        // Step 3: UPDATE - Modify subset of data
        System.Diagnostics.Debug.WriteLine("✏️  Step 3: Updating data...");
        var shiftsToUpdate = insertedShifts.Take(2).ToList();
        var tripsToUpdate = insertedTrips.Take(2).ToList();
        var expensesToUpdate = insertedExpenses.Take(2).ToList();
        
        await UpdateShifts(shiftsToUpdate, shift =>
        {
            shift.Note = $"UPDATED_{testRunId}";
            shift.Pay = (shift.Pay ?? 0) + 99.99m;
            return shift;
        });
        
        await UpdateTrips(tripsToUpdate, trip =>
        {
            trip.Note = $"UPDATED_{testRunId}";
            trip.Tip = (trip.Tip ?? 0) + 77.77m;
            return trip;
        });
        
        await UpdateExpenses(expensesToUpdate, expense =>
        {
            expense.Description = $"UPDATED_{testRunId}";
            expense.Amount = expense.Amount + 55.55m;
            return expense;
        });
        
        await Task.Delay(2000); // Allow propagation
        
        // Step 4: READ AGAIN - Validate updates
        System.Diagnostics.Debug.WriteLine("🔍 Step 4: Validating updates...");
        var updatedData = await GetSheetData();
        
        ValidateUpdatedShifts(testRunId, updatedData);
        ValidateUpdatedTrips(testRunId, updatedData);
        ValidateUpdatedExpenses(testRunId, updatedData);
        
        System.Diagnostics.Debug.WriteLine($"✅ Orchestrated workflow completed successfully: {testRunId}");
    }

    #endregion

    #region 3. Focused Scenario Tests

    [FactCheckUserSecrets]
    public async Task Workflow_DailyOperation_ShouldRecordShiftWithTrips()
    {
        // Arrange - Simulates daily workflow: start shift, record trips, end shift
        var testRunId = GenerateTestRunId();
        
        var testData = new SheetEntity();
        var today = DateTime.Today;
        
        // Create a shift for today
        var shift = new ShiftEntity
        {
            RowId = 2,
            Action = ActionType.INSERT.GetDescription(),
            Date = today.ToString(CellFormatPatterns.Date),
            Service = $"Test_{testRunId}",
            Region = "DailyWorkflow",
            Start = "09:00:00",
            Finish = "17:00:00",
            Pay = 120m,
            Tip = 25m,
            Note = "Daily workflow test"
        };
        testData.Sheets.Shifts.Add(shift);
        
        // Add trips for this shift
        for (int i = 0; i < 3; i++)
        {
            var trip = new TripEntity
            {
                RowId = 2 + i,
                Action = ActionType.INSERT.GetDescription(),
                Date = today.ToString(CellFormatPatterns.Date),
                Service = $"Test_{testRunId}",
                Type = i % 2 == 0 ? "Pickup" : "Delivery",
                Pay = 15m + i * 5,
                Tip = 3m + i,
                Note = $"Daily trip {i + 1}"
            };
            testData.Sheets.Trips.Add(trip);
        }
        
        // Act
        await InsertTestData(testData);
        await Task.Delay(2000);
        
        // Assert
        var readData = await GetSheetData();
        var dailyShifts = readData.Sheets.Shifts.Where(s => 
            s.Service?.Contains($"Test_{testRunId}") == true && 
            s.Region == "DailyWorkflow").ToList();
        var dailyTrips = readData.Sheets.Trips.Where(t => 
            t.Service?.Contains($"Test_{testRunId}") == true).ToList();
        
        Assert.Single(dailyShifts);
        // Derived from what this test inserted rather than a floor, so it fails on a lost row and on
        // a duplicated one. "At least 2" reported success for either.
        Assert.Equal(testData.Sheets.Trips.Count, dailyTrips.Count);
        
        var workflowShift = dailyShifts[0];
        Assert.NotNull(workflowShift.Start);
        Assert.NotNull(workflowShift.Finish);
        Assert.True(workflowShift.Pay > 0, "Daily shift should have pay recorded");
    }

    [FactCheckUserSecrets]
    public async Task Workflow_ExpenseTracking_ShouldRecordMultipleCategories()
    {
        // Arrange - Simulates expense tracking workflow
        var testRunId = GenerateTestRunId();
        
        var testData = new SheetEntity();
        var today = DateTime.Today;
        
        // Create expenses in different categories
        var categories = new[] { "Fuel", "Meal", "Maintenance" };
        for (int i = 0; i < categories.Length; i++)
        {
            var expense = new ExpenseEntity
            {
                RowId = 2 + i,
                Action = ActionType.INSERT.GetDescription(),
                Date = today.AddDays(-i).ToString(CellFormatPatterns.Date),  // Convert to string format
                Category = categories[i],
                Name = $"{categories[i]} Item",
                Amount = 25m + i * 10,
                Description = $"Test_{testRunId}_expense"
            };
            testData.Sheets.Expenses.Add(expense);
        }
        
        // Act
        await InsertTestData(testData);
        await Task.Delay(2000);
        
        // Assert
        var readData = await GetSheetData();
        var ourExpenses = readData.Sheets.Expenses.Where(e => 
            e.Description?.Contains($"Test_{testRunId}") == true).ToList();
        
        var expenseCategories = ourExpenses.Select(e => e.Category).Distinct().ToList();
        
        Assert.Equal(testData.Sheets.Expenses.Count, ourExpenses.Count);
        Assert.Equal(
            testData.Sheets.Expenses.Select(e => e.Category).Distinct().OrderBy(c => c),
            expenseCategories.OrderBy(c => c));
    }

    #endregion

    #region Validation Helper Methods

    private static void ValidateInsertResult(string testRunId, SheetEntity testData)
    {
        System.Diagnostics.Debug.WriteLine($"   ✓ Inserted {testData.Sheets.Shifts.Count} shifts, " +
            $"{testData.Sheets.Trips.Count} trips, {testData.Sheets.Expenses.Count} expenses for test {testRunId}");
    }

    private static List<ShiftEntity> ValidateInsertedShifts(string testRunId, SheetEntity readData, SheetEntity expectedData)
    {
        var shifts = readData.Sheets.Shifts.Where(s => 
            s.Service?.Contains($"Test_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {shifts.Count} shifts");
        
        // Exact, not a tolerance. These rows are scoped to this run's own marker, so the count is
        // knowable - and ">= expected - 1" passed whether a row went missing or an extra one appeared,
        // which is how a write landing in the wrong place (#133) or duplicating a record (#134) could
        // run through this suite untouched.
        Assert.Equal(expectedData.Sheets.Shifts.Count, shifts.Count);
        
        return shifts;
    }

    private static List<TripEntity> ValidateInsertedTrips(string testRunId, SheetEntity readData, SheetEntity expectedData)
    {
        var trips = readData.Sheets.Trips.Where(t => 
            t.Service?.Contains($"Test_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {trips.Count} trips");
        
        Assert.Equal(expectedData.Sheets.Trips.Count, trips.Count);
        
        return trips;
    }

    private static List<ExpenseEntity> ValidateInsertedExpenses(string testRunId, SheetEntity readData, SheetEntity expectedData)
    {
        var expenses = readData.Sheets.Expenses.Where(e => 
            e.Description?.Contains($"Test_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {expenses.Count} expenses");
        Assert.Equal(expectedData.Sheets.Expenses.Count, expenses.Count);
        
        return expenses;
    }

    private static void ValidateEntityStructures(
        List<ShiftEntity> shifts, 
        List<TripEntity> trips, 
        List<ExpenseEntity> expenses)
    {
        System.Diagnostics.Debug.WriteLine("   🔍 Validating entity structures...");
        
        Assert.All(shifts, shift =>
        {
            Assert.True(shift.RowId > 0, "Shift RowId should be positive");
            Assert.NotNull(shift.Date);
            Assert.NotNull(shift.Service);
            Assert.True(shift.Pay == null || shift.Pay >= 0, "Shift pay should be null or non-negative");
        });
        
        Assert.All(trips, trip =>
        {
            Assert.True(trip.RowId > 0, "Trip RowId should be positive");
            Assert.NotNull(trip.Date);
            Assert.NotNull(trip.Service);
            Assert.True(trip.Pay == null || trip.Pay >= 0, "Trip pay should be null or non-negative");
        });
        
        Assert.All(expenses, expense =>
        {
            // ExpenseEntity.Date is now a string, so we just verify it's not empty
            Assert.False(string.IsNullOrWhiteSpace(expense.Date), 
                $"Expense date should not be empty");
        });
        
        System.Diagnostics.Debug.WriteLine("   ✓ Entity structures valid");
    }

    private static void ValidateCrossEntityRelationships(List<ShiftEntity> shifts, List<TripEntity> trips)
    {
        System.Diagnostics.Debug.WriteLine("   🔍 Validating cross-entity relationships...");
        
        var shiftServices = shifts.Select(s => s.Service).Distinct().ToList();
        var tripServices = trips.Select(t => t.Service).Distinct().ToList();
        var commonServices = shiftServices.Intersect(tripServices).ToList();
        
        Assert.NotEmpty(shifts);
        Assert.NotEmpty(trips);
        Assert.True(commonServices.Count > 0, 
            "Shifts and trips should share service identifiers");
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {commonServices.Count} common services between shifts and trips");
    }

    private static void ValidateDateRanges(List<ShiftEntity> shifts, List<ExpenseEntity> expenses)
    {
        System.Diagnostics.Debug.WriteLine("   🔍 Validating date ranges...");
        
        var validDateRange = DateTime.Today.AddDays(-30);
        
        Assert.All(shifts, shift =>
        {
            if (DateTime.TryParse(shift.Date, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var shiftDate))
            {
                Assert.True(shiftDate >= validDateRange, 
                    $"Shift date should be within valid range: {shiftDate:yyyy-MM-dd}");
            }
        });
        
        Assert.All(expenses, expense =>
        {
            // ExpenseEntity.Date is now a string, so we just verify it's not empty
            Assert.False(string.IsNullOrWhiteSpace(expense.Date), 
                $"Expense date should not be empty");
        });
        
        System.Diagnostics.Debug.WriteLine("   ✓ All dates within valid range");
    }

    private static void ValidateUpdatedShifts(string testRunId, SheetEntity updatedData)
    {
        var updatedShifts = updatedData.Sheets.Shifts.Where(s => 
            s.Note?.Contains($"UPDATED_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {updatedShifts.Count} updated shifts");
        
        Assert.NotEmpty(updatedShifts);
        Assert.All(updatedShifts, shift =>
        {
            Assert.Contains($"UPDATED_{testRunId}", shift.Note);
            Assert.True(shift.Pay >= 99, $"Updated shift should have increased pay: {shift.Pay}");
        });
    }

    private static void ValidateUpdatedTrips(string testRunId, SheetEntity updatedData)
    {
        var updatedTrips = updatedData.Sheets.Trips.Where(t => 
            t.Note?.Contains($"UPDATED_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {updatedTrips.Count} updated trips");
        
        Assert.NotEmpty(updatedTrips);
        Assert.All(updatedTrips, trip =>
        {
            Assert.Contains($"UPDATED_{testRunId}", trip.Note);
            Assert.True(trip.Tip >= 70, $"Updated trip should have increased tip: {trip.Tip}");
        });
    }

    private static void ValidateUpdatedExpenses(string testRunId, SheetEntity updatedData)
    {
        var updatedExpenses = updatedData.Sheets.Expenses.Where(e => 
            e.Description?.Contains($"UPDATED_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {updatedExpenses.Count} updated expenses");
        
        Assert.NotEmpty(updatedExpenses);
        Assert.All(updatedExpenses, expense =>
        {
            Assert.Contains($"UPDATED_{testRunId}", expense.Description);
            Assert.True(expense.Amount >= 55, $"Updated expense should have increased amount: {expense.Amount}");
        });
    }

    #endregion

    #region Helper Methods




    #endregion

    #region 4. Demo Data Integration Tests




    #endregion
}

/// <summary>
/// Collection definition for Gig Google Sheets integration tests.
/// </summary>
[CollectionDefinition("GigSheetsIntegration")]
public class GigSheetsIntegrationCollection : ICollectionFixture<GigCleanSlateFixture>
{
}

/// <summary>
/// Gig's clean-slate integration fixture (see <see cref="CleanSlateSheetFixture{TEntity,TManager}"/>).
/// Deletes and recreates every canonical sheet once, before the collection's tests run. Safe because
/// spreadsheets:test:gig is configured to point at a dedicated blank test spreadsheet, not real data.
/// </summary>
public class GigCleanSlateFixture : CleanSlateSheetFixture<SheetEntity, SheetManager>
{
    public GigCleanSlateFixture() : base(
        TestConfigurationHelpers.GetGigSpreadsheet(),
        (credential, spreadsheetId) => new SheetManager(credential, spreadsheetId))
    {
    }
}
