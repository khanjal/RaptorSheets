using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Sheets;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Sheets;

/// <summary>
/// Notes are the only in-sheet explanation of what a column is for, so they have to survive the
/// entity -> SheetModel generation rather than just existing as constants.
///
/// Guards three things: that each note reaches the column it belongs to on the right sheet, that
/// every dropdown-backed note explains the column before its mechanics rather than the reverse,
/// and that no input column on Trips/Shifts/Expenses ships without a note at all. The rollup
/// sheets are deliberately excluded - their columns are computed aggregates that a note would only
/// clutter.
/// </summary>
[Category("Unit Tests")]
public class ColumnNoteTests
{
    public static TheoryData<string, string, string> NotedColumns => new()
    {
        { "Trips", SheetsConfig.HeaderNames.Region, ColumnNotes.RegionSource },
        { "Trips", SheetsConfig.HeaderNames.Tags, ColumnNotes.Tags },
        { "Trips", SheetsConfig.HeaderNames.Service, ColumnNotes.ServiceSource },
        { "Trips", SheetsConfig.HeaderNames.Name, ColumnNotes.TripNameSource },
        { "Trips", SheetsConfig.HeaderNames.AddressStart, ColumnNotes.AddressSource },
        { "Trips", SheetsConfig.HeaderNames.AddressEnd, ColumnNotes.AddressSource },
        { "Shifts", SheetsConfig.HeaderNames.Region, ColumnNotes.RegionSource },
        { "Shifts", SheetsConfig.HeaderNames.Tags, ColumnNotes.Tags },
        { "Shifts", SheetsConfig.HeaderNames.Service, ColumnNotes.ServiceSource },
        { "Trips", SheetsConfig.HeaderNames.Dropoff, ColumnNotes.Dropoff },
        { "Trips", SheetsConfig.HeaderNames.Cash, ColumnNotes.Cash },
        { "Trips", SheetsConfig.HeaderNames.Bonus, ColumnNotes.Bonus },
        { "Trips", SheetsConfig.HeaderNames.OdometerStart, ColumnNotes.Odometer },
        { "Trips", SheetsConfig.HeaderNames.OdometerEnd, ColumnNotes.Odometer },
        { "Shifts", SheetsConfig.HeaderNames.TimeStart, ColumnNotes.TimeStart },
        { "Shifts", SheetsConfig.HeaderNames.TimeEnd, ColumnNotes.TimeEnd },
        { "Shifts", SheetsConfig.HeaderNames.Cash, ColumnNotes.Cash },
        { "Shifts", SheetsConfig.HeaderNames.Bonus, ColumnNotes.Bonus },
    };

    // Every dropdown-backed note in this file opens by saying what the column holds and only then
    // explains where its values come from. They were all written the other way round originally -
    // mechanics only - which told a first-time user how to add a value but never what to put there.
    public static TheoryData<string> DropdownNotes =>
    [
        ColumnNotes.ServiceSource,
        ColumnNotes.RegionSource,
        ColumnNotes.TripNameSource,
        ColumnNotes.AddressSource,
        ColumnNotes.CategorySelf,
    ];

    [Theory]
    [MemberData(nameof(DropdownNotes))]
    public void DropdownNote_ExplainsTheColumnBeforeItsMechanics(string note)
    {
        var paragraphs = note.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);

