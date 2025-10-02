using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Gig.Tests.Integration.Base;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Integration;

[Category("Integration")]
public class WorkflowIntegrationTests : IntegrationTestBase
{
    #region End-to-End Workflows
    
    [FactCheckUserSecrets]
    public async Task CompleteDataLifecycle_ShouldWorkCorrectly()
    {
        SkipIfNoCredentials();
        
        try
        {
            await EnsureSheetsExist(TestSheets);
            
            // Create unique identifiers for this test run
            var testRunId = DateTimeOffset.UtcNow.ToString("HHmmss");
            var uniqueService = $"LifecycleService_{testRunId}";
            var uniqueRegion = $"LifecycleRegion_{testRunId}";
            var uniqueUpdatedNote = $"Lifecycle_Updated_{testRunId}";
            
            var initialData = CreateSimpleTestData(shifts: 2, tripsPerShift: 2, expenses: 2);
            
            // Customize with unique identifiers
            foreach (var shift in initialData.Shifts)
            {
                shift.Service = uniqueService;
                shift.Region = uniqueRegion;
            }
            
            await InsertTestData(initialData);
            
            var afterInsert = await GetSheetData();
            ValidateNotEmpty(afterInsert, "After Insert");
            ValidateDataCounts(afterInsert, initialData, "After Insert");
            
            // Find our specific shifts
            var ourShifts = afterInsert.Shifts.Where(s => s.Service == uniqueService).Take(1).ToList();
            Assert.True(ourShifts.Count > 0, $"Should find shifts with service '{uniqueService}'");
            
            await UpdateShifts(ourShifts, shift => {
                shift.Pay = 99999; // Very distinctive value
                shift.Note = uniqueUpdatedNote;
                return shift;
            });
            
            await Task.Delay(2000);
            
            var afterUpdate = await GetSheetData();
            var updatedShifts = afterUpdate.Shifts.Where(s => s.Pay == 99999 && s.Note == uniqueUpdatedNote).ToList();
            Assert.True(updatedShifts.Count > 0, "Should find updated shifts in lifecycle test");
            
            Assert.True(afterUpdate.Trips.Count >= initialData.Trips.Count - 1, 
                "Trips should remain after shift updates");
            Assert.True(afterUpdate.Expenses.Count >= initialData.Expenses.Count - 1, 
                "Expenses should remain after shift updates");
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
    
    [FactCheckUserSecrets]
    public async Task MultipleUpdatesWorkflow_ShouldHandleSequentialChanges()
    {
        SkipIfNoCredentials();
        
        try
        {
            await EnsureSheetsExist(TestSheets);
            
            // Create unique identifiers for this test run
            var testRunId = DateTimeOffset.UtcNow.ToString("HHmmss");
            var uniqueService = $"MultiUpdateService_{testRunId}";
            var firstUpdateRegion = $"FirstUpdate_{testRunId}";
            var secondUpdateRegion = $"SecondUpdate_{testRunId}";
            var finalNote = $"MultipleUpdates_{testRunId}";
            
            var testData = CreateSimpleTestData(shifts: 1, tripsPerShift: 2, expenses: 1);
            
            // Customize with unique identifiers
            foreach (var shift in testData.Shifts)
            {
                shift.Service = uniqueService;
                shift.Region = $"InitialRegion_{testRunId}";
            }
            
            await InsertTestData(testData);
            
            var initialData = await GetSheetData();
            ValidateNotEmpty(initialData, "MultipleUpdates Setup");
            
            // Find our specific shifts
            var ourShifts = initialData.Shifts.Where(s => s.Service == uniqueService).Take(1).ToList();
            Assert.True(ourShifts.Count > 0, $"Should find shifts with service '{uniqueService}'");
            
            // First update
            await UpdateShifts(ourShifts, shift => {
                shift.Region = firstUpdateRegion;
                return shift;
            });
            
            await Task.Delay(2000);
            
            var afterFirstUpdate = await GetSheetData();
            var firstUpdateShifts = afterFirstUpdate.Shifts.Where(s => s.Region == firstUpdateRegion).ToList();
            Assert.True(firstUpdateShifts.Count > 0, "Should find first update");
            
            // Second update on same entity
            await UpdateShifts(firstUpdateShifts, shift => {
                shift.Region = secondUpdateRegion;
                shift.Note = finalNote;
                return shift;
            });
            
            await Task.Delay(2000);
            
            var afterSecondUpdate = await GetSheetData();
            var secondUpdateShifts = afterSecondUpdate.Shifts.Where(s => 
                s.Region == secondUpdateRegion && s.Note == finalNote).ToList();
            Assert.True(secondUpdateShifts.Count > 0, "Should find second update");
            
            // Verify first update values are gone
            var remainingFirstUpdate = afterSecondUpdate.Shifts.Where(s => s.Region == firstUpdateRegion).ToList();
            Assert.Empty(remainingFirstUpdate);
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
    
    #endregion

    #region Data Consistency
    
    [FactCheckUserSecrets]
    public async Task CrossEntityConsistency_ShouldMaintainRelationships()
    {
        SkipIfNoCredentials();
        
        try
        {
            await EnsureSheetsExist(TestSheets);
            var testData = CreateSimpleTestData(shifts: 1, tripsPerShift: 3, expenses: 1);
            await InsertTestData(testData);
            
            var data = await GetSheetData();
            ValidateNotEmpty(data, "Cross Entity Consistency");
            
            Assert.True(data.Shifts.Count > 0, "Should have shifts for consistency test");
            Assert.True(data.Trips.Count > 0, "Should have trips for consistency test");
            Assert.True(data.Expenses.Count > 0, "Should have expenses for consistency test");
            
            Assert.All(data.Shifts, shift => {
                Assert.False(string.IsNullOrEmpty(shift.Service), "Shifts should have service");
            });
            
            Assert.All(data.Trips, trip => {
                Assert.False(string.IsNullOrEmpty(trip.Service), "Trips should have service");
                Assert.True(trip.Pay >= 0, "Trips should have non-negative pay");
            });
            
            Assert.All(data.Expenses, expense => {
                Assert.True(expense.Amount > 0, "Expenses should have positive amount");
                Assert.False(string.IsNullOrEmpty(expense.Category), "Expenses should have category");
            });
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;  
        }
    }
    
    #endregion

    #region Performance
    
    [FactCheckUserSecrets]
    public async Task ModerateDataVolume_ShouldHandleEfficiently()
    {
        SkipIfNoCredentials();
        
        try
        {
            await EnsureSheetsExist(TestSheets);
            
            var testData = CreateSimpleTestData(shifts: 5, tripsPerShift: 3, expenses: 8);
            
            var startTime = DateTime.UtcNow;
            
            await InsertTestData(testData);
            var retrievedData = await GetSheetData();
            
            var totalTime = DateTime.UtcNow - startTime;
            
            ValidateNotEmpty(retrievedData, "Moderate Volume");
            ValidateDataCounts(retrievedData, testData, "Moderate Volume");
            
            Assert.True(totalTime.TotalMinutes < 2, 
                $"Moderate data volume should complete within 2 minutes, took {totalTime.TotalMinutes:F1} minutes");
            
            Assert.True(retrievedData.Shifts.Count >= 3, "Should have multiple shifts");
            Assert.True(retrievedData.Trips.Count >= 8, "Should have multiple trips");  
            Assert.True(retrievedData.Expenses.Count >= 5, "Should have multiple expenses");
        }
        catch (Exception ex)
        {
            SkipIfApiError(ex);
            throw;
        }
    }
    
    #endregion
}