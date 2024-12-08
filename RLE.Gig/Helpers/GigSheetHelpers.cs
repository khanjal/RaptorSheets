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

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        var spreadsheetSheets = spreadsheet.Sheets.Select(x => x.Properties.Title.ToUpper()).ToList();
        var sheetData = new List<SheetModel>();

        // Loop through all sheets to see if they exist.
        foreach (var name in Enum.GetNames<SheetEnum>())
        {
            SheetEnum sheetEnum = (SheetEnum)Enum.Parse(typeof(SheetEnum), name);

            if (spreadsheetSheets.Contains(name))
            {
                continue;
            }

            // Get data for each missing sheet.
            switch (sheetEnum)
            {
                case SheetEnum.ADDRESSES:
                    sheetData.Add(AddressMapper.GetSheet());
                    break;
                case SheetEnum.DAILY:
                    sheetData.Add(DailyMapper.GetSheet());
                    break;
                case SheetEnum.MONTHLY:
                    sheetData.Add(MonthlyMapper.GetSheet());
                    break;
                case SheetEnum.NAMES:
                    sheetData.Add(NameMapper.GetSheet());
                    break;
                case SheetEnum.PLACES:
                    sheetData.Add(PlaceMapper.GetSheet());
                    break;
                case SheetEnum.REGIONS:
                    sheetData.Add(RegionMapper.GetSheet());
                    break;
                case SheetEnum.SERVICES:
                    sheetData.Add(ServiceMapper.GetSheet());
                    break;
                case SheetEnum.SHIFTS:
                    sheetData.Add(ShiftMapper.GetSheet());
                    break;
                case SheetEnum.TRIPS:
                    sheetData.Add(TripMapper.GetSheet());
                    break;
                case SheetEnum.TYPES:
                    sheetData.Add(TypeMapper.GetSheet());
                    break;
                case SheetEnum.WEEKDAYS:
                    sheetData.Add(WeekdayMapper.GetSheet());
                    break;
                case SheetEnum.WEEKLY:
                    sheetData.Add(WeeklyMapper.GetSheet());
                    break;
                case SheetEnum.YEARLY:
                    sheetData.Add(YearlyMapper.GetSheet());
                    break;
                default:
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

    public static SheetEntity? MapData(BatchGetValuesByDataFilterResponse response)
    {
        if (response.ValueRanges == null)
        {
            return null;
        }

        var sheet = new SheetEntity();

        // TODO: Figure out a better way to handle looping with message and entity mapping in the switch.
        foreach (var matchedValue in response.ValueRanges)
        {
            var sheetRange = matchedValue.DataFilters[0].A1Range;
            var values = matchedValue.ValueRange.Values;

            Enum.TryParse(sheetRange.ToUpper(), out SheetEnum sheetEnum);

            switch (sheetEnum)
            {
                case SheetEnum.ADDRESSES:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, AddressMapper.GetSheet()));
                    sheet.Addresses = AddressMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.DAILY:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, DailyMapper.GetSheet()));
                    sheet.Daily = DailyMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.MONTHLY:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, MonthlyMapper.GetSheet()));
                    sheet.Monthly = MonthlyMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.NAMES:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, NameMapper.GetSheet()));
                    sheet.Names = NameMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.PLACES:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, PlaceMapper.GetSheet()));
                    sheet.Places = PlaceMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.REGIONS:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, RegionMapper.GetSheet()));
                    sheet.Regions = RegionMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.SERVICES:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, ServiceMapper.GetSheet()));
                    sheet.Services = ServiceMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.SHIFTS:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, ShiftMapper.GetSheet()));
                    sheet.Shifts = ShiftMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.TRIPS:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, TripMapper.GetSheet()));
                    sheet.Trips = TripMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.TYPES:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, TypeMapper.GetSheet()));
                    sheet.Types = TypeMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.WEEKDAYS:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, WeekdayMapper.GetSheet()));
                    sheet.Weekdays = WeekdayMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.WEEKLY:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, WeeklyMapper.GetSheet()));
                    sheet.Weekly = WeeklyMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.YEARLY:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(values, YearlyMapper.GetSheet()));
                    sheet.Yearly = YearlyMapper.MapFromRangeData(values);
                    break;
            }
        }

        return sheet;
    }
}