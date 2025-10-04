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
/// Integration tests for Google Sheets operations.
/// 
/// Test Organization:
/// - Single orchestrated flow to minimize API calls
/// - Each test validates a specific aspect during the flow
/// - Shared test data across related validations
/// - Collection fixture ensures sheets exist before tests run
/// </summary>
[Collection("GoogleSheetsIntegration")]
[Category("Integration")]
[Trait("TestType", "Comprehensive")]
public class GoogleSheetsIntegrationTests : IntegrationTestBase
{
    #region 1. Environment Setup & Validation

    [FactCheckUserSecrets]
    public async Task Environment_ShouldHaveAllRequiredSheets()
    {
        // Arrange
        SkipIfNoCredentials();
        
        // Act
        var properties = await GoogleSheetManager!.GetSheetProperties(TestSheets);
        var existingSheets = properties.Where(p => !string.IsNullOrEmpty(p.Id)).ToList();
        
        // Assert
        Assert.True(existingSheets.Count >= TestSheets.Count, 
            $"Should have at least {TestSheets.Count} sheets, found {existingSheets.Count}");
        
        foreach (var sheet in existingSheets)
        {
            Assert.NotNull(sheet.Name);
            Assert.NotNull(sheet.Id);
            Assert.NotNull(sheet.Attributes);
            Assert.True(TestSheets.Contains(sheet.Name, StringComparer.OrdinalIgnoreCase),
                $"Sheet '{sheet.Name}' should be in test sheets list");
        }
    }

    [FactCheckUserSecrets]
    public async Task Environment_SheetProperties_ShouldHaveValidStructure()
    {
        // Arrange
        SkipIfNoCredentials();
        
        // Act
        var properties = await GoogleSheetManager!.GetSheetProperties(TestSheets);
        
        // Assert
        Assert.NotEmpty(properties);
        Assert.All(properties, prop =>
        {
            Assert.NotNull(prop.Name);
            Assert.NotNull(prop.Attributes);
            
            if (!string.IsNullOrEmpty(prop.Id))
            {
                // Sheet exists - validate it has headers
                Assert.True(prop.Attributes.ContainsKey("Headers") || 
                           prop.Attributes.Count == 0, 
                           $"Sheet '{prop.Name}' should have headers or empty attributes");
            }
        });
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
        SkipIfNoCredentials();
        var testRunId = GenerateTestRunId();
        System.Diagnostics.Debug.WriteLine($"?? Starting orchestrated workflow test: {testRunId}");
        
        // Step 1: INSERT - Create comprehensive test dataset
        System.Diagnostics.Debug.WriteLine("?? Step 1: Inserting test data...");
        var testData = CreateTestData(testRunId, shifts: 5, tripsPerShift: 3, expenses: 4);
        await InsertTestData(testData);
        await Task.Delay(2000); // Allow propagation
        
        ValidateInsertResult(testRunId, testData);
        
        // Step 2: READ - Retrieve and validate inserted data
        System.Diagnostics.Debug.WriteLine("?? Step 2: Reading and validating inserted data...");
        var readData = await GetSheetData();
        
        var insertedShifts = ValidateInsertedShifts(testRunId, readData, testData);
        var insertedTrips = ValidateInsertedTrips(testRunId, readData, testData);
        var insertedExpenses = ValidateInsertedExpenses(testRunId, readData, testData);
        
        ValidateEntityStructures(insertedShifts, insertedTrips, insertedExpenses);
        ValidateCrossEntityRelationships(insertedShifts, insertedTrips);
        ValidateDateRanges(insertedShifts, insertedExpenses);
        
        // Step 3: UPDATE - Modify subset of data
        System.Diagnostics.Debug.WriteLine("??  Step 3: Updating data...");
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
        System.Diagnostics.Debug.WriteLine("?? Step 4: Validating updates...");
        var updatedData = await GetSheetData();
        
        ValidateUpdatedShifts(testRunId, updatedData);
        ValidateUpdatedTrips(testRunId, updatedData);
        ValidateUpdatedExpenses(testRunId, updatedData);
        
        System.Diagnostics.Debug.WriteLine($"? Orchestrated workflow completed successfully: {testRunId}");
    }

    #endregion

    #region 3. Focused Scenario Tests

