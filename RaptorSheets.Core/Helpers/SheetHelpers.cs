using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Helpers;

public static class SheetHelpers
{
    public static List<string> CheckSheets<TEnum>(Spreadsheet? sheetInfoResponse) where TEnum : Enum
    {
        var missingSheets = new List<string>();
        var spreadsheetSheets = GetSpreadsheetSheets(sheetInfoResponse);

        // Loop through all sheets to see if they exist.
        foreach (var name in Enum.GetNames(typeof(TEnum)))
        {
            if (!spreadsheetSheets.Contains(name))
            {
                missingSheets.Add(name);
                continue;
            }
        }

        return missingSheets;
    }

    public static List<MessageEntity> CheckSheets(List<string> sheets)
    {
        var messages = new List<MessageEntity>();

        // Loop through all sheets to see if they exist.
        foreach (var sheet in sheets)
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to find sheet {sheet}", MessageTypeEnum.CHECK_SHEET));
            continue;
        }

        if (messages.Count > 0)
        {
            return messages;
        }

        messages.Add(MessageHelpers.CreateInfoMessage("All sheets found", MessageTypeEnum.CHECK_SHEET));

        return messages;
    }

    public static string GetSpreadsheetTitle(Spreadsheet? sheet)
    {
        if (sheet == null)
        {
            return string.Empty;
        }

        return sheet.Properties.Title;
    }

    public static List<string> GetSpreadsheetSheets(Spreadsheet? sheet)
    {
        if (sheet == null)
        {
            return [];
        }
        return sheet.Sheets.Select(x => x.Properties.Title.ToUpper()).ToList();
    }

    public static Dictionary<string, IList<IList<object>>> GetSheetValues(Spreadsheet sheet)
    {
        var sheetValues = new Dictionary<string, IList<IList<object>>>();
        foreach (var sheetData in sheet.Sheets)
        {
            var values = sheetData.Data[0]?.RowData
                .Select(x => (IList<object>)x.Values.Select(y => (object)y.FormattedValue).ToList())
                .Where(row => row.Count > 0 && row[0] != null && !string.IsNullOrEmpty(row[0].ToString()))
                .ToList() ?? [];

            if (values != null)
            {
                sheetValues.Add(sheetData.Properties.Title, values);
            }
        }
        return sheetValues;
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
            ColorEnum.LIGHT_PURPLE => Colors.LightPurple,
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