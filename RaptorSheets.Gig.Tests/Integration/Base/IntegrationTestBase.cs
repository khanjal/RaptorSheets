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
using System.ComponentModel;
using Xunit;

namespace RaptorSheets.Gig.Tests.Integration.Base;

/// <summary>
/// Base class for integration tests with modular, reusable operations
/// </summary>
public abstract class IntegrationTestBase
{
    protected readonly GoogleSheetManager? GoogleSheetManager;
    protected readonly List<string> TestSheets;
    
    protected IntegrationTestBase()
    {
        TestSheets = [
            SheetsConfig.SheetNames.Shifts,
            SheetsConfig.SheetNames.Trips,
            SheetsConfig.SheetNames.Expenses
        ];

        var spreadsheetId = TestConfigurationHelpers.GetGigSpreadsheet();
        var credential = TestConfigurationHelpers.GetJsonCredential();

        if (GoogleCredentialHelpers.IsCredentialFilled(credential))
            GoogleSheetManager = new GoogleSheetManager(credential, spreadsheetId);
    }

    #region Skip Helpers
    
    protected void SkipIfNoCredentials()
    {
        if (GoogleSheetManager == null)
        {
            Assert.Fail("Google Sheets credentials not available. Configure user secrets to run integration tests.");
        }
    }
    
    #endregion

    #region Test Data Generation
    
    protected static SheetEntity CreateSimpleTestData(int shifts = 2, int tripsPerShift = 2, int expenses = 3)
    {
        const int startingRowId = 2;
        
        var testData = TestGigHelpers.GenerateMultipleShifts(
            ActionTypeEnum.INSERT,
            startingRowId,
            startingRowId,
            shifts,
            tripsPerShift,
            tripsPerShift
        );

        var expenseData = GenerateSimpleExpenses(startingRowId, expenses);
        testData.Expenses.AddRange(expenseData.Expenses);

        return testData;
    }
    
    private static SheetEntity GenerateSimpleExpenses(int startingId, int count)
    {
        var sheetEntity = new SheetEntity();
        var baseDate = DateTime.Today;
        
        for (int i = 0; i < count; i++)
        {
            var expense = new ExpenseEntity
            {
                RowId = startingId + i,
                Action = ActionTypeEnum.INSERT.GetDescription(),
                Date = baseDate.AddDays(-i),
                Amount = 25.50m + i * 10,
                Category = "Test",
                Name = $"Test Expense {i + 1}",
                Description = $"Integration test expense {i + 1}"
            };
            sheetEntity.Expenses.Add(expense);
        }
        
        return sheetEntity;
    }
    
    #endregion

    #region Operations
    
    protected async Task<bool> EnsureSheetsExist(List<string> sheets)
    {
        var properties = await GoogleSheetManager!.GetSheetProperties(sheets);
        var missingSheets = sheets.Where(sheet => 
            !properties.Any(prop => prop.Name.Equals(sheet, StringComparison.OrdinalIgnoreCase) && 
                                   !string.IsNullOrEmpty(prop.Id))
        ).ToList();

        if (missingSheets.Count == 0) return true;

        var result = await GoogleSheetManager.CreateSheets(missingSheets);
        var hasErrors = result.Messages.Any(m => m.Level == MessageLevelEnum.ERROR.GetDescription());
        
        if (!hasErrors)
        {
            await Task.Delay(2000);
        }
        
        return !hasErrors;
    }
    
    protected async Task<SheetEntity> InsertTestData(SheetEntity testData)
    {
        // Fixture ensures sheets exist before tests run, so no need to check here
        
        // Only pass sheets that actually have data to avoid "not supported" errors
        var sheetsWithData = new List<string>();
        
        if (testData.Shifts.Count > 0)
            sheetsWithData.Add(SheetsConfig.SheetNames.Shifts);
        if (testData.Trips.Count > 0)
            sheetsWithData.Add(SheetsConfig.SheetNames.Trips);
        if (testData.Expenses.Count > 0)
            sheetsWithData.Add(SheetsConfig.SheetNames.Expenses);
        
        if (sheetsWithData.Count == 0)
        {
            throw new InvalidOperationException("No data provided for insertion");
        }
        
        var result = await GoogleSheetManager!.ChangeSheetData(sheetsWithData, testData);
        
        // Enhanced error checking - differentiate between critical errors and warnings
        var criticalErrors = result.Messages.Where(m => 
            m.Level == MessageLevelEnum.ERROR.GetDescription() && 
            !IsExpectedError(m.Message)).ToList();
            
        if (criticalErrors.Count > 0)
        {
            var errorDetails = string.Join("; ", criticalErrors.Select(m => $"{m.Type}: {m.Message}"));
            System.Diagnostics.Debug.WriteLine($"Critical errors during insert: {errorDetails}");
            
            // Check if this might be a missing sheets issue
            if (criticalErrors.Any(e => e.Message.Contains("Unable to save data")))
            {
                throw new InvalidOperationException($"Insert failed due to sheet configuration issue: {errorDetails}");
            }
            
            throw new InvalidOperationException($"Insert failed: {errorDetails}");
        }
        
        // Log warnings for debugging but don't fail
        var warnings = result.Messages.Where(m => m.Level == MessageLevelEnum.WARNING.GetDescription()).ToList();
        if (warnings.Count > 0)
        {
            var warningDetails = string.Join("; ", warnings.Select(m => $"{m.Type}: {m.Message}"));
            System.Diagnostics.Debug.WriteLine($"Warnings during insert: {warningDetails}");
        }
        
        await Task.Delay(1000);
        return result;
    }
    
