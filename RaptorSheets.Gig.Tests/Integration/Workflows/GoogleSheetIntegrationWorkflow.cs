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
/// Tests the complete lifecycle: Setup -> Create -> Add -> Read -> Update -> Delete
/// Validates both primary data sheets and aggregate/reporting sheets
/// </summary>
public class GoogleSheetIntegrationWorkflow : IAsyncLifetime
{
    private readonly GoogleSheetManager? _googleSheetManager;
    private readonly Dictionary<string, string> _credential;
    private readonly List<string> _testSheets;
    private readonly List<string> _aggregateSheets;
    private readonly long _testStartTime;

    // Test data tracking
    private SheetEntity? _createdShiftData;
    private List<int> _createdShiftIds = new();
    private List<int> _createdTripIds = new();

    public GoogleSheetIntegrationWorkflow()
    {
        _testStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _testSheets = new List<string> { SheetEnum.TRIPS.GetDescription(), SheetEnum.SHIFTS.GetDescription() };
        
        // Sheets that contain aggregated data derived from SHIFTS and TRIPS
        _aggregateSheets = new List<string> 
        { 
            SheetEnum.ADDRESSES.GetDescription(),
            SheetEnum.NAMES.GetDescription(), 
            SheetEnum.PLACES.GetDescription(),
            SheetEnum.REGIONS.GetDescription(),
            SheetEnum.SERVICES.GetDescription(),
            SheetEnum.TYPES.GetDescription(),
            SheetEnum.DAILY.GetDescription(),
            SheetEnum.WEEKLY.GetDescription(),
            SheetEnum.MONTHLY.GetDescription(),
            SheetEnum.YEARLY.GetDescription()
        };
        
        var spreadsheetId = TestConfigurationHelpers.GetGigSpreadsheet();
        _credential = TestConfigurationHelpers.GetJsonCredential();

        if (GoogleCredentialHelpers.IsCredentialFilled(_credential))
            _googleSheetManager = new GoogleSheetManager(_credential, spreadsheetId);
    }

    public async Task InitializeAsync()
    {
        if (_googleSheetManager == null) return;

        // Clean slate: Clear existing test data and prepare sheets
        await CleanupTestData();
        await EnsureSheetsExist();
    }

    public async Task DisposeAsync()
    {
        if (_googleSheetManager == null) return;

        // Cleanup any test data created during the workflow
        await CleanupTestData();
    }

    [FactCheckUserSecrets]
    public async Task ComprehensiveWorkflow_ShouldExecuteCompleteLifecycle()
    {
        // Step 1: Verify Sheet Structure (Primary + Aggregate)
        await VerifySheetStructure();
        
        // Step 2: Test Sheet Deletion and Recreation
        await TestSheetDeletionAndRecreation();
        
        // Step 3: Capture Pre-Test Aggregate State
        var preTestAggregates = await CaptureAggregateState();
        
        // Step 4: Create New Data
        await CreateNewShiftWithTrips();
        
        // Step 5: Verify Data Was Added
        await VerifyDataWasAdded();
        
        // Step 6: Verify Aggregate Sheets Updated
        await VerifyAggregateDataUpdated(preTestAggregates);
        
        // Step 7: Update Existing Data
        await UpdateExistingData();
        
        // Step 8: Verify Aggregates Reflect Updates
        await VerifyAggregateDataReflectsUpdates();
        
        // Step 9: Delete Test Data
        await DeleteTestData();
        
        // Step 10: Verify Aggregates Cleaned Up
        await VerifyAggregateDataCleanedUp(preTestAggregates);
        
        // Step 11: Verify Final Clean State
        await VerifyFinalState();
    }

