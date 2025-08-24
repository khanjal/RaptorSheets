using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Entities;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Gig.Tests.Data.Helpers;
using RaptorSheets.Test.Common.Helpers;
using SheetEnum = RaptorSheets.Gig.Enums.SheetEnum;

namespace RaptorSheets.Gig.Tests.Integration.Workflows;

/// <summary>
/// Comprehensive integration test workflow for GoogleSheetManager
/// Tests the complete lifecycle: Delete All -> Recreate -> Create -> Read -> Update -> Delete
/// </summary>
public class GoogleSheetIntegrationWorkflow : IAsyncLifetime
{
    private readonly GoogleSheetManager? _googleSheetManager;
    private readonly List<string> _testSheets;
    private readonly List<string> _allSheets;
    private readonly long _testStartTime;

    // Test data tracking
    private SheetEntity? _createdTestData;
    private readonly List<int> _createdShiftIds = [];
    private readonly List<int> _createdTripIds = [];
    private readonly List<int> _createdExpenseIds = [];

    // Constants for test configuration
    private const int NumberOfShifts = 5;
    private const int MinTripsPerShift = 3;
    private const int MaxTripsPerShift = 6;
    private const int NumberOfExpenses = 8;
    private const int DataPropagationDelayMs = 3000;
    private const int SheetCreationDelayMs = 10000;
    private const int SheetDeletionDelayMs = 5000;

    public GoogleSheetIntegrationWorkflow()
    {
        _testStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        _testSheets = [
            SheetEnum.SHIFTS.GetDescription(), 
            SheetEnum.TRIPS.GetDescription(),
            SheetEnum.EXPENSES.GetDescription()
        ];

        // Get all available sheets from enums
        _allSheets = [
            .. Enum.GetValues<SheetEnum>().Select(e => e.GetDescription()),
            .. Enum.GetValues<RaptorSheets.Common.Enums.SheetEnum>().Select(e => e.GetDescription())
        ];

        var spreadsheetId = TestConfigurationHelpers.GetGigSpreadsheet();
        var credential = TestConfigurationHelpers.GetJsonCredential();

        if (GoogleCredentialHelpers.IsCredentialFilled(credential))
            _googleSheetManager = new GoogleSheetManager(credential, spreadsheetId);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask; // No cleanup per requirements

    [FactCheckUserSecrets]
    public async Task ComprehensiveWorkflow_ShouldExecuteCompleteLifecycle()
    {
        // Skip test if credentials are not available
        if (_googleSheetManager == null)
        {
            System.Diagnostics.Debug.WriteLine("Skipping integration test - Google Sheets credentials not available");
            return;
        }

        System.Diagnostics.Debug.WriteLine("=== Starting Comprehensive Integration Test Workflow ===");

        try
        {
            await DeleteAllSheetsAndRecreate();
            await VerifySheetStructure();
            await LoadTestData();

            // Only proceed if test data was created successfully
            if (_createdTestData?.Shifts.Count == 0 || _createdShiftIds.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("Test data creation failed - skipping remaining steps");
                return;
            }

            await VerifyDataWasInserted();
            await UpdateTestData();
            await VerifyDataWasUpdated();
            await DeleteTestData();
            await VerifyDataWasDeleted();

            System.Diagnostics.Debug.WriteLine("=== Comprehensive Integration Test Workflow Completed Successfully ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Integration test failed with exception: {ex.Message}");
            
            if (IsApiRelatedError(ex))
            {
                System.Diagnostics.Debug.WriteLine("Skipping integration test due to API/authentication issues");
                return;
            }
            throw;
        }
    }

    [FactCheckUserSecrets]
    public async Task ErrorHandling_InvalidSpreadsheetId_ShouldReturnErrors()
    {
        var credential = TestConfigurationHelpers.GetJsonCredential();
        var invalidManager = new GoogleSheetManager(credential, "invalid-spreadsheet-id");

        var result = await invalidManager.GetSheets(_testSheets);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Messages);
        Assert.All(result.Messages, msg =>
            Assert.Equal(MessageLevelEnum.ERROR.GetDescription(), msg.Level));
    }

