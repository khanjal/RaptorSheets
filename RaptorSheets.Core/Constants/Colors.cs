using Google.Apis.Sheets.v4.Data;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Constants;

// Every value below is a genuine Google Sheets color-picker swatch (not an invented approximation)
// - sourced and cross-checked against two independent references, see issue #89. Where this
// library's name doesn't match one of Google's "standard colors" row 1:1 (the Light*/DarkYellow
// entries), it's mapped to the closest same-hue tier from Google's own light/dark ramp instead of a
// hand-picked value, so a color this library writes is always something a user could have picked by
// hand in Sheets' own UI too.
[ExcludeFromCodeCoverage]
public static class Colors
{
    public static Color Black => new() { Red = 0, Green = 0, Blue = 0 };
    public static Color Blue => new() { Red = 0, Green = 0, Blue = 1 };
    // #00ffff - Google's "Cyan" (standard colors row).
    public static Color Cyan => new() { Red = 0, Green = 1, Blue = 1 };
    // #f1c232 - Google's "dark yellow 1".
    public static Color DarkYellow => new() { Red = (float?)0.9450980392156863, Green = (float?)0.7607843137254902, Blue = (float?)0.19607843137254902 };
    // #00ff00 - Google's "Green" (standard colors row). Bright/light - pair with FontColor.BLACK, not WHITE.
    public static Color Green => new() { Red = 0, Green = 1, Blue = 0 };
    // #d0e0e3 - Google's "light cyan 3".
    public static Color LightCyan => new() { Red = (float?)0.8156862745098039, Green = (float?)0.8784313725490196, Blue = (float?)0.8901960784313725 };
    // #d9d9d9 - Google's "Light grey 1" (grayscale row).
    public static Color LightGray => new() { Red = (float?)0.8509803921568627, Green = (float?)0.8509803921568627, Blue = (float?)0.8509803921568627 };
    // #d9ead3 - Google's "light green 3".
    public static Color LightGreen => new() { Red = (float?)0.8509803921568627, Green = (float?)0.9176470588235294, Blue = (float?)0.8274509803921568 };
    // #d9d2e9 - Google's "light purple 3".
    public static Color LightPurple => new() { Red = (float?)0.8509803921568627, Green = (float?)0.8235294117647058, Blue = (float?)0.9137254901960784 };
    // #f4cccc - Google's "light red 3".
    public static Color LightRed => new() { Red = (float?)0.9568627450980393, Green = (float?)0.8, Blue = (float?)0.8 };
    // #fff2cc - Google's "light yellow 3".
    public static Color LightYellow => new() { Red = 1, Green = (float?)0.9490196078431372, Blue = (float?)0.8 };
    public static Color Orange => new() { Red = 1, Green = (float?)0.6, Blue = 0 };
    public static Color Magenta => new() { Red = 1, Green = 0, Blue = 1 };
    // #9900ff - Google's "Purple" (standard colors row).
    public static Color Purple => new() { Red = (float?)0.6, Green = 0, Blue = 1 };
    public static Color Red => new() { Red = 1, Green = 0, Blue = 0 };
    public static Color White => new() { Red = 1, Green = 1, Blue = 1 };
    public static Color Yellow => new() { Red = 1, Green = 1, Blue = 0 };
    // #980000 - Google's "Red Berry" (standard colors row).
    public static Color RedBerry => new() { Red = (float?)0.596078431372549, Green = 0, Blue = 0 };
    // #4a86e8 - Google's "Cornflower Blue" (standard colors row).
    public static Color CornflowerBlue => new() { Red = (float?)0.2901960784313725, Green = (float?)0.5254901960784314, Blue = (float?)0.9098039215686274 };
}
