using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Core.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Entities;
using HeaderEnum = RaptorSheets.Gig.Enums.HeaderEnum;
using SheetEnum = RaptorSheets.Gig.Enums.SheetEnum;
using RaptorSheets.Common.Mappers;

namespace RaptorSheets.Gig.Helpers;

public static class GigSheetHelpers
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

    public static List<string> GetSheetNames()
    {
        var sheetNames = Enum.GetNames<SheetEnum>().ToList();
        sheetNames.AddRange(Enum.GetNames<Common.Enums.SheetEnum>());

        return sheetNames;
    }

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        var spreadsheetSheets = spreadsheet.Sheets.Select(x => x.Properties.Title.ToUpper()).ToList();
        var sheetData = new List<SheetModel>();

        var sheetNames = GetSheetNames();

        // Loop through all sheets to see if they exist.
        foreach (var name in sheetNames)
        {
            if (spreadsheetSheets.Contains(name))
            {
                continue;
            }

            switch (name.ToUpper())
            {
                case nameof(SheetEnum.ADDRESSES):
                    sheetData.Add(AddressMapper.GetSheet());
                    break;
                case nameof(SheetEnum.DAILY):
                    sheetData.Add(DailyMapper.GetSheet());
                    break;
                case nameof(SheetEnum.MONTHLY):
                    sheetData.Add(MonthlyMapper.GetSheet());
                    break;
                case nameof(SheetEnum.NAMES):
                    sheetData.Add(NameMapper.GetSheet());
                    break;
                case nameof(SheetEnum.PLACES):
                    sheetData.Add(PlaceMapper.GetSheet());
                    break;
                case nameof(SheetEnum.REGIONS):
                    sheetData.Add(RegionMapper.GetSheet());
                    break;
                case nameof(Common.Enums.SheetEnum.SETUP):
                    sheetData.Add(SetupMapper.GetSheet());
                    break;
                case nameof(SheetEnum.SERVICES):
                    sheetData.Add(ServiceMapper.GetSheet());
                    break;
                case nameof(SheetEnum.SHIFTS):
                    sheetData.Add(ShiftMapper.GetSheet());
                    break;
                case nameof(SheetEnum.TRIPS):
                    sheetData.Add(TripMapper.GetSheet());
                    break;
                case nameof(SheetEnum.TYPES):
                    sheetData.Add(TypeMapper.GetSheet());
                    break;
                case nameof(SheetEnum.WEEKDAYS):
                    sheetData.Add(WeekdayMapper.GetSheet());
                    break;
                case nameof(SheetEnum.WEEKLY):
                    sheetData.Add(WeeklyMapper.GetSheet());
                    break;
                case nameof(SheetEnum.YEARLY):
                    sheetData.Add(YearlyMapper.GetSheet());
                    break;
            }
        }

        return sheetData;
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

    private static SheetEnum? GetSheetForRange(ValidationEnum validationEnum)
    {
        return validationEnum switch
        {
            ValidationEnum.RANGE_ADDRESS => SheetEnum.ADDRESSES,
            ValidationEnum.RANGE_NAME => SheetEnum.NAMES,
            ValidationEnum.RANGE_PLACE => SheetEnum.PLACES,
            ValidationEnum.RANGE_REGION => SheetEnum.REGIONS,
            ValidationEnum.RANGE_SERVICE => SheetEnum.SERVICES,
            ValidationEnum.RANGE_TYPE => SheetEnum.TYPES,
            _ => null
        };
    }

    public static List<SheetCellModel> GetCommonShiftGroupSheetHeaders(SheetModel shiftSheet, HeaderEnum keyEnum)
    {
        var sheet = new SheetModel
        {
            Headers = []
        };
        var sheetKeyRange = shiftSheet.GetRange(keyEnum.GetDescription());

        switch (keyEnum)
        {
            case HeaderEnum.REGION:
                // A - [Key]
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.REGION.GetDescription(),
                    Formula = "={\"" + HeaderEnum.REGION.GetDescription() + "\";SORT(UNIQUE({" + TripMapper.GetSheet().GetRange(HeaderEnum.REGION.GetDescription(), 2) + ";" + shiftSheet.GetRange(HeaderEnum.REGION.GetDescription(), 2) + "}))}"
                });
                break;
            case HeaderEnum.SERVICE:
                // A - [Key]
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.SERVICE.GetDescription(),
                    Formula = "={\"" + HeaderEnum.SERVICE.GetDescription() + "\";SORT(UNIQUE({" + TripMapper.GetSheet().GetRange(HeaderEnum.SERVICE.GetDescription(), 2) + ";" + shiftSheet.GetRange(HeaderEnum.SERVICE.GetDescription(), 2) + "}))}"
                });
                break;
            default:
                // A - [Key]
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = keyEnum.GetDescription(),
                    Formula = ArrayFormulaHelpers.ArrayForumlaUnique(shiftSheet.GetRange(keyEnum.GetDescription(), 2), keyEnum.GetDescription())
                });
                break;
        }
        var keyRange = sheet.GetLocalRange(keyEnum.GetDescription());
        // B - Trips
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TRIPS.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.TRIPS.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TRIPS.GetDescription())),
            Format = FormatEnum.NUMBER
        });
        // C - Pay
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.PAY.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.PAY.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_PAY.GetDescription())),
            Format = FormatEnum.ACCOUNTING
        });
        // D - Tip
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TIPS.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.TIPS.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TIPS.GetDescription())),
            Format = FormatEnum.ACCOUNTING
        });
        // E - Bonus
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.BONUS.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.BONUS.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_BONUS.GetDescription())),
            Format = FormatEnum.ACCOUNTING
        });
        // F - Total
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaTotal(keyRange, HeaderEnum.TOTAL.GetDescription(), sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()), sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription())),
            Format = FormatEnum.ACCOUNTING
        });
        // G - Cash
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.CASH.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.CASH.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_CASH.GetDescription())),
            Format = FormatEnum.ACCOUNTING
        });
        // H - Amt/Trip
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_TRIP.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TRIP.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription())})))",
            Format = FormatEnum.ACCOUNTING
        });
        // I - Dist
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.DISTANCE.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.DISTANCE.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_DISTANCE.GetDescription())),
            Format = FormatEnum.DISTANCE
        });
        // J - Amt/Dist
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription())})))",
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
                    Formula = ArrayFormulaHelpers.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), SheetEnum.SHIFTS.GetDescription(), shiftSheet.GetColumn(HeaderEnum.DATE.GetDescription()), shiftSheet.GetColumn(keyEnum.GetDescription()), true),
                    Note = ColumnNotes.DateFormat,
                    Format = FormatEnum.DATE
                });
                // L - Last Visit
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.VISIT_LAST.GetDescription(),
                    Formula = ArrayFormulaHelpers.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), SheetEnum.SHIFTS.GetDescription(), shiftSheet.GetColumn(HeaderEnum.DATE.GetDescription()), shiftSheet.GetColumn(keyEnum.GetDescription()), false),
                    Note = ColumnNotes.DateFormat,
                    Format = FormatEnum.DATE
                });
                break;
            case HeaderEnum.DATE:
                // K - Time
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.TIME_TOTAL.GetDescription(),
                    Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.TIME_TOTAL.GetDescription(), sheetKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TIME.GetDescription())),
                    Format = FormatEnum.DURATION
                });
                // L - Amt/Time
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription(),
                    Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription())}*24)))",
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
        var sheetKeyRange = refSheet.GetRange(keyEnum.GetDescription());
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
                        Formula = ArrayFormulaHelpers.ArrayForumlaUniqueFilterSort(refSheet.GetRange(keyEnum.GetDescription(), 2), keyEnum.GetDescription())
                    });
                    keyRange = sheet.GetLocalRange(keyEnum.GetDescription());

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
                        Formula = ArrayFormulaHelpers.ArrayForumlaUniqueFilter(refSheet.GetRange(keyEnum.GetDescription(), 2), keyEnum.GetDescription())
                    });
                    keyRange = sheet.GetLocalRange(keyEnum.GetDescription());
                }

                // B - Trips
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.TRIPS.GetDescription(),
                    Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.TRIPS.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.TRIPS.GetDescription())),
                    Format = FormatEnum.NUMBER
                });

                if (keyEnum == HeaderEnum.YEAR)
                {
                    // C - Days
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.DAYS.GetDescription(),
                        Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.DAYS.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.DAYS.GetDescription())),
                        Format = FormatEnum.NUMBER
                    });
                }
                else
                {
                    // C - Days
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.DAYS.GetDescription(),
                        Formula = ArrayFormulaHelpers.ArrayFormulaCountIf(keyRange, HeaderEnum.DAYS.GetDescription(), sheetKeyRange),
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
                        Formula = "={\"" + HeaderEnum.ADDRESS.GetDescription() + "\";SORT(UNIQUE({" + refSheet.GetRange(HeaderEnum.ADDRESS_END.GetDescription(), 2) + ";" + refSheet.GetRange(HeaderEnum.ADDRESS_START.GetDescription(), 2) + "}))}"
                    });

                    // B - Trips
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.TRIPS.GetDescription(),
                        Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.TRIPS.GetDescription()}\",ISBLANK({keyRange}), \"\",true,COUNTIF({refSheet.GetRange(HeaderEnum.ADDRESS_END.GetDescription(), 2)},{keyRange})+COUNTIF({refSheet.GetRange(HeaderEnum.ADDRESS_START.GetDescription(), 2)},{keyRange})))",
                        Format = FormatEnum.NUMBER
                    });
                }
                else
                {
                    // A - [Key]
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = keyEnum.GetDescription(),
                        Formula = ArrayFormulaHelpers.ArrayForumlaUnique(refSheet.GetRange(keyEnum.GetDescription(), 2), keyEnum.GetDescription())
                    });
                    keyRange = sheet.GetLocalRange(keyEnum.GetDescription());
                    // B - Trips
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.TRIPS.GetDescription(),
                        Formula = ArrayFormulaHelpers.ArrayFormulaCountIf(keyRange, HeaderEnum.TRIPS.GetDescription(), sheetKeyRange),
                        Format = FormatEnum.NUMBER
                    });
                }
                break;
        }

        // C - Pay
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.PAY.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.PAY.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.PAY.GetDescription())),
            Format = FormatEnum.ACCOUNTING
        });
        // D - Tip
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TIPS.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.TIPS.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.TIPS.GetDescription())),
            Format = FormatEnum.ACCOUNTING
        });
        // E - Bonus
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.BONUS.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.BONUS.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.BONUS.GetDescription())),
            Format = FormatEnum.ACCOUNTING
        });
        // F - Total
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaTotal(keyRange, HeaderEnum.TOTAL.GetDescription(), sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()), sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription())),
            Format = FormatEnum.ACCOUNTING
        });
        // G - Cash
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.CASH.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.CASH.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.CASH.GetDescription())),
            Format = FormatEnum.ACCOUNTING
        });
        // H - Amt/Trip
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_TRIP.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TRIP.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription())})))",
            Format = FormatEnum.ACCOUNTING
        });
        // I - Dist
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.DISTANCE.GetDescription(),
            Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.DISTANCE.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.DISTANCE.GetDescription())),
            Format = FormatEnum.DISTANCE
        });
        // J - Amt/Dist
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription())})))",
            Format = FormatEnum.ACCOUNTING
        });

        switch (keyEnum)
        {
            case HeaderEnum.ADDRESS_END:
                // K - First Visit
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.VISIT_FIRST.GetDescription(),
                    Formula = ArrayFormulaHelpers.ArrayFormulaMultipleVisit(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), SheetEnum.TRIPS.GetDescription(), refSheet.GetColumn(HeaderEnum.DATE.GetDescription()), refSheet.GetColumn(HeaderEnum.ADDRESS_START.GetDescription()), refSheet.GetColumn(keyEnum.GetDescription()), true),
                    Format = FormatEnum.DATE
                });
                // L - Last Visit
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.VISIT_LAST.GetDescription(),
                    Formula = ArrayFormulaHelpers.ArrayFormulaMultipleVisit(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), SheetEnum.TRIPS.GetDescription(), refSheet.GetColumn(HeaderEnum.DATE.GetDescription()), refSheet.GetColumn(HeaderEnum.ADDRESS_START.GetDescription()), refSheet.GetColumn(keyEnum.GetDescription()), false),
                    Format = FormatEnum.DATE
                });
                break;
            case HeaderEnum.NAME:
            case HeaderEnum.PLACE:
            case HeaderEnum.REGION:
            case HeaderEnum.SERVICE:
            case HeaderEnum.TYPE:
                // K - First Visit
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.VISIT_FIRST.GetDescription(),
                    Formula = ArrayFormulaHelpers.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), SheetEnum.TRIPS.GetDescription(), refSheet.GetColumn(HeaderEnum.DATE.GetDescription()), refSheet.GetColumn(keyEnum.GetDescription()), true),
                    Format = FormatEnum.DATE
                });
                // L - Last Visit
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.VISIT_LAST.GetDescription(),
                    Formula = ArrayFormulaHelpers.ArrayFormulaVisit(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), SheetEnum.TRIPS.GetDescription(), refSheet.GetColumn(HeaderEnum.DATE.GetDescription()), refSheet.GetColumn(keyEnum.GetDescription()), false),
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
                    Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, HeaderEnum.TIME_TOTAL.GetDescription(), sheetKeyRange, refSheet.GetRange(HeaderEnum.TIME_TOTAL.GetDescription())),
                    Format = FormatEnum.DURATION
                });
                // Amt/Time
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription(),
                    Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription())}*24)))",
                    Format = FormatEnum.ACCOUNTING
                });

                // Amt/Day
                sheet.Headers.AddColumn(new SheetCellModel
                {
                    Name = HeaderEnum.AMOUNT_PER_DAY.GetDescription(),
                    Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_DAY.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.DAYS.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.DAYS.GetDescription())})))",
                    Format = FormatEnum.ACCOUNTING
                });

                if (keyEnum != HeaderEnum.DAY)
                {
                    // Average
                    sheet.Headers.AddColumn(new SheetCellModel
                    {
                        Name = HeaderEnum.AVERAGE.GetDescription(),
                        Formula = "=ARRAYFORMULA(IFS(ROW(" + keyRange + ")=1,\"" + HeaderEnum.AVERAGE.GetDescription() + "\",ISBLANK(" + keyRange + "), \"\",true, DAVERAGE(transpose({" + sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()) + ",TRANSPOSE(if(ROW(" + sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()) + ") <= TRANSPOSE(ROW(" + sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()) + "))," + sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()) + ",))}),sequence(rows(" + sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()) + "),1),{if(,,);if(,,)})))",
                        Format = FormatEnum.ACCOUNTING
                    });
                }

                break;
        }

        return sheet.Headers;
    }

    public static SheetEntity? MapData(Spreadsheet spreadsheet)
    {
        var sheetEntity = new SheetEntity
        {
            Properties = new PropertyEntity
            {
                Name = spreadsheet.Properties.Title,
            }
        };

        var sheetValues = SheetHelpers.GetSheetValues(spreadsheet);
        foreach (var sheet in spreadsheet.Sheets)
        {
            var values = sheetValues[sheet.Properties.Title];
            ProcessSheetData(sheetEntity, sheet.Properties.Title, values);
        }

        return sheetEntity;
    }

    public static SheetEntity? MapData(BatchGetValuesByDataFilterResponse response)
    {
        if (response.ValueRanges == null)
        {
            return null;
        }

        var sheetEntity = new SheetEntity();

        foreach (var matchedValue in response.ValueRanges)
        {
            var sheetRange = matchedValue.DataFilters[0].A1Range;
            var values = matchedValue.ValueRange.Values;
            ProcessSheetData(sheetEntity, sheetRange, values);
        }

        return sheetEntity;
    }

    private static void ProcessSheetData(SheetEntity sheetEntity, string sheetName, IList<IList<object>> values)
    {
        if (values == null || values.Count == 0)
        {
            return;
        }

        var headerValues = values.First();

        switch (sheetName.ToUpper())
        {
            case nameof(SheetEnum.ADDRESSES):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, AddressMapper.GetSheet()));
                sheetEntity.Addresses = AddressMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.DAILY):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, DailyMapper.GetSheet()));
                sheetEntity.Daily = DailyMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.MONTHLY):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, MonthlyMapper.GetSheet()));
                sheetEntity.Monthly = MonthlyMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.NAMES):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, NameMapper.GetSheet()));
                sheetEntity.Names = NameMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.PLACES):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, PlaceMapper.GetSheet()));
                sheetEntity.Places = PlaceMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.REGIONS):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, RegionMapper.GetSheet()));
                sheetEntity.Regions = RegionMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.SERVICES):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, ServiceMapper.GetSheet()));
                sheetEntity.Services = ServiceMapper.MapFromRangeData(values);
                break;
            case nameof(Common.Enums.SheetEnum.SETUP):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, SetupMapper.GetSheet()));
                sheetEntity.Setup = SetupMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.SHIFTS):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, ShiftMapper.GetSheet()));
                sheetEntity.Shifts = ShiftMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.TRIPS):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, TripMapper.GetSheet()));
                sheetEntity.Trips = TripMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.TYPES):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, TypeMapper.GetSheet()));
                sheetEntity.Types = TypeMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.WEEKDAYS):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, WeekdayMapper.GetSheet()));
                sheetEntity.Weekdays = WeekdayMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.WEEKLY):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, WeeklyMapper.GetSheet()));
                sheetEntity.Weekly = WeeklyMapper.MapFromRangeData(values);
                break;
            case nameof(SheetEnum.YEARLY):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, YearlyMapper.GetSheet()));
                sheetEntity.Yearly = YearlyMapper.MapFromRangeData(values);
                break;
        }
    }
}