namespace RaptorSheets.Core.Enums;

/// <summary>
/// A sheet's <c>TabColor</c> (SheetModel) is used as the header row's *background* band color
/// (see GoogleRequestHelpers.GenerateBandingRequest), while <c>FontColor</c> is the header row's
/// text color (see SheetHelpers.HeadersToRowData). FontColor defaults to BLACK, so any sheet whose
/// TabColor is one of the darker values below needs FontColor set to WHITE (or another light color)
/// explicitly, or the header text becomes illegible.
///
/// Dark (needs a light FontColor when used as TabColor): BLACK, BLUE, GREEN, MAGENTA, PINK, PURPLE, RED.
/// Light (default BLACK FontColor is fine): CYAN, DARK_YELLOW, LIGHT_*, LIME, ORANGE, WHITE, YELLOW.
/// </summary>
public enum SheetColor
{
    // DEFAULT BLACK
    BLACK = 0,
    BLUE,
    CYAN,
    DARK_YELLOW,
    GREEN,
    LIGHT_CYAN,
    LIGHT_GRAY,
    LIGHT_GREEN,
    LIGHT_PURPLE,
    LIGHT_RED,
    LIGHT_YELLOW,
    LIME,
    MAGENTA,
    ORANGE,
    PINK,
    PURPLE,
    RED,
    YELLOW,
    WHITE

}