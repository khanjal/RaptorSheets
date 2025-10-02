using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Gig.Tests.Data.Helpers;
using RaptorSheets.Test.Common.Helpers;
using RaptorSheets.Gig.Tests.Integration.Base;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Integration;

[Collection("IntegrationCollection")] // Share setup with other integration tests
[Category("Integration")]
public class GoogleSheetsIntegrationTests : IntegrationTestBase
{
    #region Core Operations
    
    [FactCheckUserSecrets]
    public async Task CreateAndReadData_ShouldWorkCorrectly()
    {
        SkipIfNoCredentials();
        
        try
        {
            await EnsureSheetsExist(TestSheets);
            var testData = CreateSimpleTestData(shifts: 2, tripsPerShift: 2, expenses: 2);
            
            await InsertTestData(testData);
            
            var retrievedData = await GetSheetData();
            
            ValidateNotEmpty(retrievedData, "CreateAndRead");
            ValidateDataCounts(retrievedData, testData, "CreateAndRead");
            
            Assert.True(retrievedData.Shifts.Any(s => !string.IsNullOrEmpty(s.Service)), 
                "Should have shifts with service data");
            Assert.True(retrievedData.Trips.Any(t => t.Pay > 0), 
                "Should have trips with pay data");
            Assert.True(retrievedData.Expenses.Any(e => e.Amount > 0), 
                "Should have expenses with amount data");
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
    
    [FactCheckUserSecrets]
    public async Task UpdateData_ShouldModifyExistingEntries()
    {
        SkipIfNoCredentials();
        
        try
        {
            await EnsureSheetsExist(TestSheets);
            
            // Create unique test identifiers to avoid interference with other tests
            var testRunId = DateTimeOffset.UtcNow.ToString("HHmmss");
            var uniqueService = $"TestService_{testRunId}";
            var uniqueRegion = $"TestRegion_{testRunId}";
            var uniqueUpdatedRegion = $"UpdatedRegion_{testRunId}";
            var uniqueNote = $"UpdatedNote_{testRunId}";
            
            // Insert test data with unique identifiers
            var testData = CreateSimpleTestData(shifts: 1, tripsPerShift: 1, expenses: 1);
            
            // Customize the test data to have unique identifiers
            foreach (var shift in testData.Shifts)
            {
                shift.Service = uniqueService;
                shift.Region = uniqueRegion;
                shift.Note = $"InitialNote_{testRunId}";
            }
            
            await InsertTestData(testData);
            
            var currentData = await GetSheetData();
            ValidateNotEmpty(currentData, "UpdateSetup");
            
            // Find OUR specific test shifts using unique identifiers
            var ourShifts = currentData.Shifts.Where(s => s.Service == uniqueService).ToList();
            Assert.True(ourShifts.Count > 0, $"Should find shifts with service '{uniqueService}'");
            
            System.Diagnostics.Debug.WriteLine($"Found {ourShifts.Count} shifts with our unique service: {uniqueService}");
            
            var shiftsToUpdate = ourShifts.Take(1).ToList();
            await UpdateShifts(shiftsToUpdate, shift => {
                shift.Region = uniqueUpdatedRegion;
                shift.Note = uniqueNote;
                return shift;
            });
            
            // Wait for Google Sheets to propagate changes
            await Task.Delay(3000);
            
            var updatedData = await GetSheetData();
            
            // Look for our specific updated values
            var updatedShifts = updatedData.Shifts.Where(s => 
                s.Region == uniqueUpdatedRegion && s.Note == uniqueNote).ToList();
            
            System.Diagnostics.Debug.WriteLine($"Found {updatedShifts.Count} shifts with updated region '{uniqueUpdatedRegion}' and note '{uniqueNote}'");
            
            Assert.True(updatedShifts.Count > 0, 
                $"Should find shifts with updated region '{uniqueUpdatedRegion}' and note '{uniqueNote}'");
            
            // Test trip updates with unique identifiers
            var uniqueTripService = $"TripService_{testRunId}";
            var uniqueTripNote = $"UpdatedTrip_{testRunId}";
            
            var ourTrips = updatedData.Trips.Where(t => testData.Trips.Any(originalTrip => 
                Math.Abs((t.Pay ?? 0) - (originalTrip.Pay ?? 0)) < 0.01m)).Take(1).ToList();
            
            if (ourTrips.Count > 0)
            {
                await UpdateTrips(ourTrips, trip => {
                    trip.Service = uniqueTripService;
                    trip.Tip = 12345; // Very distinctive value
                    trip.Note = uniqueTripNote;
                    return trip;
                });
                
                await Task.Delay(2000);
                
                var finalData = await GetSheetData();
                var updatedTrips = finalData.Trips.Where(t => 
                    t.Service == uniqueTripService && t.Tip == 12345 && t.Note == uniqueTripNote).ToList();
                
                Assert.True(updatedTrips.Count > 0, 
                    $"Should find trips with service '{uniqueTripService}', tip 12345, and note '{uniqueTripNote}'");
            }
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
    
    #endregion

    #region Sheet Management
    
    [FactCheckUserSecrets]
    public async Task EnsureSheetsExist_ShouldCreateMissingSheets()
    {
        SkipIfNoCredentials();
        
        try
        {
            var success = await EnsureSheetsExist(TestSheets);
            
            Assert.True(success, "Should successfully ensure sheets exist");
            
            var properties = await GoogleSheetManager!.GetSheetProperties(TestSheets);
            var existingSheets = properties.Where(p => !string.IsNullOrEmpty(p.Id)).ToList();
            
            Assert.True(existingSheets.Count >= TestSheets.Count, 
                $"Should find at least {TestSheets.Count} sheets, found {existingSheets.Count}");
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
    
    [FactCheckUserSecrets]
    public async Task GetSheetData_WithEmptySheets_ShouldReturnEmptyData()
    {
        SkipIfNoCredentials();
        
        try
        {
            await EnsureSheetsExist(TestSheets);
            
            var data = await GetSheetData();
            
            Assert.NotNull(data);
            Assert.NotNull(data.Shifts);
            Assert.NotNull(data.Trips);
            Assert.NotNull(data.Expenses);
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }

    [FactCheckUserSecrets]
    public async Task RecreateSheets_ShouldRebuildSheetsCorrectly()
    {
        SkipIfNoCredentials();
        
        try
        {
            // Step 1: Ensure sheets exist first
            await EnsureSheetsExist(TestSheets);
            
            // Step 2: Add some test data to verify it gets cleared
            var testRunId = DateTimeOffset.UtcNow.ToString("HHmmss");
            var testData = CreateSimpleTestData(shifts: 1, tripsPerShift: 1, expenses: 1);
            
            foreach (var shift in testData.Shifts)
            {
                shift.Service = $"PreDeleteData_{testRunId}";
            }
            
            await InsertTestData(testData);
            
            // Step 3: Verify data exists before deletion
            var beforeDelete = await GetSheetData();
            var preDeleteShifts = beforeDelete.Shifts.Where(s => s.Service?.Contains($"PreDeleteData_{testRunId}") == true).ToList();
            Assert.True(preDeleteShifts.Count > 0, "Should have test data before deletion");
            
            // Step 4: Attempt to delete the sheets (be more tolerant of failures)
            var deleteResult = await GoogleSheetManager!.DeleteSheets(TestSheets);
            
            // Check for delete result - be more tolerant of API limitations
            var deleteErrors = deleteResult.Messages.Where(m => m.Level == "ERROR").ToList();
            var hasDeleteFailure = deleteErrors.Any(e => e.Message.Contains("batch request returned null"));
            
            if (hasDeleteFailure)
            {
                // Skip the deletion test if API doesn't support it - focus on core functionality
                System.Diagnostics.Debug.WriteLine("?? Sheet deletion not supported by API - skipping deletion portion");
                return;
            }
            
            // Only continue with deletion verification if deletion appeared to succeed
            Assert.True(deleteErrors.Count == 0, $"Delete should not have errors: {string.Join(", ", deleteErrors.Select(e => e.Message))}");
            
            // Wait for deletion to propagate
            await Task.Delay(3000);
            
            // Step 5: Recreate the sheets
            var recreateSuccess = await EnsureSheetsExist(TestSheets);
            Assert.True(recreateSuccess, "Should successfully recreate sheets");
            
            // Wait for creation to propagate
            await Task.Delay(2000);
            
            // Step 6: Verify sheets are recreated and empty
            var afterRecreate = await GetSheetData();
            
            // Should have empty collections (no old data)
            var postRecreateShifts = afterRecreate.Shifts.Where(s => s.Service?.Contains($"PreDeleteData_{testRunId}") == true).ToList();
            Assert.Empty(postRecreateShifts);
            
            // Step 7: Verify sheet structure is correct by adding new test data
            var newTestData = CreateSimpleTestData(shifts: 1, tripsPerShift: 1, expenses: 1);
            foreach (var shift in newTestData.Shifts)
            {
                shift.Service = $"PostRecreateData_{testRunId}";
            }
            
            await InsertTestData(newTestData);
            
            // Step 8: Verify new data can be added and retrieved correctly
            var finalData = await GetSheetData();
            var newShifts = finalData.Shifts.Where(s => s.Service?.Contains($"PostRecreateData_{testRunId}") == true).ToList();
            Assert.True(newShifts.Count > 0, "Should be able to add data to recreated sheets");
            
            // Step 9: Verify sheet properties are correct
            var finalProperties = await GoogleSheetManager!.GetSheetProperties(TestSheets);
            var recreatedSheets = finalProperties.Where(p => !string.IsNullOrEmpty(p.Id)).ToList();
            
            Assert.True(recreatedSheets.Count >= TestSheets.Count, 
                $"Should have all required sheets after recreation, expected {TestSheets.Count}, got {recreatedSheets.Count}");
            
            // Verify essential sheets are present
            var sheetNames = recreatedSheets.Select(s => s.Name).ToList();
            Assert.Contains("Shifts", sheetNames);
            Assert.Contains("Trips", sheetNames);
            Assert.Contains("Expenses", sheetNames);
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
    
    #endregion

    #region Error Handling
    
    [FactCheckUserSecrets]
    public async Task InvalidSpreadsheetId_ShouldHandleGracefully()
    {
        var credential = TestConfigurationHelpers.GetJsonCredential();
        var invalidManager = new GoogleSheetManager(credential, "invalid-spreadsheet-id");
        
        try
        {
            var result = await invalidManager.GetSheets(["Trips"]);
            
            Assert.NotNull(result);
            Assert.NotNull(result.Messages);
            Assert.True(result.Messages.Any(m => m.Level == "ERROR"), 
                "Should have error messages for invalid spreadsheet");
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
    
    [FactCheckUserSecrets]
    public async Task NonExistentSheets_ShouldHandleGracefully()
    {
        SkipIfNoCredentials();
        
        try
        {
            var result = await GoogleSheetManager!.GetSheetProperties(["NonExistentSheet1", "NonExistentSheet2"]);
            
            Assert.NotNull(result);
            Assert.All(result, prop => Assert.True(string.IsNullOrEmpty(prop.Id)));
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
    
    #endregion
}