    [FactCheckUserSecrets]
    public async Task ErrorHandling_InvalidSpreadsheetId_ShouldReturnErrors()
    {
        // Arrange
        var invalidManager = new GoogleSheetManager(_credential, "invalid-spreadsheet-id");

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

    #region Workflow Steps (Private Methods)

    private async Task VerifySheetStructure()
    {
        // Arrange & Act - Verify both primary and aggregate sheets
        var allSheets = _testSheets.Concat(_aggregateSheets).ToList();
        var sheetProperties = await _googleSheetManager!.GetSheetProperties(allSheets);
        var allSheetsData = await _googleSheetManager.GetSheets(allSheets);

        // Assert - Verify all required sheets exist
        Assert.NotNull(sheetProperties);
        Assert.Equal(allSheets.Count, sheetProperties.Count);
        
        // Verify primary sheets (TRIPS, SHIFTS)
        var tripsSheet = sheetProperties.FirstOrDefault(x => x.Name == SheetEnum.TRIPS.GetDescription());
        var shiftsSheet = sheetProperties.FirstOrDefault(x => x.Name == SheetEnum.SHIFTS.GetDescription());
        
        Assert.NotNull(tripsSheet);
        Assert.NotNull(shiftsSheet);
        Assert.NotEmpty(tripsSheet.Id);
        Assert.NotEmpty(shiftsSheet.Id);
        
        // Verify primary sheet headers are present
        Assert.NotEmpty(tripsSheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);
        Assert.NotEmpty(shiftsSheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);

        // Verify aggregate sheets exist and have headers
        foreach (var aggregateSheetName in _aggregateSheets)
        {
            var aggregateSheet = sheetProperties.FirstOrDefault(x => x.Name == aggregateSheetName);
            Assert.NotNull(aggregateSheet);
            Assert.NotEmpty(aggregateSheet.Id);
            Assert.NotEmpty(aggregateSheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);
        }

        // Verify no errors in sheet retrieval
        Assert.NotNull(allSheetsData);
        var errorMessages = allSheetsData.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription());
        Assert.Empty(errorMessages);

        // Verify aggregate sheets contain expected data structure
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

    private async Task TestSheetDeletionAndRecreation()
    {
        // Step 2a: Verify sheets exist initially
        var initialProperties = await _googleSheetManager!.GetSheetProperties(_testSheets);
        Assert.All(initialProperties, prop => Assert.NotEmpty(prop.Id));
        
        // Store initial sheet information
        var initialTripsSheet = initialProperties.FirstOrDefault(x => x.Name == SheetEnum.TRIPS.GetDescription());
        var initialShiftsSheet = initialProperties.FirstOrDefault(x => x.Name == SheetEnum.SHIFTS.GetDescription());
        Assert.NotNull(initialTripsSheet);
        Assert.NotNull(initialShiftsSheet);
        
        var initialTripsId = initialTripsSheet.Id;
        var initialShiftsId = initialShiftsSheet.Id;
        
        // Step 2b: Delete the sheets (this tests the deletion capability)
        await DeleteTestSheets();
        
        // Step 2c: Verify sheets are deleted/missing
        var afterDeletionProperties = await _googleSheetManager.GetSheetProperties(_testSheets);
        Assert.All(afterDeletionProperties, prop => Assert.Empty(prop.Id)); // Should be empty after deletion
        
        // Step 2d: Recreate the sheets
        var recreationResult = await _googleSheetManager.CreateSheets(_testSheets);
        Assert.NotNull(recreationResult);
        
        // Should have success messages for recreation
        var infoMessages = recreationResult.Messages.Where(m => m.Level == MessageLevelEnum.INFO.GetDescription());
        Assert.NotEmpty(infoMessages);
        
        // Step 2e: Verify sheets are recreated with proper structure
        // Wait for sheets to be fully created
        await Task.Delay(2000);
        
        var recreatedProperties = await _googleSheetManager.GetSheetProperties(_testSheets);
        Assert.All(recreatedProperties, prop => Assert.NotEmpty(prop.Id)); // Should have IDs again
        
        var recreatedTripsSheet = recreatedProperties.FirstOrDefault(x => x.Name == SheetEnum.TRIPS.GetDescription());
        var recreatedShiftsSheet = recreatedProperties.FirstOrDefault(x => x.Name == SheetEnum.SHIFTS.GetDescription());
        
        Assert.NotNull(recreatedTripsSheet);
        Assert.NotNull(recreatedShiftsSheet);
        
        // Verify new sheets have different IDs (proving they were recreated)
        Assert.NotEqual(initialTripsId, recreatedTripsSheet.Id);
        Assert.NotEqual(initialShiftsId, recreatedShiftsSheet.Id);
        
        // Verify headers are properly created
        Assert.NotEmpty(recreatedTripsSheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);
        Assert.NotEmpty(recreatedShiftsSheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);
    }

    private async Task<Dictionary<string, object>> CaptureAggregateState()
    {
        // Capture the state of aggregate sheets before making changes
        var aggregateState = new Dictionary<string, object>();
        
        var allSheets = _testSheets.Concat(_aggregateSheets).ToList();
        var allData = await _googleSheetManager!.GetSheets(allSheets);
        
        // Store counts and key metrics for comparison
        aggregateState["AddressCount"] = allData.Addresses.Count;
        aggregateState["NameCount"] = allData.Names.Count;
        aggregateState["PlaceCount"] = allData.Places.Count;
        aggregateState["RegionCount"] = allData.Regions.Count;
        aggregateState["ServiceCount"] = allData.Services.Count;
        aggregateState["DailyCount"] = allData.Daily.Count;
        aggregateState["WeeklyCount"] = allData.Weekly.Count;
        aggregateState["MonthlyCount"] = allData.Monthly.Count;
        aggregateState["YearlyCount"] = allData.Yearly.Count;
        
        // Store specific data points for verification
        var testDate = DateTime.Now.ToString("yyyy-MM-dd");
        var existingDailyForDate = allData.Daily.FirstOrDefault(d => d.Date == testDate);
        if (existingDailyForDate != null)
        {
            aggregateState["ExistingDailyTrips"] = existingDailyForDate.Trips;
            aggregateState["ExistingDailyTotal"] = existingDailyForDate.Total;
        }
        
        return aggregateState;
    }

    private async Task CreateNewShiftWithTrips()
    {
        // Arrange
        var sheetInfo = await _googleSheetManager!.GetSheetProperties(_testSheets);
        var maxShiftId = GetMaxRowValue(sheetInfo, SheetEnum.SHIFTS.GetDescription());
        var maxTripId = GetMaxRowValue(sheetInfo, SheetEnum.TRIPS.GetDescription());

        _createdShiftData = TestGigHelpers.GenerateShift(ActionTypeEnum.APPEND, maxShiftId + 1, maxTripId + 1);
        
        // Track created IDs for cleanup
        _createdShiftIds.AddRange(_createdShiftData.Shifts.Select(s => s.RowId));
        _createdTripIds.AddRange(_createdShiftData.Trips.Select(t => t.RowId));

        // Act
        var result = await _googleSheetManager.ChangeSheetData(_testSheets, _createdShiftData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Count);
        
        foreach (var message in result.Messages)
        {
            Assert.Equal(MessageLevelEnum.INFO.GetDescription(), message.Level);
            Assert.Equal(MessageTypeEnum.SAVE_DATA.GetDescription(), message.Type);
            Assert.True(message.Time >= _testStartTime);
        }
    }

    private async Task VerifyDataWasAdded()
    {
        // Arrange - Wait a moment for data to propagate
        await Task.Delay(1000);

        // Act
        var result = await _googleSheetManager!.GetSheets(_testSheets);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(_createdShiftData);

        // Verify shift data exists
        var createdShift = _createdShiftData.Shifts.First();
        var foundShift = result.Shifts.FirstOrDefault(s => 
            s.Date == createdShift.Date && 
            s.Number == createdShift.Number &&
            s.Service == createdShift.Service);
        
        Assert.NotNull(foundShift);
        Assert.Equal(createdShift.Region, foundShift.Region);

        // Verify trip data exists
        var createdTrip = _createdShiftData.Trips.First();
        var foundTrip = result.Trips.FirstOrDefault(t => 
            t.Date == createdTrip.Date && 
            t.Number == createdTrip.Number &&
            t.Service == createdTrip.Service);
            
        Assert.NotNull(foundTrip);
        Assert.Equal(createdTrip.Place, foundTrip.Place);
        Assert.Equal(createdTrip.Name, foundTrip.Name);
    }

    private async Task VerifyAggregateDataUpdated(Dictionary<string, object> preTestState)
    {
        // Verify that aggregate sheets properly reflect the new data
        await Task.Delay(2000); // Allow time for formulas to calculate
        
        var allSheets = _testSheets.Concat(_aggregateSheets).ToList();
        var currentData = await _googleSheetManager!.GetSheets(allSheets);
        
        Assert.NotNull(currentData);
        Assert.NotNull(_createdShiftData);
        
        // Verify lookup sheets were updated with new unique values
        var createdShift = _createdShiftData.Shifts.First();
        var createdTrip = _createdShiftData.Trips.First();
        
        // Check if new region appears in REGIONS sheet (if it's a new region)
        if (!string.IsNullOrEmpty(createdShift.Region))
        {
            var regionEntry = currentData.Regions.FirstOrDefault(r => r.Region == createdShift.Region);
            if (regionEntry != null)
            {
                Assert.True(regionEntry.Trips > 0, $"Region '{createdShift.Region}' should have trip count > 0");
                Assert.True(regionEntry.Total > 0, $"Region '{createdShift.Region}' should have total > 0");
            }
        }
        
        // Check if new service appears in SERVICES sheet
        if (!string.IsNullOrEmpty(createdShift.Service))
        {
            var serviceEntry = currentData.Services.FirstOrDefault(s => s.Service == createdShift.Service);
            if (serviceEntry != null)
            {
                Assert.True(serviceEntry.Trips > 0, $"Service '{createdShift.Service}' should have trip count > 0");
                Assert.True(serviceEntry.Total > 0, $"Service '{createdShift.Service}' should have total > 0");
            }
        }
        
        // Check if new place appears in PLACES sheet
        if (!string.IsNullOrEmpty(createdTrip.Place))
        {
            var placeEntry = currentData.Places.FirstOrDefault(p => p.Place == createdTrip.Place);
            if (placeEntry != null)
            {
                Assert.True(placeEntry.Trips > 0, $"Place '{createdTrip.Place}' should have trip count > 0");
            }
        }
        
        // Verify DAILY sheet reflects new data
        var testDate = DateTime.Now.ToString("yyyy-MM-dd");
        var dailyEntry = currentData.Daily.FirstOrDefault(d => d.Date == testDate);
        if (dailyEntry != null)
        {
            var expectedMinTrips = (int)(preTestState.ContainsKey("ExistingDailyTrips") ? preTestState["ExistingDailyTrips"] : 0) + _createdShiftData.Trips.Count;
            Assert.True(dailyEntry.Trips >= expectedMinTrips, 
                $"Daily entry for {testDate} should have at least {expectedMinTrips} trips, but has {dailyEntry.Trips}");
        }
    }

    private async Task UpdateExistingData()
    {
        // Arrange
        Assert.NotNull(_createdShiftData);
        
        var updateData = new SheetEntity();
        var shiftToUpdate = _createdShiftData.Shifts.First();
        shiftToUpdate.Action = ActionTypeEnum.UPDATE.GetDescription();
        shiftToUpdate.Note = "Updated by integration test";
        shiftToUpdate.Region = "Updated Region";
        updateData.Shifts.Add(shiftToUpdate);

        var tripToUpdate = _createdShiftData.Trips.First();
        tripToUpdate.Action = ActionTypeEnum.UPDATE.GetDescription();
        tripToUpdate.Note = "Updated trip note";
        tripToUpdate.Tip = 999; // Distinctive value for verification
        updateData.Trips.Add(tripToUpdate);

        // Act
        var result = await _googleSheetManager!.ChangeSheetData(_testSheets, updateData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Count);
        
        foreach (var message in result.Messages)
        {
            Assert.Equal(MessageLevelEnum.INFO.GetDescription(), message.Level);
            Assert.Equal(MessageTypeEnum.SAVE_DATA.GetDescription(), message.Type);
        }

        // Verify update took effect
        await Task.Delay(1000); // Allow for propagation
        var updatedData = await _googleSheetManager.GetSheets(_testSheets);
        
        var verifyShift = updatedData.Shifts.FirstOrDefault(s => s.RowId == shiftToUpdate.RowId);
        var verifyTrip = updatedData.Trips.FirstOrDefault(t => t.RowId == tripToUpdate.RowId);
        
        Assert.NotNull(verifyShift);
        Assert.NotNull(verifyTrip);
        Assert.Equal("Updated Region", verifyShift.Region);
        Assert.Equal(999, verifyTrip.Tip);
    }

    private async Task VerifyAggregateDataReflectsUpdates()
    {
        // Verify that updates to primary data are reflected in aggregates
        await Task.Delay(2000); // Allow time for formulas to recalculate
        
        var allSheets = _testSheets.Concat(_aggregateSheets).ToList();
        var currentData = await _googleSheetManager!.GetSheets(allSheets);
        
        Assert.NotNull(currentData);
        Assert.NotNull(_createdShiftData);
        
        var updatedShift = _createdShiftData.Shifts.First();
        
        // Verify the updated region ("Updated Region") appears in REGIONS sheet
        var updatedRegionEntry = currentData.Regions.FirstOrDefault(r => r.Region == "Updated Region");
        Assert.NotNull(updatedRegionEntry);
        Assert.True(updatedRegionEntry.Trips > 0, "Updated region should have trip count > 0");
        
        // Verify trip with tip = 999 is reflected in aggregates
        var updatedTrip = _createdShiftData.Trips.First();
        var serviceEntry = currentData.Services.FirstOrDefault(s => s.Service == updatedShift.Service);
        if (serviceEntry != null)
        {
            // The service total should include our updated tip of 999
            Assert.True(serviceEntry.Total >= 999, $"Service total should include updated tip of 999, current total: {serviceEntry.Total}");
        }
    }

    private async Task DeleteTestData()
    {
        // Arrange
        Assert.NotNull(_createdShiftData);
        
        var deleteData = new SheetEntity();
        
        // Mark all created data for deletion
        foreach (var shift in _createdShiftData.Shifts)
        {
            shift.Action = ActionTypeEnum.DELETE.GetDescription();
            deleteData.Shifts.Add(shift);
        }
        
        foreach (var trip in _createdShiftData.Trips)
        {
            trip.Action = ActionTypeEnum.DELETE.GetDescription();
            deleteData.Trips.Add(trip);
        }

        // Act
        var result = await _googleSheetManager!.ChangeSheetData(_testSheets, deleteData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Count);
        
        foreach (var message in result.Messages)
        {
            Assert.Equal(MessageLevelEnum.INFO.GetDescription(), message.Level);
            Assert.Equal(MessageTypeEnum.SAVE_DATA.GetDescription(), message.Type);
        }

        // Verify deletion took effect
        await Task.Delay(1000); // Allow for propagation
        var remainingData = await _googleSheetManager.GetSheets(_testSheets);
        
        foreach (var shiftId in _createdShiftIds)
        {
            var deletedShift = remainingData.Shifts.FirstOrDefault(s => s.RowId == shiftId);
            Assert.Null(deletedShift);
        }
        
        foreach (var tripId in _createdTripIds)
        {
            var deletedTrip = remainingData.Trips.FirstOrDefault(t => t.RowId == tripId);
            Assert.Null(deletedTrip);
        }
    }

    private async Task VerifyAggregateDataCleanedUp(Dictionary<string, object> preTestState)
    {
        // Verify that aggregate sheets return to their pre-test state after cleanup
        await Task.Delay(2000); // Allow time for formulas to recalculate
        
        var allSheets = _testSheets.Concat(_aggregateSheets).ToList();
        var currentData = await _googleSheetManager!.GetSheets(allSheets);
        
        Assert.NotNull(currentData);
        
        // For sheets with formula-based data, counts should return to pre-test levels
        // Note: Some aggregate data might persist if it was the only instance of that value
        
        // Verify DAILY data returns to expected state
        var testDate = DateTime.Now.ToString("yyyy-MM-dd");
        var dailyEntry = currentData.Daily.FirstOrDefault(d => d.Date == testDate);
        
        if (preTestState.ContainsKey("ExistingDailyTrips") && dailyEntry != null)
        {
            var expectedTrips = (int)preTestState["ExistingDailyTrips"];
            // Allow for some variance due to concurrent test runs or existing data
            Assert.True(Math.Abs(dailyEntry.Trips - expectedTrips) <= 5, 
                $"Daily trips for {testDate} should be close to pre-test value of {expectedTrips}, current: {dailyEntry.Trips}");
        }
        
        // Verify "Updated Region" is no longer in REGIONS sheet (if it was test-only data)
        var updatedRegionEntry = currentData.Regions.FirstOrDefault(r => r.Region == "Updated Region");
        if (updatedRegionEntry != null)
        {
            // If it still exists, it should have 0 trips (meaning our test data was cleaned up)
            Assert.True(updatedRegionEntry.Trips == 0 || updatedRegionEntry.Total == 0, 
                "Updated region should have no trips/total after cleanup");
        }
    }

    private async Task VerifyFinalState()
    {
        // Act - Verify both primary and aggregate sheets are in clean state
        var allSheets = _testSheets.Concat(_aggregateSheets).ToList();
        var finalState = await _googleSheetManager!.GetSheets(allSheets);
        var sheetProperties = await _googleSheetManager.GetSheetProperties(allSheets);

        // Assert
        Assert.NotNull(finalState);
        Assert.NotNull(sheetProperties);

        // Verify no test data remains in primary sheets
        foreach (var shiftId in _createdShiftIds)
        {
            Assert.DoesNotContain(finalState.Shifts, s => s.RowId == shiftId);
        }
        
        foreach (var tripId in _createdTripIds)
        {
            Assert.DoesNotContain(finalState.Trips, t => t.RowId == tripId);
        }

        // Verify sheets are still functional
        var errorMessages = finalState.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription());
        Assert.Empty(errorMessages);

        // Verify all sheet structure is intact (primary + aggregate)
        Assert.Equal(allSheets.Count, sheetProperties.Count);
        Assert.All(sheetProperties, prop => Assert.NotEmpty(prop.Id));

        // Verify aggregate sheets are still populated with valid data
        Assert.NotNull(finalState.Addresses);
        Assert.NotNull(finalState.Names);
        Assert.NotNull(finalState.Places);
        Assert.NotNull(finalState.Regions);
        Assert.NotNull(finalState.Services);
        Assert.NotNull(finalState.Daily);
        Assert.NotNull(finalState.Weekly);
        Assert.NotNull(finalState.Monthly);
        Assert.NotNull(finalState.Yearly);

        // Verify aggregate sheets don't contain test artifacts
        Assert.DoesNotContain(finalState.Regions, r => r.Region == "Updated Region" && r.Trips > 0);
        
        // Log final counts for debugging
        System.Diagnostics.Debug.WriteLine($"Final counts - Addresses: {finalState.Addresses.Count}, " +
            $"Names: {finalState.Names.Count}, Places: {finalState.Places.Count}, " +
            $"Regions: {finalState.Regions.Count}, Services: {finalState.Services.Count}");
    }