    [FactCheckUserSecrets]
    public async Task Workflow_DailyOperation_ShouldRecordShiftWithTrips()
    {
        // Arrange - Simulates daily workflow: start shift, record trips, end shift
        SkipIfNoCredentials();
        var testRunId = GenerateTestRunId();
        
        var testData = new SheetEntity();
        var today = DateTime.Today;
        
        // Create a shift for today
        var shift = new ShiftEntity
        {
            RowId = 2,
            Action = ActionTypeEnum.INSERT.GetDescription(),
            Date = today.ToString("yyyy-MM-dd"),
            Service = $"Test_{testRunId}",
            Region = "DailyWorkflow",
            Start = "09:00:00",
            Finish = "17:00:00",
            Pay = 120m,
            Tip = 25m,
            Note = "Daily workflow test"
        };
        testData.Shifts.Add(shift);
        
        // Add trips for this shift
        for (int i = 0; i < 3; i++)
        {
            var trip = new TripEntity
            {
                RowId = 2 + i,
                Action = ActionTypeEnum.INSERT.GetDescription(),
                Date = today.ToString("yyyy-MM-dd"),
                Service = $"Test_{testRunId}",
                Type = i % 2 == 0 ? "Pickup" : "Delivery",
                Pay = 15m + i * 5,
                Tip = 3m + i,
                Note = $"Daily trip {i + 1}"
            };
            testData.Trips.Add(trip);
        }
        
        // Act
        await InsertTestData(testData);
        await Task.Delay(2000);
        
        // Assert
        var readData = await GetSheetData();
        var dailyShifts = readData.Shifts.Where(s => 
            s.Service?.Contains($"Test_{testRunId}") == true && 
            s.Region == "DailyWorkflow").ToList();
        var dailyTrips = readData.Trips.Where(t => 
            t.Service?.Contains($"Test_{testRunId}") == true).ToList();
        
        Assert.Single(dailyShifts);
        Assert.True(dailyTrips.Count >= 2, $"Should have at least 2 daily trips, found {dailyTrips.Count}");
        
        var workflowShift = dailyShifts.First();
        Assert.NotNull(workflowShift.Start);
        Assert.NotNull(workflowShift.Finish);
        Assert.True(workflowShift.Pay > 0, "Daily shift should have pay recorded");
    }

