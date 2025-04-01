using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;

namespace RaptorSheets.Gig.Mappers;

public static class TripMapper
{
    public static List<TripEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var trips = new List<TripEntity>();
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

            if (value == null || value[0] == null || value[0].ToString() == "")
            {
                continue;
            }

            // Console.Write(JsonSerializer.Serialize(value));
            TripEntity trip = new()
            {
                RowId = id,
                Key = HeaderHelpers.GetStringValue(HeaderEnum.KEY.GetDescription(), value, headers),
                Date = HeaderHelpers.GetStringValue(HeaderEnum.DATE.GetDescription(), value, headers),
                Service = HeaderHelpers.GetStringValue(HeaderEnum.SERVICE.GetDescription(), value, headers),
                Number = HeaderHelpers.GetIntValue(HeaderEnum.NUMBER.GetDescription(), value, headers),
                Exclude = HeaderHelpers.GetBoolValue(HeaderEnum.EXCLUDE.GetDescription(), value, headers),
                Type = HeaderHelpers.GetStringValue(HeaderEnum.TYPE.GetDescription(), value, headers),
                Place = HeaderHelpers.GetStringValue(HeaderEnum.PLACE.GetDescription(), value, headers),
                Pickup = HeaderHelpers.GetStringValue(HeaderEnum.PICKUP.GetDescription(), value, headers),
                Dropoff = HeaderHelpers.GetStringValue(HeaderEnum.DROPOFF.GetDescription(), value, headers),
                Duration = HeaderHelpers.GetStringValue(HeaderEnum.DURATION.GetDescription(), value, headers),
                Pay = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.TIPS.GetDescription(), value, headers),
                Bonus = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.CASH.GetDescription(), value, headers),
                OdometerStart = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.ODOMETER_START.GetDescription(), value, headers),
                OdometerEnd = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.ODOMETER_END.GetDescription(), value, headers),
                Distance = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                Name = HeaderHelpers.GetStringValue(HeaderEnum.NAME.GetDescription(), value, headers),
                StartAddress = HeaderHelpers.GetStringValue(HeaderEnum.ADDRESS_START.GetDescription(), value, headers),
                EndAddress = HeaderHelpers.GetStringValue(HeaderEnum.ADDRESS_END.GetDescription(), value, headers),
                EndUnit = HeaderHelpers.GetStringValue(HeaderEnum.UNIT_END.GetDescription(), value, headers),
                OrderNumber = HeaderHelpers.GetStringValue(HeaderEnum.ORDER_NUMBER.GetDescription(), value, headers),
                Region = HeaderHelpers.GetStringValue(HeaderEnum.REGION.GetDescription(), value, headers),
                Note = HeaderHelpers.GetStringValue(HeaderEnum.NOTE.GetDescription(), value, headers),
                AmountPerTime = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.AMOUNT_PER_TIME.GetDescription(), value, headers),
                AmountPerDistance = HeaderHelpers.GetDecimalValueOrNull(HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), value, headers),
                Saved = true
            };

            trips.Add(trip);
        }
        return trips;
    }
    public static IList<IList<object?>> MapToRangeData(List<TripEntity> trips, IList<object> tripHeaders)
    {
        var rangeData = new List<IList<object?>>();

        foreach (var trip in trips)
        {
            var objectList = new List<object?>();

            foreach (var header in tripHeaders)
            {
                var headerEnum = header.ToString()!.Trim().GetValueFromName<HeaderEnum>();

                switch (headerEnum)
                {
                    case HeaderEnum.DATE:
                        objectList.Add(trip.Date);
                        break;
                    case HeaderEnum.SERVICE:
                        objectList.Add(trip.Service);
                        break;
                    case HeaderEnum.NUMBER:
                        objectList.Add(trip.Number?.ToString() ?? "");
                        break;
                    case HeaderEnum.EXCLUDE:
                        objectList.Add(trip.Exclude);
                        break;
                    case HeaderEnum.TYPE:
                        objectList.Add(trip.Type);
                        break;
                    case HeaderEnum.PLACE:
                        objectList.Add(trip.Place);
                        break;
                    case HeaderEnum.PICKUP:
                        objectList.Add(trip.Pickup);
                        break;
                    case HeaderEnum.DROPOFF:
                        objectList.Add(trip.Dropoff);
                        break;
                    case HeaderEnum.DURATION:
                        objectList.Add(trip.Duration);
                        break;
                    case HeaderEnum.PAY:
                        objectList.Add(trip.Pay?.ToString() ?? "");
                        break;
                    case HeaderEnum.TIPS:
                        objectList.Add(trip.Tip?.ToString() ?? "");
                        break;
                    case HeaderEnum.BONUS:
                        objectList.Add(trip.Bonus?.ToString() ?? "");
                        break;
                    case HeaderEnum.CASH:
                        objectList.Add(trip.Cash?.ToString() ?? "");
                        break;
                    case HeaderEnum.ODOMETER_START:
                        objectList.Add(trip.OdometerStart?.ToString() ?? "");
                        break;
                    case HeaderEnum.ODOMETER_END:
                        objectList.Add(trip.OdometerEnd?.ToString() ?? "");
                        break;
                    case HeaderEnum.DISTANCE:
                        objectList.Add(trip.Distance?.ToString() ?? "");
                        break;
                    case HeaderEnum.NAME:
                        objectList.Add(trip.Name);
                        break;
                    case HeaderEnum.ADDRESS_START:
                        objectList.Add(trip.StartAddress);
                        break;
                    case HeaderEnum.ADDRESS_END:
                        objectList.Add(trip.EndAddress);
                        break;
                    case HeaderEnum.UNIT_END:
                        objectList.Add(trip.EndUnit);
                        break;
                    case HeaderEnum.ORDER_NUMBER:
                        objectList.Add(trip.OrderNumber);
                        break;
                    case HeaderEnum.REGION:
                        objectList.Add(trip.Region);
                        break;
                    case HeaderEnum.NOTE:
                        objectList.Add(trip.Note);
                        break;
                    default:
                        objectList.Add(null);
                        break;
                }
            }

            rangeData.Add(objectList);
        }
        // Console.Write(JsonSerializer.Serialize(rangeData));
        return rangeData;
    }

    public static IList<RowData> MapToRowData(List<TripEntity> tripEntities, IList<object> headers)
    {
        var rows = new List<RowData>();

        foreach (TripEntity trip in tripEntities)
        {
            var rowData = new RowData();
            var cells = new List<CellData>();
            foreach (var header in headers)
            {
                var headerEnum = header!.ToString()!.Trim().GetValueFromName<HeaderEnum>();
                switch (headerEnum)
                {
                    case HeaderEnum.DATE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = trip.Date.ToSerialDate() } });
                        break;
                    case HeaderEnum.SERVICE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = trip.Service ?? null } });
                        break;
                    case HeaderEnum.NUMBER:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = trip.Number } });
                        break;
                    case HeaderEnum.EXCLUDE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { BoolValue = trip.Exclude } });
                        break;
                    case HeaderEnum.TYPE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = trip.Type ?? null } });
                        break;
                    case HeaderEnum.PLACE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = trip.Place ?? null } });
                        break;
                    case HeaderEnum.PICKUP:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = trip.Pickup.ToSerialTime() } });
                        break;
                    case HeaderEnum.DROPOFF:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = trip.Dropoff.ToSerialTime() } });
                        break;
                    case HeaderEnum.DURATION:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = trip.Duration.ToSerialDuration() } });
                        break;
                    case HeaderEnum.PAY:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double?)trip.Pay } });
                        break;
                    case HeaderEnum.TIPS:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double?)trip.Tip } });
                        break;
                    case HeaderEnum.BONUS:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double?)trip.Bonus } });
                        break;
                    case HeaderEnum.CASH:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double?)trip.Cash } });
                        break;
                    case HeaderEnum.ODOMETER_START:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double?)trip.OdometerStart } });
                        break;
                    case HeaderEnum.ODOMETER_END:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double?)trip.OdometerEnd } });
                        break;
                    case HeaderEnum.DISTANCE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double?)trip.Distance } });
                        break;
                    case HeaderEnum.NAME:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = trip.Name ?? null } });
                        break;
                    case HeaderEnum.ADDRESS_START:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = trip.StartAddress ?? null  } });
                        break;
                    case HeaderEnum.ADDRESS_END:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = trip.EndAddress ?? null } });
                        break;
                    case HeaderEnum.UNIT_END:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = trip.EndUnit ?? null } });
                        break;
                    case HeaderEnum.ORDER_NUMBER:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = trip.OrderNumber ?? null } });
                        break;
                    case HeaderEnum.REGION:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = trip.Region ?? null } });
                        break;
                    case HeaderEnum.NOTE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = trip.Note ?? null } });
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

    // TODO: Revisit this if we want to be able to reapply formats to columns
    public static RowData MapToRowFormat(IList<object> headers)
    {
        var rowData = new RowData();
        var cells = new List<CellData>();
        foreach (var header in headers)
        {
            var headerEnum = header!.ToString()!.Trim().GetValueFromName<HeaderEnum>();
            switch (headerEnum)
            {
                case HeaderEnum.DATE:
                    cells.Add(new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(FormatEnum.DATE) });
                    break;
                case HeaderEnum.PICKUP:
                    cells.Add(new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(FormatEnum.TIME) });
                    break;
                case HeaderEnum.DROPOFF:
                    cells.Add(new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(FormatEnum.TIME) });
                    break;
                case HeaderEnum.DURATION:
                    cells.Add(new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(FormatEnum.DURATION) });
                    break;
                case HeaderEnum.PAY:
                    cells.Add(new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(FormatEnum.ACCOUNTING) });
                    break;
                case HeaderEnum.TIPS:
                    cells.Add(new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(FormatEnum.ACCOUNTING) });
                    break;
                case HeaderEnum.BONUS:
                    cells.Add(new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(FormatEnum.ACCOUNTING) });
                    break;
                case HeaderEnum.CASH:
                    cells.Add(new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(FormatEnum.ACCOUNTING) });
                    break;
                default:
                    cells.Add(new CellData());
                    break;
            }
        }
        rowData.Values = cells;

        return rowData;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.TripSheet;

        sheet.Headers = [];

        // Date
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.DATE.GetDescription(),
            Note = ColumnNotes.DateFormat,
            Format = FormatEnum.DATE
        });
        var dateRange = sheet.GetLocalRange(HeaderEnum.DATE.GetDescription());
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
        // X
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.EXCLUDE.GetDescription(),
            Note = ColumnNotes.Exclude,
            Validation = ValidationEnum.BOOLEAN.GetDescription()
        });
        // Type
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TYPE.GetDescription(),
            Note = ColumnNotes.Types,
            Validation = ValidationEnum.RANGE_TYPE.GetDescription()
        });
        // Place
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.PLACE.GetDescription(),
            Note = ColumnNotes.Place,
            Validation = ValidationEnum.RANGE_PLACE.GetDescription()
        });
        // Pickup
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.PICKUP.GetDescription(),
            Note = ColumnNotes.Pickup
        });
        // Dropoff
        sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.DROPOFF.GetDescription() });
        // Duration
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.DURATION.GetDescription(),
            Note = ColumnNotes.Duration,
            Format = FormatEnum.DURATION
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
        // Total
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.TOTAL.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL.GetDescription()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.PAY.GetDescription())}+{sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription())}+{sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription())}))",
            Format = FormatEnum.ACCOUNTING
        });
        // Cash
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.CASH.GetDescription(),
            Format = FormatEnum.ACCOUNTING
        });
        // Odo Start
        sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.ODOMETER_START.GetDescription() });
        // Odo End
        sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.ODOMETER_END.GetDescription() });
        // Distance
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.DISTANCE.GetDescription(),
            Note = ColumnNotes.TripDistance
        });
        // Name
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.NAME.GetDescription(),
            Validation = ValidationEnum.RANGE_NAME.GetDescription()
        });
        // Start Address
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.ADDRESS_START.GetDescription(),
            Validation = ValidationEnum.RANGE_ADDRESS.GetDescription()
        });
        // End Address
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.ADDRESS_END.GetDescription(),
            Validation = ValidationEnum.RANGE_ADDRESS.GetDescription()
        });
        // End Unit
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.UNIT_END.GetDescription(),
            Note = ColumnNotes.UnitTypes
        });
        // Order Number
        sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.ORDER_NUMBER.GetDescription() });
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
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.KEY.GetDescription()}\",ISBLANK({sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription())}), \"\",true,IF({sheet.GetLocalRange(HeaderEnum.EXCLUDE.GetDescription())},{dateRange} & \"-X-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription())},IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription())}), {dateRange} & \"-0-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription())}, {dateRange} & \"-\" & {sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription())} & \"-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription())}))))",
            Note = ColumnNotes.TripKey
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
        // Amt/Time
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.GetDescription()}\",ISBLANK({sheet.GetLocalRange(HeaderEnum.DURATION.GetDescription())}), \"\", true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.DURATION.GetDescription())}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.DURATION.GetDescription())}=0,1,({sheet.GetLocalRange(HeaderEnum.DURATION.GetDescription())}*24)))))",
            Format = FormatEnum.ACCOUNTING
        });
        // Amt/Dist
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription()}\",ISBLANK({sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription())}), \"\", true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription())}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription())}))))",
            Format = FormatEnum.ACCOUNTING
        });

        return sheet;
    }
}