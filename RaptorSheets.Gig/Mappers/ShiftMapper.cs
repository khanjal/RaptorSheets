using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using HeaderEnum = RaptorSheets.Gig.Enums.HeaderEnum;

namespace RaptorSheets.Gig.Mappers;

public static class ShiftMapper
{
    public static List<ShiftEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var shifts = new List<ShiftEntity>();
        var headers = new Dictionary<int, string>();
        values = values!.Where(x => !string.IsNullOrEmpty(x[0]?.ToString())).ToList();
        var id = 0;

        foreach (var value in values)
        {
            id++;
            if (id == 1)
            {
                headers = HeaderHelpers.ParserHeader(value);
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
                Distance = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                Omit = HeaderHelpers.GetBoolValue(HeaderEnum.TIME_OMIT.GetDescription(), value, headers),
                Region = HeaderHelpers.GetStringValue(HeaderEnum.REGION.GetDescription(), value, headers),
                Note = HeaderHelpers.GetStringValue(HeaderEnum.NOTE.GetDescription(), value, headers),
                Pay = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.TIPS.GetDescription(), value, headers),
                Bonus = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.CASH.GetDescription(), value, headers),
                TotalActive = HeaderHelpers.GetStringValue(HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription(), value, headers),
                TotalTime = HeaderHelpers.GetStringValue(HeaderEnum.TOTAL_TIME.GetDescription(), value, headers),
                TotalTrips = HeaderHelpers.GetIntValue(HeaderEnum.TOTAL_TRIPS.GetDescription(), value, headers),
                TotalDistance = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.TOTAL_DISTANCE.GetDescription(), value, headers),
                TotalPay = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.TOTAL_PAY.GetDescription(), value, headers),
                TotalTips = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.TOTAL_TIPS.GetDescription(), value, headers),
                TotalBonus = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.TOTAL_BONUS.GetDescription(), value, headers),
                GrandTotal = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.TOTAL_GRAND.GetDescription(), value, headers),
                TotalCash = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.TOTAL_CASH.GetDescription(), value, headers),
                AmountPerTime = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.AMOUNT_PER_TIME.GetDescription(), value, headers),
                AmountPerDistance = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), value, headers),
                AmountPerTrip = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), value, headers),
                Saved = true
            };

            shifts.Add(shift);
        }
        return shifts;
    }

    public static IList<RowData> MapToRowData(List<ShiftEntity> shiftEntities, IList<object> headers)
    {
        var rows = new List<RowData>();

        foreach (ShiftEntity shift in shiftEntities)
        {
            var rowData = new RowData();
            var cells = new List<CellData>();
            foreach (var header in headers)
            {
                var headerEnum = header!.ToString()!.Trim().GetValueFromName<HeaderEnum>();
                switch (headerEnum)
                {
                    case HeaderEnum.DATE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = shift.Date.ToSerialDate()} });
                        break;
                    case HeaderEnum.TIME_START:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = shift.Start.ToSerialTime() } });
                        break;
                    case HeaderEnum.TIME_END:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = shift.Finish.ToSerialTime() } });
                        break;
                    case HeaderEnum.SERVICE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = shift.Service ?? null } });
                        break;
                    case HeaderEnum.NUMBER:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = shift.Number } });
                        break;
                    case HeaderEnum.TIME_ACTIVE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = shift.Active.ToSerialDuration() } });
                        break;
                    case HeaderEnum.TIME_TOTAL:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = shift.Time.ToSerialDuration() } });
                        break;
                    case HeaderEnum.TIME_OMIT:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { BoolValue = shift.Omit } });
                        break;
                    case HeaderEnum.PAY:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double?)shift.Pay } });
                        break;
                    case HeaderEnum.TIPS:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double?)shift.Tip } });
                        break;
                    case HeaderEnum.BONUS:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double?)shift.Bonus } });
                        break;
                    case HeaderEnum.CASH:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double?)shift.Cash } });
                        break;
                    case HeaderEnum.REGION:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = shift.Region ?? null } });
                        break;
                    case HeaderEnum.NOTE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = shift.Note ?? null } });
                        break;
                    default:
                        cells.Add(new CellData());
                        break;
                }
            }
            rowData.Values = cells;
            rows.Add(rowData);
        }

        return rows;
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
        sheet.Headers.UpdateColumns();

        var tripSheet = TripMapper.GetSheet();
        var dateRange = sheet.GetLocalRange(HeaderEnum.DATE.GetDescription());
        var keyRange = sheet.GetLocalRange(HeaderEnum.KEY.GetDescription());

        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.DATE:
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.TIME_START:
                case HeaderEnum.TIME_END:
                    header.Format = FormatEnum.TIME;
                    break;
                case HeaderEnum.SERVICE:
                    header.Validation = ValidationEnum.RANGE_SERVICE.GetDescription();
                    break;
                case HeaderEnum.NUMBER:
                    header.Note = ColumnNotes.ShiftNumber;
                    break;
                case HeaderEnum.TIME_ACTIVE:
                    header.Note = ColumnNotes.ActiveTime;
                    header.Format = FormatEnum.DURATION;
                    break;
                case HeaderEnum.TIME_TOTAL:
                    header.Note = ColumnNotes.TotalTime;
                    header.Format = FormatEnum.DURATION;
                    break;
                case HeaderEnum.TIME_OMIT:
                    header.Note = ColumnNotes.TimeOmit;
                    header.Validation = ValidationEnum.BOOLEAN.GetDescription();
                    break;
                case HeaderEnum.TRIPS:
                    header.Note = ColumnNotes.ShiftTrips;
                    break;
                case HeaderEnum.PAY:
                case HeaderEnum.TIPS:
                case HeaderEnum.BONUS:
                case HeaderEnum.CASH:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.DISTANCE:
                    header.Format = FormatEnum.DISTANCE;
                    header.Note = ColumnNotes.ShiftDistance;
                    break;
                case HeaderEnum.REGION:
                    header.Validation = ValidationEnum.RANGE_REGION.GetDescription();
                    break;
                case HeaderEnum.KEY:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.KEY.GetDescription()}\",ISBLANK({sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription())}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription())}), {dateRange} & \"-0-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription())}, {dateRange} & \"-\" & {sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription())} & \"-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription())})))";
                    header.Note = ColumnNotes.ShiftKey;
                    break;
                case HeaderEnum.TOTAL_TIME_ACTIVE:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TIME_ACTIVE.GetDescription())}),SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.DURATION.GetDescription())}),{sheet.GetLocalRange(HeaderEnum.TIME_ACTIVE.GetDescription())})))";
                    header.Note = ColumnNotes.TotalTimeActive;
                    header.Format = FormatEnum.DURATION;
                    break;
                case HeaderEnum.TOTAL_TIME:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIME.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF({sheet.GetLocalRange(HeaderEnum.TIME_OMIT.GetDescription())}=false,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription())}),{sheet.GetLocalRange(HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription())},{sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription())}),0)))";
                    header.Format = FormatEnum.DURATION;
                    break;
                case HeaderEnum.TOTAL_TRIPS:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TRIPS.GetDescription()}\",ISBLANK({dateRange}), \"\",true, {sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription())} + COUNTIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange})))";
                    header.Note = ColumnNotes.TotalTrips;
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.TOTAL_PAY:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_PAY.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.PAY.GetDescription())} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.PAY.GetDescription())})))";
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TOTAL_TIPS:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_TIPS.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription())} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.TIPS.GetDescription())})))";
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TOTAL_BONUS:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_BONUS.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription())} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.BONUS.GetDescription())})))";
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TOTAL_GRAND:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_GRAND.GetDescription()}\",ISBLANK({dateRange}), \"\",true, {sheet.GetLocalRange(HeaderEnum.TOTAL_PAY.GetDescription())}+{sheet.GetLocalRange(HeaderEnum.TOTAL_TIPS.GetDescription())}+{sheet.GetLocalRange(HeaderEnum.TOTAL_BONUS.GetDescription())}))";
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TOTAL_CASH:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_CASH.GetDescription()}\",ISBLANK({dateRange}), \"\",true,SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.CASH.GetDescription())})))";
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_PER_TRIP:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_TRIP.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS.GetDescription())}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS.GetDescription())}))))";
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_PER_TIME:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.GetDescription()}\",ISBLANK({dateRange}), \"\", true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}=0,1,({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}*24)))))";
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TOTAL_DISTANCE:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL_DISTANCE.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription())} + SUMIF({tripSheet.GetRange(HeaderEnum.KEY.GetDescription())},{keyRange},{tripSheet.GetRange(HeaderEnum.DISTANCE.GetDescription())})))";
                    header.Note = ColumnNotes.TotalDistance;
                    header.Format = FormatEnum.DISTANCE;
                    break;
                case HeaderEnum.AMOUNT_PER_DISTANCE:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND.GetDescription())}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_DISTANCE.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.TOTAL_DISTANCE.GetDescription())}))))";
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TRIPS_PER_HOUR:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TRIPS_PER_HOUR.GetDescription()}\",ISBLANK({dateRange}), \"\",true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}), \"\", ({sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}=0,1,({sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription())}*24))))))";
                    header.Format = FormatEnum.DISTANCE;
                    break;
                case HeaderEnum.DAY:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.DAY.GetDescription()}\",ISBLANK({dateRange}), \"\",true,DAY({dateRange})))";
                    break;
                case HeaderEnum.MONTH:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.MONTH.GetDescription()}\",ISBLANK({dateRange}), \"\",true,MONTH({dateRange})))";
                    break;
                case HeaderEnum.YEAR:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.YEAR.GetDescription()}\",ISBLANK({dateRange}), \"\",true,YEAR({dateRange})))";
                    break;
                default:
                    break;
            }
        });

        return sheet;
    }
}