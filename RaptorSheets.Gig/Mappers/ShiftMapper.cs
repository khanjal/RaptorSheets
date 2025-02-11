using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;

namespace RaptorSheets.Gig.Mappers;

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
                headers = HeaderHelpers.ParserHeader(value);
                continue;
            }

            if (value[0].ToString() == "")
            {
                continue;
            }

            ShiftEntity shift = new()
            {
                RowId = id,
                Key = HeaderHelpers.GetStringValue(HeaderEnum.KEY.GetDescription(), value, headers),
                Date = HeaderHelpers.GetStringValue(HeaderEnum.DATE.GetDescription(), value, headers),
                Start = HeaderHelpers.GetStringValue(HeaderEnum.TIME_START.GetDescription(), value, headers),
                Finish = HeaderHelpers.GetStringValue(HeaderEnum.TIME_END.GetDescription(), value, headers),
                Service = HeaderHelpers.GetStringValue(HeaderEnum.SERVICE.GetDescription(), value, headers),
                Number = HeaderHelpers.GetIntValue(HeaderEnum.NUMBER.GetDescription(), value, headers),
                Active = HeaderHelpers.GetStringValue(HeaderEnum.TIME_ACTIVE.GetDescription(), value, headers),
                Time = HeaderHelpers.GetStringValue(HeaderEnum.TIME_TOTAL.GetDescription(), value, headers),
                Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Distance = HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                Omit = HeaderHelpers.GetBoolValue(HeaderEnum.TIME_OMIT.GetDescription(), value, headers),
                Region = HeaderHelpers.GetStringValue(HeaderEnum.REGION.GetDescription(), value, headers),
                Note = HeaderHelpers.GetStringValue(HeaderEnum.NOTE.GetDescription(), value, headers),
                Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIPS.GetDescription(), value, headers),
                Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                TotalTrips = HeaderHelpers.GetIntValue(HeaderEnum.TOTAL_TRIPS.GetDescription(), value, headers),
                TotalDistance = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL_DISTANCE.GetDescription(), value, headers),
                TotalPay = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL_PAY.GetDescription(), value, headers),
                TotalTips = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL_TIPS.GetDescription(), value, headers),
                TotalBonus = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL_BONUS.GetDescription(), value, headers),
                GrandTotal = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL_GRAND.GetDescription(), value, headers),
                TotalCash = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL_CASH.GetDescription(), value, headers),
                AmountPerTime = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_TIME.GetDescription(), value, headers),
                AmountPerDistance = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), value, headers),
                AmountPerTrip = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), value, headers),
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
                        objectList.Add(shift.Number?.ToString() ?? "");
                        break;
                    case HeaderEnum.TIME_ACTIVE:
                        objectList.Add(shift.Active);
                        break;
                    case HeaderEnum.TIME_TOTAL:
                        objectList.Add(shift.Time);
                        break;
                    case HeaderEnum.TIME_OMIT:
                        objectList.Add(shift.Omit?.ToString() ?? "");
                        break;
                    case HeaderEnum.PAY:
                        objectList.Add(shift.Pay?.ToString() ?? "");
                        break;
                    case HeaderEnum.TIPS:
                        objectList.Add(shift.Tip?.ToString() ?? "");
                        break;
                    case HeaderEnum.BONUS:
                        objectList.Add(shift.Bonus?.ToString() ?? "");
                        break;
                    case HeaderEnum.CASH:
                        objectList.Add(shift.Cash?.ToString() ?? "");
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
        var sheetTripsName = SheetEnum.TRIPS.GetDescription();
        var sheetTripsTypeRange = tripSheet.Headers.First(x => x.Name == HeaderEnum.TYPE.GetDescription()).Range;

        sheet.Headers = [];

        // Date
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.DATE.GetDescription(),
            Note = ColumnNotes.DateFormat,
            Format = FormatEnum.DATE
        });
        var dateRange = sheet.GetLocalRange(HeaderEnum.DATE.GetDescription());
        // Start Time        
        sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.TIME_START.GetDescription() });
        // End Time
        sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.TIME_END.GetDescription() });
        // Service
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.SERVICE.GetDescription(),
            Validation = ValidationEnum.RANGE_SERVICE.GetDescription()
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
            Validation = ValidationEnum.BOOLEAN.GetDescription()
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
            Validation = ValidationEnum.RANGE_REGION.GetDescription()
        });
        // Note
        sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.NOTE.GetDescription() });
        // Key
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.KEY.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.KEY.GetDescription()}\",ISBLANK({sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription())}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription())}), {dateRange} & \"-0-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription())}, {dateRange} & \"-\" & {sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription())} & \"-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription())})))",
            Note = ColumnNotes.ShiftKey
        });

        var keyRange = sheet.GetLocalRange(HeaderEnum.KEY.GetDescription());

        // T Active
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TIME_ACTIVE.GetDescription())}),SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.DURATION.GetDescription())}),{sheet.GetLocalRange(HeaderEnum.TIME_ACTIVE.GetDescription())})))",
            Note = ColumnNotes.TotalTimeActive,
            Format = FormatEnum.DURATION
        });
        // T Time
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL_TIME.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIME.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF({sheet.GetLocalRange(HeaderEnum.TIME_OMIT.GetDescription())}=false,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription())}),{sheet.GetLocalRange(HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription())},{sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription())}),0)))",
            Format = FormatEnum.DURATION
        });
        // T Trips
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL_TRIPS.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TRIPS.GetDescription()}\",ISBLANK({dateRange}), \"\",true, {sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription())} + COUNTIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange})))",
            Note = ColumnNotes.TotalTrips,
            Format = FormatEnum.NUMBER
        });
        // T Pay
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL_PAY.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_PAY.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.PAY.GetDescription())} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.PAY.GetDescription())})))",
            Format = FormatEnum.ACCOUNTING
        });
        // T Tips
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL_TIPS.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIPS.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription())} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.TIPS.GetDescription())})))",
            Format = FormatEnum.ACCOUNTING
        });
        // T Bonus
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL_BONUS.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_BONUS.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription())} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.BONUS.GetDescription())})))",
            Format = FormatEnum.ACCOUNTING
        });
        // G Total
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL_GRAND.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_GRAND.GetDescription()}\",ISBLANK({dateRange}), \"\",true, {sheet.GetLocalRange(HeaderEnum.TOTAL_PAY.GetDescription())}+{sheet.GetLocalRange(HeaderEnum.TOTAL_TIPS.GetDescription())}+{sheet.GetLocalRange(HeaderEnum.TOTAL_BONUS.GetDescription())}))",
            Format = FormatEnum.ACCOUNTING
        });
        // T Cash
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL_CASH.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_CASH.GetDescription()}\",ISBLANK({dateRange}), \"\",true,SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.CASH.GetDescription())})))",
            Format = FormatEnum.ACCOUNTING
        });
        // Amt/Trip
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_TRIP.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_TRIP.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS.GetDescription())}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS.GetDescription())}))))",
            Format = FormatEnum.ACCOUNTING
        });
        // Amt/Time
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.GetDescription()}\",ISBLANK({dateRange}), \"\", true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}=0,1,({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}*24)))))",
            Format = FormatEnum.ACCOUNTING
        });
        // T Dist
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL_DISTANCE.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_DISTANCE.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription())} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.DISTANCE.GetDescription())})))",
            Note = ColumnNotes.TotalDistance,
            Format = FormatEnum.DISTANCE
        });
        // Amt/Dist
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND.GetDescription())}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_DISTANCE.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.TOTAL_DISTANCE.GetDescription())}))))",
            Format = FormatEnum.ACCOUNTING
        });
        // Trips/Hour
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TRIPS_PER_HOUR.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TRIPS_PER_HOUR.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}), \"\", ({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}=0,1,({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}*24))))))",
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