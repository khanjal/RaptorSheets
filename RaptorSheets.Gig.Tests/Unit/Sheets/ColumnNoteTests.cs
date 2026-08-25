using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Sheets;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Sheets;

/// <summary>
/// Notes are the only in-sheet explanation of what a column is for, so they have to survive the
/// entity -> SheetModel generation rather than just existing as constants. Covers the two columns
/// whose purpose is least self-evident from the header alone: Region (a bare place name) and Tags
/// (free text with a comma convention the sheet cannot otherwise communicate).
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

        Assert.Equal(2, paragraphs.Length);
        Assert.DoesNotContain("Must match", paragraphs[0]);
        Assert.StartsWith("Must match an existing", paragraphs[1]);
    }

    [Theory]
    [MemberData(nameof(NotedColumns))]
    public void GetSheet_CarriesTheExpectedColumnNote(string sheetName, string headerName, string expectedNote)
    {
        var sheet = sheetName == "Trips" ? TripSheet.GetSheet() : ShiftSheet.GetSheet();

        var header = Assert.Single(sheet.Headers, h => h.Name == headerName);
        Assert.Equal(expectedNote, header.Note);
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
    public void TagsNote_StatesTheCommaConventionAndThatItIsFreeText()
    {
        // Tags is unvalidated on purpose - nothing in the sheet signals the comma convention or
        // that a typo will not be rejected, so the note has to carry both.
        Assert.Contains("separated by commas", ColumnNotes.Tags);
        Assert.Contains("Free text", ColumnNotes.Tags);
    }
}
