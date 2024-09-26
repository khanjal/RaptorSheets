using Google.Apis.Sheets.v4.Data;
using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities.Extensions;
using RLE.Core.Utilities;
using RLE.Gig.Enums;
using RLE.Gig.Mappers;
using RLE.Gig.Constants;
using RLE.Core.Constants;
using RLE.Gig.Entities;

namespace RLE.Gig.Utilities;

public static class SheetHelper
{
    // TODO Go through all these functions and see if they can be removed
    public static string ArrayFormulaCountIf()
    {
        return "=ARRAYFORMULA(IFS(ROW($A:$A)=1,\"{0}\",ISBLANK($A:$A), \"\",true,COUNTIF({1},$A:$A)))";
    }

    public static string ArrayFormulaSumIf()
    {
        return "=ARRAYFORMULA(IFS(ROW($A:$A)=1,\"{0}\",ISBLANK($A:$A), \"\",true,SUMIF({1},$A:$A, {2})))";
    }

    public static string ArrayFormulaVisit(string headerText, string referenceSheet, string columnStart, string columnEnd, bool first)
    {
        return $"=ARRAYFORMULA(IFS(ROW($A:$A)=1,\"{headerText}\",ISBLANK($A:$A), \"\", true, IFERROR(VLOOKUP($A:$A,SORT(QUERY({referenceSheet}!{columnStart}:{columnEnd},\"SELECT {columnEnd}, {columnStart}\"),2,{first}),2,0),\"\")))";
    }

    public static List<SheetModel> GetSheets()
    {
        var sheets = new List<SheetModel>
        {
            ShiftMapper.GetSheet(),
            TripMapper.GetSheet()
        };

        return sheets;
    }

    public static string GetSpreadsheetTitle(Spreadsheet sheet)
    {
        return sheet.Properties.Title;
    }

    public static List<string> GetSpreadsheetSheets(Spreadsheet sheet)
    {
        return sheet.Sheets.Select(x => x.Properties.Title.ToUpper()).ToList();
    }

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        var spreadsheetSheets = spreadsheet.Sheets.Select(x => x.Properties.Title.ToUpper()).ToList();
        var sheetData = new List<SheetModel>();

        // Loop through all sheets to see if they exist.
        foreach (var name in Enum.GetNames<GigSheetEnum>())
        {
            GigSheetEnum sheetEnum = (GigSheetEnum)Enum.Parse(typeof(GigSheetEnum), name);

            if (spreadsheetSheets.Contains(name))
            {
                continue;
            }

            // Get data for each missing sheet.
            switch (sheetEnum)
            {
                case GigSheetEnum.ADDRESSES:
                    sheetData.Add(AddressMapper.GetSheet());
                    break;
                case GigSheetEnum.DAILY:
                    sheetData.Add(DailyMapper.GetSheet());
                    break;
                case GigSheetEnum.MONTHLY:
                    sheetData.Add(MonthlyMapper.GetSheet());
                    break;
                case GigSheetEnum.NAMES:
                    sheetData.Add(NameMapper.GetSheet());
                    break;
                case GigSheetEnum.PLACES:
                    sheetData.Add(PlaceMapper.GetSheet());
                    break;
                case GigSheetEnum.REGIONS:
                    sheetData.Add(RegionMapper.GetSheet());
                    break;
                case GigSheetEnum.SERVICES:
                    sheetData.Add(ServiceMapper.GetSheet());
                    break;
                case GigSheetEnum.SHIFTS:
                    sheetData.Add(ShiftMapper.GetSheet());
                    break;
                case GigSheetEnum.TRIPS:
                    sheetData.Add(TripMapper.GetSheet());
                    break;
                case GigSheetEnum.TYPES:
                    sheetData.Add(TypeMapper.GetSheet());
                    break;
                case GigSheetEnum.WEEKDAYS:
                    sheetData.Add(WeekdayMapper.GetSheet());
                    break;
                case GigSheetEnum.WEEKLY:
                    sheetData.Add(WeeklyMapper.GetSheet());
                    break;
                case GigSheetEnum.YEARLY:
                    sheetData.Add(YearlyMapper.GetSheet());
                    break;
                default:
                    break;
            }
        }