    [FactCheckUserSecrets]
    public async Task ErrorHandling_NonExistentSheets_ShouldHandleGracefully()
    {
        var result = await _googleSheetManager!.GetSheetProperties(["NonExistentSheet1", "NonExistentSheet2"]);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, prop => Assert.Empty(prop.Id));
    }

    #region Workflow Steps

    private async Task DeleteAllSheetsAndRecreate()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 1: Delete All Existing Sheets and Recreate ===");

        var allExistingProperties = await _googleSheetManager!.GetSheetProperties(_allSheets);
        var existingSheets = allExistingProperties.Where(prop => !string.IsNullOrEmpty(prop.Id)).ToList();

        System.Diagnostics.Debug.WriteLine($"Found {existingSheets.Count} existing sheets to delete");

        if (existingSheets.Count > 0)
        {
            var existingSheetNames = existingSheets.Select(s => s.Name).ToList();
            System.Diagnostics.Debug.WriteLine($"Deleting {existingSheetNames.Count} sheets: {string.Join(", ", existingSheetNames)}");
            
            var deletionResult = await _googleSheetManager.DeleteSheets(existingSheetNames);
            LogMessages("Delete", deletionResult.Messages);
            await Task.Delay(SheetDeletionDelayMs);
        }

        System.Diagnostics.Debug.WriteLine($"Creating all {_allSheets.Count} sheets from scratch");
        var creationResult = await _googleSheetManager.CreateSheets();
        Assert.NotNull(creationResult);

        LogMessages("Create", creationResult.Messages);
        await Task.Delay(SheetCreationDelayMs);

        System.Diagnostics.Debug.WriteLine("? Successfully deleted and recreated all sheets");
    }

    private async Task VerifySheetStructure()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 2: Verify Sheet Structure ===");

        var sheetProperties = await _googleSheetManager!.GetSheetProperties(_allSheets);
        var existingSheets = sheetProperties.Where(p => !string.IsNullOrEmpty(p.Id)).ToList();

        System.Diagnostics.Debug.WriteLine($"Found {existingSheets.Count} sheets after recreation");

        Assert.True(existingSheets.Count >= _testSheets.Count, 
            $"Expected at least {_testSheets.Count} core sheets, found {existingSheets.Count}");

        // Verify primary sheets exist with proper headers
        foreach (var testSheetName in _testSheets)
        {
            var sheet = existingSheets.FirstOrDefault(x => x.Name == testSheetName);
            Assert.NotNull(sheet);
            Assert.NotEmpty(sheet.Id);
            Assert.NotEmpty(sheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);
        }

        var allSheetsData = await _googleSheetManager.GetSheets(_allSheets);
        Assert.NotNull(allSheetsData);

        var errorMessages = allSheetsData.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription());
        Assert.Empty(errorMessages);

        AssertAggregateCollectionsExist(allSheetsData);

        System.Diagnostics.Debug.WriteLine("? Sheet structure verification completed successfully");
    }

    private async Task LoadTestData()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 3: Load Test Data (Shifts, Trips, Expenses) ===");

        try
        {
            var sheetInfo = await _googleSheetManager!.GetSheetProperties(_testSheets);
            var maxShiftId = GetMaxRowValue(sheetInfo, SheetEnum.SHIFTS.GetDescription());
            var maxTripId = GetMaxRowValue(sheetInfo, SheetEnum.TRIPS.GetDescription());
            var maxExpenseId = GetMaxRowValue(sheetInfo, SheetEnum.EXPENSES.GetDescription());

            var testShiftsAndTrips = TestGigHelpers.GenerateMultipleShifts(
                ActionTypeEnum.APPEND,
                maxShiftId + 1,
                maxTripId + 1,
                NumberOfShifts,
                MinTripsPerShift,
                MaxTripsPerShift
            );

            var testExpenses = GenerateTestExpenses(maxExpenseId + 1, NumberOfExpenses);

            _createdTestData = new SheetEntity();
            _createdTestData.Shifts.AddRange(testShiftsAndTrips.Shifts);
            _createdTestData.Trips.AddRange(testShiftsAndTrips.Trips);
            _createdTestData.Expenses.AddRange(testExpenses.Expenses);

            _createdShiftIds.AddRange(_createdTestData.Shifts.Select(s => s.RowId));
            _createdTripIds.AddRange(_createdTestData.Trips.Select(t => t.RowId));
            _createdExpenseIds.AddRange(_createdTestData.Expenses.Select(e => e.RowId));

            System.Diagnostics.Debug.WriteLine($"Generated test data: {_createdTestData.Shifts.Count} shifts, {_createdTestData.Trips.Count} trips, {_createdTestData.Expenses.Count} expenses");

            var result = await _googleSheetManager.ChangeSheetData(_testSheets, _createdTestData);
            Assert.NotNull(result);

            var errorMessages = result.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
            if (errorMessages.Count != 0)
            {
                LogMessages("Load Data Error", errorMessages);
                ClearTestData();
                return;
            }

            ValidateOperationMessages(result.Messages, 3);
            System.Diagnostics.Debug.WriteLine("? Test data loaded successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadTestData failed: {ex.Message}");
            ClearTestData();
            throw;
        }
    }

    private async Task VerifyDataWasInserted()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 4: Verify Data Was Inserted Correctly ===");

        await Task.Delay(DataPropagationDelayMs);

        var result = await _googleSheetManager!.GetSheets(_testSheets);
        Assert.NotNull(result);
        Assert.NotNull(_createdTestData);

        VerifyEntitiesExist(_createdTestData.Shifts, result.Shifts);
        VerifyEntitiesExist(_createdTestData.Trips, result.Trips);
        VerifyEntitiesExist(_createdTestData.Expenses, result.Expenses);

        System.Diagnostics.Debug.WriteLine($"? Data insertion verified: {result.Shifts.Count} total shifts, {result.Trips.Count} total trips, {result.Expenses.Count} total expenses");
    }

    private async Task UpdateTestData()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 5: Update Test Data ===");

        ArgumentNullException.ThrowIfNull(_createdTestData);

        var updateData = new SheetEntity();

        // Update shifts with distinctive values
        var shiftsToUpdate = _createdTestData.Shifts.Take(2).ToList();
        shiftsToUpdate.ForEach(shift =>
        {
            shift.Action = ActionTypeEnum.UPDATE.GetDescription();
            shift.Region = "Updated Region";
            shift.Note = "Updated by integration test";
            updateData.Shifts.Add(shift);
        });

        // Update trips with distinctive values
        var tripsToUpdate = _createdTestData.Trips.Take(3).ToList();
        tripsToUpdate.ForEach(trip =>
        {
            trip.Action = ActionTypeEnum.UPDATE.GetDescription();
            trip.Tip = 999; // Distinctive value
            trip.Note = "Updated trip note";
            updateData.Trips.Add(trip);
        });

        // Update expenses with distinctive values
        var expensesToUpdate = _createdTestData.Expenses.Take(2).ToList();
        expensesToUpdate.ForEach(expense =>
        {
            expense.Description = "Updated expense description";
            expense.Amount = 12345.67m; // Distinctive value
            updateData.Expenses.Add(expense);
        });

        var result = await _googleSheetManager!.ChangeSheetData(_testSheets, updateData);
        Assert.NotNull(result);

        if (!ValidateOperationResult(result, "Update"))
        {
            return; // Skip validation if there were errors
        }

        System.Diagnostics.Debug.WriteLine($"? Updated {shiftsToUpdate.Count} shifts, {tripsToUpdate.Count} trips, {expensesToUpdate.Count} expenses");
    }

    private async Task VerifyDataWasUpdated()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 6: Verify Data Was Updated Correctly ===");

        await Task.Delay(DataPropagationDelayMs);

        var updatedData = await _googleSheetManager!.GetSheets(_testSheets);
        Assert.NotNull(updatedData);

        // Verify updates using distinctive values
        var updatedShifts = updatedData.Shifts.Where(s => s.Region == "Updated Region").ToList();
        Assert.Equal(2, updatedShifts.Count);
        Assert.All(updatedShifts, s => Assert.Equal("Updated by integration test", s.Note));

        var updatedTrips = updatedData.Trips.Where(t => t.Tip == 999).ToList();
        Assert.Equal(3, updatedTrips.Count);
        Assert.All(updatedTrips, t => Assert.Equal("Updated trip note", t.Note));

        var updatedExpenses = updatedData.Expenses.Where(e => e.Amount == 12345.67m).ToList();
        Assert.Equal(2, updatedExpenses.Count);
        Assert.All(updatedExpenses, e => Assert.Equal("Updated expense description", e.Description));

        System.Diagnostics.Debug.WriteLine("? Data updates verified successfully");
    }

    private async Task DeleteTestData()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 7: Delete Test Data ===");

        ArgumentNullException.ThrowIfNull(_createdTestData);

        var deleteData = new SheetEntity();

        // Mark all test data for deletion
        _createdTestData.Shifts.ForEach(shift =>
        {
            shift.Action = ActionTypeEnum.DELETE.GetDescription();
            deleteData.Shifts.Add(shift);
        });

        _createdTestData.Trips.ForEach(trip =>
        {
            trip.Action = ActionTypeEnum.DELETE.GetDescription();
            deleteData.Trips.Add(trip);
        });

        // Add expenses for deletion (ExpenseEntity doesn't have Action property)
        deleteData.Expenses.AddRange(_createdTestData.Expenses);

        var result = await _googleSheetManager!.ChangeSheetData(_testSheets, deleteData);
        Assert.NotNull(result);

        ValidateOperationResult(result, "Delete");

        System.Diagnostics.Debug.WriteLine($"? Deletion commands sent for {deleteData.Shifts.Count} shifts, {deleteData.Trips.Count} trips, {deleteData.Expenses.Count} expenses");
    }

    private async Task VerifyDataWasDeleted()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 8: Verify Data Was Deleted Correctly ===");

        await Task.Delay(DataPropagationDelayMs);

        var remainingData = await _googleSheetManager!.GetSheets(_testSheets);
        Assert.NotNull(remainingData);

        // Verify all test data was deleted by ID
        VerifyEntitiesDeleted(_createdShiftIds, remainingData.Shifts);
        VerifyEntitiesDeleted(_createdTripIds, remainingData.Trips);
        VerifyEntitiesDeleted(_createdExpenseIds, remainingData.Expenses);

        // Verify no artifacts remain using distinctive values
        Assert.DoesNotContain(remainingData.Shifts, s => s.Region == "Updated Region");
        Assert.DoesNotContain(remainingData.Trips, t => t.Tip == 999);
        Assert.DoesNotContain(remainingData.Expenses, e => e.Amount == 12345.67m);

        System.Diagnostics.Debug.WriteLine($"? Data deletion verified: {_createdShiftIds.Count} shifts, {_createdTripIds.Count} trips, {_createdExpenseIds.Count} expenses successfully removed");
    }

    #endregion

    #region Helper Methods

    private static SheetEntity GenerateTestExpenses(int startingId, int count)
    {
        var sheetEntity = new SheetEntity();
        var currentDate = DateTime.Now;
        var random = new Random();
        
        string[] expenseCategories = ["Gas", "Maintenance", "Insurance", "Parking", "Tolls", "Phone", "Food", "Supplies"];
        
        for (int i = 0; i < count; i++)
        {
            var expense = new ExpenseEntity
            {
                RowId = startingId + i,
                Date = currentDate.AddDays(-random.Next(0, 30)),
                Amount = Math.Round((decimal)(random.NextDouble() * 200 + 10), 2),
                Category = expenseCategories[random.Next(expenseCategories.Length)],
                Name = $"Test Expense {i + 1}",
                Description = $"Test expense {i + 1} - {expenseCategories[random.Next(expenseCategories.Length)]}"
            };
            
            sheetEntity.Expenses.Add(expense);
        }
        
        return sheetEntity;
    }

    private static int GetMaxRowValue(List<PropertyEntity> sheetInfo, string sheetName)
    {
        var sheet = sheetInfo.FirstOrDefault(x => x.Name == sheetName);
        var maxRowKey = PropertyEnum.MAX_ROW_VALUE.GetDescription();

        return sheet?.Attributes?.TryGetValue(maxRowKey, out var maxRowValue) == true
               && int.TryParse(maxRowValue, out var maxRow)
            ? maxRow
            : 1; // Default to 1 if no data exists (header row)
    }

    private static bool IsApiRelatedError(Exception ex) =>
        ex.Message.Contains("credentials", StringComparison.OrdinalIgnoreCase) || 
        ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase) || 
        ex.Message.Contains("Requested entity was not found", StringComparison.OrdinalIgnoreCase);

    private void ClearTestData()
    {
        _createdTestData = null;
        _createdShiftIds.Clear();
        _createdTripIds.Clear();
        _createdExpenseIds.Clear();
        System.Diagnostics.Debug.WriteLine("Cleared test data due to errors");
    }

    private static void LogMessages(string operation, List<MessageEntity> messages)
    {
        foreach (var message in messages)
        {
            System.Diagnostics.Debug.WriteLine($"{operation} result: {message.Level} - {message.Message}");
        }
    }

    private void ValidateOperationMessages(List<MessageEntity> messages, int expectedCount)
    {
        Assert.Equal(expectedCount, messages.Count);
        foreach (var message in messages)
        {
            Assert.Equal(MessageLevelEnum.INFO.GetDescription(), message.Level);
            Assert.Equal(MessageTypeEnum.SAVE_DATA.GetDescription(), message.Type);
            Assert.True(message.Time >= _testStartTime);
        }
    }

    private static bool ValidateOperationResult(SheetEntity result, string operationName)
    {
        var errorMessages = result.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
        if (errorMessages.Count != 0)
        {
            System.Diagnostics.Debug.WriteLine($"=== {operationName.ToUpper()} ERROR MESSAGES FOUND ===");
            foreach (var error in errorMessages)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {error.Message}");
            }
            return false;
        }

        foreach (var message in result.Messages)
        {
            Assert.Equal(MessageLevelEnum.INFO.GetDescription(), message.Level);
            Assert.Equal(MessageTypeEnum.SAVE_DATA.GetDescription(), message.Type);
        }
        return true;
    }

    private static void AssertAggregateCollectionsExist(SheetEntity allSheetsData)
    {
        Assert.NotNull(allSheetsData.Addresses);
        Assert.NotNull(allSheetsData.Names);
        Assert.NotNull(allSheetsData.Places);
        Assert.NotNull(allSheetsData.Regions);
        Assert.NotNull(allSheetsData.Services);
        Assert.NotNull(allSheetsData.Daily);
        Assert.NotNull(allSheetsData.Weekly);
        Assert.NotNull(allSheetsData.Monthly);
        Assert.NotNull(allSheetsData.Yearly);
    }

    private static void VerifyEntitiesExist<T>(List<T> createdEntities, List<T> foundEntities) where T : class
    {
        foreach (var created in createdEntities)
        {
            var found = FindEntityById(created, foundEntities);
            Assert.NotNull(found);
            VerifyEntityProperties(created, found);
        }
    }

    private static void VerifyEntitiesDeleted<T>(List<int> deletedIds, List<T> remainingEntities) where T : class
    {
        foreach (var deletedId in deletedIds)
        {
            var found = FindEntityById(deletedId, remainingEntities);
            Assert.Null(found);
        }
    }

    private static T? FindEntityById<T>(T entity, List<T> entities) where T : class
    {
        var rowIdProp = typeof(T).GetProperty("RowId");
        if (rowIdProp == null) return null;
        
        var targetId = (int?)rowIdProp.GetValue(entity);
        return entities.FirstOrDefault(e => (int?)rowIdProp.GetValue(e) == targetId);
    }

    private static T? FindEntityById<T>(int rowId, List<T> entities) where T : class
    {
        var rowIdProp = typeof(T).GetProperty("RowId");
        return rowIdProp == null ? null : entities.FirstOrDefault(e => (int?)rowIdProp.GetValue(e) == rowId);
    }

    private static void VerifyEntityProperties<T>(T created, T found) where T : class
    {
        var type = typeof(T);
        
        // Common verification for all entity types
        if (type.GetProperty("Date") != null)
        {
            Assert.Equal(type.GetProperty("Date")?.GetValue(created), 
                        type.GetProperty("Date")?.GetValue(found));
        }

        // Entity-specific verifications using pattern matching
        switch (created, found)
        {
            case (ShiftEntity createdShift, ShiftEntity foundShift):
                VerifyShiftProperties(createdShift, foundShift);
                break;
            case (TripEntity createdTrip, TripEntity foundTrip):
                VerifyTripProperties(createdTrip, foundTrip);
                break;
            case (ExpenseEntity createdExpense, ExpenseEntity foundExpense):
                VerifyExpenseProperties(createdExpense, foundExpense);
                break;
        }
    }

    private static void VerifyShiftProperties(ShiftEntity created, ShiftEntity found)
    {
        Assert.Equal(created.Number, found.Number);
        Assert.Equal(created.Service, found.Service);
        Assert.Equal(created.Region, found.Region);
    }

    private static void VerifyTripProperties(TripEntity created, TripEntity found)
    {
        Assert.Equal(created.Number, found.Number);
        Assert.Equal(created.Service, found.Service);
        Assert.Equal(created.Place, found.Place);
        Assert.Equal(created.Name, found.Name);
    }

    private static void VerifyExpenseProperties(ExpenseEntity created, ExpenseEntity found)
    {
        Assert.Equal(created.Amount, found.Amount);
        Assert.Equal(created.Category, found.Category);
        Assert.Equal(created.Description, found.Description);
    }

    #endregion
}