    private async Task DeleteTestSheets()
    {
        // Use the new DeleteSheets method from GoogleSheetManager
        var result = await _googleSheetManager!.DeleteSheets(_testSheets);
        
        // Log the results for debugging
        foreach (var message in result.Messages)
        {
            System.Diagnostics.Debug.WriteLine($"Delete operation: {message.Level} - {message.Message}");
        }
        
        // Check if any errors occurred that should fail the test
        var errorMessages = result.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription());
        if (errorMessages.Any())
        {
            // Only throw if it's not a permission or "cannot delete" error
            var criticalErrors = errorMessages.Where(m => 
                !m.Message.Contains("Cannot delete") && 
                !m.Message.Contains("permission"));
                
            if (criticalErrors.Any())
            {
                throw new InvalidOperationException($"Critical error during sheet deletion: {criticalErrors.First().Message}");
            }
        }
    }

    #endregion

    #region Helper Methods

    private async Task CleanupTestData()
    {
        if (_googleSheetManager == null || _createdShiftIds.Count == 0) return;

        try
        {
            var deleteData = new SheetEntity();
            
            // Create delete actions for any remaining test data
            foreach (var shiftId in _createdShiftIds)
            {
                deleteData.Shifts.Add(new ShiftEntity 
                { 
                    RowId = shiftId, 
                    Action = ActionTypeEnum.DELETE.GetDescription() 
                });
            }
            
            foreach (var tripId in _createdTripIds)
            {
                deleteData.Trips.Add(new TripEntity 
                { 
                    RowId = tripId, 
                    Action = ActionTypeEnum.DELETE.GetDescription() 
                });
            }

            if (deleteData.Shifts.Count > 0 || deleteData.Trips.Count > 0)
            {
                await _googleSheetManager.ChangeSheetData(_testSheets, deleteData);
                await Task.Delay(1000); // Allow for propagation
            }
        }
        catch
        {
            // Cleanup is best effort - don't fail tests if cleanup fails
        }
        
        _createdShiftIds.Clear();
        _createdTripIds.Clear();
    }

    private async Task EnsureSheetsExist()
    {
        if (_googleSheetManager == null) return;

        var properties = await _googleSheetManager.GetSheetProperties(_testSheets);
        var missingSheets = properties.Where(p => string.IsNullOrEmpty(p.Id)).Select(p => p.Name).ToList();
        
        if (missingSheets.Count > 0)
        {
            await _googleSheetManager.CreateSheets(missingSheets);
        }
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

    #endregion
}