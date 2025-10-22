using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Managers;

/// <summary>
/// Demo data generation for Google Sheets.
/// Generates realistic sample data for testing, demos, and initial setup.
/// Use GenerateDemoData() to create sample data, then ChangeSheetData() to insert it.
/// For convenience, the manager provides wrapper methods like SetupDemo() and PopulateDemoData()
/// to combine creation and insertion of demo data.
/// </summary>
public partial class GoogleSheetManager
{
    #region Demo Data Generation

    /// <summary>
    /// Generates demo data without inserting it into the spreadsheet.
    /// Allows inspection, modification, or testing before insertion.
    /// This is the core method - consuming applications can wrap this with convenience methods.
    /// </summary>
    /// <param name="startDate">Start date for demo data (defaults to 30 days ago)</param>
    /// <param name="endDate">End date for demo data (defaults to today)</param>
    /// <param name="seed">Optional seed for deterministic/reproducible demo data (useful for testing)</param>
    /// <returns>SheetEntity populated with realistic demo data (Shifts, Trips, Expenses)</returns>
    /// <example>
    /// <code>
    /// // Generate demo data
    /// var demoData = manager.GenerateDemoData();
    /// 
    /// // Generate deterministic demo data for testing
    /// var testData = manager.GenerateDemoData(seed: 42);
    /// 
    /// // Optionally modify it
    /// demoData.Shifts = demoData.Shifts.Take(10).ToList();
    /// 
    /// // Insert it
    /// var sheets = new List&lt;string&gt; { "Shifts", "Trips", "Expenses" };
    /// await manager.ChangeSheetData(sheets, demoData);
    /// 
    /// // Or wrap it in a convenience method:
    /// public async Task&lt;SheetEntity&gt; SetupDemo(DateTime? start = null, DateTime? end = null)
    /// {
    ///     await CreateAllSheets();
    ///     var demoData = GenerateDemoData(start, end);
    ///     return await ChangeSheetData(new[] { "Shifts", "Trips", "Expenses" }, demoData);
    /// }
    /// </code>
    /// </example>
    public SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;
        
        return DemoHelpers.GenerateDemoData(start, end, seed);
    }

    #endregion
}
