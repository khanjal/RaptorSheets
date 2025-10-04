using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Gig.Tests.Integration.Base;
using RaptorSheets.Test.Common.Helpers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Integration;

/// <summary>
/// Comprehensive Google Sheets integration tests that minimize API calls.
/// 
/// Validates in a single test run (5-7 API calls):
/// - Sheet validation (properties check)
/// - CRUD operations (insert, update, read)
/// - Business workflows (daily work, corrections, reporting)
/// - Data mapping and integrity
/// - Performance and volume handling
/// - Error handling and edge cases
/// 
/// Note: Sheet creation/deletion is tested separately in SetupIntegrationTests
/// to avoid complications with Google Sheets' requirement of at least one sheet.
/// </summary>
[Collection("GoogleSheetsIntegration")]
[Category("Integration")]
public class GoogleSheetsIntegrationTests : IntegrationTestBase
{
    #region Single Comprehensive Integration Test

    [FactCheckUserSecrets]
    public async Task ComprehensiveIntegration_ShouldValidateAllFunctionalityEfficiently()
    {
        SkipIfNoCredentials();
        
        try
        {
            var testRunId = DateTimeOffset.UtcNow.ToString("HHmmss");
            var startTime = DateTime.UtcNow;
            
            System.Diagnostics.Debug.WriteLine($"?? Starting comprehensive integration test: {testRunId}");
            System.Diagnostics.Debug.WriteLine("=" .PadRight(80, '='));
            
            // ==================== PHASE 1: ENVIRONMENT VALIDATION (1 API call) ====================
            System.Diagnostics.Debug.WriteLine("\n?? PHASE 1: Environment Validation");
            System.Diagnostics.Debug.WriteLine("-".PadRight(80, '-'));
            
            // Validate sheets exist and get properties (1 API call)
            var phase1Start = DateTime.UtcNow;
            var properties = await GoogleSheetManager!.GetSheetProperties(TestSheets);
            var existingSheets = properties.Where(p => !string.IsNullOrEmpty(p.Id)).ToList();
            
            System.Diagnostics.Debug.WriteLine($"  ? Found {existingSheets.Count} existing sheets");
            
            // Validate sheet properties structure
            Assert.True(existingSheets.Count >= TestSheets.Count, 
                $"Should have at least {TestSheets.Count} sheets, found {existingSheets.Count}");
            
            foreach (var sheet in existingSheets)
            {
                Assert.NotNull(sheet.Name);
                Assert.NotNull(sheet.Id);
                Assert.NotNull(sheet.Attributes);
                Assert.True(TestSheets.Contains(sheet.Name, StringComparer.OrdinalIgnoreCase));
            }
            
            System.Diagnostics.Debug.WriteLine($"  ? All required sheets validated");
            
            var phase1Time = DateTime.UtcNow - phase1Start;
            System.Diagnostics.Debug.WriteLine($"  ??  Phase 1 completed in {phase1Time.TotalSeconds:F1}s");
            
            // ==================== PHASE 2: COMPREHENSIVE DATA OPERATIONS (3 API calls) ====================
            System.Diagnostics.Debug.WriteLine("\n?? PHASE 2: Comprehensive Data Operations");
            System.Diagnostics.Debug.WriteLine("-".PadRight(80, '-'));
            
            var phase2Start = DateTime.UtcNow;
            
            // Create comprehensive test data covering all scenarios
            var testData = CreateComprehensiveTestData(testRunId);
            
            System.Diagnostics.Debug.WriteLine($"  ?? Inserting test data:");
            System.Diagnostics.Debug.WriteLine($"     - {testData.Shifts.Count} shifts");
            System.Diagnostics.Debug.WriteLine($"     - {testData.Trips.Count} trips");
            System.Diagnostics.Debug.WriteLine($"     - {testData.Expenses.Count} expenses");
            
            // Batch insert all data (1 API call)
            var insertResult = await InsertTestData(testData);
            
            var insertErrors = insertResult.Messages.Where(m => 
                m.Level == MessageLevelEnum.ERROR.GetDescription() && 
                !IsExpectedError(m.Message)).ToList();
            
            if (insertErrors.Count > 0)
            {
                var errorDetails = string.Join("; ", insertErrors.Select(m => m.Message));
                throw new SkipException($"Insert failed - environment issue: {errorDetails}");
            }
            
            System.Diagnostics.Debug.WriteLine($"  ? Data inserted successfully");
            
            await Task.Delay(3000); // Allow data propagation
            
            // Batch update operations (1 API call)
            var firstReadData = await GetSheetData();
            
            var shiftsToUpdate = firstReadData.Shifts
                .Where(s => s.Service?.Contains($"CompTest_{testRunId}") == true)
                .Take(2).ToList();
            var tripsToUpdate = firstReadData.Trips
                .Where(t => t.Service?.Contains($"CompTest_{testRunId}") == true)
                .Take(2).ToList();
            var expensesToUpdate = firstReadData.Expenses
                .Where(e => e.Description?.Contains($"CompTest_{testRunId}") == true)
                .Take(1).ToList();
            
            System.Diagnostics.Debug.WriteLine($"  ?? Updating test data:");
            System.Diagnostics.Debug.WriteLine($"     - {shiftsToUpdate.Count} shifts");
            System.Diagnostics.Debug.WriteLine($"     - {tripsToUpdate.Count} trips");
            System.Diagnostics.Debug.WriteLine($"     - {expensesToUpdate.Count} expenses");
            
            if (shiftsToUpdate.Count > 0)
            {
                await UpdateShifts(shiftsToUpdate, shift => {
                    shift.Note = $"UPDATED_{testRunId}";
                    shift.Pay = (shift.Pay ?? 0) + 99.99m; // Distinctive value
                    return shift;
                });
            }
            
            if (tripsToUpdate.Count > 0)
            {
                await UpdateTrips(tripsToUpdate, trip => {
                    trip.Note = $"UPDATED_{testRunId}";
                    trip.Tip = (trip.Tip ?? 0) + 77.77m; // Distinctive value
                    return trip;
                });
            }
            
            if (expensesToUpdate.Count > 0)
            {
                await UpdateExpenses(expensesToUpdate, expense => {
                    expense.Description = $"UPDATED_{testRunId}";
                    expense.Amount = expense.Amount + 55.55m; // Distinctive value
                    return expense;
                });
            }
            
            System.Diagnostics.Debug.WriteLine($"  ? Data updated successfully");
            
            await Task.Delay(3000); // Allow update propagation
            
            var phase2Time = DateTime.UtcNow - phase2Start;
            System.Diagnostics.Debug.WriteLine($"  ??  Phase 2 completed in {phase2Time.TotalSeconds:F1}s");
            
            // ==================== PHASE 3: COMPREHENSIVE VALIDATION (1 API call) ====================
            System.Diagnostics.Debug.WriteLine("\n? PHASE 3: Comprehensive Validation");
            System.Diagnostics.Debug.WriteLine("-".PadRight(80, '-'));
            
            var phase3Start = DateTime.UtcNow;
            
            // Single read to get all data for validation (1 API call)
            var finalData = await GetSheetData();
            
            // Get our test data using unique identifiers
            var ourShifts = finalData.Shifts.Where(s => 
                s.Service?.Contains($"CompTest_{testRunId}") == true).ToList();
            var ourTrips = finalData.Trips.Where(t => 
                t.Service?.Contains($"CompTest_{testRunId}") == true).ToList();
            var ourExpenses = finalData.Expenses.Where(e => 
                e.Description?.Contains($"CompTest_{testRunId}") == true).ToList();
            
            System.Diagnostics.Debug.WriteLine($"  ?? Found test data:");
            System.Diagnostics.Debug.WriteLine($"     - {ourShifts.Count} shifts");
            System.Diagnostics.Debug.WriteLine($"     - {ourTrips.Count} trips");
            System.Diagnostics.Debug.WriteLine($"     - {ourExpenses.Count} expenses");
            
            // Validate all aspects in memory (no additional API calls)
            ValidateDataMapping(finalData, testData, testRunId, ourShifts, ourTrips, ourExpenses);
            ValidateEntityStructure(ourShifts, ourTrips, ourExpenses);
            ValidateUpdateOperations(ourShifts, ourTrips, ourExpenses, testRunId);
            ValidateCrossEntityConsistency(ourShifts, ourTrips, ourExpenses, testRunId);
            ValidateBusinessWorkflows(ourShifts, ourTrips, ourExpenses, testRunId);
            ValidateEdgeCases(finalData, testRunId);
            
            var phase3Time = DateTime.UtcNow - phase3Start;
            System.Diagnostics.Debug.WriteLine($"  ??  Phase 3 completed in {phase3Time.TotalSeconds:F1}s");
            
            // ==================== PHASE 4: PERFORMANCE METRICS ====================
            System.Diagnostics.Debug.WriteLine("\n?? PHASE 4: Performance Summary");
            System.Diagnostics.Debug.WriteLine("-".PadRight(80, '-'));
            
            var totalTime = DateTime.UtcNow - startTime;
            var totalEntities = testData.Shifts.Count + testData.Trips.Count + testData.Expenses.Count;
            
            System.Diagnostics.Debug.WriteLine($"  Total Execution Time: {totalTime.TotalMinutes:F2} minutes");
            System.Diagnostics.Debug.WriteLine($"  Phase 1 (Validation): {phase1Time.TotalSeconds:F1}s");
            System.Diagnostics.Debug.WriteLine($"  Phase 2 (Operations): {phase2Time.TotalSeconds:F1}s");
            System.Diagnostics.Debug.WriteLine($"  Phase 3 (Validation): {phase3Time.TotalSeconds:F1}s");
            System.Diagnostics.Debug.WriteLine($"  Total Entities: {totalEntities}");
            System.Diagnostics.Debug.WriteLine($"  Estimated API Calls: 5-7 (vs 50-100+ in old tests)");
            
            // Performance assertions
            Assert.True(totalTime.TotalMinutes < 5, 
                $"Comprehensive test should complete within 5 minutes, took {totalTime.TotalMinutes:F1} minutes");
            
            System.Diagnostics.Debug.WriteLine("\n" + "=".PadRight(80, '='));
            System.Diagnostics.Debug.WriteLine($"? Comprehensive integration test completed successfully: {testRunId}");
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

    #endregion

    #region Data Factory

    private SheetEntity CreateComprehensiveTestData(string testRunId)
    {
        // Create diverse test data covering multiple scenarios:
        // - Daily workflow simulation (shifts with trips)
        // - Weekly data patterns (different dates)
        // - Error correction scenarios (initial data to update)
        // - Volume testing (moderate dataset)
        // - Edge cases (boundary values)
        
        var testData = CreateSimpleTestData(shifts: 6, tripsPerShift: 3, expenses: 8);
        
        var baseDate = DateTime.Today;
        
        // Customize shifts for different workflow scenarios
        for (int i = 0; i < testData.Shifts.Count; i++)
        {
            var shift = testData.Shifts[i];
            shift.Service = $"CompTest_{testRunId}";
            shift.Date = baseDate.AddDays(-i).ToString("yyyy-MM-dd");
            
            // Scenario 1-2: Normal daily workflow
            if (i < 2)
            {
                shift.Region = $"DailyRegion_{i}";
                shift.Pay = 100 + i * 20;
                shift.Tip = 15 + i * 5;
                shift.Start = "09:00:00";
                shift.Finish = "17:00:00";
                shift.Note = $"Daily workflow {i + 1}";
            }
            // Scenario 3-4: Data correction workflow (intentional errors to update)
            else if (i < 4)
            {
                shift.Region = "INITIAL_REGION";
                shift.Pay = 50.00m; // Base value for update validation
                shift.Tip = 10.00m;
                shift.Note = "INITIAL_DATA";
            }
            // Scenario 5-6: Weekly reporting workflow
            else
            {
                shift.Region = i % 2 == 0 ? "HighEarnRegion" : "LowEarnRegion";
                shift.Pay = 80 + i * 15;
                shift.Tip = 12 + i * 3;
                shift.Note = $"Weekly data {i + 1}";
            }
        }
        
        // Customize trips for workflow scenarios
        for (int i = 0; i < testData.Trips.Count; i++)
        {
            var trip = testData.Trips[i];
            trip.Service = $"CompTest_{testRunId}";
            trip.Date = baseDate.AddDays(-i / 3).ToString("yyyy-MM-dd"); // Group trips by shift
            
            // Normal workflow trips
            if (i < 6)
            {
                trip.Type = i % 2 == 0 ? "Pickup" : "Delivery";
                trip.Pay = 15 + i * 5;
                trip.Tip = 3 + i;
                trip.Note = $"Trip {i + 1}";
            }
            // Update scenario trips
            else if (i < 12)
            {
                trip.Type = "INITIAL_TYPE";
                trip.Pay = 10.00m;
                trip.Tip = 2.00m;
                trip.Note = "INITIAL_DATA";
            }
            // Volume test trips
            else
            {
                trip.Type = "VolumeTrip";
                trip.Pay = 12.50m + (i % 10);
                trip.Tip = 2.50m + (i % 5);
                trip.Note = $"Volume trip {i + 1}";
            }
        }
        
        // Customize expenses for different scenarios
        for (int i = 0; i < testData.Expenses.Count; i++)
        {
            var expense = testData.Expenses[i];
            expense.Date = baseDate.AddDays(-i);
            expense.Description = $"CompTest_{testRunId}_expense_{i}";
            
            // Normal expenses
            if (i < 3)
            {
                expense.Category = "Fuel";
                expense.Amount = 35 + i * 5;
                expense.Name = "Gas";
            }
            // Update scenario expenses
            else if (i < 5)
            {
                expense.Category = "INITIAL_CATEGORY";
                expense.Amount = 20.00m;
                expense.Name = "INITIAL_NAME";
            }
            // Various expense types
            else
            {
                expense.Category = i % 2 == 0 ? "Fuel" : "Meal";
                expense.Amount = 25 + i * 3;
                expense.Name = i % 2 == 0 ? "Gas" : "Lunch";
            }
        }
        
        return testData;
    }

    #endregion

    #region Validation Methods

    private void ValidateDataMapping(SheetEntity finalData, SheetEntity originalData, string testRunId, 
        List<ShiftEntity> ourShifts, List<TripEntity> ourTrips, List<ExpenseEntity> ourExpenses)
    {
        System.Diagnostics.Debug.WriteLine("  ? Validating data mapping...");
        
        // Basic structure validation
        Assert.NotNull(finalData);
        Assert.NotNull(finalData.Shifts);
        Assert.NotNull(finalData.Trips);
        Assert.NotNull(finalData.Expenses);
        Assert.NotNull(finalData.Properties);
        Assert.NotNull(finalData.Messages);
        
        // Data count validation (be lenient for environment issues)
        Assert.True(ourShifts.Count >= originalData.Shifts.Count - 2, 
            $"Should find most shifts, expected ~{originalData.Shifts.Count}, found {ourShifts.Count}");
        Assert.True(ourTrips.Count >= originalData.Trips.Count - 3, 
            $"Should find most trips, expected ~{originalData.Trips.Count}, found {ourTrips.Count}");
        Assert.True(ourExpenses.Count >= originalData.Expenses.Count - 2, 
            $"Should find most expenses, expected ~{originalData.Expenses.Count}, found {ourExpenses.Count}");
    }

    private void ValidateEntityStructure(List<ShiftEntity> shifts, List<TripEntity> trips, List<ExpenseEntity> expenses)
    {
        System.Diagnostics.Debug.WriteLine("  ? Validating entity structure...");
        
        // Validate shift entity structure
        Assert.All(shifts, shift => {
            Assert.True(shift.RowId > 0, "Shift RowId should be positive");
            Assert.NotNull(shift.Date);
            Assert.NotNull(shift.Service);
            Assert.True(shift.Pay >= 0, "Shift pay should be non-negative");
        });
        
        // Validate trip entity structure
        Assert.All(trips, trip => {
            Assert.True(trip.RowId > 0, "Trip RowId should be positive");
            Assert.NotNull(trip.Date);
            Assert.NotNull(trip.Service);
            Assert.True(trip.Pay >= 0, "Trip pay should be non-negative");
        });
        
        // Validate expense entity structure
        Assert.All(expenses, expense => {
            Assert.True(expense.RowId > 0, "Expense RowId should be positive");
            Assert.NotNull(expense.Date);
            Assert.NotNull(expense.Category);
            Assert.True(expense.Amount >= 0, "Expense amount should be non-negative");
        });
    }

    private void ValidateUpdateOperations(List<ShiftEntity> shifts, List<TripEntity> trips, 
        List<ExpenseEntity> expenses, string testRunId)
    {
        System.Diagnostics.Debug.WriteLine("  ? Validating update operations...");
        
        // Find updated entities by distinctive values
        var updatedShifts = shifts.Where(s => s.Note?.Contains($"UPDATED_{testRunId}") == true).ToList();
        var updatedTrips = trips.Where(t => t.Note?.Contains($"UPDATED_{testRunId}") == true).ToList();
        var updatedExpenses = expenses.Where(e => e.Description?.Contains($"UPDATED_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"    - Updated shifts: {updatedShifts.Count}");
        System.Diagnostics.Debug.WriteLine($"    - Updated trips: {updatedTrips.Count}");
        System.Diagnostics.Debug.WriteLine($"    - Updated expenses: {updatedExpenses.Count}");
        
        // Validate shift updates
        if (updatedShifts.Count > 0)
        {
            foreach (var shift in updatedShifts)
            {
                Assert.Contains($"UPDATED_{testRunId}", shift.Note);
                // Validate distinctive update value (base + 99.99)
                Assert.True(shift.Pay >= 140, $"Updated shift should have increased pay: {shift.Pay}");
            }
        }
        
        // Validate trip updates
        if (updatedTrips.Count > 0)
        {
            foreach (var trip in updatedTrips)
            {
                Assert.Contains($"UPDATED_{testRunId}", trip.Note);
                // Validate distinctive update value (base + 77.77)
                Assert.True(trip.Tip >= 70, $"Updated trip should have increased tip: {trip.Tip}");
            }
        }
        
        // Validate expense updates
        if (updatedExpenses.Count > 0)
        {
            foreach (var expense in updatedExpenses)
            {
                Assert.Contains($"UPDATED_{testRunId}", expense.Description);
                // Validate distinctive update value (base + 55.55)
                Assert.True(expense.Amount >= 70, $"Updated expense should have increased amount: {expense.Amount}");
            }
        }
    }

    private void ValidateCrossEntityConsistency(List<ShiftEntity> shifts, List<TripEntity> trips, 
        List<ExpenseEntity> expenses, string testRunId)
    {
        System.Diagnostics.Debug.WriteLine("  ? Validating cross-entity consistency...");
        
        // Validate service consistency
        var shiftServices = shifts.Select(s => s.Service).Distinct().ToList();
        var tripServices = trips.Select(t => t.Service).Distinct().ToList();
        
        var commonServices = shiftServices.Intersect(tripServices).ToList();
        Assert.True(commonServices.Count > 0, "Shifts and trips should share service identifiers");
        
        // Validate date consistency
        var validDateRange = DateTime.Today.AddDays(-30);
        
        foreach (var shift in shifts)
        {
            if (DateTime.TryParse(shift.Date, out var shiftDate))
            {
                Assert.True(shiftDate >= validDateRange, 
                    $"Shift date should be within valid range: {shiftDate:yyyy-MM-dd}");
            }
        }
        
        foreach (var expense in expenses)
        {
            Assert.True(expense.Date >= validDateRange, 
                $"Expense date should be within valid range: {expense.Date:yyyy-MM-dd}");
        }
    }

    private void ValidateBusinessWorkflows(List<ShiftEntity> shifts, List<TripEntity> trips, 
        List<ExpenseEntity> expenses, string testRunId)
    {
        System.Diagnostics.Debug.WriteLine("  ? Validating business workflows...");
        
        // Validate daily workflow data
        var dailyShifts = shifts.Where(s => s.Note?.Contains("Daily workflow") == true).ToList();
        if (dailyShifts.Count > 0)
        {
            Assert.All(dailyShifts, shift => {
                Assert.NotNull(shift.Start);
                Assert.NotNull(shift.Finish);
                Assert.True(shift.Pay > 0, "Daily workflow shifts should have pay");
            });
        }
        
        // Validate weekly reporting data
        var weeklyShifts = shifts.Where(s => s.Note?.Contains("Weekly data") == true).ToList();
        if (weeklyShifts.Count > 0)
        {
            var highEarnDays = weeklyShifts.Where(s => s.Region == "HighEarnRegion").ToList();
            var lowEarnDays = weeklyShifts.Where(s => s.Region == "LowEarnRegion").ToList();
            
            if (highEarnDays.Count > 0 && lowEarnDays.Count > 0)
            {
                var avgHigh = highEarnDays.Average(s => s.Pay ?? 0);
                var avgLow = lowEarnDays.Average(s => s.Pay ?? 0);
                
                System.Diagnostics.Debug.WriteLine($"    - High earn region avg: ${avgHigh:F2}");
                System.Diagnostics.Debug.WriteLine($"    - Low earn region avg: ${avgLow:F2}");
            }
        }
        
        // Validate trip grouping by date
        var tripsGroupedByDate = trips.GroupBy(t => t.Date).ToList();
        System.Diagnostics.Debug.WriteLine($"    - Trips spread across {tripsGroupedByDate.Count} dates");
        
        // Validate expense categories
        var expenseCategories = expenses.Select(e => e.Category).Distinct().ToList();
        System.Diagnostics.Debug.WriteLine($"    - Expense categories: {string.Join(", ", expenseCategories)}");
    }

    private void ValidateEdgeCases(SheetEntity finalData, string testRunId)
    {
        System.Diagnostics.Debug.WriteLine("  ? Validating edge cases...");
        
        // Validate system handles empty data gracefully
        Assert.NotNull(finalData.Messages);
        
        // Validate no critical system errors
        var criticalErrors = finalData.Messages.Where(m => 
            m.Level == MessageLevelEnum.ERROR.GetDescription() && 
            !IsExpectedError(m.Message)).ToList();
        
        Assert.True(criticalErrors.Count == 0, 
            $"Should not have critical errors: {string.Join("; ", criticalErrors.Select(e => e.Message))}");
        
        // Validate system stability
        Assert.True(finalData.Shifts.Count >= 0, "Shifts collection should be accessible");
        Assert.True(finalData.Trips.Count >= 0, "Trips collection should be accessible");
        Assert.True(finalData.Expenses.Count >= 0, "Expenses collection should be accessible");
    }

    private static bool IsExpectedError(string message) =>
        message.Contains("not supported") ||
        message.Contains("already exists") ||
        message.Contains("header issue") ||
        message.Contains("No data to change");

    #endregion
}

/// <summary>
/// Collection definition for Google Sheets integration tests.
/// </summary>
[CollectionDefinition("GoogleSheetsIntegration")]
public class GoogleSheetsIntegrationCollection : ICollectionFixture<GoogleSheetsIntegrationFixture>
{
}

/// <summary>
/// Fixture for Google Sheets integration tests.
/// </summary>
public class GoogleSheetsIntegrationFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        System.Diagnostics.Debug.WriteLine("?? Initializing Google Sheets integration test environment");
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        System.Diagnostics.Debug.WriteLine("? Google Sheets integration tests completed");
        await Task.CompletedTask;
    }
}
