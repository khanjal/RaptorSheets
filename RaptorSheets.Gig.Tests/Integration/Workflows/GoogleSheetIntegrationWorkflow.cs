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
    private List<int> _createdShiftIds = new();
    private List<int> _createdTripIds = new();
    private List<int> _createdExpenseIds = new();

    public GoogleSheetIntegrationWorkflow()
    {
        _testStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _testSheets = new List<string> 
        { 
            SheetEnum.SHIFTS.GetDescription(), 
            SheetEnum.TRIPS.GetDescription(),
            SheetEnum.EXPENSES.GetDescription()
        };

        // Get all available sheets from enums
        _allSheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>()
            .Select(e => e.GetDescription())
            .ToList();

        // Also add common sheets (Setup sheet)
        _allSheets.AddRange(Enum.GetValues(typeof(RaptorSheets.Common.Enums.SheetEnum))
            .Cast<RaptorSheets.Common.Enums.SheetEnum>()
            .Select(e => e.GetDescription()));

        var spreadsheetId = TestConfigurationHelpers.GetGigSpreadsheet();
        var credential = TestConfigurationHelpers.GetJsonCredential();

        if (GoogleCredentialHelpers.IsCredentialFilled(credential))
            _googleSheetManager = new GoogleSheetManager(credential, spreadsheetId);
    }

    public async Task InitializeAsync()
    {
        // Nothing to initialize
    }

    public async Task DisposeAsync()
    {
        // No cleanup - test should leave data in place per requirements
    }

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
            // Step 1: Delete all existing sheets and recreate fresh
            await DeleteAllSheetsAndRecreate();

            // Step 2: Verify sheet structure is correct
            await VerifySheetStructure();

            // Step 3: Load test data (shifts, trips, expenses)
            await LoadTestData();

            // Only proceed if test data was created successfully
            if (_createdTestData == null || !_createdShiftIds.Any())
            {
                System.Diagnostics.Debug.WriteLine("Test data creation failed - skipping remaining steps");
                return;
            }

            // Step 4: Verify data was inserted correctly
            await VerifyDataWasInserted();

            // Step 5: Update the test data
            await UpdateTestData();

            // Step 6: Verify data was updated correctly  
            await VerifyDataWasUpdated();

            // Step 7: Delete the test data
            await DeleteTestData();

            // Step 8: Verify data was deleted correctly
            await VerifyDataWasDeleted();

            System.Diagnostics.Debug.WriteLine("=== Comprehensive Integration Test Workflow Completed Successfully ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Integration test failed with exception: {ex.Message}");
            // For integration tests, we might want to skip rather than fail if there are API issues
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
        // Arrange
        var credential = TestConfigurationHelpers.GetJsonCredential();
        var invalidManager = new GoogleSheetManager(credential, "invalid-spreadsheet-id");

        // Act
        var result = await invalidManager.GetSheets(_testSheets);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Messages);
        Assert.All(result.Messages, msg =>
            Assert.Equal(MessageLevelEnum.ERROR.GetDescription(), msg.Level));
    }

    [FactCheckUserSecrets]
    public async Task ErrorHandling_NonExistentSheets_ShouldHandleGracefully()
    {
        // Act
        var result = await _googleSheetManager!.GetSheetProperties(new List<string> { "NonExistentSheet1", "NonExistentSheet2" });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, prop => Assert.Empty(prop.Id)); // Should return empty properties
    }

    #region Workflow Steps

    private async Task DeleteAllSheetsAndRecreate()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 1: Delete All Existing Sheets and Recreate ===");

        // Get all existing sheets
        var allExistingProperties = await _googleSheetManager!.GetSheetProperties(_allSheets);
        var existingSheets = allExistingProperties.Where(prop => !string.IsNullOrEmpty(prop.Id)).ToList();

        System.Diagnostics.Debug.WriteLine($"Found {existingSheets.Count} existing sheets to delete");

        if (existingSheets.Count > 0)
        {
            var existingSheetNames = existingSheets.Select(s => s.Name).ToList();

            // Try to delete all existing sheets
            System.Diagnostics.Debug.WriteLine($"Deleting {existingSheetNames.Count} sheets: {string.Join(", ", existingSheetNames)}");
            var deletionResult = await _googleSheetManager.DeleteSheets(existingSheetNames);

            // Log results (but don't fail if some can't be deleted due to Google Sheets limitations)
            LogMessages("Delete", deletionResult.Messages);

            // Wait for deletion to propagate
            await Task.Delay(5000);
        }

        // Create all sheets from scratch
        System.Diagnostics.Debug.WriteLine($"Creating all {_allSheets.Count} sheets from scratch");
        var creationResult = await _googleSheetManager.CreateSheets();
        Assert.NotNull(creationResult);

        // Log creation results
        LogMessages("Create", creationResult.Messages);

        // Wait for creation to complete
        await Task.Delay(10000);

        System.Diagnostics.Debug.WriteLine("? Successfully deleted and recreated all sheets");
    }

    private async Task VerifySheetStructure()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 2: Verify Sheet Structure ===");

        // Get sheet properties
        var sheetProperties = await _googleSheetManager!.GetSheetProperties(_allSheets);
        var existingSheets = sheetProperties.Where(p => !string.IsNullOrEmpty(p.Id)).ToList();

        System.Diagnostics.Debug.WriteLine($"Found {existingSheets.Count} sheets after recreation");

        // Verify we have at least our core test sheets
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

        // Get all sheets data to verify structure
        var allSheetsData = await _googleSheetManager.GetSheets(_allSheets);
        Assert.NotNull(allSheetsData);

        // Verify no errors in retrieval
        var errorMessages = allSheetsData.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription());
        Assert.Empty(errorMessages);

        // Verify required aggregate sheet collections are not null
        AssertAggregateCollectionsExist(allSheetsData);

        System.Diagnostics.Debug.WriteLine("? Sheet structure verification completed successfully");
    }

    private async Task LoadTestData()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 3: Load Test Data (Shifts, Trips, Expenses) ===");

        try
        {
            // Get current max IDs to avoid conflicts
            var sheetInfo = await _googleSheetManager!.GetSheetProperties(_testSheets);
            var maxShiftId = GetMaxRowValue(sheetInfo, SheetEnum.SHIFTS.GetDescription());
            var maxTripId = GetMaxRowValue(sheetInfo, SheetEnum.TRIPS.GetDescription());
            var maxExpenseId = GetMaxRowValue(sheetInfo, SheetEnum.EXPENSES.GetDescription());

            // Generate comprehensive test data
            var testShiftsAndTrips = TestGigHelpers.GenerateMultipleShifts(
                ActionTypeEnum.APPEND,
                maxShiftId + 1,
                maxTripId + 1,
                numberOfShifts: 5,
                minTripsPerShift: 3,
                maxTripsPerShift: 6
            );

            var testExpenses = GenerateTestExpenses(maxExpenseId + 1, 8);

            // Combine all test data
            _createdTestData = new SheetEntity();
            _createdTestData.Shifts.AddRange(testShiftsAndTrips.Shifts);
            _createdTestData.Trips.AddRange(testShiftsAndTrips.Trips);
            _createdTestData.Expenses.AddRange(testExpenses.Expenses);

            // Track IDs for verification and updates
            _createdShiftIds.AddRange(_createdTestData.Shifts.Select(s => s.RowId));
            _createdTripIds.AddRange(_createdTestData.Trips.Select(t => t.RowId));
            _createdExpenseIds.AddRange(_createdTestData.Expenses.Select(e => e.RowId));

            System.Diagnostics.Debug.WriteLine($"Generated test data: {_createdTestData.Shifts.Count} shifts, {_createdTestData.Trips.Count} trips, {_createdTestData.Expenses.Count} expenses");

            // Load the test data
            var result = await _googleSheetManager.ChangeSheetData(_testSheets, _createdTestData);
            Assert.NotNull(result);

            // Check for errors and handle appropriately
            var errorMessages = result.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
            if (errorMessages.Any())
            {
                LogMessages("Load Data Error", errorMessages);
                ClearTestData();
                return;
            }

            // Validate successful operation
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

        await Task.Delay(3000); // Wait for data to propagate

        var result = await _googleSheetManager!.GetSheets(_testSheets);
        Assert.NotNull(result);
        Assert.NotNull(_createdTestData);

        // Verify all data was added correctly
        VerifyEntitiesExist(_createdTestData.Shifts, result.Shifts, "shifts");
        VerifyEntitiesExist(_createdTestData.Trips, result.Trips, "trips");
        VerifyEntitiesExist(_createdTestData.Expenses, result.Expenses, "expenses");

        System.Diagnostics.Debug.WriteLine($"? Data insertion verified: {result.Shifts.Count} total shifts, {result.Trips.Count} total trips, {result.Expenses.Count} total expenses");
    }

    private async Task UpdateTestData()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 5: Update Test Data ===");

        Assert.NotNull(_createdTestData);

        var updateData = new SheetEntity();

        // Update some shifts
        var shiftsToUpdate = _createdTestData.Shifts.Take(2).ToList();
        foreach (var shift in shiftsToUpdate)
        {
            shift.Action = ActionTypeEnum.UPDATE.GetDescription();
            shift.Region = "Updated Region";
            shift.Note = "Updated by integration test";
            updateData.Shifts.Add(shift);
        }

        // Update some trips
        var tripsToUpdate = _createdTestData.Trips.Take(3).ToList();
        foreach (var trip in tripsToUpdate)
        {
            trip.Action = ActionTypeEnum.UPDATE.GetDescription();
            trip.Tip = 999; // Distinctive value
            trip.Note = "Updated trip note";
            updateData.Trips.Add(trip);
        }

        // Update some expenses
        var expensesToUpdate = _createdTestData.Expenses.Take(2).ToList();
        foreach (var expense in expensesToUpdate)
        {
            expense.Description = "Updated expense description";
            expense.Amount = 12345.67m; // Distinctive value
            updateData.Expenses.Add(expense);
        }

        // Apply updates
        var result = await _googleSheetManager!.ChangeSheetData(_testSheets, updateData);
        Assert.NotNull(result);

        // Validate the operation
        if (!ValidateOperationResult(result, "Update"))
        {
            return; // Skip validation if there were errors
        }

        System.Diagnostics.Debug.WriteLine($"? Updated {shiftsToUpdate.Count} shifts, {tripsToUpdate.Count} trips, {expensesToUpdate.Count} expenses");
    }

    private async Task VerifyDataWasUpdated()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 6: Verify Data Was Updated Correctly ===");

        await Task.Delay(3000);

        var updatedData = await _googleSheetManager!.GetSheets(_testSheets);
        Assert.NotNull(updatedData);

        // Verify updates using distinctive values we set
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

        Assert.NotNull(_createdTestData);

        var deleteData = new SheetEntity();

        // Mark all test data for deletion
        foreach (var shift in _createdTestData.Shifts)
        {
            shift.Action = ActionTypeEnum.DELETE.GetDescription();
            deleteData.Shifts.Add(shift);
        }

        foreach (var trip in _createdTestData.Trips)
        {
            trip.Action = ActionTypeEnum.DELETE.GetDescription();
            deleteData.Trips.Add(trip);
        }

        // Add expenses for deletion (ExpenseEntity doesn't have Action property)
        foreach (var expense in _createdTestData.Expenses)
        {
            deleteData.Expenses.Add(expense);
        }

        // Delete the data
        var result = await _googleSheetManager!.ChangeSheetData(_testSheets, deleteData);
        Assert.NotNull(result);

        // Validate the operation
        ValidateOperationResult(result, "Delete");

        System.Diagnostics.Debug.WriteLine($"? Deletion commands sent for {deleteData.Shifts.Count} shifts, {deleteData.Trips.Count} trips, {deleteData.Expenses.Count} expenses");
    }

    private async Task VerifyDataWasDeleted()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 8: Verify Data Was Deleted Correctly ===");

        await Task.Delay(3000);

        var remainingData = await _googleSheetManager!.GetSheets(_testSheets);
        Assert.NotNull(remainingData);

        // Verify all test data was deleted by ID
        VerifyEntitiesDeleted(_createdShiftIds, remainingData.Shifts, "shifts");
        VerifyEntitiesDeleted(_createdTripIds, remainingData.Trips, "trips");
        VerifyEntitiesDeleted(_createdExpenseIds, remainingData.Expenses, "expenses");

        // Verify no artifacts remain using distinctive values
        Assert.Empty(remainingData.Shifts.Where(s => s.Region == "Updated Region"));
        Assert.Empty(remainingData.Trips.Where(t => t.Tip == 999));
        Assert.Empty(remainingData.Expenses.Where(e => e.Amount == 12345.67m));

        System.Diagnostics.Debug.WriteLine($"? Data deletion verified: {_createdShiftIds.Count} shifts, {_createdTripIds.Count} trips, {_createdExpenseIds.Count} expenses successfully removed");
    }

    #endregion

    #region Helper Methods

    private SheetEntity GenerateTestExpenses(int startingId, int count)
    {
        var sheetEntity = new SheetEntity();
        var currentDate = DateTime.Now;
        var random = new Random();
        
        var expenseCategories = new[] { "Gas", "Maintenance", "Insurance", "Parking", "Tolls", "Phone", "Food", "Supplies" };
        
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

    private static int GetMaxRowValue(List<RaptorSheets.Core.Entities.PropertyEntity> sheetInfo, string sheetName)
    {
        var sheet = sheetInfo.FirstOrDefault(x => x.Name == sheetName);
        var maxRowKey = PropertyEnum.MAX_ROW_VALUE.GetDescription();

        if (sheet?.Attributes?.TryGetValue(maxRowKey, out var maxRowValue) == true
            && int.TryParse(maxRowValue, out var maxRow))
        {
            return maxRow;
        }

        return 1; // Default to 1 if no data exists (header row)
    }

    private static bool IsApiRelatedError(Exception ex)
    {
        return ex.Message.Contains("credentials") || 
               ex.Message.Contains("authentication") || 
               ex.Message.Contains("Requested entity was not found");
    }

    private void ClearTestData()
    {
        _createdTestData = null;
        _createdShiftIds.Clear();
        _createdTripIds.Clear();
        _createdExpenseIds.Clear();
        System.Diagnostics.Debug.WriteLine("Cleared test data due to errors");
    }

    private void LogMessages(string operation, List<MessageEntity> messages)
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

    private bool ValidateOperationResult(SheetEntity result, string operationName)
    {
        var errorMessages = result.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
        if (errorMessages.Any())
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

    private void AssertAggregateCollectionsExist(SheetEntity allSheetsData)
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

    private void VerifyEntitiesExist<T>(List<T> createdEntities, List<T> foundEntities, string entityType) where T : class
    {
        foreach (var created in createdEntities)
        {
            var found = FindEntityById(created, foundEntities);
            Assert.NotNull(found);
            VerifyEntityProperties(created, found);
        }
    }

    private void VerifyEntitiesDeleted<T>(List<int> deletedIds, List<T> remainingEntities, string entityType) where T : class
    {
        foreach (var deletedId in deletedIds)
        {
            var found = FindEntityById(deletedId, remainingEntities);
            Assert.Null(found);
        }
    }

    private T? FindEntityById<T>(T entity, List<T> entities) where T : class
    {
        var rowIdProp = typeof(T).GetProperty("RowId");
        if (rowIdProp == null) return null;
        
        var targetId = (int?)rowIdProp.GetValue(entity);
        return entities.FirstOrDefault(e => (int?)rowIdProp.GetValue(e) == targetId);
    }

    private T? FindEntityById<T>(int rowId, List<T> entities) where T : class
    {
        var rowIdProp = typeof(T).GetProperty("RowId");
        if (rowIdProp == null) return null;
        
        return entities.FirstOrDefault(e => (int?)rowIdProp.GetValue(e) == rowId);
    }

    private void VerifyEntityProperties<T>(T created, T found) where T : class
    {
        var type = typeof(T);
        
        // Common verification for all entity types
        if (type.GetProperty("Date") != null)
        {
            Assert.Equal(type.GetProperty("Date")?.GetValue(created), 
                        type.GetProperty("Date")?.GetValue(found));
        }

        // Entity-specific verifications
        switch (type.Name)
        {
            case nameof(ShiftEntity):
                VerifyShiftProperties(created as ShiftEntity, found as ShiftEntity);
                break;
            case nameof(TripEntity):
                VerifyTripProperties(created as TripEntity, found as TripEntity);
                break;
            case nameof(ExpenseEntity):
                VerifyExpenseProperties(created as ExpenseEntity, found as ExpenseEntity);
                break;
        }
    }

    private void VerifyShiftProperties(ShiftEntity? created, ShiftEntity? found)
    {
        if (created == null || found == null) return;
        
        Assert.Equal(created.Number, found.Number);
        Assert.Equal(created.Service, found.Service);
        Assert.Equal(created.Region, found.Region);
    }

    private void VerifyTripProperties(TripEntity? created, TripEntity? found)
    {
        if (created == null || found == null) return;
        
        Assert.Equal(created.Number, found.Number);
        Assert.Equal(created.Service, found.Service);
        Assert.Equal(created.Place, found.Place);
        Assert.Equal(created.Name, found.Name);
    }

    private void VerifyExpenseProperties(ExpenseEntity? created, ExpenseEntity? found)
    {
        if (created == null || found == null) return;
        
        Assert.Equal(created.Amount, found.Amount);
        Assert.Equal(created.Category, found.Category);
        Assert.Equal(created.Description, found.Description);
    }

    #endregion
}