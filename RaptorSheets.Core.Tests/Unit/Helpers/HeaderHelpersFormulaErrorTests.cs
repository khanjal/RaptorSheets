using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

/// <summary>
/// A sheet whose headers are produced by a spilling QUERY (Deliveries, Locations) reports every
/// expected header as missing the moment that formula errors, because the row reads "#VALUE!"
/// instead of the header text. The anchor cell holding the formula is the one header that is not
/// HideHeaderName, so it becomes an insertion candidate - and inserting there shifts the broken
/// formula sideways, turning a recoverable formula error into a corrupted layout.
/// </summary>
public class HeaderHelpersFormulaErrorTests
{
    private static SheetModel BuildSheet() => new()
    {
        Name = "Locations",
        Headers =
        [
            new SheetCellModel { Name = "Place", Index = 0 },
            new SheetCellModel { Name = "Address", Index = 1, HideHeaderName = true },
            new SheetCellModel { Name = "Trips", Index = 2, HideHeaderName = true },
        ]
    };

    [Theory]
    [InlineData("#VALUE!")]
    [InlineData("#REF!")]
    [InlineData("#N/A")]
    [InlineData("#NAME?")]
    [InlineData("#DIV/0!")]
    [InlineData("#NUM!")]
    [InlineData("#NULL!")]
    [InlineData("#ERROR!")]
    public void CheckSheetHeaders_WithFormulaErrorInHeaderRow_InsertsNothing(string error)
    {
        HeaderHelpers.CheckSheetHeaders([error], BuildSheet(), out var insertionInfo);

        Assert.Empty(insertionInfo);
    }

    [Fact]
    public void CheckSheetHeaders_WithFormulaError_SaysTheFormulaIsFailing()
    {
        // The message has to point at the formula, not at missing columns - otherwise the obvious
        // reaction is to let self-heal "fix" it, which is the harmful path.
        var messages = HeaderHelpers.CheckSheetHeaders(["#VALUE!"], BuildSheet(), out _);

        var message = Assert.Single(messages);
        Assert.Contains("#VALUE!", message.Message);
        Assert.Contains("formula", message.Message, System.StringComparison.OrdinalIgnoreCase);
        Assert.Contains("reapply", message.Message, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CheckSheetHeaders_IsCaseInsensitiveAboutErrorText()
    {
        HeaderHelpers.CheckSheetHeaders(["#value!"], BuildSheet(), out var insertionInfo);

        Assert.Empty(insertionInfo);
    }

    [Fact]
    public void CheckSheetHeaders_WithoutError_StillDetectsAGenuinelyMissingColumn()
    {
        // The guard must not blanket-disable insertion - a real missing column on a healthy sheet
        // is still the case self-heal exists for.
        HeaderHelpers.CheckSheetHeaders(["Somewhere Else"], BuildSheet(), out var insertionInfo);

        var missing = Assert.Single(insertionInfo);
        Assert.Equal("Place", missing.ColumnName);
    }

    [Fact]
    public void CheckSheetHeaders_DoesNotMistakeAHeaderContainingErrorTextForAnError()
    {
        // Matching is on the whole trimmed cell, so a column legitimately named like this is safe.
        var sheet = new SheetModel
        {
            Name = "Widgets",
            Headers = [new SheetCellModel { Name = "#N/A Reason", Index = 0 }]
        };

        var messages = HeaderHelpers.CheckSheetHeaders(["#N/A Reason"], sheet, out var insertionInfo);

        Assert.Empty(insertionInfo);
        Assert.DoesNotContain(messages, m => m.Message.Contains("formula that builds"));
    }
}
