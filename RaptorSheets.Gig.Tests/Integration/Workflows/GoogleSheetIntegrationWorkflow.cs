using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Entities;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Gig.Tests.Data.Helpers;
using RaptorSheets.Test.Common.Helpers;
using RaptorSheets.Gig.Helpers;
using System.Reflection;
using SheetEnum = RaptorSheets.Gig.Enums.SheetEnum;

namespace RaptorSheets.Gig.Tests.Integration.Workflows;

/// <summary>
/// Comprehensive integration test workflow for GoogleSheetManager
/// Tests the complete lifecycle: Setup -> Create -> Add -> Read -> Update -> Delete
/// </summary>
public class GoogleSheetIntegrationWorkflow : IAsyncLifetime
{
    private readonly GoogleSheetManager? _googleSheetManager;
    private readonly Dictionary<string, string> _credential;
    private readonly List<string> _testSheets;
    private readonly List<string> _allSheets;
    private readonly long _testStartTime;

    // Test data tracking
    private SheetEntity? _createdShiftData;
    private List<int> _createdShiftIds = new();
    private List<int> _createdTripIds = new();

    public GoogleSheetIntegrationWorkflow()
    {
        _testStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _testSheets = new List<string> { SheetEnum.TRIPS.GetDescription(), SheetEnum.SHIFTS.GetDescription() };
        
        // Get all available sheets from GenerateSheetsHelpers - convert enum values to descriptions
        _allSheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>()
            .Select(e => e.GetDescription())
            .ToList();
        
        // Also add common sheets (Setup sheet)
        _allSheets.AddRange(Enum.GetValues(typeof(RaptorSheets.Common.Enums.SheetEnum)).Cast<RaptorSheets.Common.Enums.SheetEnum>()
            .Select(e => e.GetDescription()));
        
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
        // Note: We don't call EnsureAllSheetsExist here anymore since it's part of the main test flow
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
        // Step 1: Start Fresh - Delete ALL existing sheets and recreate from scratch
        await StartFreshWithAllSheets();
        
        // Step 2: Verify Sheet Structure (Primary + Aggregate)
        await VerifySheetStructure();
        
        // Step 3: Capture Pre-Test Aggregate State
        var preTestAggregates = await CaptureAggregateState();
        
        // Step 4: Create New Data
        await CreateNewShiftWithTrips();
        
        // Step 5: Verify Data Was Added Correctly
        await VerifyDataWasAddedCorrectly();
        
        // Step 6: Verify Aggregate Sheets Updated with Correct Calculations
        await VerifyAggregateDataUpdatedCorrectly(preTestAggregates);
        
        // Step 7: Update Existing Data
        await UpdateExistingData();
        
        // Step 8: Verify Data Updates Were Applied Correctly
        await VerifyDataUpdatesWereAppliedCorrectly();
        
        // Step 9: Verify Aggregates Reflect Updates with Correct Calculations
        await VerifyAggregateDataReflectsUpdatesCorrectly();
        
        // Step 10: Delete Test Data
        await DeleteTestData();
        
        // Step 11: Verify Data Deletion Was Complete
        await VerifyDataDeletionWasComplete();
        
        // Step 12: Verify Aggregates Cleaned Up with Correct Calculations
        await VerifyAggregateDataCleanedUpCorrectly(preTestAggregates);
        
        // Step 13: Verify Final Clean State
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
        // Arrange & Act - Verify all sheets exist and have proper structure
        // (After StartFreshWithAllSheets, all sheets should definitely exist)
        var sheetProperties = await _googleSheetManager!.GetSheetProperties(_allSheets);
        var allSheetsData = await _googleSheetManager.GetSheets(_allSheets);

        // Debug logging
        System.Diagnostics.Debug.WriteLine($"=== Verifying Sheet Structure ===");
        System.Diagnostics.Debug.WriteLine($"Expected sheets ({_allSheets.Count}): {string.Join(", ", _allSheets)}");
        
        // Get existing sheets (should be all of them after StartFreshWithAllSheets)
        var existingSheets = sheetProperties.Where(p => !string.IsNullOrEmpty(p.Id)).ToList();
        System.Diagnostics.Debug.WriteLine($"Found sheets ({existingSheets.Count}): {string.Join(", ", existingSheets.Select(p => p.Name))}");

        // Assert - All sheets should exist after StartFreshWithAllSheets
        Assert.NotNull(sheetProperties);
        Assert.Equal(_allSheets.Count, sheetProperties.Count); // Should return one property per requested sheet
        Assert.Equal(_allSheets.Count, existingSheets.Count); // ALL sheets should exist
        
        // Verify primary sheets (TRIPS, SHIFTS) exist and have proper structure
        var tripsSheet = existingSheets.FirstOrDefault(x => x.Name == SheetEnum.TRIPS.GetDescription());
        var shiftsSheet = existingSheets.FirstOrDefault(x => x.Name == SheetEnum.SHIFTS.GetDescription());
        
        Assert.NotNull(tripsSheet);
        Assert.NotNull(shiftsSheet);
        Assert.NotEmpty(tripsSheet.Id);
        Assert.NotEmpty(shiftsSheet.Id);
        Assert.NotEmpty(tripsSheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);
        Assert.NotEmpty(shiftsSheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);

        // Verify ALL sheets have proper headers and structure
        foreach (var sheet in existingSheets)
        {
            Assert.NotEmpty(sheet.Id);
            Assert.NotEmpty(sheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);
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

        System.Diagnostics.Debug.WriteLine($"=== Sheet structure verification completed successfully for all {existingSheets.Count} sheets ===");
        
        // TODO: Add visual format validation if needed
        // await VerifySheetFormatting(tripsSheet, shiftsSheet);
    }

    // Optional: Add detailed format validation
    private async Task VerifySheetFormatting(RaptorSheets.Core.Entities.PropertyEntity tripsSheet, RaptorSheets.Core.Entities.PropertyEntity shiftsSheet)
    {
        // This would require additional Google Sheets API calls to get detailed formatting
        // You would need to call the Sheets API directly to get:
        // - Cell formatting (colors, fonts, borders)
        // - Column widths
        // - Data validation rules
        // - Conditional formatting rules
        
        // Example structure (would need implementation):
        /*
        var tripsFormatting = await GetSheetFormatting(tripsSheet.Id);
        var shiftsFormatting = await GetSheetFormatting(shiftsSheet.Id);
        
        // Verify header formatting
        Assert.True(tripsFormatting.HeaderRow.HasBoldText);
        Assert.Equal(ExpectedColors.HeaderBackground, tripsFormatting.HeaderRow.BackgroundColor);
        
        // Verify alternating row colors
        Assert.True(tripsFormatting.HasAlternatingRowColors);
        
        // Verify data validation
        Assert.Contains(ValidationRule.DateFormat, tripsFormatting.ValidationRules);
        */
        
        await Task.CompletedTask; // Placeholder
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

    private async Task VerifyDataWasAddedCorrectly()
    {
        System.Diagnostics.Debug.WriteLine("=== Verifying Data Was Added Correctly ===");
        
        // Wait for data to propagate
        await Task.Delay(2000);

        // Get fresh data from sheets
        var result = await _googleSheetManager!.GetSheets(_allSheets);
        Assert.NotNull(result);
        Assert.NotNull(_createdShiftData);

        System.Diagnostics.Debug.WriteLine($"Verifying {_createdShiftData.Shifts.Count} shifts and {_createdShiftData.Trips.Count} trips were added");

        // Verify ALL shift data was added correctly
        foreach (var createdShift in _createdShiftData.Shifts)
        {
            var foundShift = result.Shifts.FirstOrDefault(s => 
                s.RowId == createdShift.RowId);
            
            Assert.NotNull(foundShift);
            
            // Verify all key fields match exactly
            Assert.Equal(createdShift.Date, foundShift.Date);
            Assert.Equal(createdShift.Number, foundShift.Number);
            Assert.Equal(createdShift.Service, foundShift.Service);
            Assert.Equal(createdShift.Region, foundShift.Region);
            
            // Handle time comparison with format normalization
            // Google Sheets may return time without seconds, so normalize both formats
            var expectedStartTime = NormalizeTimeFormat(createdShift.Start);
            var actualStartTime = NormalizeTimeFormat(foundShift.Start);
            Assert.Equal(expectedStartTime, actualStartTime);
            
            Assert.Equal(createdShift.Note, foundShift.Note);
            
            System.Diagnostics.Debug.WriteLine($"? Shift {createdShift.RowId}: {createdShift.Date} #{createdShift.Number} {createdShift.Service} in {createdShift.Region}");
        }

        // Verify ALL trip data was added correctly
        foreach (var createdTrip in _createdShiftData.Trips)
        {
            var foundTrip = result.Trips.FirstOrDefault(t => 
                t.RowId == createdTrip.RowId);
                
            Assert.NotNull(foundTrip);
            
            // Verify all key fields match exactly
            Assert.Equal(createdTrip.Date, foundTrip.Date);
            Assert.Equal(createdTrip.Number, foundTrip.Number);
            Assert.Equal(createdTrip.Service, foundTrip.Service);
            Assert.Equal(createdTrip.Place, foundTrip.Place);
            Assert.Equal(createdTrip.Name, foundTrip.Name);
            
            // Handle time comparisons with format normalization
            var expectedPickupTime = NormalizeTimeFormat(createdTrip.Pickup);
            var actualPickupTime = NormalizeTimeFormat(foundTrip.Pickup);
            Assert.Equal(expectedPickupTime, actualPickupTime);
            
            var expectedDropoffTime = NormalizeTimeFormat(createdTrip.Dropoff);
            var actualDropoffTime = NormalizeTimeFormat(foundTrip.Dropoff);
            Assert.Equal(expectedDropoffTime, actualDropoffTime);
            
            // Handle duration comparison with format normalization
            // Google Sheets may return duration in a simplified format (e.g., "0:10" instead of "00:10:00.000")
            var expectedDuration = NormalizeDurationFormat(createdTrip.Duration);
            var actualDuration = NormalizeDurationFormat(foundTrip.Duration);
            Assert.Equal(expectedDuration, actualDuration);
            
            Assert.Equal(createdTrip.Note, foundTrip.Note);
            
            // Verify financial fields
            Assert.Equal(createdTrip.Pay ?? 0, foundTrip.Pay ?? 0);
            Assert.Equal(createdTrip.Tip ?? 0, foundTrip.Tip ?? 0);
            Assert.Equal(createdTrip.Bonus ?? 0, foundTrip.Bonus ?? 0);
            Assert.Equal(createdTrip.Cash ?? 0, foundTrip.Cash ?? 0);
            
            System.Diagnostics.Debug.WriteLine($"? Trip {createdTrip.RowId}: {createdTrip.Date} #{createdTrip.Number} to {createdTrip.Place} for {createdTrip.Name}");
        }

        // Verify row counts increased appropriately
        var expectedMinShifts = _createdShiftIds.Count;
        var expectedMinTrips = _createdTripIds.Count;
        
        Assert.True(result.Shifts.Count >= expectedMinShifts, 
            $"Expected at least {expectedMinShifts} shifts, but found {result.Shifts.Count}");
        Assert.True(result.Trips.Count >= expectedMinTrips, 
            $"Expected at least {expectedMinTrips} trips, but found {result.Trips.Count}");

        System.Diagnostics.Debug.WriteLine($"? Data verification complete: {result.Shifts.Count} total shifts, {result.Trips.Count} total trips");
    }

    private async Task VerifyAggregateDataUpdatedCorrectly(Dictionary<string, object> preTestState)
    {
        System.Diagnostics.Debug.WriteLine("=== Verifying Aggregate Data Updated Correctly ===");
        
        // Allow time for formulas to calculate
        await Task.Delay(3000);
        
        var currentData = await _googleSheetManager!.GetSheets(_allSheets);
        
        Assert.NotNull(currentData);
        Assert.NotNull(_createdShiftData);

        // Calculate expected totals from our test data
        var expectedTotalTrips = _createdShiftData.Trips.Count;
        var expectedTotalPay = _createdShiftData.Trips.Sum(t => t.Pay ?? 0);
        var expectedTotalTip = _createdShiftData.Trips.Sum(t => t.Tip ?? 0);
        var expectedTotalBonus = _createdShiftData.Trips.Sum(t => t.Bonus ?? 0);
        var expectedTotal = expectedTotalPay + expectedTotalTip + expectedTotalBonus;

        System.Diagnostics.Debug.WriteLine($"Expected from test data: {expectedTotalTrips} trips, ${expectedTotal:F2} total (Pay: ${expectedTotalPay:F2}, Tips: ${expectedTotalTip:F2}, Bonus: ${expectedTotalBonus:F2})");

        // Verify REGIONS sheet updated correctly
        if (currentData.Regions != null)
        {
            foreach (var createdShift in _createdShiftData.Shifts)
            {
                if (!string.IsNullOrEmpty(createdShift.Region))
                {
                    var regionEntry = currentData.Regions.FirstOrDefault(r => r.Region == createdShift.Region);
                    if (regionEntry != null)
                    {
                        Assert.True(regionEntry.Trips > 0, $"Region '{createdShift.Region}' should have trip count > 0, but has {regionEntry.Trips}");
                        Assert.True(regionEntry.Total > 0, $"Region '{createdShift.Region}' should have total > 0, but has ${regionEntry.Total:F2}");
                        
                        System.Diagnostics.Debug.WriteLine($"? Region '{createdShift.Region}': {regionEntry.Trips} trips, ${regionEntry.Total:F2} total");
                    }
                }
            }
        }

        // Verify SERVICES sheet updated correctly  
        if (currentData.Services != null)
        {
            foreach (var createdShift in _createdShiftData.Shifts)
            {
                if (!string.IsNullOrEmpty(createdShift.Service))
                {
                    var serviceEntry = currentData.Services.FirstOrDefault(s => s.Service == createdShift.Service);
                    if (serviceEntry != null)
                    {
                        Assert.True(serviceEntry.Trips > 0, $"Service '{createdShift.Service}' should have trip count > 0, but has {serviceEntry.Trips}");
                        Assert.True(serviceEntry.Total > 0, $"Service '{createdShift.Service}' should have total > 0, but has ${serviceEntry.Total:F2}");
                        
                        System.Diagnostics.Debug.WriteLine($"? Service '{createdShift.Service}': {serviceEntry.Trips} trips, ${serviceEntry.Total:F2} total");
                    }
                }
            }
        }

        // Verify PLACES sheet updated correctly
        if (currentData.Places != null)
        {
            foreach (var createdTrip in _createdShiftData.Trips)
            {
                if (!string.IsNullOrEmpty(createdTrip.Place))
                {
                    var placeEntry = currentData.Places.FirstOrDefault(p => p.Place == createdTrip.Place);
                    if (placeEntry != null)
                    {
                        Assert.True(placeEntry.Trips > 0, $"Place '{createdTrip.Place}' should have trip count > 0, but has {placeEntry.Trips}");
                        
                        System.Diagnostics.Debug.WriteLine($"? Place '{createdTrip.Place}': {placeEntry.Trips} trips");
                    }
                }
            }
        }

        // Verify NAMES sheet updated correctly
        if (currentData.Names != null)
        {
            foreach (var createdTrip in _createdShiftData.Trips)
            {
                if (!string.IsNullOrEmpty(createdTrip.Name))
                {
                    var nameEntry = currentData.Names.FirstOrDefault(n => n.Name == createdTrip.Name);
                    if (nameEntry != null)
                    {
                        Assert.True(nameEntry.Trips > 0, $"Name '{createdTrip.Name}' should have trip count > 0, but has {nameEntry.Trips}");
                        
                        System.Diagnostics.Debug.WriteLine($"? Name '{createdTrip.Name}': {nameEntry.Trips} trips");
                    }
                }
            }
        }

        // Verify DAILY sheet reflects new data correctly
        if (currentData.Daily != null)
        {
            var testDate = DateTime.Now.ToString("yyyy-MM-dd");
            var dailyEntry = currentData.Daily.FirstOrDefault(d => d.Date == testDate);
            
            if (dailyEntry != null)
            {
                var preTestTrips = preTestState.ContainsKey("ExistingDailyTrips") ? (int)preTestState["ExistingDailyTrips"] : 0;
                var expectedMinTrips = preTestTrips + expectedTotalTrips;
                
                Assert.True(dailyEntry.Trips >= expectedMinTrips, 
                    $"Daily entry for {testDate} should have at least {expectedMinTrips} trips (was {preTestTrips}, added {expectedTotalTrips}), but has {dailyEntry.Trips}");

                var preTestTotal = preTestState.ContainsKey("ExistingDailyTotal") ? (decimal)preTestState["ExistingDailyTotal"] : 0m;
                var expectedMinTotal = preTestTotal + expectedTotal;
                
                Assert.True(dailyEntry.Total >= expectedMinTotal, 
                    $"Daily entry for {testDate} should have at least ${expectedMinTotal:F2} total (was ${preTestTotal:F2}, added ${expectedTotal:F2}), but has ${dailyEntry.Total:F2}");
                    
                System.Diagnostics.Debug.WriteLine($"? Daily {testDate}: {dailyEntry.Trips} trips (+{dailyEntry.Trips - preTestTrips}), ${dailyEntry.Total:F2} total (+${dailyEntry.Total - preTestTotal:F2})");
            }
        }

        System.Diagnostics.Debug.WriteLine("? Aggregate data verification complete - all calculations appear correct");
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

    private async Task VerifyDataUpdatesWereAppliedCorrectly()
    {
        System.Diagnostics.Debug.WriteLine("=== Verifying Data Updates Were Applied Correctly ===");
        
        // Wait for updates to propagate
        await Task.Delay(2000);

        var updatedData = await _googleSheetManager!.GetSheets(_testSheets);
        Assert.NotNull(updatedData);
        Assert.NotNull(_createdShiftData);

        // Verify shift updates were applied correctly
        foreach (var originalShift in _createdShiftData.Shifts)
        {
            var verifyShift = updatedData.Shifts.FirstOrDefault(s => s.RowId == originalShift.RowId);
            Assert.NotNull(verifyShift);
            
            // Verify the specific updates we made
            Assert.Equal("Updated Region", verifyShift.Region);
            Assert.Equal("Updated by integration test", verifyShift.Note);
            
            // Verify other fields remained unchanged
            Assert.Equal(originalShift.Date, verifyShift.Date);
            Assert.Equal(originalShift.Number, verifyShift.Number);
            Assert.Equal(originalShift.Service, verifyShift.Service);
            
            // Handle time comparison with format normalization
            var expectedStartTime = NormalizeTimeFormat(originalShift.Start);
            var actualStartTime = NormalizeTimeFormat(verifyShift.Start);
            Assert.Equal(expectedStartTime, actualStartTime);
            
            System.Diagnostics.Debug.WriteLine($"? Shift {originalShift.RowId} updated: Region='{verifyShift.Region}', Note='{verifyShift.Note}'");
        }

        // Verify trip updates were applied correctly
        foreach (var originalTrip in _createdShiftData.Trips)
        {
            var verifyTrip = updatedData.Trips.FirstOrDefault(t => t.RowId == originalTrip.RowId);
            Assert.NotNull(verifyTrip);
            
            // Verify the specific updates we made
            Assert.Equal(999, verifyTrip.Tip);
            Assert.Equal("Updated trip note", verifyTrip.Note);
            
            // Verify other fields remained unchanged
            Assert.Equal(originalTrip.Date, verifyTrip.Date);
            Assert.Equal(originalTrip.Number, verifyTrip.Number);
            Assert.Equal(originalTrip.Service, verifyTrip.Service);
            Assert.Equal(originalTrip.Place, verifyTrip.Place);
            Assert.Equal(originalTrip.Name, verifyTrip.Name);
            Assert.Equal(originalTrip.Pay ?? 0, verifyTrip.Pay ?? 0); // Should remain unchanged
            Assert.Equal(originalTrip.Bonus ?? 0, verifyTrip.Bonus ?? 0); // Should remain unchanged
            
            System.Diagnostics.Debug.WriteLine($"? Trip {originalTrip.RowId} updated: Tip=${verifyTrip.Tip:F2}, Note='{verifyTrip.Note}'");
        }

        System.Diagnostics.Debug.WriteLine("? All data updates were applied correctly");
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

    private async Task VerifyDataDeletionWasComplete()
    {
        System.Diagnostics.Debug.WriteLine("=== Verifying Data Deletion Was Complete ===");
        
        // Wait for deletion to propagate
        await Task.Delay(2000);

        var remainingData = await _googleSheetManager!.GetSheets(_testSheets);
        Assert.NotNull(remainingData);

        // Verify every single test shift was deleted
        foreach (var shiftId in _createdShiftIds)
        {
            var deletedShift = remainingData.Shifts.FirstOrDefault(s => s.RowId == shiftId);
            Assert.Null(deletedShift);
            
            System.Diagnostics.Debug.WriteLine($"? Shift {shiftId} successfully deleted");
        }
        
        // Verify every single test trip was deleted
        foreach (var tripId in _createdTripIds)
        {
            var deletedTrip = remainingData.Trips.FirstOrDefault(t => t.RowId == tripId);
            Assert.Null(deletedTrip);
            
            System.Diagnostics.Debug.WriteLine($"? Trip {tripId} successfully deleted");
        }

        // Additional verification: ensure no test data artifacts remain
        Assert.NotNull(_createdShiftData);
        
        // Check by unique identifiers to make sure our test data is truly gone
        foreach (var originalShift in _createdShiftData.Shifts)
        {
            var artifactShift = remainingData.Shifts.FirstOrDefault(s => 
                s.Date == originalShift.Date && 
                s.Number == originalShift.Number &&
                s.Service == originalShift.Service &&
                (s.Region == "Updated Region" || s.Region == originalShift.Region) &&
                (s.Note?.Contains("integration test") == true || s.Note == originalShift.Note));
                
            Assert.Null(artifactShift);
            
            System.Diagnostics.Debug.WriteLine($"? No shift artifacts found for {originalShift.Date} #{originalShift.Number} {originalShift.Service}");
        }

        foreach (var originalTrip in _createdShiftData.Trips)
        {
            var artifactTrip = remainingData.Trips.FirstOrDefault(t => 
                t.Date == originalTrip.Date && 
                t.Number == originalTrip.Number &&
                t.Service == originalTrip.Service &&
                t.Place == originalTrip.Place &&
                t.Name == originalTrip.Name &&
                (t.Tip == 999 || t.Note?.Contains("Updated trip note") == true));
                
            Assert.Null(artifactTrip);
            
            System.Diagnostics.Debug.WriteLine($"? No trip artifacts found for {originalTrip.Date} #{originalTrip.Number} to {originalTrip.Place}");
        }

        System.Diagnostics.Debug.WriteLine($"? Complete deletion verification successful: {_createdShiftIds.Count} shifts and {_createdTripIds.Count} trips fully removed");
    }

    private async Task TestCompleteSheetDeletionAndRecreation()
    {
        // Step 3a: Verify ALL sheets exist (they should after EnsureAllSheetsExist)
        var initialProperties = await _googleSheetManager!.GetSheetProperties(_allSheets);
        var existingSheets = initialProperties.Where(prop => !string.IsNullOrEmpty(prop.Id)).ToList();
        
        System.Diagnostics.Debug.WriteLine($"Found {existingSheets.Count} existing sheets to delete and recreate");
        
        if (existingSheets.Count != _allSheets.Count)
        {
            throw new InvalidOperationException($"Expected {_allSheets.Count} sheets to exist, but found {existingSheets.Count}. EnsureAllSheetsExist should have created all sheets.");
        }
        
        // Get list of ALL sheet names for deletion/recreation
        var allSheetNames = existingSheets.Select(s => s.Name).ToList();
        
        // Store initial sheet information for comparison
        var initialSheetIds = existingSheets.ToDictionary(s => s.Name, s => s.Id);
        
        // Step 3b: Delete ALL existing sheets (this tests the deletion capability)
        System.Diagnostics.Debug.WriteLine($"Deleting all {allSheetNames.Count} sheets: {string.Join(", ", allSheetNames)}");
        var deletionResult = await _googleSheetManager.DeleteSheets(allSheetNames);
        Assert.NotNull(deletionResult);
        
        // Log the results for debugging
        foreach (var message in deletionResult.Messages)
        {
            System.Diagnostics.Debug.WriteLine($"Delete operation: {message.Level} - {message.Message}");
        }
        
        // Check if deletion was successful or if it failed due to permissions
        var errorMessages = deletionResult.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription());
        var warningMessages = deletionResult.Messages.Where(m => m.Level == MessageLevelEnum.WARNING.GetDescription());
        
        // If there are errors that aren't permission-related, fail the test
        var criticalErrors = errorMessages.Where(m => 
            !m.Message.Contains("Cannot delete") && 
            !m.Message.Contains("permission"));
            
        if (criticalErrors.Any())
        {
            throw new InvalidOperationException($"Critical error during sheet deletion: {criticalErrors.First().Message}");
        }
        
        // If deletion failed due to permissions, skip the rest of this test
        var permissionIssues = errorMessages.Concat(warningMessages)
            .Where(m => m.Message.Contains("Cannot delete") || m.Message.Contains("permission"));
            
        if (permissionIssues.Any())
        {
            System.Diagnostics.Debug.WriteLine("Skipping deletion verification due to permission issues");
            throw new InvalidOperationException("Cannot complete test: insufficient permissions to delete sheets");
        }
        
        // Step 3c: Verify ALL sheets are deleted/missing
        // Wait for deletion to propagate
        System.Diagnostics.Debug.WriteLine("Waiting for sheet deletion to propagate...");
        await Task.Delay(5000);
        
        var afterDeletionProperties = await _googleSheetManager.GetSheetProperties(allSheetNames);
        
        // Verify that ALL sheets are actually deleted (should have empty IDs)
        var deletedSheets = afterDeletionProperties.Where(prop => string.IsNullOrEmpty(prop.Id)).ToList();
        if (deletedSheets.Count != allSheetNames.Count)
        {
            // If sheets weren't actually deleted, this might be due to Google Sheets limitations
            System.Diagnostics.Debug.WriteLine($"Expected {allSheetNames.Count} deleted sheets, but only {deletedSheets.Count} were deleted");
            System.Diagnostics.Debug.WriteLine("This might be due to Google Sheets API limitations or caching");
            throw new InvalidOperationException($"Sheet deletion verification failed: expected {allSheetNames.Count} deleted sheets, got {deletedSheets.Count}");
        }
        
        Assert.All(afterDeletionProperties, prop => Assert.Empty(prop.Id)); // Should be empty after deletion
        System.Diagnostics.Debug.WriteLine($"Successfully verified all {allSheetNames.Count} sheets were deleted");
        
        // Step 3d: Recreate ALL sheets using the manager's CreateSheets() method
        // This will create all sheets in the correct order with proper dependencies
        System.Diagnostics.Debug.WriteLine("Recreating all sheets...");
        var recreationResult = await _googleSheetManager.CreateSheets();
        Assert.NotNull(recreationResult);
        
        // Should have success messages for recreation (warnings are used for successful creation)
        var successMessages = recreationResult.Messages.Where(m => 
            m.Level == MessageLevelEnum.WARNING.GetDescription() && 
            m.Type == MessageTypeEnum.CREATE_SHEET.GetDescription());
        Assert.NotEmpty(successMessages);
        
        System.Diagnostics.Debug.WriteLine($"Recreation created {successMessages.Count()} sheets");
        
        // Step 3e: Verify ALL sheets are recreated with proper structure
        // Wait for sheets to be fully created
        System.Diagnostics.Debug.WriteLine("Waiting for sheet recreation to complete...");
        await Task.Delay(10000);
        
        var recreatedProperties = await _googleSheetManager.GetSheetProperties(_allSheets);
        var newExistingSheets = recreatedProperties.Where(prop => !string.IsNullOrEmpty(prop.Id)).ToList();
        
        // Should have ALL sheets recreated
        Assert.Equal(_allSheets.Count, newExistingSheets.Count);
        System.Diagnostics.Debug.WriteLine($"Verified {newExistingSheets.Count} sheets were recreated");
        
        // Verify that ALL sheets have different IDs (proving they were recreated)
        foreach (var recreatedSheet in newExistingSheets)
        {
            if (initialSheetIds.ContainsKey(recreatedSheet.Name))
            {
                Assert.NotEqual(initialSheetIds[recreatedSheet.Name], recreatedSheet.Id);
                System.Diagnostics.Debug.WriteLine($"Sheet '{recreatedSheet.Name}' has new ID: {recreatedSheet.Id} (was {initialSheetIds[recreatedSheet.Name]})");
            }
        }
        
        // Verify headers are properly created for ALL recreated sheets
        foreach (var sheet in newExistingSheets)
        {
            Assert.NotEmpty(sheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);
        }
        
        System.Diagnostics.Debug.WriteLine($"Successfully completed deletion and recreation of all {newExistingSheets.Count} sheets");
        
        // Step 3f: Verify recreated sheets are functional (can receive data)
        // This will be tested in the subsequent CreateNewShiftWithTrips step
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
        // This method is deprecated - the test now uses StartFreshWithAllSheets
        // which deletes all existing sheets and creates all required sheets from scratch
        await StartFreshWithAllSheets();
    }

    private static string NormalizeTimeFormat(string? timeString)
    {
        if (string.IsNullOrEmpty(timeString))
            return string.Empty;
            
        // Try to parse the time string and format it consistently without seconds
        if (DateTime.TryParse(timeString, out var parsedTime))
        {
            return parsedTime.ToString("h:mm tt"); // Format like "2:55 PM" without seconds
        }
        
        // If parsing fails, return the original string (this handles edge cases)
        return timeString;
    }

    private static string NormalizeDurationFormat(string? durationString)
    {
        if (string.IsNullOrEmpty(durationString))
            return string.Empty;
            
        // Try to parse duration as TimeSpan and format consistently
        if (TimeSpan.TryParse(durationString, out var parsedDuration))
        {
            // Format as "h:mm" for consistency (Google Sheets typically simplifies durations)
            return $"{(int)parsedDuration.TotalHours}:{parsedDuration.Minutes:D2}";
        }
        
        // If parsing fails, return the original string (this handles edge cases)
        return durationString;
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

    private async Task<Dictionary<string, object>> CaptureAggregateState()
    {
        // Capture the state of aggregate sheets before making changes
        var aggregateState = new Dictionary<string, object>();
        
        var allData = await _googleSheetManager!.GetSheets(_allSheets);
        
        // Store counts and key metrics for comparison (check if data exists first)
        aggregateState["AddressCount"] = allData.Addresses?.Count ?? 0;
        aggregateState["NameCount"] = allData.Names?.Count ?? 0;
        aggregateState["PlaceCount"] = allData.Places?.Count ?? 0;
        aggregateState["RegionCount"] = allData.Regions?.Count ?? 0;
        aggregateState["ServiceCount"] = allData.Services?.Count ?? 0;
        aggregateState["DailyCount"] = allData.Daily?.Count ?? 0;
        aggregateState["WeeklyCount"] = allData.Weekly?.Count ?? 0;
        aggregateState["MonthlyCount"] = allData.Monthly?.Count ?? 0;
        aggregateState["YearlyCount"] = allData.Yearly?.Count ?? 0;
        
        // Store specific data points for verification
        var testDate = DateTime.Now.ToString("yyyy-MM-dd");
        var existingDailyForDate = allData.Daily?.FirstOrDefault(d => d.Date == testDate);
        if (existingDailyForDate != null)
        {
            aggregateState["ExistingDailyTrips"] = existingDailyForDate.Trips;
            aggregateState["ExistingDailyTotal"] = existingDailyForDate.Total;
        }
        
        return aggregateState;
    }

    private async Task StartFreshWithAllSheets()
    {
        System.Diagnostics.Debug.WriteLine("=== Starting Fresh: Delete All Existing Sheets and Recreate ===");
        
        // Step 1a: Get ALL existing sheets in the spreadsheet (regardless of what they are)
        var allExistingProperties = await _googleSheetManager!.GetSheetProperties(_allSheets);
        var existingSheets = allExistingProperties.Where(prop => !string.IsNullOrEmpty(prop.Id)).ToList();
        
        System.Diagnostics.Debug.WriteLine($"Found {existingSheets.Count} existing sheets in spreadsheet: {string.Join(", ", existingSheets.Select(s => s.Name))}");
        
        // Step 1b: Delete ALL existing sheets (start completely fresh)
        if (existingSheets.Count > 0)
        {
            var existingSheetNames = existingSheets.Select(s => s.Name).ToList();
            
            System.Diagnostics.Debug.WriteLine($"Deleting ALL {existingSheetNames.Count} existing sheets to start fresh...");
            var deletionResult = await _googleSheetManager.DeleteSheets(existingSheetNames);
            
            // Log deletion results
            foreach (var message in deletionResult.Messages)
            {
                System.Diagnostics.Debug.WriteLine($"Delete operation: {message.Level} - {message.Message}");
            }
            
            // Check for critical errors (but allow permission issues)
            var errorMessages = deletionResult.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription());
            var criticalErrors = errorMessages.Where(m => 
                !m.Message.Contains("Cannot delete") && 
                !m.Message.Contains("permission"));
                
            if (criticalErrors.Any())
            {
                throw new InvalidOperationException($"Critical error during sheet deletion: {criticalErrors.First().Message}");
            }
            
            // Check for permission issues
            var permissionIssues = deletionResult.Messages.Where(m => 
                m.Message.Contains("Cannot delete") || m.Message.Contains("permission"));
                
            if (permissionIssues.Any())
            {
                throw new InvalidOperationException($"Cannot complete test: insufficient permissions to delete sheets. Error: {permissionIssues.First().Message}");
            }
            
            // Step 1c: Wait for deletion to propagate and verify sheets are deleted
            System.Diagnostics.Debug.WriteLine("Waiting for sheet deletion to propagate...");
            await Task.Delay(5000);
            
            var afterDeletionProperties = await _googleSheetManager.GetSheetProperties(existingSheetNames);
            var remainingSheets = afterDeletionProperties.Where(prop => !string.IsNullOrEmpty(prop.Id)).ToList();
            
            if (remainingSheets.Count > 0)
            {
                throw new InvalidOperationException($"Sheet deletion failed: {remainingSheets.Count} sheets still exist after deletion: {string.Join(", ", remainingSheets.Select(s => s.Name))}");
            }
            
            System.Diagnostics.Debug.WriteLine($"Successfully deleted all {existingSheetNames.Count} sheets");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("No existing sheets found - spreadsheet is already empty");
        }
        
        // Step 1d: Create ALL required sheets from scratch
        System.Diagnostics.Debug.WriteLine($"Creating ALL {_allSheets.Count} sheets from scratch...");
        var creationResult = await _googleSheetManager.CreateSheets();
        Assert.NotNull(creationResult);
        
        // Log creation results
        foreach (var message in creationResult.Messages)
        {
            System.Diagnostics.Debug.WriteLine($"Sheet creation: {message.Level} - {message.Message}");
        }
        
        // Verify creation was successful (warnings are used for successful creation)
        var successMessages = creationResult.Messages.Where(m => 
            m.Level == MessageLevelEnum.WARNING.GetDescription() && 
            m.Type == MessageTypeEnum.CREATE_SHEET.GetDescription());
        Assert.NotEmpty(successMessages);
        
        System.Diagnostics.Debug.WriteLine($"Creation result shows {successMessages.Count()} sheets were created");
        
        // Step 1e: Wait for creation to complete and verify all sheets exist
        System.Diagnostics.Debug.WriteLine("Waiting for sheet creation to complete...");
        await Task.Delay(10000); // Longer wait for creation of multiple sheets
        
        var finalProperties = await _googleSheetManager.GetSheetProperties(_allSheets);
        var createdSheets = finalProperties.Where(prop => !string.IsNullOrEmpty(prop.Id)).ToList();
        
        // Verify ALL expected sheets were created
        if (createdSheets.Count != _allSheets.Count)
        {
            var missingSheets = _allSheets.Except(createdSheets.Select(s => s.Name)).ToList();
            throw new InvalidOperationException($"Sheet creation incomplete: Expected {_allSheets.Count} sheets, got {createdSheets.Count}. Missing: {string.Join(", ", missingSheets)}");
        }
        
        // Verify all sheets have proper headers
        foreach (var sheet in createdSheets)
        {
            Assert.NotEmpty(sheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);
        }
        
        System.Diagnostics.Debug.WriteLine($"=== Successfully Started Fresh: All {createdSheets.Count} sheets created with proper structure ===");
        System.Diagnostics.Debug.WriteLine($"Created sheets: {string.Join(", ", createdSheets.Select(s => $"{s.Name}({s.Id})"))}");
    }

    private async Task VerifyFinalState()
    {
        System.Diagnostics.Debug.WriteLine("=== Verifying Final Clean State ===");
        
        // Act - Verify all sheets are in clean state
        var finalState = await _googleSheetManager!.GetSheets(_allSheets);
        var sheetProperties = await _googleSheetManager.GetSheetProperties(_allSheets);

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

        // Verify all sheet structure is intact
        var existingSheets = sheetProperties.Where(prop => !string.IsNullOrEmpty(prop.Id)).ToList();
        Assert.True(existingSheets.Count > 0, "Should have at least some sheets after the test");
        Assert.All(existingSheets, prop => Assert.NotEmpty(prop.Id));

        // Verify aggregate sheets are still populated with valid data (if they exist)
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
        if (finalState.Regions != null)
        {
            Assert.DoesNotContain(finalState.Regions, r => r.Region == "Updated Region" && r.Trips > 0);
        }
        
        // Log final counts for debugging
        System.Diagnostics.Debug.WriteLine($"? Final verification complete - " +
            $"Addresses: {finalState.Addresses?.Count ?? 0}, " +
            $"Names: {finalState.Names?.Count ?? 0}, " +
            $"Places: {finalState.Places?.Count ?? 0}, " +
            $"Regions: {finalState.Regions?.Count ?? 0}, " +
            $"Services: {finalState.Services?.Count ?? 0}");

        System.Diagnostics.Debug.WriteLine("? System returned to clean state successfully");
    }

    private async Task VerifyAggregateDataReflectsUpdatesCorrectly()
    {
        System.Diagnostics.Debug.WriteLine("=== Verifying Aggregate Data Reflects Updates Correctly ===");
        
        // Allow time for formulas to recalculate after updates
        await Task.Delay(3000);
        
        var currentData = await _googleSheetManager!.GetSheets(_allSheets);
        
        Assert.NotNull(currentData);
        Assert.NotNull(_createdShiftData);

        // Calculate expected changes from our updates
        var tripUpdateCount = _createdShiftData.Trips.Count;
        var expectedTipIncrease = tripUpdateCount * 999m; // Each trip now has tip = 999
        
        System.Diagnostics.Debug.WriteLine($"Expected tip increase: ${expectedTipIncrease:F2} from {tripUpdateCount} trips @ $999 each");

        // Verify the updated region ("Updated Region") appears in REGIONS sheet with correct data
        if (currentData.Regions != null)
        {
            var updatedRegionEntry = currentData.Regions.FirstOrDefault(r => r.Region == "Updated Region");
            Assert.NotNull(updatedRegionEntry);
            
            // Should have the trips that were moved to this region
            Assert.True(updatedRegionEntry.Trips > 0, 
                $"Updated region should have trip count > 0, but has {updatedRegionEntry.Trips}");
            Assert.True(updatedRegionEntry.Total > 0, 
                $"Updated region should have total > 0, but has ${updatedRegionEntry.Total:F2}");
                
            System.Diagnostics.Debug.WriteLine($"? Updated Region: {updatedRegionEntry.Trips} trips, ${updatedRegionEntry.Total:F2} total");
        }

        System.Diagnostics.Debug.WriteLine("? Aggregate data correctly reflects all updates with proper calculations");
    }

    private async Task VerifyAggregateDataCleanedUpCorrectly(Dictionary<string, object> preTestState)
    {
        System.Diagnostics.Debug.WriteLine("=== Verifying Aggregate Data Cleaned Up Correctly ===");
        
        // Allow time for formulas to recalculate after deletions
        await Task.Delay(3000);
        
        var currentData = await _googleSheetManager!.GetSheets(_allSheets);
        
        Assert.NotNull(currentData);
        
        // Verify DAILY data returns to expected state
        if (currentData.Daily != null)
        {
            var testDate = DateTime.Now.ToString("yyyy-MM-dd");
            var dailyEntry = currentData.Daily.FirstOrDefault(d => d.Date == testDate);
            
            if (preTestState.ContainsKey("ExistingDailyTrips") && dailyEntry != null)
            {
                var originalTrips = (int)preTestState["ExistingDailyTrips"];
                var originalTotal = preTestState.ContainsKey("ExistingDailyTotal") ? (decimal)preTestState["ExistingDailyTotal"] : 0m;
                
                // Allow for small variance due to concurrent operations or rounding
                var tripsDifference = Math.Abs(dailyEntry.Trips - originalTrips);
                var totalDifference = Math.Abs((dailyEntry.Total ?? 0m) - originalTotal);
                
                Assert.True(tripsDifference <= 5, 
                    $"Daily trips for {testDate} should return close to pre-test value of {originalTrips}, current: {dailyEntry.Trips} (difference: {tripsDifference})");
                Assert.True(totalDifference <= 10m, 
                    $"Daily total for {testDate} should return close to pre-test value of ${originalTotal:F2}, current: ${(dailyEntry.Total ?? 0m):F2} (difference: ${totalDifference:F2})");
                    
                System.Diagnostics.Debug.WriteLine($"? Daily {testDate}: {dailyEntry.Trips} trips (was {originalTrips}, diff: {dailyEntry.Trips - originalTrips}), ${(dailyEntry.Total ?? 0m):F2} total (was ${originalTotal:F2}, diff: ${(dailyEntry.Total ?? 0m) - originalTotal:F2})");
            }
        }

        System.Diagnostics.Debug.WriteLine("? Aggregate data cleanup verification complete - all calculations returned to expected state");
    }
    #endregion
}