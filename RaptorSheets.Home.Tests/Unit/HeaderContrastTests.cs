using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Helpers;

namespace RaptorSheets.Home.Tests.Unit;

/// <summary>
/// Enforces the header-contrast convention documented on <see cref="SheetColor"/>: a sheet's
/// TabColor becomes the header row's background band (GoogleRequestHelpers.GenerateBandingRequest),
/// while FontColor (defaults to BLACK) is the header row's text color. A dark TabColor with no
/// explicit FontColor renders illegible header text - this test computes perceived brightness (YIQ)
/// from the actual TabColor RGB so it stays correct even if Colors.cs values change, rather than
/// relying on a hardcoded "dark colors" list going stale.
/// </summary>
public class HeaderContrastTests
{
    [Theory]
    [MemberData(nameof(AllSheetNames))]
    public void DarkTabColor_HasLightFontColor(string sheetName)
    {
        var sheet = HomeSheetHelpers.GetSheetLayout(sheetName)!;

        if (!IsDark(sheet.TabColor))
        {
            return; // light TabColor - default BLACK font is legible, no FontColor needed
        }

        Assert.True(!IsDark(sheet.FontColor),
            $"'{sheetName}' has a dark TabColor ({sheet.TabColor}) but FontColor is {sheet.FontColor} " +
            "(likely the BLACK default) - header text will be illegible. Set FontColor to WHITE.");
    }

    public static IEnumerable<object[]> AllSheetNames() =>
        SheetsConfig.SheetUtilities.GetAllSheetNames().Select(n => new object[] { n });

    /// <summary>
    /// YIQ perceived-brightness formula (the same threshold used by common WCAG-adjacent contrast
    /// heuristics): below 0.5 reads as "dark" against black text.
    /// </summary>
    private static bool IsDark(SheetColor color)
    {
        var rgb = SheetHelpers.GetColor(color);
        var yiq = ((rgb.Red ?? 0) * 299 + (rgb.Green ?? 0) * 587 + (rgb.Blue ?? 0) * 114) / 1000;
        return yiq < 0.5;
    }
}