        return sheetData;
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

    public static DataValidationRule GetDataValidation(ValidationEnum validation)
    {
        var dataValidation = new DataValidationRule();

        switch (validation)
        {
            case ValidationEnum.BOOLEAN:
                dataValidation.Condition = new BooleanCondition { Type = "BOOLEAN" };
                break;
            case ValidationEnum.RANGE_ADDRESS:
            case ValidationEnum.RANGE_NAME:
            case ValidationEnum.RANGE_PLACE:
            case ValidationEnum.RANGE_REGION:
            case ValidationEnum.RANGE_SERVICE:
            case ValidationEnum.RANGE_TYPE:
                var values = new List<ConditionValue> { new() { UserEnteredValue = $"={GetSheetForRange(validation)?.GetDescription()}!A2:A" } };
                dataValidation.Condition = new BooleanCondition { Type = "ONE_OF_RANGE", Values = values };
                dataValidation.ShowCustomUi = true;
                dataValidation.Strict = false;
                break;
        }

        return dataValidation;
    }

    private static GigSheetEnum? GetSheetForRange(ValidationEnum validationEnum)
    {
        return validationEnum switch
        {
            ValidationEnum.RANGE_ADDRESS => GigSheetEnum.ADDRESSES,
            ValidationEnum.RANGE_NAME => GigSheetEnum.NAMES,
            ValidationEnum.RANGE_PLACE => GigSheetEnum.PLACES,
            ValidationEnum.RANGE_REGION => GigSheetEnum.REGIONS,
            ValidationEnum.RANGE_SERVICE => GigSheetEnum.SERVICES,
            ValidationEnum.RANGE_TYPE => GigSheetEnum.TYPES,
            _ => null
        };
    }

