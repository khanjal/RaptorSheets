using Google.Apis.Sheets.v4.Data;

namespace GigRaptorLib.Constants
{
    // https://www.rapidtables.com/convert/color/hex-to-rgb.html
    public static class Colors
    {
        public static Color Black => new() { Red = 0, Green = 0, Blue = 0 };
        public static Color Blue => new() { Red = 0, Green = 0, Blue = 1 };
        public static Color Cyan => new() { Red = (float?)0.3, Green = (float?)0.8, Blue = (float?)0.9 };
        public static Color DarkYellow => new() { Red = (float?)0.9686274509803922, Green = (float?)0.796078431372549, Blue = (float?)0.30196078431372547 };
        public static Color Green => new() { Red = 0, Green = (float?)0.5, Blue = 0 };
        public static Color LightCyan => new() { Red = (float?)0.9, Green = (float?)1, Blue = (float?)1 };
        public static Color LightGray => new() { Red = (float?)0.9058823529411765, Green = (float?)0.9764705882352941, Blue = (float?)0.9372549019607843 };
        public static Color LightGreen => new() { Red = (float?)0.38823529411764707, Green = (float?)0.8235294117647058, Blue = (float?)0.592156862745098 };
        public static Color LightRed => new() { Red = (float?)1, Green = (float?)0.9, Blue = (float?)0.85 };
        public static Color LightYellow => new() { Red = (float?)0.996078431372549, Green = (float?)0.9725490196078431, Blue = (float?)0.8901960784313725 };
        public static Color Lime => new() { Red = 0, Green = 1, Blue = 0 };
        public static Color Orange => new() { Red = 1, Green = (float?)0.6, Blue = 0 };
        public static Color Magenta => new() { Red = 1, Green = 0, Blue = 1 };
        public static Color Purple => new() { Red = (float?)0.5, Green = 0, Blue = (float?)0.5 };
        public static Color Red => new() { Red = 1, Green = 0, Blue = 0 };
        public static Color White => new() { Red = 1, Green = 1, Blue = 1 };
        public static Color Yellow => new() { Red = 1, Green = 1, Blue = 0 };
    }
}
