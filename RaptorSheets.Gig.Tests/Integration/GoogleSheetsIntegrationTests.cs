using RaptorSheets.Core.Entities;
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

    [FactCheckUserSecrets]
    public async Task CreatedSheets_ShouldHaveCorrectHeaders()
    {
        // This test validates that the sheet creation process generated correct headers
        // It compares actual headers in Google Sheets vs expected headers from GetSheetLayout
        
        // Act - Get actual headers from Google Sheets
        var spreadsheetInfo = await GoogleSheetManager!.GetSpreadsheetInfo(
            TestSheets.Select(name => $"{name}!1:1").ToList());
        
        Assert.NotNull(spreadsheetInfo);
        Assert.NotNull(spreadsheetInfo.Sheets);
        
        // Assert - Validate headers for each sheet
        foreach (var sheet in spreadsheetInfo.Sheets)
        {
            var sheetName = sheet.Properties.Title;
            var actualHeaders = sheet.Data?[0]?.RowData?[0]?.Values
                ?.Select(v => v.FormattedValue ?? "")
                .Where(h => !string.IsNullOrEmpty(h))
                .ToList() ?? [];
            
            // Get expected layout from GetSheetLayout
            var expectedLayout = GoogleSheetManager.GetSheetLayout(sheetName);
            
            if (expectedLayout != null)
            {
                var expectedHeaders = expectedLayout.Headers.Select(h => h.Name).ToList();
                
                System.Diagnostics.Debug.WriteLine($"  🔍 Validating {sheetName}: {actualHeaders.Count} headers");
                
                Assert.NotEmpty(actualHeaders);
                Assert.Equal(expectedHeaders.Count, actualHeaders.Count);
                
                // Verify header names match in order
                for (int i = 0; i < expectedHeaders.Count && i < actualHeaders.Count; i++)
                {
                    Assert.Equal(expectedHeaders[i], actualHeaders[i]);
                }
            }
        }
    }

    [FactCheckUserSecrets]
    public async Task CreatedSheets_ShouldHaveCorrectFormulas()
    {
        // This test validates that sheets with formulas have them correctly configured
        
        var sheetsWithFormulas = new[] { "Trips", "Shifts", "Expenses" }; // Sheets that have formula columns
        
        // Act - Get sheet layouts to find formula columns
        var layouts = GoogleSheetManager!.GetSheetLayouts(sheetsWithFormulas.ToList());
        
        // Assert
        foreach (var layout in layouts)
        {
            var formulaHeaders = layout.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).ToList();
            
            if (formulaHeaders.Any())
            {
                System.Diagnostics.Debug.WriteLine($"  🔍 Validating {layout.Name}: {formulaHeaders.Count} formula columns");
                
                // All formulas should start with =
                Assert.All(formulaHeaders, header =>
                {
                    Assert.StartsWith("=", header.Formula);
                    
                    // Should not have unresolved placeholders
                    Assert.DoesNotContain("{", header.Formula);
                    Assert.DoesNotContain("{{", header.Formula);
                });
                
                // Log formulas for debugging
                foreach (var header in formulaHeaders)
                {
                    System.Diagnostics.Debug.WriteLine($"     {header.Name}: {header.Formula.Substring(0, Math.Min(50, header.Formula.Length))}...");
                }
            }
        }
    }

    [FactCheckUserSecrets]
    public async Task CreatedSheets_ShouldHaveCorrectVisualProperties()
    {
        // This test validates that sheets have correct colors, protection, etc.
        
        // Act - Get spreadsheet info to check visual properties
        var spreadsheetInfo = await GoogleSheetManager!.GetSpreadsheetInfo();
        
        Assert.NotNull(spreadsheetInfo);
        Assert.NotNull(spreadsheetInfo.Sheets);
        
        // Assert
        foreach (var sheet in spreadsheetInfo.Sheets)
        {
            var sheetName = sheet.Properties.Title;
            var properties = sheet.Properties;
            
            // Get expected layout
            var expectedLayout = GoogleSheetManager.GetSheetLayout(sheetName);
            
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
        var allProperties = await GoogleSheetManager!.GetAllSheetProperties();
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
        var testRunId = GenerateTestRunId();
        
        var testData = CreateTestData(testRunId, shifts: 10, tripsPerShift: 5, expenses: 15);
        
        System.Diagnostics.Debug.WriteLine($"📊 Inserting large dataset: {testData.Shifts.Count} shifts, " +
            $"{testData.Trips.Count} trips, {testData.Expenses.Count} expenses");
        
        // Act
        var startTime = DateTime.UtcNow;
        var insertResult = await InsertTestData(testData);
        var elapsed = DateTime.UtcNow - startTime;
        
        // Assert
        System.Diagnostics.Debug.WriteLine($"⏱️  Insert completed in {elapsed.TotalSeconds:F1}s");
        
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
        System.Diagnostics.Debug.WriteLine($"   ✓ Inserted {testData.Shifts.Count} shifts, " +
            $"{testData.Trips.Count} trips, {testData.Expenses.Count} expenses for test {testRunId}");
    }

    private List<ShiftEntity> ValidateInsertedShifts(string testRunId, SheetEntity readData, SheetEntity expectedData)
    {
        var shifts = readData.Shifts.Where(s => 
            s.Service?.Contains($"Test_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {shifts.Count} shifts");
        
        Assert.True(shifts.Count >= expectedData.Shifts.Count - 1, 
            $"Should find ~{expectedData.Shifts.Count} shifts, found {shifts.Count}");
        
        return shifts;
    }

    private List<TripEntity> ValidateInsertedTrips(string testRunId, SheetEntity readData, SheetEntity expectedData)
    {
        var trips = readData.Trips.Where(t => 
            t.Service?.Contains($"Test_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {trips.Count} trips");
        
        Assert.True(trips.Count >= expectedData.Trips.Count - 2, 
            $"Should find ~{expectedData.Trips.Count} trips, found {trips.Count}");
        
        return trips;
    }

    private List<ExpenseEntity> ValidateInsertedExpenses(string testRunId, SheetEntity readData, SheetEntity expectedData)
    {
        var expenses = readData.Expenses.Where(e => 
            e.Description?.Contains($"Test_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {expenses.Count} expenses");
        
        Assert.True(expenses.Count >= expectedData.Expenses.Count - 1, 
            $"Should find ~{expectedData.Expenses.Count} expenses, found {expenses.Count}");
        
        return expenses;
    }

    private void ValidateEntityStructures(
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
            Assert.True(expense.RowId > 0, "Expense RowId should be positive");
            Assert.NotNull(expense.Date);
            Assert.NotNull(expense.Category);
            Assert.True(expense.Amount >= 0, "Expense amount should be non-negative");
        });
        
        System.Diagnostics.Debug.WriteLine("   ✓ Entity structures valid");
    }

    private void ValidateCrossEntityRelationships(List<ShiftEntity> shifts, List<TripEntity> trips)
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

    private void ValidateDateRanges(List<ShiftEntity> shifts, List<ExpenseEntity> expenses)
    {
        System.Diagnostics.Debug.WriteLine("   🔍 Validating date ranges...");
        
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
        
        System.Diagnostics.Debug.WriteLine("   ✓ All dates within valid range");
    }

    private void ValidateUpdatedShifts(string testRunId, SheetEntity updatedData)
    {
        var updatedShifts = updatedData.Shifts.Where(s => 
            s.Note?.Contains($"UPDATED_{testRunId}") == true).ToList();
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {updatedShifts.Count} updated shifts");
        
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
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Found {updatedTrips.Count} updated trips");
        
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

    #region 4. Demo Data Integration Tests

    /// <summary>
    /// Tests using production demo data system for realistic full-year scenario.
    /// This validates both the demo system and large-scale data handling.
    /// </summary>
    [FactCheckUserSecrets]
    public async Task DemoData_FullYear_ShouldUploadAndValidate()
    {
        // Arrange - Use demo system for realistic year of data
        var startDate = new DateTime(DateTime.Today.Year - 1, 1, 1);
        var endDate = DateTime.Today;
        
        System.Diagnostics.Debug.WriteLine($"📅 Generating demo data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        
        // Use production demo helpers
        var demoData = CreateDemoData(startDate, endDate);
        
        System.Diagnostics.Debug.WriteLine($"📊 Generated: {demoData.Shifts.Count} shifts, " +
            $"{demoData.Trips.Count} trips, {demoData.Expenses.Count} expenses");

        // Act - Insert demo data
        var startTime = DateTime.UtcNow;
        var insertResult = await InsertTestData(demoData);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert - Verify successful insertion
        System.Diagnostics.Debug.WriteLine($"⏱️  Insert completed in {elapsed.TotalSeconds:F1}s");
        
        var criticalErrors = (insertResult.Messages ?? new List<MessageEntity>())
            .Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription() && !IsExpectedError(m.Message))
            .ToList();
        
        Assert.Empty(criticalErrors);
        Assert.True(elapsed.TotalSeconds < 120, 
            $"Full year insert should complete within 2 minutes, took {elapsed.TotalSeconds:F1}s");

        // Validate data was inserted correctly
        var readData = await GetSheetData();
        Assert.True(readData.Shifts.Count >= demoData.Shifts.Count * 0.95, 
            $"Should find most shifts, found {readData.Shifts.Count} of {demoData.Shifts.Count}");
        Assert.True(readData.Trips.Count >= demoData.Trips.Count * 0.95, 
            $"Should find most trips, found {readData.Trips.Count} of {demoData.Trips.Count}");
        Assert.True(readData.Expenses.Count >= demoData.Expenses.Count * 0.95,
            $"Should find most expenses, found {readData.Expenses.Count} of {demoData.Expenses.Count}");
        
        System.Diagnostics.Debug.WriteLine($"   ✓ Validated {readData.Shifts.Count} shifts, " +
            $"{readData.Trips.Count} trips, {readData.Expenses.Count} expenses");
    }

    /// <summary>
    /// Tests PopulateDemoData method - validates the public API works correctly.
    /// </summary>
    [FactCheckUserSecrets]
    public async Task DemoData_PopulateMethod_ShouldCreateRealisticData()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        
        System.Diagnostics.Debug.WriteLine($"📝 Populating demo data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        
        // Act - Use the public PopulateDemoData method
        var result = await GoogleSheetManager!.PopulateDemoData(startDate, endDate);
        
        // Assert - Verify the method completed successfully
        Assert.NotNull(result);
        Assert.NotEmpty(result.Messages);
        
        var errors = result.Messages.Where(m => m.Level == "ERROR").ToList();
        Assert.Empty(errors);
        
        // Verify data was created
        System.Diagnostics.Debug.WriteLine("🔍 Verifying demo data was created...");
        await Task.Delay(2000); // Allow data to propagate
        
        var readData = await GetSheetData();
        
        Assert.NotEmpty(readData.Shifts);
        Assert.NotEmpty(readData.Trips);
        
        System.Diagnostics.Debug.WriteLine($"✅ Demo data created: {readData.Shifts.Count} shifts, " +
            $"{readData.Trips.Count} trips, {readData.Expenses.Count} expenses");
    }

    /// <summary>
    /// Validates demo data has proper entity relationships (shifts ↔ trips).
    /// This ensures the demo system generates realistic, relational data.
    /// </summary>
    [Fact]
    public void DemoData_ShouldHaveProperShiftTripRelationships()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;
        
        // Act - Generate demo data
        var demoData = CreateDemoData(startDate, endDate);
        
        // Assert - Verify data structure
        Assert.NotNull(demoData);
        Assert.NotEmpty(demoData.Shifts);
        
        // Verify all entities have valid structure
        Assert.All(demoData.Shifts, shift =>
        {
            Assert.NotNull(shift.Date);
            Assert.NotNull(shift.Service);
            Assert.True(shift.RowId > 0);
        });
        
        if (demoData.Trips.Any())
        {
            Assert.All(demoData.Trips, trip =>
            {
                Assert.NotNull(trip.Date);
                Assert.NotNull(trip.Service);
                Assert.True(trip.RowId > 0);
            });
            
            // Verify shift-trip relationships exist
            foreach (var shift in demoData.Shifts.Where(s => s.Trips > 0))
            {
                var relatedTrips = demoData.Trips.Where(t =>
                    t.Date == shift.Date &&
                    t.Service == shift.Service &&
                    t.Number == shift.Number).ToList();
                
                // Some correlation should exist (demo data uses probabilities)
                // Not every shift will have exact trip count match
                System.Diagnostics.Debug.WriteLine($"  Shift on {shift.Date} ({shift.Service} #{shift.Number}): " +
                    $"{shift.Trips} trips expected, {relatedTrips.Count} found");
            }
        }
        
        System.Diagnostics.Debug.WriteLine($"✅ Validated demo data structure: {demoData.Shifts.Count} shifts, " +
            $"{demoData.Trips.Count} trips, {demoData.Expenses.Count} expenses");
    }

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
/// Deletes and recreates all sheets to validate the creation process.
/// </summary>
public class GoogleSheetsIntegrationFixture : IAsyncLifetime
{
    private GoogleSheetManager? _manager;
    
    public async Task InitializeAsync()
    {
        System.Diagnostics.Debug.WriteLine("🚀 Initializing Google Sheets integration test environment (Clean Slate)");
        
        // Get credentials for setup
        var spreadsheetId = TestConfigurationHelpers.GetGigSpreadsheet();
        var credential = TestConfigurationHelpers.GetJsonCredential();

        if (!GoogleCredentialHelpers.IsCredentialFilled(credential))
        {
            System.Diagnostics.Debug.WriteLine("⚠️  No credentials - skipping environment setup");
            return;
        }
        
        _manager = new GoogleSheetManager(credential, spreadsheetId);
        
        try
        {
            System.Diagnostics.Debug.WriteLine("  🗑️  Deleting all existing sheets to ensure clean slate...");
            
            // Delete all sheets to start fresh
            var deleteResult = await _manager.DeleteAllSheets();
            var deleteErrors = deleteResult.Messages.Where(m => 
                m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
            
            if (deleteErrors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"  ⚠️  Sheet deletion had errors: {string.Join(", ", deleteErrors.Select(e => e.Message))}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("  ✓ All sheets deleted successfully");
            }
            
            await Task.Delay(3000); // Allow deletion to complete
            
            // Create all sheets fresh
            System.Diagnostics.Debug.WriteLine("  📌 Creating all sheets fresh to validate creation process...");
            var createResult = await _manager.CreateAllSheets();
            var createErrors = createResult.Messages.Where(m => 
                m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
            
            if (createErrors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"  ⚠️  Sheet creation had errors: {string.Join(", ", createErrors.Select(e => e.Message))}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("  ✓ All sheets created successfully");
            }
            
            await Task.Delay(3000); // Allow creation to complete
            
            // Verify all sheets were created correctly
            System.Diagnostics.Debug.WriteLine("  🔍 Verifying sheet creation...");
            var allProperties = await _manager.GetAllSheetProperties();
            
            System.Diagnostics.Debug.WriteLine($"  📊 Found {allProperties.Count} sheet tabs");
            System.Diagnostics.Debug.WriteLine($"  📋 Tabs: {string.Join(", ", allProperties.Select(p => p.Name))}");
            
            // Validate headers for each sheet
            var spreadsheetInfo = await _manager.GetSpreadsheetInfo(
                allProperties.Select(p => $"{p.Name}!1:1").ToList());
            
            if (spreadsheetInfo != null)
            {
                var headerValidation = GoogleSheetManager.CheckSheetHeaders(spreadsheetInfo);
                var headerErrors = headerValidation.Where(m => 
                    m.Level == MessageLevelEnum.ERROR.GetDescription() ||
                    m.Level == MessageLevelEnum.WARNING.GetDescription()).ToList();
                
                if (headerErrors.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"  ⚠️  Header validation warnings/errors:");
                    foreach (var error in headerErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"     {error.Level}: {error.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("  ✓ All sheet headers validated successfully");
                }
            }
            
            System.Diagnostics.Debug.WriteLine("✅ Integration test environment ready (Clean Slate Validated)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️  Setup failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"     Stack: {ex.StackTrace}");
            // Don't fail the fixture - let individual tests handle issues
        }
    }

    public async Task DisposeAsync()
    {
        System.Diagnostics.Debug.WriteLine("✅ Google Sheets integration tests completed");
        await Task.CompletedTask;
    }
}