        // Mechanics go last rather than strictly second - a note may need a middle paragraph
        // (Name explains that a return flips its meaning) without inverting the ordering rule.
        Assert.True(paragraphs.Length >= 2, $"expected meaning and mechanics paragraphs, got {paragraphs.Length}");
        Assert.DoesNotContain("Choose an existing", paragraphs[0]);
        Assert.StartsWith("Choose an existing", paragraphs[^1]);
    }

    [Theory]
    [MemberData(nameof(NotedColumns))]
    public void GetSheet_CarriesTheExpectedColumnNote(string sheetName, string headerName, string expectedNote)
    {
        var sheet = sheetName == "Trips" ? TripSheet.GetSheet() : ShiftSheet.GetSheet();

        var header = Assert.Single(sheet.Headers, h => h.Name == headerName);
        // Formatted columns get a "Cell Format: ..." hint appended by EntitySheetConfigHelper,
        // so the stored note is the constant plus a suffix rather than the constant alone.
        Assert.StartsWith(expectedNote, header.Note);
    }

    [Fact]
    public void RegionNote_ExplainsTheColumnBeforeItsDropdownMechanics()
    {
        // The note used to open with "Must match an existing region...", which says where the
        // dropdown values come from but never what a region actually is.
        Assert.StartsWith("City, area, or zone", ColumnNotes.RegionSource);
        Assert.Contains("Regions sheet", ColumnNotes.RegionSource);
    }

    [Fact]
    public void EveryInputColumnOnTheEnteredSheetsHasANote()
    {
        // Trips/Shifts/Expenses are the only sheets a user types into - the rest are rollups whose
        // columns are computed aggregates and would only be noise if annotated. Columns that are
        // derived even on these sheets (Day/Month/Year, Amt/*, and the formula totals) are excluded
        // for the same reason.
        string[] derived =
        [
            "Day", "Month", "Year", "Week", "Weekday", "Total", "Key",
            "Amt/Trip", "Amt/Hour", "Amt/Dist", "Amt/Day", "Trips/Hour",
        ];

        foreach (var (name, sheet) in new[]
        {
            ("Trips", TripSheet.GetSheet()),
            ("Shifts", ShiftSheet.GetSheet()),
            ("Expenses", ExpenseSheet.GetSheet()),
        })
        {
            var missing = sheet.Headers
                .Where(h => string.IsNullOrWhiteSpace(h.Formula))
                .Where(h => !derived.Contains(h.Name))
                .Where(h => string.IsNullOrWhiteSpace(h.Note))
                .Select(h => h.Name)
                .ToList();

            Assert.True(missing.Count == 0, $"{name} has un-noted input columns: {string.Join(", ", missing)}");
        }
    }

    [Theory]
    [InlineData("Trips")]
    [InlineData("Shifts")]
    [InlineData("Expenses")]
    public void DateColumn_DescribesItsFormatWithAnExample(string sheetName)
    {
        // Date used to carry ColumnNotes.DateFormat ("Format: YYYY-MM-DD"), which the generated
        // cell-format line now states more completely - pattern plus a rendered example. Keeping
        // both said the same thing twice, so the hand-written note was dropped.
        var sheet = sheetName switch
        {
            "Trips" => TripSheet.GetSheet(),
            "Shifts" => ShiftSheet.GetSheet(),
            _ => ExpenseSheet.GetSheet(),
        };

        var header = Assert.Single(sheet.Headers, h => h.Name == SheetsConfig.HeaderNames.Date);

        Assert.Contains("Cell Format:", header.Note);
        Assert.Contains("2026-03-09", header.Note);
        Assert.DoesNotContain("Format: YYYY-MM-DD", header.Note);
    }

    [Fact]
    public void TypesNote_OffersAValueForTheReturnCaseNameAndPlaceReferTo()
    {
        // Name and Place both tell the user to "set Type to match" on a return, so Type has to
        // actually suggest a value for it. The Types sheet is a rollup of types already used -
        // there is no seeded list - so this note is the only place the canonical set is written
        // down, and a type missing here is a type nobody knows to enter.
        Assert.Contains("Return", ColumnNotes.Types);
        Assert.Contains("set Type to match", ColumnNotes.TripNameSource);
        Assert.Contains("set Type to match", ColumnNotes.Place);
    }

    [Fact]
    public void TagsNote_StatesTheCommaConventionAndThatItIsFreeText()
    {
        // Tags is unvalidated on purpose - nothing in the sheet signals the comma convention or
        // that a typo will not be rejected, so the note has to carry both.
        Assert.Contains("separated by commas", ColumnNotes.Tags);
        Assert.Contains("Free text", ColumnNotes.Tags);
    }
}
