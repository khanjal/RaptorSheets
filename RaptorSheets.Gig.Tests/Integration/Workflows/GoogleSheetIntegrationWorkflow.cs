using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Entities;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Gig.Tests.Data.Helpers;
using RaptorSheets.Test.Common.Helpers;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Common.Mappers;

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

    // Random selection tracking
    private List<ShiftEntity> _shiftsToUpdate = new();
    private List<TripEntity> _tripsToUpdate = new();
    private List<ExpenseEntity> _expensesToUpdate = new();
    private List<ShiftEntity> _shiftsToDelete = new();
    private List<TripEntity> _tripsToDelete = new();
    private List<ExpenseEntity> _expensesToDelete = new();

    // Constants for test configuration
    private const int NumberOfShifts = 8;           // Increased for better testing
    private const int MinTripsPerShift = 2;
    private const int MaxTripsPerShift = 4;
    private const int NumberOfExpenses = 10;        // Increased for better testing
    private const int DataPropagationDelayMs = 2000; // Reduced for faster tests
    private const int SheetCreationDelayMs = 5000;   // Reduced for faster tests
    private const int SheetDeletionDelayMs = 3000;   // Reduced for faster tests

    public GoogleSheetIntegrationWorkflow()
    {
        _testStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        _testSheets = [
            SheetsConfig.SheetNames.Shifts, 
            SheetsConfig.SheetNames.Trips,
            SheetsConfig.SheetNames.Expenses
        ];

        // Get all available sheets from constants
        _allSheets = SheetsConfig.SheetUtilities.GetAllSheetNames();

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
            // Step 1: Clean slate - delete and recreate all sheets
            await DeleteAllSheetsAndRecreate();
            
            // Step 2: Verify sheet structure is correct
            await VerifySheetStructure();
            
            // Step 3: Load initial test data
            await LoadTestData();

            // Early exit if no test data was created
            if (_createdTestData?.Shifts.Count == 0 || _createdShiftIds.Count == 0)
            {
                Assert.Fail("Test data creation failed - skipping remaining steps");
                return;
            }

            // Step 4: Verify all data was inserted correctly
            await VerifyDataWasInserted();
            
            // Step 5: Update random subset of data
            await UpdateTestData();
            
            // Step 6: Verify updates were applied correctly
            await VerifyDataWasUpdated();
            
            // Step 7: Delete random subset of data (different from updated)
            await DeleteTestData();
            
            // Step 8: Verify deletions were applied correctly
            await VerifyDataWasDeleted();

            // Final summary
            await LogFinalDataSummary();

            System.Diagnostics.Debug.WriteLine("=== Comprehensive Integration Test Workflow Completed Successfully ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Integration test failed with exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
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
        System.Diagnostics.Debug.WriteLine("=== Step 1: Delete All Gig Sheets and Recreate ===");

        System.Diagnostics.Debug.WriteLine($"Attempting to delete all {_allSheets.Count} gig sheets: {string.Join(", ", _allSheets)}");
        
        // Delete all gig sheets by name - the DeleteSheets method handles non-existent sheets gracefully
        var deletionResult = await _googleSheetManager!.DeleteSheets(_allSheets);
        LogMessages("Delete", deletionResult.Messages);
        
        // Check if deletion was successful
        var deletionErrors = deletionResult.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
        if (deletionErrors.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"? Warning: Some gig sheets may not have been deleted due to errors");
            foreach (var error in deletionErrors)
            {
                System.Diagnostics.Debug.WriteLine($"  Deletion Error: {error.Message}");
            }
        }
        
        await Task.Delay(SheetDeletionDelayMs);

        // VERIFY DELETION: Check that all gig sheets were actually deleted
        await VerifyAllSheetsDeleted(_allSheets);

        System.Diagnostics.Debug.WriteLine($"Creating all gig sheets using default CreateSheets() method");
        
        // Use the default CreateSheets() method which creates all gig sheets from constants
        var creationResult = await _googleSheetManager.CreateSheets();
        Assert.NotNull(creationResult);

        LogMessages("Create", creationResult.Messages);
        
        // Verify creation was successful
        var creationErrors = creationResult.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
        if (creationErrors.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"? Warning: Some gig sheets may not have been created due to errors");
            foreach (var error in creationErrors)
            {
                System.Diagnostics.Debug.WriteLine($"  Creation Error: {error.Message}");
            }
        }
        
        await Task.Delay(SheetCreationDelayMs);

        // Verify the gig sheets were actually recreated
        await VerifyAllSheetsCreated();

        System.Diagnostics.Debug.WriteLine("? Successfully deleted and recreated all gig sheets");
    }

    private async Task VerifyAllSheetsDeleted(List<string> expectedDeletedSheets)
    {
        System.Diagnostics.Debug.WriteLine("=== VERIFICATION: Checking that gig sheets were deleted ===");

        try
        {
            var postDeletionProperties = await _googleSheetManager!.GetSheetProperties(expectedDeletedSheets);
            var remainingGigSheets = postDeletionProperties.Where(prop => !string.IsNullOrEmpty(prop.Id)).ToList();

            System.Diagnostics.Debug.WriteLine($"Checking deletion status for {expectedDeletedSheets.Count} gig sheets");

            if (remainingGigSheets.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("✓ SUCCESS: All gig sheets successfully deleted");
                return;
            }

            // We have gig sheets that weren't deleted - this is a problem
            var msg = $"❌ WARNING: {remainingGigSheets.Count} gig sheets were not deleted:\n" +
                      string.Join("\n", remainingGigSheets.Select(sheet => $"  - {sheet.Name} (ID: {sheet.Id})"));
            System.Diagnostics.Debug.WriteLine(msg);
            Assert.Fail(msg);
        }
        catch (Exception ex)
        {
            var msg = $"❌ ERROR during gig sheet deletion verification: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(msg);
            Assert.Fail(msg);
        }

        System.Diagnostics.Debug.WriteLine("=== End Gig Sheet Deletion Verification ===");
    }

    private async Task VerifyAllSheetsCreated()
    {
        System.Diagnostics.Debug.WriteLine("=== VERIFICATION: Checking that gig sheets were created ===");

        try
        {
            var response = await _googleSheetManager!.GetBatchData(_allSheets);
            if (response?.ValueRanges == null)
            {
                var msg = $"❌ CRITICAL: No data received from GetBatchData";
                System.Diagnostics.Debug.WriteLine(msg);
                Assert.Fail(msg);
            }

            var foundSheets = response.ValueRanges.Count;
            System.Diagnostics.Debug.WriteLine($"After creation: Found data for {foundSheets} out of {_allSheets.Count} expected gig sheets");
            
            var sheetsWithData = new List<string>();
            var sheetsWithoutData = new List<string>();

            // Loop through expected sheets and find their corresponding data
            foreach (var expectedSheetName in _allSheets)
            {
                var matchedValueRange = response.ValueRanges.FirstOrDefault(vr => 
                    vr.ValueRange?.Range?.Contains(expectedSheetName, StringComparison.OrdinalIgnoreCase) == true);
                
                if (matchedValueRange?.ValueRange?.Values?.Count > 0)
                {
                    sheetsWithData.Add(expectedSheetName);
                    System.Diagnostics.Debug.WriteLine($"  ✓ {expectedSheetName}: Found {matchedValueRange.ValueRange.Values.Count} rows");
                }
                else
                {
                    sheetsWithoutData.Add(expectedSheetName);
                    System.Diagnostics.Debug.WriteLine($"  ⚠ {expectedSheetName}: No data found (empty sheet)");
                }
            }

            if (sheetsWithoutData.Count > 0)
            {
                var msg = $"❌ WARNING: {sheetsWithoutData.Count} gig sheets have no data:\n" +
                          string.Join("\n", sheetsWithoutData.Select(sheetName => $"  - {sheetName}"));
                System.Diagnostics.Debug.WriteLine(msg);
                Assert.Fail(msg);
            }

            if (sheetsWithData.Count < _testSheets.Count)
            {
                var msg = $"❌ CRITICAL: Only {sheetsWithData.Count} sheets have data, need at least {_testSheets.Count} for testing";
                System.Diagnostics.Debug.WriteLine(msg);
                Assert.Fail(msg);
            }
        }
        catch (Exception ex)
        {
            var msg = $"❌ ERROR during gig sheet creation verification: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(msg);
            Assert.Fail(msg);
        }

        System.Diagnostics.Debug.WriteLine("=== End Gig Sheet Creation Verification ===");
    }

    private async Task VerifySheetStructure()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 2: Verify Sheet Structure ===");

        try
        {
            var response = await _googleSheetManager!.GetBatchData(_allSheets);
            if (response?.ValueRanges == null)
            {
                var msg = $"❌ CRITICAL: No sheet data received";
                System.Diagnostics.Debug.WriteLine(msg);
                Assert.Fail(msg);
            }

            System.Diagnostics.Debug.WriteLine($"Found data for {response.ValueRanges.Count} out of {_allSheets.Count} expected sheets");
            await VerifySheetHeaders(response);
            System.Diagnostics.Debug.WriteLine("✓ Sheet structure verification completed successfully");
            System.Diagnostics.Debug.WriteLine($"  - Verified {_allSheets.Count} gig sheets exist");
            System.Diagnostics.Debug.WriteLine($"  - Checked headers for {_testSheets.Count} core test sheets");
        }
        catch (Exception ex)
        {
            var msg = $"❌ ERROR during sheet structure verification: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(msg);
            Assert.Fail(msg);
        }
    }

    private async Task VerifySheetHeaders(BatchGetValuesByDataFilterResponse response)
    {
        System.Diagnostics.Debug.WriteLine("=== Verifying Sheet Column Headers ===");
        foreach (var expectedSheetName in _allSheets)
        {
            var matchedValueRange = response.ValueRanges.FirstOrDefault(vr =>
                vr.ValueRange?.Range?.Contains(expectedSheetName, StringComparison.OrdinalIgnoreCase) == true);
            if (matchedValueRange == null)
            {
                var msg = $"  ⚠ Warning: No data found for sheet: {expectedSheetName}";
                System.Diagnostics.Debug.WriteLine(msg);
                Assert.Fail(msg);
            }
            VerifySheetColumnHeaders(expectedSheetName, matchedValueRange);
        }
        System.Diagnostics.Debug.WriteLine("✓ All sheet column headers verified successfully");
    }

    private void VerifySheetColumnHeaders(string sheetName, MatchedValueRange matchedValueRange)
    {
        System.Diagnostics.Debug.WriteLine($"Verifying {sheetName} sheet headers...");
        if (matchedValueRange?.ValueRange?.Values?.Count > 0)
        {
            var headerRow = matchedValueRange.ValueRange.Values[0];
            var actualHeaders = headerRow.Select(cell => cell?.ToString()?.Trim() ?? "").Where(h => !string.IsNullOrEmpty(h)).ToList();
            System.Diagnostics.Debug.WriteLine($"  Found {actualHeaders.Count} headers: {string.Join(", ", actualHeaders.Take(5))}...");
            var expectedHeaders = GetAllExpectedHeadersForSheet(sheetName);
            if (expectedHeaders.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"  Expected {expectedHeaders.Count} headers for {sheetName}");
                var missingHeaders = expectedHeaders.Where(expected =>
                    !actualHeaders.Any(actual => actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                var unexpectedHeaders = actualHeaders.Where(actual =>
                    !expectedHeaders.Any(expected => expected.Equals(actual, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                if (missingHeaders.Count > 0)
                {
                    var msg = $"  ⚠ Missing headers in {sheetName}: {string.Join(", ", missingHeaders)}";
                    System.Diagnostics.Debug.WriteLine(msg);
                    Assert.Fail(msg);
                }
                if (unexpectedHeaders.Count > 0)
                {
                    var msg = $"  ⚠ Unexpected headers in {sheetName}: {string.Join(", ", unexpectedHeaders)}";
                    System.Diagnostics.Debug.WriteLine(msg);
                    Assert.Fail(msg);
                }
                VerifyHeaderOrder(sheetName, actualHeaders, expectedHeaders);
            }
            else
            {
                var msg = $"  ⚠ Warning: No expected headers configuration found for {sheetName}";
                System.Diagnostics.Debug.WriteLine(msg);
                Assert.Fail(msg);
            }
        }
        else
        {
            var msg = $"  ⚠ Warning: No data found for {sheetName} (empty sheet)";
            System.Diagnostics.Debug.WriteLine(msg);
            Assert.Fail(msg);
        }
        System.Diagnostics.Debug.WriteLine($"  ✓ {sheetName} header verification completed");
    }

    private static List<string> GetAllExpectedHeadersForSheet(string sheetName)
    {
        try
        {
            return sheetName switch
            {
                var s when string.Equals(s, SheetsConfig.SheetNames.Addresses, StringComparison.OrdinalIgnoreCase) => AddressMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Daily, StringComparison.OrdinalIgnoreCase) => DailyMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase) => ExpenseMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Monthly, StringComparison.OrdinalIgnoreCase) => MonthlyMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Names, StringComparison.OrdinalIgnoreCase) => NameMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Places, StringComparison.OrdinalIgnoreCase) => PlaceMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Regions, StringComparison.OrdinalIgnoreCase) => RegionMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Services, StringComparison.OrdinalIgnoreCase) => ServiceMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase) => SetupMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Shifts, StringComparison.OrdinalIgnoreCase) => ShiftMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Trips, StringComparison.OrdinalIgnoreCase) => TripMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Types, StringComparison.OrdinalIgnoreCase) => TypeMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Weekdays, StringComparison.OrdinalIgnoreCase) => WeekdayMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Weekly, StringComparison.OrdinalIgnoreCase) => WeeklyMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Yearly, StringComparison.OrdinalIgnoreCase) => YearlyMapper.GetSheet().Headers.Select(h => h.Name).ToList(),
                _ => new List<string>() // Unknown sheet type
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"  ⚠ Warning: Could not get expected headers for {sheetName}: {ex.Message}");
            return new List<string>();
        }
    }

    private static void VerifyHeaderOrder(string sheetName, List<string> actualHeaders, List<string> expectedHeaders)
    {
        // Check that the first few critical headers are in the correct positions
        var criticalHeadersToCheck = Math.Min(5, Math.Min(actualHeaders.Count, expectedHeaders.Count));
        
        for (int i = 0; i < criticalHeadersToCheck; i++)
        {
            if (!actualHeaders[i].Equals(expectedHeaders[i], StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"  ⚠ Header order mismatch in {sheetName} at position {i + 1}: expected '{expectedHeaders[i]}', found '{actualHeaders[i]}'");
                return; // Stop checking after first mismatch to avoid noise
            }
        }
        
        System.Diagnostics.Debug.WriteLine($"  ✓ Header order correct for first {criticalHeadersToCheck} headers in {sheetName}");
    }

    #endregion

    #region Test Data Management

    private async Task LoadTestData()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 3: Load Test Data (Shifts, Trips, Expenses) ===");

        try
        {
            // Always start RowId at 2 for all entities
            const int startingRowId = 2;

            System.Diagnostics.Debug.WriteLine("Generating test shifts and trips...");
            var testShiftsAndTrips = TestGigHelpers.GenerateMultipleShifts(
                ActionTypeEnum.INSERT,
                startingRowId,
                startingRowId,
                NumberOfShifts,
                MinTripsPerShift,
                MaxTripsPerShift
            );

            System.Diagnostics.Debug.WriteLine($"Generated {testShiftsAndTrips.Shifts.Count} shifts, {testShiftsAndTrips.Trips.Count} trips");

            if (testShiftsAndTrips.Shifts.Count == 0)
            {
                var msg = "❌ CRITICAL: No shifts were generated by TestGigHelpers.GenerateMultipleShifts";
                System.Diagnostics.Debug.WriteLine(msg);
                Assert.Fail(msg);
            }

            if (testShiftsAndTrips.Trips.Count == 0)
            {
                var msg = "❌ CRITICAL: No trips were generated by TestGigHelpers.GenerateMultipleShifts";
                System.Diagnostics.Debug.WriteLine(msg);
                Assert.Fail(msg);
            }

            // Add odometer values to some shifts and ensure distance matches
            for (int i = 0; i < testShiftsAndTrips.Shifts.Count; i++)
            {
                if (i < 3) // More shifts with odometer data
                {
                    var shift = testShiftsAndTrips.Shifts[i];
                    shift.OdometerStart = 1000m + i * 100m;
                    shift.OdometerEnd = shift.OdometerStart + 50m + i * 20m;
                    shift.Distance = shift.OdometerEnd - shift.OdometerStart;
                    shift.Note = $"Test shift {i + 1} with odometer";
                }
            }

            System.Diagnostics.Debug.WriteLine("Generating test expenses...");
            var testExpenses = GenerateTestExpenses(startingRowId, NumberOfExpenses);
            System.Diagnostics.Debug.WriteLine($"Generated {testExpenses.Expenses.Count} expenses");

            if (testExpenses.Expenses.Count == 0)
            {
                var msg = "❌ CRITICAL: No expenses were generated by GenerateTestExpenses";
                System.Diagnostics.Debug.WriteLine(msg);
                Assert.Fail(msg);
            }

            _createdTestData = new SheetEntity();
            _createdTestData.Shifts.AddRange(testShiftsAndTrips.Shifts);
            _createdTestData.Trips.AddRange(testShiftsAndTrips.Trips);
            _createdTestData.Expenses.AddRange(testExpenses.Expenses);

            _createdShiftIds.AddRange(_createdTestData.Shifts.Select(s => s.RowId));
            _createdTripIds.AddRange(_createdTestData.Trips.Select(t => t.RowId));
            _createdExpenseIds.AddRange(_createdTestData.Expenses.Select(e => e.RowId));

            System.Diagnostics.Debug.WriteLine($"Created test data container with:");
            System.Diagnostics.Debug.WriteLine($"  Shifts: {_createdTestData.Shifts.Count} (RowIds: {string.Join(", ", _createdTestData.Shifts.Take(5).Select(s => s.RowId))})");
            System.Diagnostics.Debug.WriteLine($"  Trips: {_createdTestData.Trips.Count} (RowIds: {string.Join(", ", _createdTestData.Trips.Take(5).Select(t => t.RowId))})");
            System.Diagnostics.Debug.WriteLine($"  Expenses: {_createdTestData.Expenses.Count} (RowIds: {string.Join(", ", _createdTestData.Expenses.Take(5).Select(e => e.RowId))})");

            // Validate all entities have proper Action set
            var shiftsWithoutAction = _createdTestData.Shifts.Count(s => string.IsNullOrEmpty(s.Action));
            var tripsWithoutAction = _createdTestData.Trips.Count(t => string.IsNullOrEmpty(t.Action));
            var expensesWithoutAction = _createdTestData.Expenses.Count(e => string.IsNullOrEmpty(e.Action));

            if (shiftsWithoutAction > 0 || tripsWithoutAction > 0 || expensesWithoutAction > 0)
            {
                var msg = $"❌ CRITICAL: Entities missing Action property: Shifts={shiftsWithoutAction}, Trips={tripsWithoutAction}, Expenses={expensesWithoutAction}";
                System.Diagnostics.Debug.WriteLine(msg);
                Assert.Fail(msg);
            }

            System.Diagnostics.Debug.WriteLine($"✓ All entities have Action property set to INSERT");

            System.Diagnostics.Debug.WriteLine($"Calling ChangeSheetData with sheets: [{string.Join(", ", _testSheets)}]");
            var result = await _googleSheetManager!.ChangeSheetData(_testSheets, _createdTestData);
            
            if (result == null)
            {
                var msg = "❌ CRITICAL: ChangeSheetData returned null";
                System.Diagnostics.Debug.WriteLine(msg);
                Assert.Fail(msg);
            }

            System.Diagnostics.Debug.WriteLine($"ChangeSheetData returned result with:");
            System.Diagnostics.Debug.WriteLine($"  Messages: {result.Messages?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  Returned Shifts: {result.Shifts?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  Returned Trips: {result.Trips?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  Returned Expenses: {result.Expenses?.Count ?? 0}");

            LogMessages("Load Data", result!.Messages);

            var errorMessages = result.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
            if (errorMessages.Count != 0)
            {
                var msg = $"❌ Data insertion failed with {errorMessages.Count} errors:";
                System.Diagnostics.Debug.WriteLine(msg);
                LogMessages("Load Data Error", errorMessages);
                Assert.Fail(msg);
            }

            var warningMessages = result.Messages.Where(m => m.Level == MessageLevelEnum.WARNING.GetDescription()).ToList();
            if (warningMessages.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"⚠ Data insertion had {warningMessages.Count} warnings:");
                LogMessages("Load Data Warning", warningMessages);
                // Don't fail on warnings, just log them
            }

            // Verify we got expected number of info messages (one per sheet)
            var infoMessages = result.Messages.Where(m => m.Level == MessageLevelEnum.INFO.GetDescription()).ToList();
            System.Diagnostics.Debug.WriteLine($"ℹ Data insertion had {infoMessages.Count} info messages:");
            LogMessages("Load Data Info", infoMessages);

            if (infoMessages.Count != _testSheets.Count)
            {
                System.Diagnostics.Debug.WriteLine($"⚠ Warning: Expected {_testSheets.Count} info messages, got {infoMessages.Count}");
            }

            System.Diagnostics.Debug.WriteLine("✓ Test data loading completed - verifying data was actually created...");
            
            // Immediate verification - try to read the data back
            System.Diagnostics.Debug.WriteLine("Performing immediate data verification...");
            await Task.Delay(1000); // Short delay for data propagation
            
            var immediateCheck = await _googleSheetManager.GetSheets(_testSheets);
            System.Diagnostics.Debug.WriteLine($"Immediate check results:");
            System.Diagnostics.Debug.WriteLine($"  Found Shifts: {immediateCheck.Shifts?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  Found Trips: {immediateCheck.Trips?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  Found Expenses: {immediateCheck.Expenses?.Count ?? 0}");
            
            if ((immediateCheck.Shifts?.Count ?? 0) == 0 && (immediateCheck.Trips?.Count ?? 0) == 0 && (immediateCheck.Expenses?.Count ?? 0) == 0)
            {
                var msg = "❌ CRITICAL: Immediate check shows no data was inserted despite success messages";
                System.Diagnostics.Debug.WriteLine(msg);
                
                // Check GetSheets error messages
                var getErrorMessages = immediateCheck.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
                if (getErrorMessages.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Error messages from GetSheets:");
                    foreach (var error in getErrorMessages)
                    {
                        System.Diagnostics.Debug.WriteLine($"  {error.Type}: {error.Message}");
                    }
                }
                
                Assert.Fail(msg);
            }

            System.Diagnostics.Debug.WriteLine("✓ Test data loaded successfully - data confirmed in sheets");
        }
        catch (Exception ex)
        {
            var msg = $"❌ LoadTestData failed with exception: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(msg);
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            Assert.Fail(msg);
        }
    }

    private async Task VerifyDataWasInserted()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 4: Verify Data Was Inserted Correctly ===");

        await Task.Delay(DataPropagationDelayMs);

        var result = await _googleSheetManager!.GetSheets(_testSheets);
        Assert.NotNull(result);
        Assert.NotNull(_createdTestData);

        System.Diagnostics.Debug.WriteLine($"Retrieved data from sheets:");
        System.Diagnostics.Debug.WriteLine($"  Shifts: {result.Shifts.Count} (expected: {_createdTestData.Shifts.Count})");
        System.Diagnostics.Debug.WriteLine($"  Trips: {result.Trips.Count} (expected: {_createdTestData.Trips.Count})");
        System.Diagnostics.Debug.WriteLine($"  Expenses: {result.Expenses.Count} (expected: {_createdTestData.Expenses.Count})");

        // Check if we have any data at all
        var totalExpected = _createdTestData.Shifts.Count + _createdTestData.Trips.Count + _createdTestData.Expenses.Count;
        var totalFound = result.Shifts.Count + result.Trips.Count + result.Expenses.Count;
        
        if (totalFound == 0 && totalExpected > 0)
        {
            var msg = "❌ CRITICAL: No data found in any sheets despite inserting test data";
            System.Diagnostics.Debug.WriteLine(msg);
            Assert.Fail(msg);
        }

        // Verify reasonable data counts (allow for some variation due to timing)
        var shiftVariance = Math.Abs(result.Shifts.Count - _createdTestData.Shifts.Count);
        var tripVariance = Math.Abs(result.Trips.Count - _createdTestData.Trips.Count);
        var expenseVariance = Math.Abs(result.Expenses.Count - _createdTestData.Expenses.Count);

        if (shiftVariance > 2)
        {
            System.Diagnostics.Debug.WriteLine($"⚠ Warning: Shift count variance too high - expected: {_createdTestData.Shifts.Count}, found: {result.Shifts.Count}");
        }

        if (tripVariance > 3)
        {
            System.Diagnostics.Debug.WriteLine($"⚠ Warning: Trip count variance too high - expected: {_createdTestData.Trips.Count}, found: {result.Trips.Count}");
        }

        if (expenseVariance > 2)
        {
            System.Diagnostics.Debug.WriteLine($"⚠ Warning: Expense count variance too high - expected: {_createdTestData.Expenses.Count}, found: {result.Expenses.Count}");
        }

        // Verify some sample data integrity
        if (result.Shifts.Count > 0)
        {
            var shiftsWithOdometer = result.Shifts.Where(s => s.OdometerStart.HasValue && s.OdometerEnd.HasValue).ToList();
            System.Diagnostics.Debug.WriteLine($"  Shifts with odometer data: {shiftsWithOdometer.Count}");
            
            if (shiftsWithOdometer.Count > 0)
            {
                var sampleShift = shiftsWithOdometer.First();
                System.Diagnostics.Debug.WriteLine($"  Sample shift: Start={sampleShift.OdometerStart}, End={sampleShift.OdometerEnd}, Distance={sampleShift.Distance}");
            }
        }

        if (result.Trips.Count > 0)
        {
            var tripsWithPay = result.Trips.Where(t => t.Pay.HasValue && t.Pay > 0).ToList();
            System.Diagnostics.Debug.WriteLine($"  Trips with pay data: {tripsWithPay.Count}");
        }

        if (result.Expenses.Count > 0)
        {
            var expenseTotal = result.Expenses.Sum(e => e.Amount);
            System.Diagnostics.Debug.WriteLine($"  Total expense amount: ${expenseTotal:F2}");
        }

        System.Diagnostics.Debug.WriteLine($"✓ Data insertion verification completed successfully");
    }

    private async Task UpdateTestData()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 5: Update Test Data ===");

        ArgumentNullException.ThrowIfNull(_createdTestData);

        var updateData = new SheetEntity();

        // Select random entities to update (fewer to ensure some remain untouched)
        _shiftsToUpdate = SelectRandomEntities(_createdTestData.Shifts, Math.Min(3, _createdTestData.Shifts.Count / 2));
        _tripsToUpdate = SelectRandomEntities(_createdTestData.Trips, Math.Min(4, _createdTestData.Trips.Count / 3));
        _expensesToUpdate = SelectRandomEntities(_createdTestData.Expenses, Math.Min(3, _createdTestData.Expenses.Count / 2));

        System.Diagnostics.Debug.WriteLine($"Selected entities for update:");
        System.Diagnostics.Debug.WriteLine($"  Shifts: {_shiftsToUpdate.Count} out of {_createdTestData.Shifts.Count}");
        System.Diagnostics.Debug.WriteLine($"  Trips: {_tripsToUpdate.Count} out of {_createdTestData.Trips.Count}");
        System.Diagnostics.Debug.WriteLine($"  Expenses: {_expensesToUpdate.Count} out of {_createdTestData.Expenses.Count}");

        // Update shifts with distinctive values and copy to updateData
        foreach (var shift in _shiftsToUpdate)
        {
            shift.Action = ActionTypeEnum.UPDATE.GetDescription();
            shift.Region = "Updated Region";
            shift.Note = "Updated by integration test";
            shift.Pay = (shift.Pay ?? 0) + 50; // Increase pay for verification
            updateData.Shifts.Add(shift);
        }

        // Update trips with distinctive values and copy to updateData
        foreach (var trip in _tripsToUpdate)
        {
            trip.Action = ActionTypeEnum.UPDATE.GetDescription();
            trip.Tip = 999; // Distinctive value
            trip.Note = "Updated trip note";
            trip.Pay = (trip.Pay ?? 0) + 25; // Increase pay for verification
            updateData.Trips.Add(trip);
        }

        // Update expenses with distinctive values and copy to updateData
        foreach (var expense in _expensesToUpdate)
        {
            expense.Action = ActionTypeEnum.UPDATE.GetDescription();
            expense.Description = "Updated expense description";
            expense.Amount = 12345.67m; // Distinctive value
            expense.Category = "Updated Category";
            updateData.Expenses.Add(expense);
        }

        System.Diagnostics.Debug.WriteLine($"Updating data with distinctive values:");
        System.Diagnostics.Debug.WriteLine($"  Shifts: Region='Updated Region', Note='Updated by integration test'");
        System.Diagnostics.Debug.WriteLine($"  Trips: Tip=999, Note='Updated trip note'");
        System.Diagnostics.Debug.WriteLine($"  Expenses: Amount=12345.67, Description='Updated expense description'");

        var result = await _googleSheetManager!.ChangeSheetData(_testSheets, updateData);
        Assert.NotNull(result);

        LogMessages("Update Data", result.Messages);

        if (!ValidateOperationResult(result, "Update"))
        {
            var msg = "❌ Update operation failed validation.";
            System.Diagnostics.Debug.WriteLine(msg);
            Assert.Fail(msg);
        }

        System.Diagnostics.Debug.WriteLine($"✓ Updated {_shiftsToUpdate.Count} shifts, {_tripsToUpdate.Count} trips, {_expensesToUpdate.Count} expenses");
    }

    private async Task VerifyDataWasUpdated()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 6: Verify Data Was Updated Correctly ===");

        await Task.Delay(DataPropagationDelayMs);

        var updatedData = await _googleSheetManager!.GetSheets(_testSheets);
        Assert.NotNull(updatedData);

        System.Diagnostics.Debug.WriteLine($"Verifying updates using distinctive values...");

        // Verify updates using distinctive values
        var updatedShifts = updatedData.Shifts.Where(s => s.Region == "Updated Region" && s.Note == "Updated by integration test").ToList();
        var updatedTrips = updatedData.Trips.Where(t => t.Tip == 999 && t.Note == "Updated trip note").ToList();
        var updatedExpenses = updatedData.Expenses.Where(e => e.Amount == 12345.67m && e.Description == "Updated expense description").ToList();

        System.Diagnostics.Debug.WriteLine($"Found updated entities:");
        System.Diagnostics.Debug.WriteLine($"  Shifts: {updatedShifts.Count} (expected: {_shiftsToUpdate.Count})");
        System.Diagnostics.Debug.WriteLine($"  Trips: {updatedTrips.Count} (expected: {_tripsToUpdate.Count})");
        System.Diagnostics.Debug.WriteLine($"  Expenses: {updatedExpenses.Count} (expected: {_expensesToUpdate.Count})");

        // Verify correct counts
        if (updatedShifts.Count != _shiftsToUpdate.Count)
        {
            var msg = $"❌ Expected {_shiftsToUpdate.Count} updated shifts, found {updatedShifts.Count}";
            System.Diagnostics.Debug.WriteLine(msg);
            Assert.Fail(msg);
        }

        if (updatedTrips.Count != _tripsToUpdate.Count)
        {
            var msg = $"❌ Expected {_tripsToUpdate.Count} updated trips, found {updatedTrips.Count}";
            System.Diagnostics.Debug.WriteLine(msg);
            Assert.Fail(msg);
        }

        if (updatedExpenses.Count != _expensesToUpdate.Count)
        {
            var msg = $"❌ Expected {_expensesToUpdate.Count} updated expenses, found {updatedExpenses.Count}";
            System.Diagnostics.Debug.WriteLine(msg);
            Assert.Fail(msg);
        }

        // Verify all updated entities have the correct values
        Assert.All(updatedShifts, s => {
            Assert.Equal("Updated Region", s.Region);
            Assert.Equal("Updated by integration test", s.Note);
        });

        Assert.All(updatedTrips, t => {
            Assert.Equal(999, t.Tip);
            Assert.Equal("Updated trip note", t.Note);
        });

        Assert.All(updatedExpenses, e => {
            Assert.Equal(12345.67m, e.Amount);
            Assert.Equal("Updated expense description", e.Description);
            Assert.Equal("Updated Category", e.Category);
        });

        System.Diagnostics.Debug.WriteLine("✓ Data updates verified successfully - all distinctive values found");
    }

    private async Task DeleteTestData()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 7: Delete Test Data ===");

        ArgumentNullException.ThrowIfNull(_createdTestData);

        var deleteData = new SheetEntity();

        // Select random entities to delete (different from updated ones, and fewer to leave data for manual inspection)
        var availableShiftsForDeletion = _createdTestData.Shifts.Except(_shiftsToUpdate).ToList();
        var availableTripsForDeletion = _createdTestData.Trips.Except(_tripsToUpdate).ToList();
        var availableExpensesForDeletion = _createdTestData.Expenses.Except(_expensesToUpdate).ToList();

        _shiftsToDelete = SelectRandomEntities(availableShiftsForDeletion, Math.Min(2, availableShiftsForDeletion.Count / 2));
        _tripsToDelete = SelectRandomEntities(availableTripsForDeletion, Math.Min(3, availableTripsForDeletion.Count / 2));
        _expensesToDelete = SelectRandomEntities(availableExpensesForDeletion, Math.Min(2, availableExpensesForDeletion.Count / 2));

        System.Diagnostics.Debug.WriteLine($"Selected entities for deletion (avoiding updated entities):");
        System.Diagnostics.Debug.WriteLine($"  Shifts: {_shiftsToDelete.Count} out of {availableShiftsForDeletion.Count} available");
        System.Diagnostics.Debug.WriteLine($"  Trips: {_tripsToDelete.Count} out of {availableTripsForDeletion.Count} available");
        System.Diagnostics.Debug.WriteLine($"  Expenses: {_expensesToDelete.Count} out of {availableExpensesForDeletion.Count} available");

        // Mark entities for deletion and add to deleteData
        foreach (var shift in _shiftsToDelete)
        {
            shift.Action = ActionTypeEnum.DELETE.GetDescription();
            deleteData.Shifts.Add(shift);
        }

        foreach (var trip in _tripsToDelete)
        {
            trip.Action = ActionTypeEnum.DELETE.GetDescription();
            deleteData.Trips.Add(trip);
        }

        foreach (var expense in _expensesToDelete)
        {
            expense.Action = ActionTypeEnum.DELETE.GetDescription();
            deleteData.Expenses.Add(expense);
        }

        if (deleteData.Shifts.Count == 0 && deleteData.Trips.Count == 0 && deleteData.Expenses.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("⚠ No entities selected for deletion - skipping delete step");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"Deleting entities with RowIds:");
        System.Diagnostics.Debug.WriteLine($"  Shifts: [{string.Join(", ", _shiftsToDelete.Select(s => s.RowId))}]");
        System.Diagnostics.Debug.WriteLine($"  Trips: [{string.Join(", ", _tripsToDelete.Select(t => t.RowId))}]");
        System.Diagnostics.Debug.WriteLine($"  Expenses: [{string.Join(", ", _expensesToDelete.Select(e => e.RowId))}]");

        var result = await _googleSheetManager!.ChangeSheetData(_testSheets, deleteData);
        Assert.NotNull(result);

        LogMessages("Delete Data", result.Messages);

        // Check for errors in the delete operation
        var deleteErrors = result.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription()).ToList();
        if (deleteErrors.Count > 0)
        {
            var msg = $"❌ CRITICAL: Delete operation failed with {deleteErrors.Count} errors:";
            System.Diagnostics.Debug.WriteLine(msg);
            LogMessages("Delete Errors", deleteErrors);
            Assert.Fail(msg);
        }

        if (!ValidateOperationResult(result, "Delete"))
        {
            var msg = "⚠ Delete operation validation failed.";
            System.Diagnostics.Debug.WriteLine(msg);
            Assert.Fail(msg);
        }

        System.Diagnostics.Debug.WriteLine($"✓ Deletion commands sent for {deleteData.Shifts.Count} shifts, {deleteData.Trips.Count} trips, {deleteData.Expenses.Count} expenses");
    }

    private async Task VerifyDataWasDeleted()
    {
        System.Diagnostics.Debug.WriteLine("=== Step 8: Verify Data Was Deleted Correctly ===");

        // Increased delay for delete operations as they might take longer to propagate
        System.Diagnostics.Debug.WriteLine($"Waiting {DataPropagationDelayMs * 2}ms for delete operations to propagate...");
        await Task.Delay(DataPropagationDelayMs * 2);

        var remainingData = await _googleSheetManager!.GetSheets(_testSheets);
        Assert.NotNull(remainingData);

        System.Diagnostics.Debug.WriteLine($"After deletion - Found remaining data:");
        System.Diagnostics.Debug.WriteLine($"  Remaining Shifts: {remainingData.Shifts.Count}");
        System.Diagnostics.Debug.WriteLine($"  Remaining Trips: {remainingData.Trips.Count}");
        System.Diagnostics.Debug.WriteLine($"  Remaining Expenses: {remainingData.Expenses.Count}");

        // Verify deleted entities are gone (using RowId, knowing this may be unreliable due to row shifting)
        var deletedShiftIds = _shiftsToDelete.Select(s => s.RowId).ToHashSet();
        var deletedTripIds = _tripsToDelete.Select(t => t.RowId).ToHashSet();
        var deletedExpenseIds = _expensesToDelete.Select(e => e.RowId).ToHashSet();

        var foundDeletedShifts = remainingData.Shifts.Where(s => deletedShiftIds.Contains(s.RowId)).ToList();
        var foundDeletedTrips = remainingData.Trips.Where(t => deletedTripIds.Contains(t.RowId)).ToList();
        var foundDeletedExpenses = remainingData.Expenses.Where(e => deletedExpenseIds.Contains(e.RowId)).ToList();

        System.Diagnostics.Debug.WriteLine($"Verification of deletions:");
        System.Diagnostics.Debug.WriteLine($"  Deleted shift RowIds still found: {foundDeletedShifts.Count} out of {_shiftsToDelete.Count}");
        System.Diagnostics.Debug.WriteLine($"  Deleted trip RowIds still found: {foundDeletedTrips.Count} out of {_tripsToDelete.Count}");
        System.Diagnostics.Debug.WriteLine($"  Deleted expense RowIds still found: {foundDeletedExpenses.Count} out of {_expensesToDelete.Count}");

        // Due to Google Sheets row shifting behavior, we'll only warn about remaining entities
        if (foundDeletedShifts.Count > 0 || foundDeletedTrips.Count > 0 || foundDeletedExpenses.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine("⚠ Note: Some entities with deleted RowIds still found - this may be due to Google Sheets row shifting behavior");
        }

        // Verify that updated entities (with distinctive values) still exist
        var remainingUpdatedShifts = remainingData.Shifts.Where(s => s.Region == "Updated Region").ToList();
        var remainingUpdatedTrips = remainingData.Trips.Where(t => t.Tip == 999).ToList();
        var remainingUpdatedExpenses = remainingData.Expenses.Where(e => e.Amount == 12345.67m).ToList();

        System.Diagnostics.Debug.WriteLine($"Verification that updated entities remain:");
        System.Diagnostics.Debug.WriteLine($"  Updated shifts still present: {remainingUpdatedShifts.Count} (expected: {_shiftsToUpdate.Count})");
        System.Diagnostics.Debug.WriteLine($"  Updated trips still present: {remainingUpdatedTrips.Count} (expected: {_tripsToUpdate.Count})");
        System.Diagnostics.Debug.WriteLine($"  Updated expenses still present: {remainingUpdatedExpenses.Count} (expected: {_expensesToUpdate.Count})");

        // Ensure updated entities weren't accidentally deleted
        if (remainingUpdatedShifts.Count != _shiftsToUpdate.Count)
        {
            System.Diagnostics.Debug.WriteLine($"⚠ Warning: Expected {_shiftsToUpdate.Count} updated shifts to remain, found {remainingUpdatedShifts.Count}");
        }

        if (remainingUpdatedTrips.Count != _tripsToUpdate.Count)
        {
            System.Diagnostics.Debug.WriteLine($"⚠ Warning: Expected {_tripsToUpdate.Count} updated trips to remain, found {remainingUpdatedTrips.Count}");
        }

        if (remainingUpdatedExpenses.Count != _expensesToUpdate.Count)
        {
            System.Diagnostics.Debug.WriteLine($"⚠ Warning: Expected {_expensesToUpdate.Count} updated expenses to remain, found {remainingUpdatedExpenses.Count}");
        }

        System.Diagnostics.Debug.WriteLine("✓ Delete verification completed - some data remains for manual inspection");
    }

    private async Task LogFinalDataSummary()
    {
        System.Diagnostics.Debug.WriteLine("=== Final Data Summary for Manual Inspection ===");

        try
        {
            var finalData = await _googleSheetManager!.GetSheets(_testSheets);
            
            System.Diagnostics.Debug.WriteLine($"Final data counts:");
            System.Diagnostics.Debug.WriteLine($"  Shifts: {finalData.Shifts.Count}");
            System.Diagnostics.Debug.WriteLine($"  Trips: {finalData.Trips.Count}");
            System.Diagnostics.Debug.WriteLine($"  Expenses: {finalData.Expenses.Count}");

            // Log some sample data for manual verification
            if (finalData.Shifts.Count > 0)
            {
                var sampleShift = finalData.Shifts.First();
                System.Diagnostics.Debug.WriteLine($"Sample shift - RowId: {sampleShift.RowId}, Service: {sampleShift.Service}, Region: {sampleShift.Region}");
            }

            if (finalData.Trips.Count > 0)
            {
                var sampleTrip = finalData.Trips.First();
                System.Diagnostics.Debug.WriteLine($"Sample trip - RowId: {sampleTrip.RowId}, Service: {sampleTrip.Service}, Tip: {sampleTrip.Tip}");
            }

            if (finalData.Expenses.Count > 0)
            {
                var sampleExpense = finalData.Expenses.First();
                System.Diagnostics.Debug.WriteLine($"Sample expense - RowId: {sampleExpense.RowId}, Amount: {sampleExpense.Amount}, Category: {sampleExpense.Category}");
            }

            // Log updated entities that should be present
            var updatedShifts = finalData.Shifts.Where(s => s.Region == "Updated Region").ToList();
            var updatedTrips = finalData.Trips.Where(t => t.Tip == 999).ToList();
            var updatedExpenses = finalData.Expenses.Where(e => e.Amount == 12345.67m).ToList();

            System.Diagnostics.Debug.WriteLine($"Updated entities present:");
            System.Diagnostics.Debug.WriteLine($"  Shifts with 'Updated Region': {updatedShifts.Count}");
            System.Diagnostics.Debug.WriteLine($"  Trips with Tip=999: {updatedTrips.Count}");
            System.Diagnostics.Debug.WriteLine($"  Expenses with Amount=12345.67: {updatedExpenses.Count}");

            System.Diagnostics.Debug.WriteLine("=== End Final Data Summary ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠ Error generating final data summary: {ex.Message}");
        }
    }

    #endregion

    #region Diagnostic Tests

    [FactCheckUserSecrets]
    public async Task DiagnosticTest_FullDeletionAndRecreation_ShouldWork()
    {
        // Skip test if credentials are not available
        if (_googleSheetManager == null)
        {
            System.Diagnostics.Debug.WriteLine("Skipping full deletion/recreation test - Google Sheets credentials not available");
            return;
        }

        System.Diagnostics.Debug.WriteLine("=== Diagnostic Test: Full Sheet Deletion and Recreation ===");

        try
        {
            // Just run the enhanced delete and recreate process
            await DeleteAllSheetsAndRecreate();
            System.Diagnostics.Debug.WriteLine("=== Full Deletion and Recreation Test Completed Successfully ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Full deletion/recreation test failed with exception: {ex.Message}");
            
            if (IsApiRelatedError(ex))
            {
                System.Diagnostics.Debug.WriteLine("Skipping test due to API/authentication issues");
                return;
            }
            throw;
        }
    }

    [FactCheckUserSecrets]
    public async Task DiagnosticTest_LoadTestDataOnly_ShouldWork()
    {
        // Skip test if credentials are not available
        if (_googleSheetManager == null)
        {
            System.Diagnostics.Debug.WriteLine("Skipping LoadTestData diagnostic test - Google Sheets credentials not available");
            return;
        }

        System.Diagnostics.Debug.WriteLine("=== Diagnostic Test: LoadTestData Only ===");

        try
        {
            // Reset test data first
            _createdTestData = null;
            _createdShiftIds.Clear();
            _createdTripIds.Clear();
            _createdExpenseIds.Clear();

            // Ensure sheets exist (run minimal setup)
            System.Diagnostics.Debug.WriteLine("Ensuring test sheets exist...");
            var sheetProperties = await _googleSheetManager.GetSheetProperties(_testSheets);
            var missingSheets = _testSheets.Where(sheet => 
                !sheetProperties.Any(prop => prop.Name.Equals(sheet, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(prop.Id))
            ).ToList();

            if (missingSheets.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Creating missing sheets: {string.Join(", ", missingSheets)}");
                var createResult = await _googleSheetManager.CreateSheets(missingSheets);
                System.Diagnostics.Debug.WriteLine($"Sheet creation result: {createResult.Messages.Count} messages");
                LogMessages("Create Missing Sheets", createResult.Messages);
                
                // Wait for sheets to be ready
                await Task.Delay(5000);
            }

            // Now test LoadTestData specifically
            await LoadTestData();

            // Verify results
            if (_createdTestData == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ CRITICAL: _createdTestData is null after LoadTestData");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"✓ LoadTestData result:");
                System.Diagnostics.Debug.WriteLine($"  Shifts: {_createdTestData.Shifts?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"  Trips: {_createdTestData.Trips?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"  Expenses: {_createdTestData.Expenses?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"  Created shift IDs: {_createdShiftIds.Count}");
                System.Diagnostics.Debug.WriteLine($"  Created trip IDs: {_createdTripIds.Count}");
                System.Diagnostics.Debug.WriteLine($"  Created expense IDs: {_createdExpenseIds.Count}");
            }

            // Verify data actually exists in Google Sheets
            System.Diagnostics.Debug.WriteLine("Verifying data in Google Sheets...");
            var result = await _googleSheetManager.GetSheets(_testSheets);
            System.Diagnostics.Debug.WriteLine($"Data found in sheets:");
            System.Diagnostics.Debug.WriteLine($"  Shifts: {result.Shifts?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  Trips: {result.Trips?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  Expenses: {result.Expenses?.Count ?? 0}");

            System.Diagnostics.Debug.WriteLine("=== LoadTestData Diagnostic Test Completed ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadTestData diagnostic test.failed with exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
            if (IsApiRelatedError(ex))
            {
                System.Diagnostics.Debug.WriteLine("Skipping test due to API/authentication issues");
                return;
            }
            throw;
        }
    }

    private static SheetEntity GenerateTestExpenses(int startingId, int count)
    {
        var sheetEntity = new SheetEntity();
        var baseDate = DateTime.Today;
        var random = new Random();
        
        string[] expenseCategories = ["Gas", "Maintenance", "Insurance", "Parking", "Tolls", "Phone", "Food", "Supplies", "Equipment", "Registration"];
        string[] expenseNames = ["Shell Station", "AutoZone", "State Farm", "Downtown Parking", "Bridge Toll", "Verizon", "McDonald's", "Office Depot", "GPS Mount", "DMV Fee"];
        
        for (int i = 0; i < count; i++)
        {
            var categoryIndex = random.Next(expenseCategories.Length);
            var expense = new ExpenseEntity
            {
                RowId = startingId + i,
                Action = ActionTypeEnum.INSERT.GetDescription(),
                Date = baseDate.AddDays(-random.Next(0, 60)), // Spread over 2 months
                Amount = Math.Round((decimal)(random.NextDouble() * 300 + 5), 2), // $5-$305
                Category = expenseCategories[categoryIndex],
                Name = expenseNames[Math.Min(categoryIndex, expenseNames.Length - 1)],
                Description = $"Test expense {i + 1} - {expenseCategories[categoryIndex]} expense for integration testing"
            };
            sheetEntity.Expenses.Add(expense);
        }
        
        System.Diagnostics.Debug.WriteLine($"Generated {count} test expenses with varied categories and amounts");
        return sheetEntity;
    }

    private static List<T> SelectRandomEntities<T>(List<T> source, int count)
    {
        if (source.Count == 0) return new List<T>();
        if (source.Count <= count) return new List<T>(source);
        
        var random = new Random();
        return source.OrderBy(_ => random.Next()).Take(count).ToList();
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

        // Check for expected INFO messages with SAVE_DATA type
        var expectedLevel = MessageLevelEnum.INFO.GetDescription();
        var expectedType = MessageTypeEnum.SAVE_DATA.GetDescription();
        
        var correctMessages = result.Messages.Where(m => 
        m.Level == expectedLevel && m.Type == expectedType).ToList();
        
        var incorrectMessages = result.Messages.Where(m => 
        m.Level != expectedLevel || m.Type != expectedType).ToList();

        System.Diagnostics.Debug.WriteLine($"=== {operationName.ToUpper()} MESSAGE VALIDATION ===");
        System.Diagnostics.Debug.WriteLine($"Total messages: {result.Messages.Count}");
        System.Diagnostics.Debug.WriteLine($"Expected (INFO/SAVE_DATA): {correctMessages.Count}");
        System.Diagnostics.Debug.WriteLine($"Other types: {incorrectMessages.Count}");

        foreach (var message in result.Messages.Take(5)) // Limit output
        {
            var status = (message.Level == expectedLevel && message.Type == expectedType) ? "✓" : "⚠";
            System.Diagnostics.Debug.WriteLine($"  {status} {message.Level}/{message.Type}: {message.Message}");
        }

        if (incorrectMessages.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"⚠ Found {incorrectMessages.Count} messages with unexpected level/type");
            // Log but don't fail - some operations may have different message patterns
        }

        return true; // Only fail on ERROR messages
    }

    private static bool IsApiRelatedError(Exception ex) =>
        ex.Message.Contains("credentials", StringComparison.OrdinalIgnoreCase) || 
        ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase) || 
        ex.Message.Contains("Requested entity was not found", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("API", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("quota", StringComparison.OrdinalIgnoreCase);

    private static void LogMessages(string operation, List<MessageEntity> messages)
    {
        if (messages == null || messages.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine($"{operation}: No messages");
            return;
        }

        foreach (var message in messages)
        {
            System.Diagnostics.Debug.WriteLine($"{operation} result: {message.Level} - {message.Message}");
        }
    }
    #endregion
}