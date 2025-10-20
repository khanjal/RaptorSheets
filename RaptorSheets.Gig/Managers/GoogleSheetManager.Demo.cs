using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Managers;

/// <summary>
/// Demo data generation for Google Sheets.
/// Handles creation of realistic sample data without inserting it.
/// Use GenerateDemoData() to create sample data, then use ChangeSheetData() to insert it.
/// </summary>
public partial class GoogleSheetManager
{
    #region Demo Data Generation

    /// <summary>
    /// Generates demo data without inserting it into the spreadsheet.
    /// Allows inspection, modification, or testing before insertion.
    /// </summary>
    /// <param name="startDate">Start date for demo data (defaults to 30 days ago)</param>
    /// <param name="endDate">End date for demo data (defaults to today)</param>
    /// <returns>SheetEntity populated with realistic demo data (Shifts, Trips, Expenses)</returns>
    /// <example>
    /// <code>
    /// // Generate demo data
    /// var demoData = manager.GenerateDemoData();
    /// 
    /// // Optionally modify it
    /// demoData.Shifts = demoData.Shifts.Take(10).ToList();
    /// 
    /// // Insert it
    /// var sheets = new List&lt;string&gt; { "Shifts", "Trips", "Expenses" };
    /// await manager.ChangeSheetData(sheets, demoData);
    /// </code>
    /// </example>
    public SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;
        
        return DemoHelpers.GenerateDemoData(start, end);
    }

    #endregion
}
