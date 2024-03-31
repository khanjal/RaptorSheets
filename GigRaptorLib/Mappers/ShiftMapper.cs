using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Mappers
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
                    headers = HeaderParser.ParserHeader(value);
                    continue;
                }

                if (value[0].ToString() == "")
                {
                    continue;
                }

                ShiftEntity shift = new()
                {
                    Id = id,
                    Key = HeaderParser.GetStringValue(HeaderEnum.KEY.DisplayName(), value, headers),
                    Date = HeaderParser.GetStringValue(HeaderEnum.DATE.DisplayName(), value, headers),
                    Start = HeaderParser.GetStringValue(HeaderEnum.TIME_START.DisplayName(), value, headers),
                    Finish = HeaderParser.GetStringValue(HeaderEnum.TIME_END.DisplayName(), value, headers),
                    Service = HeaderParser.GetStringValue(HeaderEnum.SERVICE.DisplayName(), value, headers),
                    Number = HeaderParser.GetIntValue(HeaderEnum.NUMBER.DisplayName(), value, headers),
                    Active = HeaderParser.GetStringValue(HeaderEnum.TIME_ACTIVE.DisplayName(), value, headers),
                    Time = HeaderParser.GetStringValue(HeaderEnum.TIME_TOTAL.DisplayName(), value, headers),
                    Trips = HeaderParser.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Distance = HeaderParser.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
                    Omit = HeaderParser.GetBoolValue(HeaderEnum.TIME_OMIT.DisplayName(), value, headers),
                    Region = HeaderParser.GetStringValue(HeaderEnum.REGION.DisplayName(), value, headers),
                    Note = HeaderParser.GetStringValue(HeaderEnum.NOTE.DisplayName(), value, headers),
                    Pay = HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderParser.GetDecimalValue(HeaderEnum.TIPS.DisplayName(), value, headers),
                    Bonus = HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    TotalTrips = HeaderParser.GetIntValue(HeaderEnum.TOTAL_TRIPS.DisplayName(), value, headers),
                    TotalDistance = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL_DISTANCE.DisplayName(), value, headers),
                    TotalPay = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL_PAY.DisplayName(), value, headers),
                    TotalTips = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL_TIPS.DisplayName(), value, headers),
                    TotalBonus = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL_BONUS.DisplayName(), value, headers),
                    GrandTotal = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL_GRAND.DisplayName(), value, headers),
                    TotalCash = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL_CASH.DisplayName(), value, headers),
                    AmountPerTime = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_TIME.DisplayName(), value, headers),
                    AmountPerDistance = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName(), value, headers),
                    AmountPerTrip = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_TRIP.DisplayName(), value, headers),
                    Saved = true
                };

                shifts.Add(shift);
            }
            return shifts;
        }
        public static IList<IList<object>> MapToRangeData(List<ShiftEntity> shifts, IList<object> shiftHeaders)
        {
            var rangeData = new List<IList<object>>();

            foreach (var shift in shifts)
            {
                var objectList = new List<object>();

                foreach (var header in shiftHeaders)
                {
                    var headerEnum = header.ToString().Trim().GetValueFromName<HeaderEnum>();
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
            var sheetTripsName = SheetEnum.TRIPS.DisplayName();
            var sheetTripsTypeRange = tripSheet.Headers.FirstOrDefault(x => x.Name == HeaderEnum.TYPE.DisplayName()).Range;

            sheet.Headers = new List<SheetCellModel>();

            // Date
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DATE.DisplayName(),
                Note = "Format: YYYY-MM-DD",
                Format = FormatEnum.DATE
            });
            var dateRange = sheet.GetLocalRange(HeaderEnum.DATE);
            // Start Time        
            sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.TIME_START.DisplayName() });
            // End Time
            sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.TIME_END.DisplayName() });
            // Service
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.SERVICE.DisplayName(),
                Validation = ValidationEnum.RANGE_SERVICE
            });
            // #
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.NUMBER.DisplayName(),
                Note = $"Shift Number 1-9{(char)10}{(char)10}Leave blank if there is only shift for that service for that day."
            });
            // Active Time
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TIME_ACTIVE.DisplayName(),
                Note = "Time with a delivery.{(char)10}{(char)10}Can be filled out on requests sheet if you have that info.",
                Format = FormatEnum.DURATION
            });
            // Total Time
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TIME_TOTAL.DisplayName(),
                Note = "Total time",
                Format = FormatEnum.DURATION
            });
            // Omit
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TIME_OMIT.DisplayName(),
                Note = $"Omit time from non service specific totals. Mainly useful if you multi app so you can get a more accurate $/hour calculation.{(char)10}{(char)10}Active time is still counted for the day from omitted shifts.{(char)10}{(char)10}IE: Omit Uber if you have it also running during DoorDash.",
                Validation = ValidationEnum.BOOLEAN
            });
            // Trips
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TRIPS.DisplayName(),
                Note = $"Requests/Deliveries/Trips{(char)10}{(char)10}Use this column if you don't track requests or need to increase the number."
            });
            // Pay
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.PAY.DisplayName(),
                Format = FormatEnum.ACCOUNTING
            });
            // Tips
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TIPS.DisplayName(),
                Format = FormatEnum.ACCOUNTING
            });
            // Bonus
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.BONUS.DisplayName(),
                Format = FormatEnum.ACCOUNTING
            });
            // Cash
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.CASH.DisplayName(),
                Format = FormatEnum.ACCOUNTING
            });
            // Distance
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DISTANCE.DisplayName(),
                Format = FormatEnum.DISTANCE,
                Note = "Distance not accounted for on the Requests sheet."
            });
            // Region
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.REGION.DisplayName(),
                Validation = ValidationEnum.RANGE_REGION
            });
            // Note
            sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.NOTE.DisplayName() });
            // Key
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.KEY.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.KEY.DisplayName()}\",ISBLANK({sheet.GetLocalRange(HeaderEnum.SERVICE)}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.NUMBER)}), {dateRange} & \"-0-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE)}, {dateRange} & \"-\" & {sheet.GetLocalRange(HeaderEnum.NUMBER)} & \"-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE)})))",
                Note = "Used to connect requests to shifts."
            });

            var keyRange = sheet.GetLocalRange(HeaderEnum.KEY);

            // T Active
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_TIME_ACTIVE.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIME_ACTIVE.DisplayName()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TIME_ACTIVE)}),SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.DURATION)}),{sheet.GetLocalRange(HeaderEnum.TIME_ACTIVE)})))",
                Note = "Total Active time from Requests and Shifts sheets.",
                Format = FormatEnum.DURATION
            });
            // T Time
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_TIME.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIME.DisplayName()}\",ISBLANK({dateRange}), \"\",true,IF({sheet.GetLocalRange(HeaderEnum.TIME_OMIT)}=false,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}),{sheet.GetLocalRange(HeaderEnum.TOTAL_TIME_ACTIVE)},{sheet.GetLocalRange(HeaderEnum.TIME_TOTAL)}),0)))",
                Format = FormatEnum.DURATION
            });
            // T Trips
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_TRIPS.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TRIPS.DisplayName()}\",ISBLANK({dateRange}), \"\",true, {sheet.GetLocalRange(HeaderEnum.TRIPS)} + COUNTIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange})))",
                Note = "Number of requests during a shift.",
                Format = FormatEnum.NUMBER
            });
            // T Pay
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_PAY.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_PAY.DisplayName()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.PAY)} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.PAY)})))",
                Format = FormatEnum.ACCOUNTING
            });
            // T Tips
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_TIPS.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIPS.DisplayName()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.TIPS)} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.TIPS)})))",
                Format = FormatEnum.ACCOUNTING
            });
            // T Bonus
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_BONUS.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_BONUS.DisplayName()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.BONUS)} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.BONUS)})))",
                Format = FormatEnum.ACCOUNTING
            });
            // G Total
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_GRAND.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_GRAND.DisplayName()}\",ISBLANK({dateRange}), \"\",true, {sheet.GetLocalRange(HeaderEnum.TOTAL_PAY)}+{sheet.GetLocalRange(HeaderEnum.TOTAL_TIPS)}+{sheet.GetLocalRange(HeaderEnum.TOTAL_BONUS)}))",
                Format = FormatEnum.ACCOUNTING
            });
            // T Cash
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_CASH.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_CASH.DisplayName()}\",ISBLANK({dateRange}), \"\",true,SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.CASH)})))",
                Format = FormatEnum.ACCOUNTING
            });
            // Amt/Trip
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_TRIP.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_TRIP.DisplayName()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS)}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND)}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS)}=0,1,{sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS)}))))",
                Format = FormatEnum.ACCOUNTING
            });
            // Amt/Time
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_TIME.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.DisplayName()}\",ISBLANK({dateRange}), \"\", true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND)}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}=0,1,({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}*24)))))",
                Format = FormatEnum.ACCOUNTING
            });
            // T Dist
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL_DISTANCE.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_DISTANCE.DisplayName()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.DISTANCE)} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY)},{keyRange},{tripSheet.GetRange(HeaderEnum.DISTANCE)})))",
                Note = "Total Miles/Kilometers from Requests and Shifts",
                Format = FormatEnum.DISTANCE
            });
            // Amt/Dist
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND)}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND)}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_DISTANCE)}=0,1,{sheet.GetLocalRange(HeaderEnum.TOTAL_DISTANCE)}))))",
                Format = FormatEnum.ACCOUNTING
            });
            // Trips/Hour
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TRIPS_PER_HOUR.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TRIPS_PER_HOUR.DisplayName()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}), \"\", ({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS)}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}=0,1,({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME)}*24))))))",
                Format = FormatEnum.DISTANCE
            });
            // Day
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DAY.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.DAY.DisplayName()}\",ISBLANK({dateRange}), \"\",true,DAY({dateRange})))"
            });
            // Month
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.MONTH.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.MONTH.DisplayName()}\",ISBLANK({dateRange}), \"\",true,MONTH({dateRange})))"
            });
            // Year
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.YEAR.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.YEAR.DisplayName()}\",ISBLANK({dateRange}), \"\",true,YEAR({dateRange})))"
            });

            return sheet;
        }
    }
}