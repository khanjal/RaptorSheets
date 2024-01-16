using GigRaptorLib.Enums;
using GigRaptorLib.Mappers;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities.Extensions;
using Google.Apis.Sheets.v4.Data;

namespace GigRaptorLib.Utilities
{
    public static class SheetHelper
    {

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
            var sheets = new List<SheetModel>();

            sheets.Add(ShiftMapper.GetSheet());
            sheets.Add(TripMapper.GetSheet());

            return sheets;
        }

        // https://www.rapidtables.com/convert/color/hex-to-rgb.html
        public static Color? GetColor(ColorEnum colorEnum)
        {
            return colorEnum switch
            {
                ColorEnum.BLACK => new Color { Red = 0, Green = 0, Blue = 0 },
                ColorEnum.BLUE => new Color { Red = 0, Green = 0, Blue = 1 },
                ColorEnum.CYAN => new Color { Red = (float?)0.3, Green = (float?)0.8, Blue = (float?)0.9 },
                ColorEnum.DARK_YELLOW => new Color { Red = (float?)0.9686274509803922, Green = (float?)0.796078431372549, Blue = (float?)0.30196078431372547 },
                ColorEnum.GREEN => new Color { Red = 0, Green = (float?)0.5, Blue = 0 },
                ColorEnum.LIGHT_CYAN => new Color { Red = (float?)0.9, Green = (float?)1, Blue = (float?)1 },
                ColorEnum.LIGHT_GRAY => new Color { Red = (float?)0.9058823529411765, Green = (float?)0.9764705882352941, Blue = (float?)0.9372549019607843 },
                ColorEnum.LIGHT_GREEN => new Color { Red = (float?)0.38823529411764707, Green = (float?)0.8235294117647058, Blue = (float?)0.592156862745098 },
                ColorEnum.LIGHT_RED => new Color { Red = (float?)1, Green = (float?)0.9, Blue = (float?)0.85 },
                ColorEnum.LIGHT_YELLOW => new Color { Red = (float?)0.996078431372549, Green = (float?)0.9725490196078431, Blue = (float?)0.8901960784313725 },
                ColorEnum.LIME => new Color { Red = 0, Green = 1, Blue = 0 },
                ColorEnum.ORANGE => new Color { Red = 1, Green = (float?)0.6, Blue = 0 },
                ColorEnum.MAGENTA or ColorEnum.PINK => new Color { Red = 1, Green = 0, Blue = 1 },
                ColorEnum.PURPLE => new Color { Red = (float?)0.5, Green = 0, Blue = (float?)0.5 },
                ColorEnum.RED => new Color { Red = 1, Green = 0, Blue = 0 },
                ColorEnum.WHITE => new Color { Red = 1, Green = 1, Blue = 1 },
                ColorEnum.YELLOW => new Color { Red = 1, Green = 1, Blue = 0 },
                _ => null,
            };
        }

        public static string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

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
                var cell = new CellData();
                cell.UserEnteredFormat = new CellFormat
                {
                    TextFormat = new TextFormat
                    {
                        Bold = true
                    }
                };

                var value = new ExtendedValue();

                if (!string.IsNullOrEmpty(header.Formula))
                {
                    value.FormulaValue = header.Formula;

                    if (!sheet.ProtectSheet)
                    {
                        var border = new Border();
                        border.Style = BorderStyleEnum.SOLID_THICK.ToString();
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
            var cellFormat = new CellFormat();

            switch (format)
            {
                case FormatEnum.ACCOUNTING:
                    cellFormat.NumberFormat = new NumberFormat { Type = "NUMBER", Pattern = "_(\"$\"* #,##0.00_);_(\"$\"* \\(#,##0.00\\);_(\"$\"* \"-\"??_);_(@_)" };
                    break;
                case FormatEnum.DATE:
                    cellFormat.NumberFormat = new NumberFormat { Type = "DATE", Pattern = "yyyy-mm-dd" };
                    break;
                case FormatEnum.DISTANCE:
                    cellFormat.NumberFormat = new NumberFormat { Type = "NUMBER", Pattern = "#,##0.0" };
                    break;
                case FormatEnum.DURATION:
                    cellFormat.NumberFormat = new NumberFormat { Type = "DATE", Pattern = "[h]:mm" };
                    break;
                case FormatEnum.NUMBER:
                    cellFormat.NumberFormat = new NumberFormat { Type = "NUMBER", Pattern = "#,##0" };
                    break;
                case FormatEnum.TEXT:
                    cellFormat.NumberFormat = new NumberFormat { Type = "TEXT" };
                    break;
                case FormatEnum.TIME:
                    cellFormat.NumberFormat = new NumberFormat { Type = "DATE", Pattern = "hh:mm:ss am/pm" };
                    break;
                case FormatEnum.WEEKDAY:
                    cellFormat.NumberFormat = new NumberFormat { Type = "DATE", Pattern = "ddd" };
                    break;
                default:
                    break;
            }

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
                    var values = new List<ConditionValue> { new ConditionValue { UserEnteredValue = $"={GetSheetForRange(validation).DisplayName()}!A2:A" } };
                    dataValidation.Condition = new BooleanCondition { Type = "ONE_OF_RANGE", Values = values };
                    dataValidation.ShowCustomUi = true;
                    dataValidation.Strict = false;
                    break;
            }

            return dataValidation;
        }

