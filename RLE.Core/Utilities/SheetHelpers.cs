using Google.Apis.Sheets.v4.Data;
using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities.Extensions;
using RLE.Core.Utilities;
using RLE.Core.Constants;
using RLE.Gig.Constants;

namespace RLE.Gig.Utilities;

public static class SheetHelpers
{
    public static string GetSpreadsheetTitle(Spreadsheet sheet)
    {
        return sheet.Properties.Title;
    }

    public static List<string> GetSpreadsheetSheets(Spreadsheet sheet)
    {
        return sheet.Sheets.Select(x => x.Properties.Title.ToUpper()).ToList();
    }

    // https://www.rapidtables.com/convert/color/hex-to-rgb.html
    public static Color GetColor(ColorEnum colorEnum)
    {
        return colorEnum switch
        {
            ColorEnum.BLACK => Colors.Black,
            ColorEnum.BLUE => Colors.Blue,
            ColorEnum.CYAN => Colors.Cyan,
            ColorEnum.DARK_YELLOW => Colors.DarkYellow,
            ColorEnum.GREEN => Colors.Green,
            ColorEnum.LIGHT_CYAN => Colors.LightCyan,
            ColorEnum.LIGHT_GRAY => Colors.LightGray,
            ColorEnum.LIGHT_GREEN => Colors.LightGreen,
            ColorEnum.LIGHT_RED => Colors.LightRed,
            ColorEnum.LIGHT_YELLOW => Colors.LightYellow,
            ColorEnum.LIME => Colors.Lime,
            ColorEnum.ORANGE => Colors.Orange,
            ColorEnum.MAGENTA or ColorEnum.PINK => Colors.Magenta,
            ColorEnum.PURPLE => Colors.Purple,
            ColorEnum.RED => Colors.Red,
            ColorEnum.WHITE => Colors.White,
            ColorEnum.YELLOW => Colors.Yellow,
            _ => Colors.White,
        };
    }

    public static string GetColumnName(int index)
    {
        var letters = GoogleConfig.ColumnLetters;
        var value = string.Empty;

        if (index >= letters.Length)
            value += letters[index / letters.Length - 1];

        value += letters[index % letters.Length];

        return value;
    }

    public static IList<IList<object>> HeadersToList(List<SheetCellModel> headers)
    {
        var rangeData = new List<IList<object>>();
        var objectList = new List<object>();

        foreach (var header in headers)
        {
            if (!string.IsNullOrEmpty(header.Formula))
            {
                objectList.Add(header.Formula);
            }
            else
            {
                objectList.Add(header.Name);
            }
        }

        rangeData.Add(objectList);

        return rangeData;
    }

    public static IList<RowData> HeadersToRowData(SheetModel sheet)
    {
        var rows = new List<RowData>();
        var row = new RowData();
        var cells = new List<CellData>();

        foreach (var header in sheet.Headers)
        {
            var cell = new CellData
            {
                UserEnteredFormat = new CellFormat
                {
                    TextFormat = new TextFormat
                    {
                        Bold = true
                    }
                }
            };

            var value = new ExtendedValue();

            if (!string.IsNullOrEmpty(header.Formula))
            {
                value.FormulaValue = header.Formula;

                if (!sheet.ProtectSheet)
                {
                    var border = new Border
                    {
                        Style = BorderStyleEnum.SOLID_THICK.ToString()
                    };
                    cell.UserEnteredFormat.Borders = new Borders { Bottom = border, Left = border, Right = border, Top = border };
                }
            }
            else
            {
                value.StringValue = header.Name;
            }

            if (!string.IsNullOrEmpty(header.Note))
            {
                cell.Note = header.Note;
            }

            cell.UserEnteredValue = value;
            cells.Add(cell);
        }

        row.Values = cells;
        rows.Add(row);

        return rows;
    }

    // https://developers.google.com/sheets/api/guides/formats
    public static CellFormat GetCellFormat(FormatEnum format)
    {
        var cellFormat = new CellFormat
        {
            NumberFormat = format switch
            {
                FormatEnum.ACCOUNTING => new NumberFormat { Type = "NUMBER", Pattern = CellFormatPatterns.Accounting },
                FormatEnum.DATE => new NumberFormat { Type = "DATE", Pattern = CellFormatPatterns.Date },
                FormatEnum.DISTANCE => new NumberFormat { Type = "NUMBER", Pattern = CellFormatPatterns.Distance },
                FormatEnum.DURATION => new NumberFormat { Type = "DATE", Pattern = CellFormatPatterns.Duration },
                FormatEnum.NUMBER => new NumberFormat { Type = "NUMBER", Pattern = CellFormatPatterns.Number },
                FormatEnum.TEXT => new NumberFormat { Type = "TEXT" },
                FormatEnum.TIME => new NumberFormat { Type = "DATE", Pattern = CellFormatPatterns.Time },
                FormatEnum.WEEKDAY => new NumberFormat { Type = "DATE", Pattern = CellFormatPatterns.Weekday },
                _ => new NumberFormat { Type = "TEXT" }
            }
        };

        return cellFormat;
    }

}