    public static List<SheetCellModel> GetCommonShiftGroupSheetHeaders(SheetModel shiftSheet, HeaderEnum keyEnum)
    {
        var sheet = new SheetModel
        {
            Headers = []
        };
        var sheetKeyRange = shiftSheet.GetRange(keyEnum);

        switch (keyEnum)
        {
            case HeaderEnum.REGION:
                // A - [Key]
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.REGION.GetDescription(),
                    Formula = "={\"" + HeaderEnum.REGION.GetDescription() + "\";SORT(UNIQUE({" + TripMapper.GetSheet().GetRange(HeaderEnum.REGION, 2) + ";" + shiftSheet.GetRange(HeaderEnum.REGION, 2) + "}))}"
                });
                break;
            case HeaderEnum.SERVICE:
                // A - [Key]
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.SERVICE.GetDescription(),
                    Formula = "={\"" + HeaderEnum.SERVICE.GetDescription() + "\";SORT(UNIQUE({" + TripMapper.GetSheet().GetRange(HeaderEnum.SERVICE, 2) + ";" + shiftSheet.GetRange(HeaderEnum.SERVICE, 2) + "}))}"
                });
                break;
            default:
                // A - [Key]
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = keyEnum.GetDescription(),
                    Formula = ArrayFormulaHelper.ArrayForumlaUnique(shiftSheet.GetRange(keyEnum, 2), keyEnum.GetDescription())
                });
                break;
        }
        var keyRange = sheet.GetLocalRange(keyEnum);
        // B - Trips
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TRIPS.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TRIPS.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TRIPS)),
            Format = FormatEnum.NUMBER
        });
        // C - Pay
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.PAY.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.PAY.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_PAY)),
            Format = FormatEnum.ACCOUNTING
        });
        // D - Tip
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TIPS.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TIPS.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TIPS)),
            Format = FormatEnum.ACCOUNTING
        });
        // E - Bonus
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.BONUS.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.BONUS.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_BONUS)),
            Format = FormatEnum.ACCOUNTING
        });
        // F - Total
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaTotal(keyRange, HeaderEnum.TOTAL.GetDescription(), sheet.GetLocalRange(HeaderEnum.PAY), sheet.GetLocalRange(HeaderEnum.TIPS), sheet.GetLocalRange(HeaderEnum.BONUS)),
            Format = FormatEnum.ACCOUNTING
        });
        // G - Cash
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.CASH.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.CASH.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_CASH)),
            Format = FormatEnum.ACCOUNTING
        });
        // H - Amt/Trip
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_TRIP.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TRIP.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.TRIPS)}=0,1,{sheet.GetLocalRange(HeaderEnum.TRIPS)})))",
            Format = FormatEnum.ACCOUNTING
        });
        // I - Dist
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.DISTANCE.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.DISTANCE.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_DISTANCE)),
            Format = FormatEnum.DISTANCE
        });
        // J - Amt/Dist
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.DISTANCE)}=0,1,{sheet.GetLocalRange(HeaderEnum.DISTANCE)})))",
            Format = FormatEnum.ACCOUNTING
        });

        switch (keyEnum)
        {
            case HeaderEnum.ADDRESS:
            case HeaderEnum.NAME:
            case HeaderEnum.PLACE:
            case HeaderEnum.REGION:
            case HeaderEnum.SERVICE:
            case HeaderEnum.TYPE:
                // K - First Visit
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.VISIT_FIRST.GetDescription(),
                    Formula = ArrayFormulaHelper.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), GigSheetEnum.SHIFTS.GetDescription(), shiftSheet.GetColumn(HeaderEnum.DATE), shiftSheet.GetColumn(keyEnum), true),
                    Note = ColumnNotes.DateFormat,
                    Format = FormatEnum.DATE
                });
                // L - Last Visit
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.VISIT_LAST.GetDescription(),
                    Formula = ArrayFormulaHelper.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), GigSheetEnum.SHIFTS.GetDescription(), shiftSheet.GetColumn(HeaderEnum.DATE), shiftSheet.GetColumn(keyEnum), false),
                    Note = ColumnNotes.DateFormat,
                    Format = FormatEnum.DATE
                });
                break;
            case HeaderEnum.DATE:
                // K - Time
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.TIME_TOTAL.GetDescription(),
                    Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TIME_TOTAL.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TIME)),
                    Format = FormatEnum.DURATION
                });
                // L - Amt/Time
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription(),
                    Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}=0,1,{sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}*24)))",
                    Format = FormatEnum.ACCOUNTING
                });
                break;
        }

        return sheet.Headers;
    }

    public static List<SheetCellModel> GetCommonTripGroupSheetHeaders(SheetModel refSheet, HeaderEnum keyEnum)
    {
        var sheet = new SheetModel
        {
            Headers = []
        };
        var sheetKeyRange = refSheet.GetRange(keyEnum);
        var keyRange = GoogleConfig.KeyRange; // This should be the default but could cause issues if not the first field.

        // A - [Key]
        switch (keyEnum)
        {
            case HeaderEnum.DAY:
            case HeaderEnum.WEEK:
            case HeaderEnum.MONTH:
            case HeaderEnum.YEAR:
                if (keyEnum == HeaderEnum.DAY)
                {
                    // A - [Key]
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = keyEnum.GetDescription(),
                        Formula = ArrayFormulaHelper.ArrayForumlaUniqueFilterSort(refSheet.GetRange(keyEnum, 2), keyEnum.GetDescription())
                    });
                    keyRange = sheet.GetLocalRange(keyEnum);

                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.WEEKDAY.GetDescription(),
                        Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.WEEKDAY.GetDescription()}\",ISBLANK({keyRange}), \"\", true,TEXT({keyRange}+1,\"ddd\")))",
                    });
                }
                else
                {
                    // A - [Key]
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = keyEnum.GetDescription(),
                        Formula = ArrayFormulaHelper.ArrayForumlaUniqueFilter(refSheet.GetRange(keyEnum, 2), keyEnum.GetDescription())
                    });
                    keyRange = sheet.GetLocalRange(keyEnum);
                }

                // B - Trips
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.TRIPS.GetDescription(),
                    Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TRIPS.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.TRIPS)),
                    Format = FormatEnum.NUMBER
                });

                if (keyEnum == HeaderEnum.YEAR)
                {
                    // C - Days
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.DAYS.GetDescription(),
                        Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.DAYS.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.DAYS)),
                        Format = FormatEnum.NUMBER
                    });
                }
                else
                {
                    // C - Days
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.DAYS.GetDescription(),
                        Formula = ArrayFormulaHelper.ArrayFormulaCountIf(keyRange, HeaderEnum.DAYS.GetDescription(), sheetKeyRange),
                        Format = FormatEnum.NUMBER
                    });
                }

                break;
            default:
                if (keyEnum == HeaderEnum.ADDRESS_END)
                {
                    // A - [Key]
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.ADDRESS.GetDescription(),
                        Formula = "={\"" + HeaderEnum.ADDRESS.GetDescription() + "\";SORT(UNIQUE({" + refSheet.GetRange(HeaderEnum.ADDRESS_END, 2) + ";" + refSheet.GetRange(HeaderEnum.ADDRESS_START, 2) + "}))}"
                    });

                    // B - Trips
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.TRIPS.GetDescription(),
                        Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.TRIPS.GetDescription()}\",ISBLANK({keyRange}), \"\",true,COUNTIF({refSheet.GetRange(HeaderEnum.ADDRESS_END, 2)},{keyRange})+COUNTIF({refSheet.GetRange(HeaderEnum.ADDRESS_START, 2)},{keyRange})))",
                        Format = FormatEnum.NUMBER
                    });
                }
                else
                {
                    // A - [Key]
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = keyEnum.GetDescription(),
                        Formula = ArrayFormulaHelper.ArrayForumlaUnique(refSheet.GetRange(keyEnum, 2), keyEnum.GetDescription())
                    });
                    keyRange = sheet.GetLocalRange(keyEnum);
                    // B - Trips
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.TRIPS.GetDescription(),
                        Formula = ArrayFormulaHelper.ArrayFormulaCountIf(keyRange, HeaderEnum.TRIPS.GetDescription(), sheetKeyRange),
                        Format = FormatEnum.NUMBER
                    });
                }
                break;
        }

        // C - Pay
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.PAY.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.PAY.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.PAY)),
            Format = FormatEnum.ACCOUNTING
        });
        // D - Tip
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TIPS.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TIPS.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.TIPS)),
            Format = FormatEnum.ACCOUNTING
        });
        // E - Bonus
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.BONUS.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.BONUS.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.BONUS)),
            Format = FormatEnum.ACCOUNTING
        });
        // F - Total
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaTotal(keyRange, HeaderEnum.TOTAL.GetDescription(), sheet.GetLocalRange(HeaderEnum.PAY), sheet.GetLocalRange(HeaderEnum.TIPS), sheet.GetLocalRange(HeaderEnum.BONUS)),
            Format = FormatEnum.ACCOUNTING
        });
        // G - Cash
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.CASH.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.CASH.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.CASH)),
            Format = FormatEnum.ACCOUNTING
        });
        // H - Amt/Trip
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_TRIP.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TRIP.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.TRIPS)}=0,1,{sheet.GetLocalRange(HeaderEnum.TRIPS)})))",
            Format = FormatEnum.ACCOUNTING
        });
        // I - Dist
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.DISTANCE.GetDescription(),
            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.DISTANCE.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.DISTANCE)),
            Format = FormatEnum.DISTANCE
        });
        // J - Amt/Dist
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.DISTANCE)}=0,1,{sheet.GetLocalRange(HeaderEnum.DISTANCE)})))",
            Format = FormatEnum.ACCOUNTING
        });

        switch (keyEnum)
        {
            case HeaderEnum.ADDRESS_END:
            case HeaderEnum.NAME:
            case HeaderEnum.PLACE:
            case HeaderEnum.REGION:
            case HeaderEnum.SERVICE:
            case HeaderEnum.TYPE:
                // K - First Visit
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.VISIT_FIRST.GetDescription(),
                    Formula = ArrayFormulaHelper.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), GigSheetEnum.TRIPS.GetDescription(), refSheet.GetColumn(HeaderEnum.DATE), refSheet.GetColumn(keyEnum), true),
                    Format = FormatEnum.DATE
                });
                // L - Last Visit
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.VISIT_LAST.GetDescription(),
                    Formula = ArrayFormulaHelper.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), GigSheetEnum.TRIPS.GetDescription(), refSheet.GetColumn(HeaderEnum.DATE), refSheet.GetColumn(keyEnum), false),
                    Format = FormatEnum.DATE
                });
                break;
            case HeaderEnum.DAY:
            case HeaderEnum.WEEK:
            case HeaderEnum.MONTH:
            case HeaderEnum.YEAR:
                // Time
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.TIME_TOTAL.GetDescription(),
                    Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TIME_TOTAL.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.TIME_TOTAL)),
                    Format = FormatEnum.DURATION
                });
                // Amt/Time
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription(),
                    Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}=0,1,{sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}*24)))",
                    Format = FormatEnum.ACCOUNTING
                });

                // Amt/Day
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.AMOUNT_PER_DAY.GetDescription(),
                    Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_DAY.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.DAYS)}=0,1,{sheet.GetLocalRange(HeaderEnum.DAYS)})))",
                    Format = FormatEnum.ACCOUNTING
                });

                if (keyEnum != HeaderEnum.DAY)
                {
                    // Average
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.AVERAGE.GetDescription(),
                        Formula = "=ARRAYFORMULA(IFS(ROW(" + keyRange + ")=1,\"" + HeaderEnum.AVERAGE.GetDescription() + "\",ISBLANK(" + keyRange + "), \"\",true, DAVERAGE(transpose({" + sheet.GetLocalRange(HeaderEnum.TOTAL) + ",TRANSPOSE(if(ROW(" + sheet.GetLocalRange(HeaderEnum.TOTAL) + ") <= TRANSPOSE(ROW(" + sheet.GetLocalRange(HeaderEnum.TOTAL) + "))," + sheet.GetLocalRange(HeaderEnum.TOTAL) + ",))}),sequence(rows(" + sheet.GetLocalRange(HeaderEnum.TOTAL) + "),1),{if(,,);if(,,)})))",
                        Format = FormatEnum.ACCOUNTING
                    });
                }

                break;
        }

        return sheet.Headers;
    }

    public static GigSheetEntity? MapData(BatchGetValuesByDataFilterResponse response)
    {
        if (response.ValueRanges == null)
        {
            return null;
        }

        var sheet = new GigSheetEntity();

        // TODO: Figure out a better way to handle looping with message and entity mapping in the switch.
        foreach (var matchedValue in response.ValueRanges)
        {
            var sheetRange = matchedValue.DataFilters[0].A1Range;
            var values = matchedValue.ValueRange.Values;

            Enum.TryParse(sheetRange.ToUpper(), out GigSheetEnum sheetEnum);

            switch (sheetEnum)
            {
                case GigSheetEnum.ADDRESSES:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, AddressMapper.GetSheet()));
                    sheet.Addresses = AddressMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.DAILY:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, DailyMapper.GetSheet()));
                    sheet.Daily = DailyMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.MONTHLY:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, MonthlyMapper.GetSheet()));
                    sheet.Monthly = MonthlyMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.NAMES:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, NameMapper.GetSheet()));
                    sheet.Names = NameMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.PLACES:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, PlaceMapper.GetSheet()));
                    sheet.Places = PlaceMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.REGIONS:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, RegionMapper.GetSheet()));
                    sheet.Regions = RegionMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.SERVICES:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, ServiceMapper.GetSheet()));
                    sheet.Services = ServiceMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.SHIFTS:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, ShiftMapper.GetSheet()));
                    sheet.Shifts = ShiftMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.TRIPS:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, TripMapper.GetSheet()));
                    sheet.Trips = TripMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.TYPES:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, TypeMapper.GetSheet()));
                    sheet.Types = TypeMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.WEEKDAYS:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, WeekdayMapper.GetSheet()));
                    sheet.Weekdays = WeekdayMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.WEEKLY:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, WeeklyMapper.GetSheet()));
                    sheet.Weekly = WeeklyMapper.MapFromRangeData(values);
                    break;
                case GigSheetEnum.YEARLY:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, YearlyMapper.GetSheet()));
                    sheet.Yearly = YearlyMapper.MapFromRangeData(values);
                    break;
            }
        }

        return sheet;
    }
}