        private static SheetEnum GetSheetForRange(ValidationEnum validationEnum)
        {
            return validationEnum switch
            {
                ValidationEnum.RANGE_ADDRESS => SheetEnum.ADDRESSES,
                ValidationEnum.RANGE_NAME => SheetEnum.NAMES,
                ValidationEnum.RANGE_PLACE => SheetEnum.PLACES,
                ValidationEnum.RANGE_REGION => SheetEnum.REGIONS,
                ValidationEnum.RANGE_SERVICE => SheetEnum.SERVICES,
                ValidationEnum.RANGE_TYPE => SheetEnum.TYPES,
                _ => SheetEnum.YEARLY,
            };
        }

        public static List<SheetCellModel> GetCommonShiftGroupSheetHeaders(SheetModel shiftSheet, HeaderEnum keyEnum)
        {
            var sheet = new SheetModel
            {
                Headers = new List<SheetCellModel>()
            };
            var sheetKeyRange = shiftSheet.GetRange(keyEnum);

            switch (keyEnum)
            {
                case HeaderEnum.REGION:
                    // A - [Key]
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.REGION.DisplayName(),
                        Formula = "={\"" + HeaderEnum.REGION.DisplayName() + "\";SORT(UNIQUE({" + TripMapper.GetSheet().GetRange(HeaderEnum.REGION, 2) + ";" + shiftSheet.GetRange(HeaderEnum.REGION, 2) + "}))}"
                    });
                    break;
                case HeaderEnum.SERVICE:
                    // A - [Key]
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.SERVICE.DisplayName(),
                        Formula = "={\"" + HeaderEnum.SERVICE.DisplayName() + "\";SORT(UNIQUE({" + TripMapper.GetSheet().GetRange(HeaderEnum.SERVICE, 2) + ";" + shiftSheet.GetRange(HeaderEnum.SERVICE, 2) + "}))}"
                    });
                    break;
                default:
                    // A - [Key]
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = keyEnum.DisplayName(),
                        Formula = ArrayFormulaHelper.ArrayForumlaUnique(shiftSheet.GetRange(keyEnum, 2), keyEnum.DisplayName())
                    });
                    break;
            }
            var keyRange = sheet.GetLocalRange(keyEnum);
            // B - Trips
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TRIPS.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TRIPS.DisplayName(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TRIPS)),
                Format = FormatEnum.NUMBER
            });
            // C - Pay
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.PAY.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.PAY.DisplayName(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_PAY)),
                Format = FormatEnum.ACCOUNTING
            });
            // D - Tip
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TIPS.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TIPS.DisplayName(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TIPS)),
                Format = FormatEnum.ACCOUNTING
            });
            // E - Bonus
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.BONUS.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.BONUS.DisplayName(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_BONUS)),
                Format = FormatEnum.ACCOUNTING
            });
            // F - Total
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaTotal(keyRange, HeaderEnum.TOTAL.DisplayName(), sheet.GetLocalRange(HeaderEnum.PAY), sheet.GetLocalRange(HeaderEnum.TIPS), sheet.GetLocalRange(HeaderEnum.BONUS)),
                Format = FormatEnum.ACCOUNTING
            });
            // G - Cash
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.CASH.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.CASH.DisplayName(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_CASH)),
                Format = FormatEnum.ACCOUNTING
            });
            // H - Amt/Trip
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_TRIP.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TRIP.DisplayName()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.TRIPS)}=0,1,{sheet.GetLocalRange(HeaderEnum.TRIPS)})))",
                Format = FormatEnum.ACCOUNTING
            });
            // I - Dist
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DISTANCE.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.DISTANCE.DisplayName(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_DISTANCE)),
                Format = FormatEnum.DISTANCE
            });
            // J - Amt/Dist
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.DISTANCE)}=0,1,{sheet.GetLocalRange(HeaderEnum.DISTANCE)})))",
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
                        Name = HeaderEnum.VISIT_FIRST.DisplayName(),
                        Formula = ArrayFormulaHelper.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_FIRST.DisplayName(), SheetEnum.SHIFTS.DisplayName(), shiftSheet.GetColumn(HeaderEnum.DATE), shiftSheet.GetColumn(keyEnum), true),
                        Note = "Format: YYYY-MM-DD",
                        Format = FormatEnum.DATE
                    });
                    // L - Last Visit
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.VISIT_LAST.DisplayName(),
                        Formula = ArrayFormulaHelper.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_LAST.DisplayName(), SheetEnum.SHIFTS.DisplayName(), shiftSheet.GetColumn(HeaderEnum.DATE), shiftSheet.GetColumn(keyEnum), false),
                        Note = "Format: YYYY-MM-DD",
                        Format = FormatEnum.DATE
                    });
                    break;
                case HeaderEnum.DATE:
                    // K - Time
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.TIME_TOTAL.DisplayName(),
                        Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TIME_TOTAL.DisplayName(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TIME)),
                        Format = FormatEnum.DURATION
                    });
                    // L - Amt/Time
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.AMOUNT_PER_TIME.DisplayName(),
                        Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.DisplayName()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}=0,1,{sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}*24)))",
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
                Headers = new List<SheetCellModel>()
            };
            var sheetKeyRange = refSheet.GetRange(keyEnum);
            var keyRange = "A1:A"; // This should be the default but could cause issues if not the first field.

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
                            Name = keyEnum.DisplayName(),
                            Formula = ArrayFormulaHelper.ArrayForumlaUniqueFilterSort(refSheet.GetRange(keyEnum, 2), keyEnum.DisplayName())
                        });
                        keyRange = sheet.GetLocalRange(keyEnum);

                        sheet.Headers.AddColumn(new SheetCellModel
                        {
                            Name = HeaderEnum.WEEKDAY.DisplayName(),
                            Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.WEEKDAY.DisplayName()}\",ISBLANK({keyRange}), \"\", true,TEXT({keyRange}+1,\"ddd\")))",
                        });
                    }
                    else
                    {
                        // A - [Key]
                        sheet.Headers.AddColumn(new SheetCellModel
                        {
                            Name = keyEnum.DisplayName(),
                            Formula = ArrayFormulaHelper.ArrayForumlaUniqueFilter(refSheet.GetRange(keyEnum, 2), keyEnum.DisplayName())
                        });
                        keyRange = sheet.GetLocalRange(keyEnum);
                    }

                    // B - Trips
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.TRIPS.DisplayName(),
                        Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TRIPS.DisplayName(), sheetKeyRange, refSheet.GetRange(HeaderEnum.TRIPS)),
                        Format = FormatEnum.NUMBER
                    });

                    if (keyEnum == HeaderEnum.YEAR)
                    {
                        // C - Days
                        sheet.Headers.AddColumn(new SheetCellModel
                        {
                            Name = HeaderEnum.DAYS.DisplayName(),
                            Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.DAYS.DisplayName(), sheetKeyRange, refSheet.GetRange(HeaderEnum.DAYS)),
                            Format = FormatEnum.NUMBER
                        });
                    }
                    else
                    {
                        // C - Days
                        sheet.Headers.AddColumn(new SheetCellModel
                        {
                            Name = HeaderEnum.DAYS.DisplayName(),
                            Formula = ArrayFormulaHelper.ArrayFormulaCountIf(keyRange, HeaderEnum.DAYS.DisplayName(), sheetKeyRange),
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
                            Name = HeaderEnum.ADDRESS.DisplayName(),
                            Formula = "={\"" + HeaderEnum.ADDRESS.DisplayName() + "\";SORT(UNIQUE({" + refSheet.GetRange(HeaderEnum.ADDRESS_END, 2) + ";" + refSheet.GetRange(HeaderEnum.ADDRESS_START, 2) + "}))}"
                        });

                        // B - Trips
                        sheet.Headers.AddColumn(new SheetCellModel
                        {
                            Name = HeaderEnum.TRIPS.DisplayName(),
                            Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.TRIPS.DisplayName()}\",ISBLANK({keyRange}), \"\",true,COUNTIF({refSheet.GetRange(HeaderEnum.ADDRESS_END, 2)},{keyRange})+COUNTIF({refSheet.GetRange(HeaderEnum.ADDRESS_START, 2)},{keyRange})))",
                            Format = FormatEnum.NUMBER
                        });
                    }
                    else
                    {
                        // A - [Key]
                        sheet.Headers.AddColumn(new SheetCellModel
                        {
                            Name = keyEnum.DisplayName(),
                            Formula = ArrayFormulaHelper.ArrayForumlaUnique(refSheet.GetRange(keyEnum, 2), keyEnum.DisplayName())
                        });
                        keyRange = sheet.GetLocalRange(keyEnum);
                        // B - Trips
                        sheet.Headers.AddColumn(new SheetCellModel
                        {
                            Name = HeaderEnum.TRIPS.DisplayName(),
                            Formula = ArrayFormulaHelper.ArrayFormulaCountIf(keyRange, HeaderEnum.TRIPS.DisplayName(), sheetKeyRange),
                            Format = FormatEnum.NUMBER
                        });
                    }
                    break;
            }

            // C - Pay
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.PAY.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.PAY.DisplayName(), sheetKeyRange, refSheet.GetRange(HeaderEnum.PAY)),
                Format = FormatEnum.ACCOUNTING
            });
            // D - Tip
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TIPS.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TIPS.DisplayName(), sheetKeyRange, refSheet.GetRange(HeaderEnum.TIPS)),
                Format = FormatEnum.ACCOUNTING
            });
            // E - Bonus
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.BONUS.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.BONUS.DisplayName(), sheetKeyRange, refSheet.GetRange(HeaderEnum.BONUS)),
                Format = FormatEnum.ACCOUNTING
            });
            // F - Total
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaTotal(keyRange, HeaderEnum.TOTAL.DisplayName(), sheet.GetLocalRange(HeaderEnum.PAY), sheet.GetLocalRange(HeaderEnum.TIPS), sheet.GetLocalRange(HeaderEnum.BONUS)),
                Format = FormatEnum.ACCOUNTING
            });
            // G - Cash
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.CASH.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.CASH.DisplayName(), sheetKeyRange, refSheet.GetRange(HeaderEnum.CASH)),
                Format = FormatEnum.ACCOUNTING
            });
            // H - Amt/Trip
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_TRIP.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TRIP.DisplayName()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.TRIPS)}=0,1,{sheet.GetLocalRange(HeaderEnum.TRIPS)})))",
                Format = FormatEnum.ACCOUNTING
            });
            // I - Dist
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DISTANCE.DisplayName(),
                Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.DISTANCE.DisplayName(), sheetKeyRange, refSheet.GetRange(HeaderEnum.DISTANCE)),
                Format = FormatEnum.DISTANCE
            });
            // J - Amt/Dist
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.DISTANCE)}=0,1,{sheet.GetLocalRange(HeaderEnum.DISTANCE)})))",
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
                        Name = HeaderEnum.VISIT_FIRST.DisplayName(),
                        Formula = ArrayFormulaHelper.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_FIRST.DisplayName(), SheetEnum.TRIPS.DisplayName(), refSheet.GetColumn(HeaderEnum.DATE), refSheet.GetColumn(keyEnum), true),
                        Format = FormatEnum.DATE
                    });
                    // L - Last Visit
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.VISIT_LAST.DisplayName(),
                        Formula = ArrayFormulaHelper.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_LAST.DisplayName(), SheetEnum.TRIPS.DisplayName(), refSheet.GetColumn(HeaderEnum.DATE), refSheet.GetColumn(keyEnum), false),
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
                        Name = HeaderEnum.TIME_TOTAL.DisplayName(),
                        Formula = ArrayFormulaHelper.ArrayFormulaSumIf(keyRange, HeaderEnum.TIME_TOTAL.DisplayName(), sheetKeyRange, refSheet.GetRange(HeaderEnum.TIME_TOTAL)),
                        Format = FormatEnum.DURATION
                    });
                    // Amt/Time
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.AMOUNT_PER_TIME.DisplayName(),
                        Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.DisplayName()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}=0,1,{sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}*24)))",
                        Format = FormatEnum.ACCOUNTING
                    });

                    // Amt/Day
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.AMOUNT_PER_DAY.DisplayName(),
                        Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_DAY.DisplayName()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.DAYS)}=0,1,{sheet.GetLocalRange(HeaderEnum.DAYS)})))",
                        Format = FormatEnum.ACCOUNTING
                    });

                    if (keyEnum != HeaderEnum.DAY)
                    {
                        // Average
                        sheet.Headers.AddColumn(new SheetCellModel
                        {
                            Name = HeaderEnum.AVERAGE.DisplayName(),
                            Formula = "=ARRAYFORMULA(IFS(ROW(" + keyRange + ")=1,\"" + HeaderEnum.AVERAGE.DisplayName() + "\",ISBLANK(" + keyRange + "), \"\",true, DAVERAGE(transpose({" + sheet.GetLocalRange(HeaderEnum.TOTAL) + ",TRANSPOSE(if(ROW(" + sheet.GetLocalRange(HeaderEnum.TOTAL) + ") <= TRANSPOSE(ROW(" + sheet.GetLocalRange(HeaderEnum.TOTAL) + "))," + sheet.GetLocalRange(HeaderEnum.TOTAL) + ",))}),sequence(rows(" + sheet.GetLocalRange(HeaderEnum.TOTAL) + "),1),{if(,,);if(,,)})))",
                            Format = FormatEnum.ACCOUNTING
                        });
                    }

                    break;
            }

            return sheet.Headers;
        }
    }
}