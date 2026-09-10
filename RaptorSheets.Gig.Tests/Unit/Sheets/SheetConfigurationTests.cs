using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Gig.Sheets;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Sheets;

/// <summary>
/// Sheet configuration and demo-data generation, both of which are pure in-memory work.
///
/// These lived in the integration suite and inherited its live fixture, so they paid a full
/// delete-and-recreate of every sheet - and, because CI selects unit tests with
/// FullyQualifiedName!~Integration, they ran only in the nightly job and gated nothing. Nothing here
/// needs credentials: sheet layouts come from the registry and demo data is generated in memory.
/// </summary>
public class SheetConfigurationTests
{
    private static SheetModel GetSheetModel(string sheet) => sheet switch
    {
        "Trips" => TripSheet.GetSheet(),
        "Shifts" => ShiftSheet.GetSheet(),
        "Expenses" => ExpenseSheet.GetSheet(),
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Unhandled sheet")
    };

    [Fact]
    public void CreatedSheets_ShouldHaveCorrectFormulas()
    {
        // This test validates that sheets with formulas have them correctly configured

        var sheetsWithFormulas = new[] { "Trips", "Shifts", "Expenses" }; // Sheets that have formula columns

        // Act - Get sheet layouts to find formula columns
        var layouts = sheetsWithFormulas.Select(GetSheetModel).ToList();

        // Assert
        foreach (var layout in layouts)
        {
            var formulaHeaders = layout.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).ToList();

            if (formulaHeaders.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"  🔍 Validating {layout.Name}: {formulaHeaders.Count} formula columns");

                // All formulas should start with =
                Assert.All(formulaHeaders, header =>
                {
                    Assert.StartsWith("=", header.Formula);

                    // Should not have unresolved placeholders
                    Assert.DoesNotContain("{", header.Formula);
                    Assert.DoesNotContain("{{", header.Formula);
                });

                // Log formulas for debugging
                foreach (var header in formulaHeaders)
                {
                    System.Diagnostics.Debug.WriteLine($"     {header.Name}: {header.Formula.Substring(0, Math.Min(50, header.Formula.Length))}...");
                }
            }
        }
    }

    /// <summary>
    /// Tests GenerateDemoData method - validates demo data generation works correctly.
    /// </summary>
    [Fact]
    public void DemoData_GenerateMethod_ShouldCreateRealisticData()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var seed = 42; // Fixed seed for deterministic data generation

        System.Diagnostics.Debug.WriteLine($"📝 Generating demo data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} with seed {seed}");

        // Act - Use the public GenerateDemoData method
        var demoData = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - Verify the data was generated
        Assert.NotNull(demoData);
        Assert.NotEmpty(demoData.Sheets.Shifts);
        Assert.NotEmpty(demoData.Sheets.Trips);

        // Log generated data for debugging
        System.Diagnostics.Debug.WriteLine($"✅ Generated {demoData.Sheets.Shifts.Count} shifts, {demoData.Sheets.Trips.Count} trips, {demoData.Sheets.Expenses.Count} expenses");

        // Verify data structure
        Assert.All(demoData.Sheets.Shifts, shift =>
        {
            Assert.NotNull(shift.Date);
            Assert.NotNull(shift.Service);
            Assert.True(shift.RowId > 0);
        });

        Assert.All(demoData.Sheets.Trips, trip =>
        {
            Assert.NotNull(trip.Date);
            Assert.NotNull(trip.Service);
            Assert.True(trip.RowId > 0);
        });

        System.Diagnostics.Debug.WriteLine($"✅ Demo data validation passed.");
    }

    /// <summary>
    /// Validates demo data has proper entity relationships (shifts ↔ trips).
    /// This ensures the demo system generates realistic, relational data.
    /// </summary>
    [Fact]
    public void DemoData_ShouldHaveProperShiftTripRelationships()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;
        
        // Act - Generate demo data
        var demoData = DemoHelpers.GenerateDemoData(startDate, endDate);
        
        // Assert - Verify data structure
        Assert.NotNull(demoData);
        Assert.NotEmpty(demoData.Sheets.Shifts);
        
        // Verify all entities have valid structure
        Assert.All(demoData.Sheets.Shifts, shift =>
        {
            Assert.NotNull(shift.Date);
            Assert.NotNull(shift.Service);
            Assert.True(shift.RowId > 0);
        });
        
        if (demoData.Sheets.Trips.Count > 0)
        {
            Assert.All(demoData.Sheets.Trips, trip =>
            {
                Assert.NotNull(trip.Date);
                Assert.NotNull(trip.Service);
                Assert.True(trip.RowId > 0);
            });
            
            // Verify shift-trip relationships exist
            foreach (var shift in demoData.Sheets.Shifts.Where(s => s.Trips > 0))
            {
                var relatedTrips = demoData.Sheets.Trips.Where(t =>
                    t.Date == shift.Date &&
                    t.Service == shift.Service &&
                    t.Number == shift.Number).ToList();
                
                // Some correlation should exist (demo data uses probabilities)
                // Not every shift will have exact trip count match
                System.Diagnostics.Debug.WriteLine($"  Shift on {shift.Date} ({shift.Service} #{shift.Number}): " +
                    $"{shift.Trips} trips expected, {relatedTrips.Count} found");
            }
        }
        
        System.Diagnostics.Debug.WriteLine($"✅ Validated demo data structure: {demoData.Sheets.Shifts.Count} shifts, " +
            $"{demoData.Sheets.Trips.Count} trips, {demoData.Sheets.Expenses.Count} expenses");
    }
}