    protected async Task<SheetEntity> GetSheetData()
    {
        var result = await GoogleSheetManager!.GetSheets(TestSheets);
        return result;
    }
    
    protected async Task<SheetEntity> UpdateShifts(List<ShiftEntity> shifts, Func<ShiftEntity, ShiftEntity> updateAction)
    {
        var updateData = new SheetEntity();
        
        foreach (var shift in shifts)
        {
            var updated = updateAction(shift);
            updated.Action = ActionTypeEnum.UPDATE.GetDescription();
            updateData.Shifts.Add(updated);
        }
        
        // Only pass the Shifts sheet to avoid "not supported" errors for empty Trips/Expenses
        var shiftsSheetOnly = new List<string> { SheetsConfig.SheetNames.Shifts };
        var result = await GoogleSheetManager!.ChangeSheetData(shiftsSheetOnly, updateData);
        
        var criticalErrors = result.Messages.Where(m => 
            m.Level == MessageLevelEnum.ERROR.GetDescription() && 
            !IsExpectedError(m.Message)).ToList();
            
        if (criticalErrors.Count > 0)
        {
            var errorDetails = string.Join("; ", criticalErrors.Select(m => $"{m.Type}: {m.Message}"));
            throw new InvalidOperationException($"Update failed: {errorDetails}");
        }
        
        await Task.Delay(1000);
        return result;
    }
    
    protected async Task<SheetEntity> UpdateTrips(List<TripEntity> trips, Func<TripEntity, TripEntity> updateAction)
    {
        var updateData = new SheetEntity();
        
        foreach (var trip in trips)
        {
            var updated = updateAction(trip);
            updated.Action = ActionTypeEnum.UPDATE.GetDescription();
            updateData.Trips.Add(updated);
        }
        
        // Only pass the Trips sheet to avoid "not supported" errors for empty Shifts/Expenses
        var tripsSheetOnly = new List<string> { SheetsConfig.SheetNames.Trips };
        var result = await GoogleSheetManager!.ChangeSheetData(tripsSheetOnly, updateData);
        
        var criticalErrors = result.Messages.Where(m => 
            m.Level == MessageLevelEnum.ERROR.GetDescription() && 
            !IsExpectedError(m.Message)).ToList();
            
        if (criticalErrors.Count > 0)
        {
            var errorDetails = string.Join("; ", criticalErrors.Select(m => $"{m.Type}: {m.Message}"));
            throw new InvalidOperationException($"Update failed: {errorDetails}");
        }
        
        await Task.Delay(1000);
        return result;
    }
    
    protected async Task<SheetEntity> UpdateExpenses(List<ExpenseEntity> expenses, Func<ExpenseEntity, ExpenseEntity> updateAction)
    {
        var updateData = new SheetEntity();
        
        foreach (var expense in expenses)
        {
            var updated = updateAction(expense);
            updated.Action = ActionTypeEnum.UPDATE.GetDescription();
            updateData.Expenses.Add(updated);
        }
        
        // Only pass the Expenses sheet to avoid "not supported" errors for empty Shifts/Trips
        var expensesSheetOnly = new List<string> { SheetsConfig.SheetNames.Expenses };
        var result = await GoogleSheetManager!.ChangeSheetData(expensesSheetOnly, updateData);
        
        var criticalErrors = result.Messages.Where(m => 
            m.Level == MessageLevelEnum.ERROR.GetDescription() && 
            !IsExpectedError(m.Message)).ToList();
            
        if (criticalErrors.Count > 0)
        {
            var errorDetails = string.Join("; ", criticalErrors.Select(m => $"{m.Type}: {m.Message}"));
            throw new InvalidOperationException($"Update failed: {errorDetails}");
        }
        
        await Task.Delay(1000);
        return result;
    }
    
    #endregion

    #region Validation
    
    protected static void ValidateDataCounts(SheetEntity actual, SheetEntity expected, string operation)
    {
        Assert.True(actual.Shifts.Count >= expected.Shifts.Count - 1, 
            $"{operation}: Expected ~{expected.Shifts.Count} shifts, got {actual.Shifts.Count}");
        Assert.True(actual.Trips.Count >= expected.Trips.Count - 1, 
            $"{operation}: Expected ~{expected.Trips.Count} trips, got {actual.Trips.Count}");
        Assert.True(actual.Expenses.Count >= expected.Expenses.Count - 1, 
            $"{operation}: Expected ~{expected.Expenses.Count} expenses, got {actual.Expenses.Count}");
    }
    
    protected static void ValidateNotEmpty(SheetEntity data, string operation)
    {
        var totalEntities = data.Shifts.Count + data.Trips.Count + data.Expenses.Count;
        Assert.True(totalEntities > 0, $"{operation}: No data found in any sheets");
    }
    
    #endregion

    #region Utilities
    
    private static bool IsApiRelatedError(Exception ex) =>
        ex.Message.Contains("credentials", StringComparison.OrdinalIgnoreCase) || 
        ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase) || 
        ex.Message.Contains("Requested entity was not found", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("API", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("sheet configuration issue", StringComparison.OrdinalIgnoreCase);
    
    private static bool IsExpectedError(string message) =>
        message.Contains("not supported") ||  // Expected when sheet doesn't support certain operations
        message.Contains("already exists") ||  // Expected when trying to create existing sheets
        message.Contains("header issue") ||    // Expected when headers don't match exactly
        message.Contains("No data to change"); // Expected when trying to change empty data
    
    #endregion
}