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
/// </summary>
public class GoogleSheetIntegrationWorkflow : IAsyncLifetime
{
    private readonly GoogleSheetManager? _googleSheetManager;
    private readonly Dictionary<string, string> _credential;
    private readonly List<string> _testSheets;
    private readonly long _testStartTime;

    // Test data tracking
    private SheetEntity? _createdShiftData;
    private List<int> _createdShiftIds = new();
    private List<int> _createdTripIds = new();

    public GoogleSheetIntegrationWorkflow()
    {
        _testStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _testSheets = new List<string> { SheetEnum.TRIPS.GetDescription(), SheetEnum.SHIFTS.GetDescription() };
        
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
        // Step 1: Verify Sheet Structure
        await VerifySheetStructure();
        
        // Step 2: Create New Data
        await CreateNewShiftWithTrips();
        
        // Step 3: Verify Data Was Added
        await VerifyDataWasAdded();
        
        // Step 4: Update Existing Data
        await UpdateExistingData();
        
        // Step 5: Delete Test Data
        await DeleteTestData();
        
        // Step 6: Verify Final Clean State
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
        // Arrange & Act
        var sheetProperties = await _googleSheetManager!.GetSheetProperties(_testSheets);
        var allSheets = await _googleSheetManager.GetSheets();

        // Assert - Verify all required sheets exist
        Assert.NotNull(sheetProperties);
        Assert.Equal(2, sheetProperties.Count);
        
        var tripsSheet = sheetProperties.FirstOrDefault(x => x.Name == SheetEnum.TRIPS.GetDescription());
        var shiftsSheet = sheetProperties.FirstOrDefault(x => x.Name == SheetEnum.SHIFTS.GetDescription());
        
        Assert.NotNull(tripsSheet);
        Assert.NotNull(shiftsSheet);
        Assert.NotEmpty(tripsSheet.Id);
        Assert.NotEmpty(shiftsSheet.Id);
        
        // Verify headers are present
        Assert.NotEmpty(tripsSheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);
        Assert.NotEmpty(shiftsSheet.Attributes[PropertyEnum.HEADERS.GetDescription()]);

        // Verify no errors in sheet retrieval
        Assert.NotNull(allSheets);
        var errorMessages = allSheets.Messages.Where(m => m.Level == MessageLevelEnum.ERROR.GetDescription());
        Assert.Empty(errorMessages);
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

    private async Task VerifyFinalState()
    {
        // Act
        var finalState = await _googleSheetManager!.GetSheets(_testSheets);
        var sheetProperties = await _googleSheetManager.GetSheetProperties(_testSheets);

        // Assert
        Assert.NotNull(finalState);
        Assert.NotNull(sheetProperties);

        // Verify no test data remains
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

        // Verify sheet structure is intact
        Assert.Equal(2, sheetProperties.Count);
        Assert.All(sheetProperties, prop => Assert.NotEmpty(prop.Id));
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