namespace RaptorSheets.Core.Enums;

/// <summary>
/// A sheet's <c>TabColor</c> (SheetModel) is used as the header row's *background* band color
/// (see GoogleRequestHelpers.GenerateBandingRequest), while <c>FontColor</c> is the header row's
/// text color (see SheetHelpers.HeadersToRowData). FontColor defaults to BLACK, so any sheet whose
/// TabColor is one of the darker values below needs FontColor set to WHITE (or another light color)
/// explicitly, or the header text becomes illegible.
///
/// Dark (needs a light FontColor when used as TabColor): BLACK, BLUE, CORNFLOWER_BLUE, MAGENTA, PURPLE, RED, RED_BERRY.
/// Light (default BLACK FontColor is fine): CYAN, DARK_YELLOW_1, GREEN, LIGHT_*, ORANGE, WHITE, YELLOW.
/// </summary>
public enum SheetColor
{
    // DEFAULT BLACK
    BLACK = 0,
    BLUE,
    CYAN,
    DARK_YELLOW_1,
    GREEN,
    LIGHT_CYAN_3,
    LIGHT_GREY_1,
    LIGHT_GREEN_3,
    LIGHT_PURPLE_3,
    LIGHT_RED_3,
    LIGHT_YELLOW_3,
    MAGENTA,
    ORANGE,
    PURPLE,
    RED,
    YELLOW,
    WHITE,
    RED_BERRY,
    CORNFLOWER_BLUE
}
