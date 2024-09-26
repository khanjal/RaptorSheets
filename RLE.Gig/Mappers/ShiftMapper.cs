using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities;
using RLE.Core.Utilities.Extensions;
using RLE.Gig.Constants;
using RLE.Gig.Entities;
using RLE.Gig.Enums;

namespace RLE.Gig.Mappers
{
    public static class ShiftMapper
    {
        public static List<ShiftEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var shifts = new List<ShiftEntity>();
            var headers = new Dictionary<int, string>();
            var id = 0;

            foreach (var value in values)
            {
                id++;
                if (id == 1)
                {
                    headers = HeaderHelper.ParserHeader(value);
                    continue;
                }

                if (value[0].ToString() == "")
                {
                    continue;
                }

                ShiftEntity shift = new()
                {
                    Id = id,
                    Key = HeaderHelper.GetStringValue(HeaderEnum.KEY.GetDescription(), value, headers),
                    Date = HeaderHelper.GetStringValue(HeaderEnum.DATE.GetDescription(), value, headers),
                    Start = HeaderHelper.GetStringValue(HeaderEnum.TIME_START.GetDescription(), value, headers),
                    Finish = HeaderHelper.GetStringValue(HeaderEnum.TIME_END.GetDescription(), value, headers),
                    Service = HeaderHelper.GetStringValue(HeaderEnum.SERVICE.GetDescription(), value, headers),
                    Number = HeaderHelper.GetIntValue(HeaderEnum.NUMBER.GetDescription(), value, headers),
                    Active = HeaderHelper.GetStringValue(HeaderEnum.TIME_ACTIVE.GetDescription(), value, headers),
                    Time = HeaderHelper.GetStringValue(HeaderEnum.TIME_TOTAL.GetDescription(), value, headers),
                    Trips = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                    Distance = HeaderHelper.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                    Omit = HeaderHelper.GetBoolValue(HeaderEnum.TIME_OMIT.GetDescription(), value, headers),
                    Region = HeaderHelper.GetStringValue(HeaderEnum.REGION.GetDescription(), value, headers),
                    Note = HeaderHelper.GetStringValue(HeaderEnum.NOTE.GetDescription(), value, headers),
                    Pay = HeaderHelper.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                    Tip = HeaderHelper.GetDecimalValue(HeaderEnum.TIPS.GetDescription(), value, headers),
                    Bonus = HeaderHelper.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                    Total = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                    Cash = HeaderHelper.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                    TotalTrips = HeaderHelper.GetIntValue(HeaderEnum.TOTAL_TRIPS.GetDescription(), value, headers),
                    TotalDistance = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL_DISTANCE.GetDescription(), value, headers),
                    TotalPay = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL_PAY.GetDescription(), value, headers),
                    TotalTips = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL_TIPS.GetDescription(), value, headers),
                    TotalBonus = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL_BONUS.GetDescription(), value, headers),
                    GrandTotal = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL_GRAND.GetDescription(), value, headers),
                    TotalCash = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL_CASH.GetDescription(), value, headers),
                    AmountPerTime = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_TIME.GetDescription(), value, headers),
                    AmountPerDistance = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), value, headers),
                    AmountPerTrip = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), value, headers),
                    Saved = true
                };

                shifts.Add(shift);
            }
            return shifts;
        }
        public static IList<IList<object?>> MapToRangeData(List<ShiftEntity> shifts, IList<object> shiftHeaders)
        {
            var rangeData = new List<IList<object?>>();

            foreach (var shift in shifts)
            {
                var objectList = new List<object?>();

                foreach (var header in shiftHeaders)
                {
                    var headerEnum = header!.ToString()!.Trim().GetValueFromName<HeaderEnum>();
                    // Console.WriteLine($"Header: {headerEnum}");

                    switch (headerEnum)
                    {
                        case HeaderEnum.DATE:
                            objectList.Add(shift.Date);
                            break;
                        case HeaderEnum.TIME_START:
                            objectList.Add(shift.Start);
                            break;
                        case HeaderEnum.TIME_END:
                            objectList.Add(shift.Finish);
                            break;
                        case HeaderEnum.SERVICE:
                            objectList.Add(shift.Service);
                            break;
                        case HeaderEnum.NUMBER:
                            objectList.Add(shift.Number);
                            break;
                        case HeaderEnum.TIME_ACTIVE:
                            objectList.Add(shift.Active);
                            break;
                        case HeaderEnum.TIME_TOTAL:
                            objectList.Add(shift.Time);
                            break;
                        case HeaderEnum.TIME_OMIT:
                            objectList.Add(shift.Omit);
                            break;
                        case HeaderEnum.PAY:
                            objectList.Add(shift.Pay);
                            break;
                        case HeaderEnum.TIPS:
                            objectList.Add(shift.Tip);
                            break;
                        case HeaderEnum.BONUS:
                            objectList.Add(shift.Bonus);
                            break;
                        case HeaderEnum.CASH:
                            objectList.Add(shift.Cash);
                            break;
                        case HeaderEnum.REGION:
                            objectList.Add(shift.Region);
                            break;
                        case HeaderEnum.NOTE:
                            objectList.Add(shift.Note);
                            break;
                        default:
                            objectList.Add(null);
                            break;
                    }
                }

                // Console.WriteLine("Map Shift");
                // Console.WriteLine(JsonSerializer.Serialize(objectList));

                rangeData.Add(objectList);
            }

            return rangeData;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.ShiftSheet;

            var tripSheet = TripMapper.GetSheet();
            var sheetTripsName = GigSheetEnum.TRIPS.GetDescription();
            var sheetTripsTypeRange = tripSheet.Headers.First(x => x.Name == HeaderEnum.TYPE.GetDescription()).Range;

            sheet.Headers = [];

            // Date
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DATE.GetDescription(),
                Note = ColumnNotes.DateFormat,
                Format = FormatEnum.DATE
            });
            var dateRange = sheet.GetLocalRange(HeaderEnum.DATE);
            // Start Time        
            sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.TIME_START.GetDescription() });
            // End Time
            sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.TIME_END.GetDescription() });
            // Service
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.SERVICE.GetDescription(),
                Validation = ValidationEnum.RANGE_SERVICE
            });
            // #
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.NUMBER.GetDescription(),
                Note = ColumnNotes.ShiftNumber
            });
            // Active Time
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TIME_ACTIVE.GetDescription(),
                Note = ColumnNotes.ActiveTime,
                Format = FormatEnum.DURATION
            });
            // Total Time
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TIME_TOTAL.GetDescription(),
                Note = ColumnNotes.TotalTime,
                Format = FormatEnum.DURATION
            });
            // Omit
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TIME_OMIT.GetDescription(),
                Note = ColumnNotes.TimeOmit,
                Validation = ValidationEnum.BOOLEAN
            });
            // Trips
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TRIPS.GetDescription(),
                Note = ColumnNotes.ShiftTrips
            });
            // Pay
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.PAY.GetDescription(),
                Format = FormatEnum.ACCOUNTING
            });
            // Tips
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TIPS.GetDescription(),
                Format = FormatEnum.ACCOUNTING
            });
            // Bonus
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.BONUS.GetDescription(),
                Format = FormatEnum.ACCOUNTING
            });
            // Cash
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.CASH.GetDescription(),
                Format = FormatEnum.ACCOUNTING
            });
            // Distance
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DISTANCE.GetDescription(),
                Format = FormatEnum.DISTANCE,
                Note = ColumnNotes.ShiftDistance
            });
            // Region
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.REGION.GetDescription(),
                Validation = ValidationEnum.RANGE_REGION
            });
            // Note
            sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.NOTE.GetDescription() });
            // Key
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.KEY.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.KEY.GetDescription()}\",ISBLANK({sheet.GetLocalRange(HeaderEnum.SERVICE)}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.NUMBER)}), {dateRange} & \"-0-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE)}, {dateRange} & \"-\" & {sheet.GetLocalRange(HeaderEnum.NUMBER)} & \"-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE)})))",
                Note = ColumnNotes.ShiftKey
            });

            var keyRange = sheet.GetLocalRange(HeaderEnum.KEY);

            // T Active
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TIME_ACTIVE)}),SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.DURATION)}),{sheet.GetLocalRange(HeaderEnum.TIME_ACTIVE)})))",
                Note = ColumnNotes.TotalTimeActive,
                Format = FormatEnum.DURATION
            });
            // T Time
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_TIME.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIME.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF({sheet.GetLocalRange(HeaderEnum.TIME_OMIT)}=false,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}),{sheet.GetLocalRange(HeaderEnum.TOTAL_TIME_ACTIVE)},{sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}),0)))",
                Format = FormatEnum.DURATION
            });
            // T Trips
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_TRIPS.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TRIPS.GetDescription()}\",ISBLANK({dateRange}), \"\",true, {sheet.GetLocalRange(HeaderEnum.TRIPS)} + COUNTIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange})))",
                Note = ColumnNotes.TotalTrips,
                Format = FormatEnum.NUMBER
            });
            // T Pay
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_PAY.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_PAY.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.PAY)} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.PAY)})))",
                Format = FormatEnum.ACCOUNTING
            });
            // T Tips
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_TIPS.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIPS.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.TIPS)} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.TIPS)})))",
                Format = FormatEnum.ACCOUNTING
            });
            // T Bonus
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_BONUS.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_BONUS.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.BONUS)} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.BONUS)})))",
                Format = FormatEnum.ACCOUNTING
            });
            // G Total
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_GRAND.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_GRAND.GetDescription()}\",ISBLANK({dateRange}), \"\",true, {sheet.GetLocalRange(HeaderEnum.TOTAL_PAY)}+{sheet.GetLocalRange(HeaderEnum.TOTAL_TIPS)}+{sheet.GetLocalRange(HeaderEnum.TOTAL_BONUS)}))",
                Format = FormatEnum.ACCOUNTING
            });
            // T Cash
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_CASH.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_CASH.GetDescription()}\",ISBLANK({dateRange}), \"\",true,SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.CASH)})))",
                Format = FormatEnum.ACCOUNTING
            });
            // Amt/Trip
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_TRIP.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_TRIP.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS)}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND)}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS)}=0,1,{sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS)}))))",
                Format = FormatEnum.ACCOUNTING
            });
            // Amt/Time
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.GetDescription()}\",ISBLANK({dateRange}), \"\", true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND)}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}=0,1,({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}*24)))))",
                Format = FormatEnum.ACCOUNTING
            });
            // T Dist
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_DISTANCE.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_DISTANCE.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.DISTANCE)} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.DISTANCE)})))",
                Note = ColumnNotes.TotalDistance,
                Format = FormatEnum.DISTANCE
            });
            // Amt/Dist
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND)}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND)}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_DISTANCE)}=0,1,{sheet.GetLocalRange(HeaderEnum.TOTAL_DISTANCE)}))))",
                Format = FormatEnum.ACCOUNTING
            });
            // Trips/Hour
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TRIPS_PER_HOUR.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TRIPS_PER_HOUR.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}), \"\", ({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS)}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}=0,1,({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}*24))))))",
                Format = FormatEnum.DISTANCE
            });
            // Day
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DAY.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.DAY.GetDescription()}\",ISBLANK({dateRange}), \"\",true,DAY({dateRange})))"
            });
            // Month
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.MONTH.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.MONTH.GetDescription()}\",ISBLANK({dateRange}), \"\",true,MONTH({dateRange})))"
            });
            // Year
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.YEAR.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.YEAR.GetDescription()}\",ISBLANK({dateRange}), \"\",true,YEAR({dateRange})))"
            });

            return sheet;
        }
    }
}