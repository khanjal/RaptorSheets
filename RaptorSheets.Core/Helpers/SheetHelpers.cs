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
        var spreadsheetSheets = GetSpreadsheetSheets(sheetInfoResponse);

        return Enum.GetNames(typeof(TEnum)).Where(name => !spreadsheetSheets.Contains(name)).ToList();
    }

    public static List<MessageEntity> CheckSheets(List<string> sheets)
    {
        var messages = new List<MessageEntity>();

        // Loop through all sheets to see if they exist.
        foreach (var sheet in sheets)
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to find sheet {sheet}", MessageType.CHECK_SHEET));
        }

        if (messages.Count > 0)
        {
            return messages;
        }

        messages.Add(MessageHelpers.CreateInfoMessage("All sheets found", MessageType.CHECK_SHEET));

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
        if (sheet == null || sheet.Sheets == null)
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
            if (sheetData.Data[0].RowData == null)
            {
                sheetValues.Add(sheetData.Properties.Title, []);
                continue;
            }

            var values = sheetData.Data[0]?.RowData
                .Select(x => (IList<object>)x.Values.Select(y => (object)y.FormattedValue).ToList())
                .Where(row => row.Count > 0 && row[0] != null && !string.IsNullOrEmpty(row[0].ToString()))
                .ToList();

            sheetValues.Add(sheetData.Properties.Title, values ?? []);
        }
        return sheetValues;
    }

    // https://www.rapidtables.com/convert/color/hex-to-rgb.html
    public static Color GetColor(SheetColor colorEnum)
    {
        return colorEnum switch
        {
            SheetColor.BLACK => Colors.Black,
            SheetColor.BLUE => Colors.Blue,
            SheetColor.CYAN => Colors.Cyan,
            SheetColor.DARK_YELLOW => Colors.DarkYellow,
            SheetColor.GREEN => Colors.Green,
            SheetColor.LIGHT_CYAN => Colors.LightCyan,
            SheetColor.LIGHT_GRAY => Colors.LightGray,
            SheetColor.LIGHT_GREEN => Colors.LightGreen,
            SheetColor.LIGHT_PURPLE => Colors.LightPurple,
            SheetColor.LIGHT_RED => Colors.LightRed,
            SheetColor.LIGHT_YELLOW => Colors.LightYellow,
            SheetColor.LIME => Colors.Lime,
            SheetColor.ORANGE => Colors.Orange,
            SheetColor.MAGENTA or SheetColor.PINK => Colors.Magenta,
            SheetColor.PURPLE => Colors.Purple,
            SheetColor.RED => Colors.Red,
            SheetColor.WHITE => Colors.White,
            SheetColor.YELLOW => Colors.Yellow,
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
        var row = new RowData
        {
            Values = sheet.Headers.Select(header => BuildHeaderCell(header, sheet)).ToList()
        };

        rows.Add(row);

        return rows;
    }

    private static CellData BuildHeaderCell(SheetCellModel header, SheetModel sheet)
    {
        var cell = new CellData
        {
            UserEnteredFormat = new CellFormat
            {
                TextFormat = new TextFormat
                {
                    Bold = true,
                    ForegroundColor = GetColor(sheet.FontColor),
                }
            }
        };

        var value = new ExtendedValue();

        // Use a formula if the header explicitly has one (non-empty) or if the header is protected.
        // This ensures an empty-string formula only counts when protection is intended.
        if (header.Protect || !string.IsNullOrEmpty(header.Formula))
        {
            // Distinguish between null and empty string formulas:
            // - null => use the header Name as the formula value
            // - empty string ("") => treat as an explicit empty formula
            value.FormulaValue = header.Formula ?? header.Name;

            if (!sheet.ProtectSheet)
            {
                var border = new Border
                {
                    Style = BorderStyle.SOLID_THICK.ToString()
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

        // If requested, do not set a UserEnteredValue so the cell remains empty (allows formula spills)
        cell.UserEnteredValue = header.HideHeaderName ? null : value;

        return cell;
    }

    // https://developers.google.com/sheets/api/guides/formats
    public static CellFormat GetCellFormat(Format format)
    {
        var cellFormat = new CellFormat
        {
            NumberFormat = format switch
            {
                Format.ACCOUNTING => new NumberFormat { Type = CellFormatPatterns.CellFormatNumber, Pattern = CellFormatPatterns.Accounting },
                Format.CURRENCY => new NumberFormat { Type = CellFormatPatterns.CellFormatNumber, Pattern = CellFormatPatterns.Currency },
                Format.DATE => new NumberFormat { Type = CellFormatPatterns.CellFormatDate, Pattern = CellFormatPatterns.Date },
                Format.DISTANCE => new NumberFormat { Type = CellFormatPatterns.CellFormatNumber, Pattern = CellFormatPatterns.Distance },
                Format.DURATION => new NumberFormat { Type = CellFormatPatterns.CellFormatDate, Pattern = CellFormatPatterns.Duration },
                Format.NUMBER => new NumberFormat { Type = CellFormatPatterns.CellFormatNumber, Pattern = CellFormatPatterns.Number },
                Format.TEXT => new NumberFormat { Type = CellFormatPatterns.CellFormatText },
                Format.TIME => new NumberFormat { Type = CellFormatPatterns.CellFormatDate, Pattern = CellFormatPatterns.Time },
                Format.WEEKDAY => new NumberFormat { Type = CellFormatPatterns.CellFormatDate, Pattern = CellFormatPatterns.Weekday },
                _ => new NumberFormat { Type = CellFormatPatterns.CellFormatText }
            }
        };

        return cellFormat;
    }

    /// <summary>
    /// Get cell format with a custom number format pattern.
    /// This overload allows specifying a custom pattern that overrides the default for the Format.
    /// </summary>
    /// <param name="format">The format type</param>
    /// <param name="customPattern">Custom number format pattern (e.g., "#,##0.0")</param>
    /// <returns>CellFormat with the custom pattern applied</returns>
    public static CellFormat GetCellFormat(Format format, string customPattern)
    {
        var cellFormat = GetCellFormat(format);
        
        // Override the pattern with the custom one if provided
        if (!string.IsNullOrEmpty(customPattern) && cellFormat.NumberFormat != null)
        {
            cellFormat.NumberFormat.Pattern = customPattern;
        }
        
        return cellFormat;
    }

}