using Google.Apis.Sheets.v4.Data;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Constants;

// Every value below is a genuine Google Sheets color-picker swatch (not an invented approximation)
// - sourced and cross-checked against two independent references, see issue #89. Where this
// library's name doesn't match one of Google's "standard colors" row 1:1 (the Light*/DarkYellow1
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
    public static Color DarkYellow1 => new() { Red = (float?)0.9450980392156863, Green = (float?)0.7607843137254902, Blue = (float?)0.19607843137254902 };
    // #00ff00 - Google's "Green" (standard colors row). Bright/light - pair with FontColor.BLACK, not WHITE.
    public static Color Green => new() { Red = 0, Green = 1, Blue = 0 };
    // #d0e0e3 - Google's "light cyan 3".
    public static Color LightCyan3 => new() { Red = (float?)0.8156862745098039, Green = (float?)0.8784313725490196, Blue = (float?)0.8901960784313725 };
    // #d9d9d9 - Google's "Light grey 1" (grayscale row).
    public static Color LightGrey1 => new() { Red = (float?)0.8509803921568627, Green = (float?)0.8509803921568627, Blue = (float?)0.8509803921568627 };
    // #d9ead3 - Google's "light green 3".
    public static Color LightGreen3 => new() { Red = (float?)0.8509803921568627, Green = (float?)0.9176470588235294, Blue = (float?)0.8274509803921568 };
    // #d9d2e9 - Google's "light purple 3".
    public static Color LightPurple3 => new() { Red = (float?)0.8509803921568627, Green = (float?)0.8235294117647058, Blue = (float?)0.9137254901960784 };
    // #f4cccc - Google's "light red 3".
    public static Color LightRed3 => new() { Red = (float?)0.9568627450980393, Green = (float?)0.8, Blue = (float?)0.8 };
    // #fff2cc - Google's "light yellow 3".
    public static Color LightYellow3 => new() { Red = 1, Green = (float?)0.9490196078431372, Blue = (float?)0.8 };
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
    // #434343 - Google's 'Dark grey 4' (grayscale row).
    public static Color DarkGrey4 => new() { Red = (float?)0.2627450980392157, Green = (float?)0.2627450980392157, Blue = (float?)0.2627450980392157 };
    // #666666 - Google's 'Dark grey 3' (grayscale row).
    public static Color DarkGrey3 => new() { Red = (float?)0.4, Green = (float?)0.4, Blue = (float?)0.4 };
    // #999999 - Google's 'Dark grey 2' (grayscale row).
    public static Color DarkGrey2 => new() { Red = (float?)0.6, Green = (float?)0.6, Blue = (float?)0.6 };
    // #b7b7b7 - Google's 'Dark grey 1' (grayscale row).
    public static Color DarkGrey1 => new() { Red = (float?)0.7176470588235294, Green = (float?)0.7176470588235294, Blue = (float?)0.7176470588235294 };
    // #cccccc - Google's 'Grey' (grayscale row).
    public static Color Grey => new() { Red = (float?)0.8, Green = (float?)0.8, Blue = (float?)0.8 };
    // #efefef - Google's 'Light grey 2' (grayscale row).
    public static Color LightGrey2 => new() { Red = (float?)0.9372549019607843, Green = (float?)0.9372549019607843, Blue = (float?)0.9372549019607843 };
    // #f3f3f3 - Google's 'Light grey 3' (grayscale row).
    public static Color LightGrey3 => new() { Red = (float?)0.9529411764705882, Green = (float?)0.9529411764705882, Blue = (float?)0.9529411764705882 };
    // #e6b8af - Google's 'light red berry 3'.
    public static Color LightRedBerry3 => new() { Red = (float?)0.9019607843137255, Green = (float?)0.7215686274509804, Blue = (float?)0.6862745098039216 };
    // #fce5cd - Google's 'light orange 3'.
    public static Color LightOrange3 => new() { Red = (float?)0.9882352941176471, Green = (float?)0.8980392156862745, Blue = (float?)0.803921568627451 };
    // #c9daf8 - Google's 'light cornflower blue 3'.
    public static Color LightCornflowerBlue3 => new() { Red = (float?)0.788235294117647, Green = (float?)0.8549019607843137, Blue = (float?)0.9725490196078431 };
    // #cfe2f3 - Google's 'light blue 3'.
    public static Color LightBlue3 => new() { Red = (float?)0.8117647058823529, Green = (float?)0.8862745098039215, Blue = (float?)0.9529411764705882 };
    // #ead1dc - Google's 'light magenta 3'.
    public static Color LightMagenta3 => new() { Red = (float?)0.9176470588235294, Green = (float?)0.8196078431372549, Blue = (float?)0.8627450980392157 };
}