    [FactCheckUserSecrets]
    public async Task Workflow_ExpenseTracking_ShouldRecordMultipleCategories()
    {
        // Arrange - Simulates expense tracking workflow
        SkipIfNoCredentials();
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
                Action = ActionTypeEnum.INSERT.GetDescription(),
                Date = today.AddDays(-i),
                Category = categories[i],
                Name = $"{categories[i]} Item",
                Amount = 25m + i * 10,
                Description = $"Test_{testRunId}_expense"
            };
            testData.Expenses.Add(expense);
        }
        
        // Act
        await InsertTestData(testData);
        await Task.Delay(2000);
        
        // Assert
        var readData = await GetSheetData();
        var ourExpenses = readData.Expenses.Where(e => 
            e.Description?.Contains($"Test_{testRunId}") == true).ToList();
        
        var expenseCategories = ourExpenses.Select(e => e.Category).Distinct().ToList();
        
        Assert.True(ourExpenses.Count >= 2, $"Should have at least 2 expenses, found {ourExpenses.Count}");
        Assert.True(expenseCategories.Count >= 2, 
            $"Should have multiple expense categories, found {expenseCategories.Count}");
    }

    [FactCheckUserSecrets]
    public async Task LargeDataset_ShouldHandleVolumeEfficiently()
    {
        // Arrange
        SkipIfNoCredentials();
        var testRunId = GenerateTestRunId();
        
        var testData = CreateTestData(testRunId, shifts: 10, tripsPerShift: 5, expenses: 15);
        
        System.Diagnostics.Debug.WriteLine($"?? Inserting large dataset: {testData.Shifts.Count} shifts, " +
            $"{testData.Trips.Count} trips, {testData.Expenses.Count} expenses");
        
        // Act
        var startTime = DateTime.UtcNow;
        var insertResult = await InsertTestData(testData);
        var elapsed = DateTime.UtcNow - startTime;
        
        // Assert
        System.Diagnostics.Debug.WriteLine($"??  Insert completed in {elapsed.TotalSeconds:F1}s");
        
        var criticalErrors = insertResult.Messages.Where(m => 
            m.Level == MessageLevelEnum.ERROR.GetDescription() && 
            !IsExpectedError(m.Message)).ToList();
        
        Assert.Empty(criticalErrors);
        Assert.True(elapsed.TotalSeconds < 30, 
            $"Large insert should complete within 30 seconds, took {elapsed.TotalSeconds:F1}s");
    }

    #endregion

    #region Validation Helper Methods

    private void ValidateInsertResult(string testRunId, SheetEntity testData)
    {
        System.Diagnostics.Debug.WriteLine($"   ? Inserted {testData.Shifts.Count} shifts, " +
            $"{testData.Trips.Count} trips, {testData.Expenses.Count} expenses for test {testRunId}");
    }

    private List<ShiftEntity> ValidateInsertedShifts(string testRunId, SheetEntity readData, SheetEntity expectedData)
    {
        var shifts = readData.Shifts.Where(s => 
            s.Service?.Contains($"Test_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ? Found {shifts.Count} shifts");
        
        Assert.True(shifts.Count >= expectedData.Shifts.Count - 1, 
            $"Should find ~{expectedData.Shifts.Count} shifts, found {shifts.Count}");
        
        return shifts;
    }

    private List<TripEntity> ValidateInsertedTrips(string testRunId, SheetEntity readData, SheetEntity expectedData)
    {
        var trips = readData.Trips.Where(t => 
            t.Service?.Contains($"Test_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ? Found {trips.Count} trips");
        
        Assert.True(trips.Count >= expectedData.Trips.Count - 2, 
            $"Should find ~{expectedData.Trips.Count} trips, found {trips.Count}");
        
        return trips;
    }

    private List<ExpenseEntity> ValidateInsertedExpenses(string testRunId, SheetEntity readData, SheetEntity expectedData)
    {
        var expenses = readData.Expenses.Where(e => 
            e.Description?.Contains($"Test_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ? Found {expenses.Count} expenses");
        
        Assert.True(expenses.Count >= expectedData.Expenses.Count - 1, 
            $"Should find ~{expectedData.Expenses.Count} expenses, found {expenses.Count}");
        
        return expenses;
    }

    private void ValidateEntityStructures(
        List<ShiftEntity> shifts, 
        List<TripEntity> trips, 
        List<ExpenseEntity> expenses)
    {
        System.Diagnostics.Debug.WriteLine("   ?? Validating entity structures...");
        
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
            Assert.True(expense.RowId > 0, "Expense RowId should be positive");
            Assert.NotNull(expense.Date);
            Assert.NotNull(expense.Category);
            Assert.True(expense.Amount >= 0, "Expense amount should be non-negative");
        });
        
        System.Diagnostics.Debug.WriteLine("   ? Entity structures valid");
    }

    private void ValidateCrossEntityRelationships(List<ShiftEntity> shifts, List<TripEntity> trips)
    {
        System.Diagnostics.Debug.WriteLine("   ?? Validating cross-entity relationships...");
        
        var shiftServices = shifts.Select(s => s.Service).Distinct().ToList();
        var tripServices = trips.Select(t => t.Service).Distinct().ToList();
        var commonServices = shiftServices.Intersect(tripServices).ToList();
        
        Assert.NotEmpty(shifts);
        Assert.NotEmpty(trips);
        Assert.True(commonServices.Count > 0, 
            "Shifts and trips should share service identifiers");
        
        System.Diagnostics.Debug.WriteLine($"   ? Found {commonServices.Count} common services between shifts and trips");
    }

    private void ValidateDateRanges(List<ShiftEntity> shifts, List<ExpenseEntity> expenses)
    {
        System.Diagnostics.Debug.WriteLine("   ?? Validating date ranges...");
        
        var validDateRange = DateTime.Today.AddDays(-30);
        
        Assert.All(shifts, shift =>
        {
            if (DateTime.TryParse(shift.Date, out var shiftDate))
            {
                Assert.True(shiftDate >= validDateRange, 
                    $"Shift date should be within valid range: {shiftDate:yyyy-MM-dd}");
            }
        });
        
        Assert.All(expenses, expense =>
        {
            Assert.True(expense.Date >= validDateRange, 
                $"Expense date should be within valid range: {expense.Date:yyyy-MM-dd}");
        });
        
        System.Diagnostics.Debug.WriteLine("   ? All dates within valid range");
    }

    private void ValidateUpdatedShifts(string testRunId, SheetEntity updatedData)
    {
        var updatedShifts = updatedData.Shifts.Where(s => 
            s.Note?.Contains($"UPDATED_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ? Found {updatedShifts.Count} updated shifts");
        
        Assert.NotEmpty(updatedShifts);
        Assert.All(updatedShifts, shift =>
        {
            Assert.Contains($"UPDATED_{testRunId}", shift.Note);
            Assert.True(shift.Pay >= 99, $"Updated shift should have increased pay: {shift.Pay}");
        });
    }

    private void ValidateUpdatedTrips(string testRunId, SheetEntity updatedData)
    {
        var updatedTrips = updatedData.Trips.Where(t => 
            t.Note?.Contains($"UPDATED_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ? Found {updatedTrips.Count} updated trips");
        
        Assert.NotEmpty(updatedTrips);
        Assert.All(updatedTrips, trip =>
        {
            Assert.Contains($"UPDATED_{testRunId}", trip.Note);
            Assert.True(trip.Tip >= 70, $"Updated trip should have increased tip: {trip.Tip}");
        });
    }

    private void ValidateUpdatedExpenses(string testRunId, SheetEntity updatedData)
    {
        var updatedExpenses = updatedData.Expenses.Where(e => 
            e.Description?.Contains($"UPDATED_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ? Found {updatedExpenses.Count} updated expenses");
        
        Assert.NotEmpty(updatedExpenses);
        Assert.All(updatedExpenses, expense =>
        {
            Assert.Contains($"UPDATED_{testRunId}", expense.Description);
            Assert.True(expense.Amount >= 55, $"Updated expense should have increased amount: {expense.Amount}");
        });
    }

    #endregion

    #region Helper Methods

    private static string GenerateTestRunId() => DateTimeOffset.UtcNow.ToString("HHmmss");

    private SheetEntity CreateTestData(string testRunId, int shifts, int tripsPerShift, int expenses)
    {
        var testData = CreateSimpleTestData(shifts, tripsPerShift, expenses);
        var baseDate = DateTime.Today;
        
        // Tag all data with test run ID
        foreach (var shift in testData.Shifts)
        {
            shift.Service = $"Test_{testRunId}";
            shift.Date = baseDate.AddDays(-testData.Shifts.IndexOf(shift)).ToString("yyyy-MM-dd");
        }
        
        foreach (var trip in testData.Trips)
        {
            trip.Service = $"Test_{testRunId}";
            trip.Date = baseDate.AddDays(-testData.Trips.IndexOf(trip) / tripsPerShift).ToString("yyyy-MM-dd");
        }
        
        foreach (var expense in testData.Expenses)
        {
            expense.Description = $"Test_{testRunId}_expense";
            expense.Date = baseDate.AddDays(-testData.Expenses.IndexOf(expense));
        }
        
        return testData;
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
/// Handles one-time environment setup for all tests in the collection.
/// </summary>
public class GoogleSheetsIntegrationFixture : IAsyncLifetime
{
    private GoogleSheetManager? _manager;
    
    public async Task InitializeAsync()
    {
        System.Diagnostics.Debug.WriteLine("?? Initializing Google Sheets integration test environment");
        
        // Get credentials for setup
        var spreadsheetId = TestConfigurationHelpers.GetGigSpreadsheet();
        var credential = TestConfigurationHelpers.GetJsonCredential();

        if (!GoogleCredentialHelpers.IsCredentialFilled(credential))
        {
            System.Diagnostics.Debug.WriteLine("??  No credentials - skipping environment setup");
            return;
        }
        
        _manager = new GoogleSheetManager(credential, spreadsheetId);
        
        try
        {
            System.Diagnostics.Debug.WriteLine("  ?? Ensuring sheets exist in correct order...");
            
            // Get current sheet state
            var allTabNames = await _manager.GetAllSheetTabNames();
            var allProperties = await _manager.GetAllSheetProperties();
            var requiredSheets = allProperties.Select(p => p.Name).ToList();
            
            // Check if all required sheets exist
            var missingSheets = requiredSheets.Where(required => 
                !allTabNames.Any(tab => string.Equals(tab, required, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            
            if (missingSheets.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"  ?? Missing sheets detected: {string.Join(", ", missingSheets)}");
                System.Diagnostics.Debug.WriteLine("  ?? Creating all sheets in correct order...");
                
                // Create all sheets to ensure proper ordering
                var createResult = await _manager.CreateAllSheets();
                var createErrors = createResult.Messages.Where(m => 
                    m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
                
                if (createErrors.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"  ??  Sheet creation had errors: {string.Join(", ", createErrors.Select(e => e.Message))}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("  ? All sheets created successfully");
                }
                
                await Task.Delay(2000); // Allow creation to complete
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("  ? All required sheets already exist");
            }
            
            System.Diagnostics.Debug.WriteLine("? Integration test environment ready");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"??  Setup failed: {ex.Message}");
            // Don't fail the fixture - let individual tests handle issues
        }
    }

    public async Task DisposeAsync()
    {
        System.Diagnostics.Debug.WriteLine("? Google Sheets integration tests completed");
        await Task.CompletedTask;
    }
}
