using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Mappers
{
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
                    headers = HeaderParser.ParserHeader(value);
                    continue;
                }

                if (value[0].ToString() == "")
                {
                    continue;
                }

                // Console.Write(JsonSerializer.Serialize(value));
                TripEntity trip = new()
                {
                    Id = id,
                    Key = HeaderParser.GetStringValue(HeaderEnum.KEY.DisplayName(), value, headers),
                    Date = HeaderParser.GetStringValue(HeaderEnum.DATE.DisplayName(), value, headers),
                    Service = HeaderParser.GetStringValue(HeaderEnum.SERVICE.DisplayName(), value, headers),
                    Number = HeaderParser.GetIntValue(HeaderEnum.NUMBER.DisplayName(), value, headers),
                    Exclude = HeaderParser.GetBoolValue(HeaderEnum.EXCLUDE.DisplayName(), value, headers),
                    Type = HeaderParser.GetStringValue(HeaderEnum.TYPE.DisplayName(), value, headers),
                    Place = HeaderParser.GetStringValue(HeaderEnum.PLACE.DisplayName(), value, headers),
                    Pickup = HeaderParser.GetStringValue(HeaderEnum.PICKUP.DisplayName(), value, headers),
                    Dropoff = HeaderParser.GetStringValue(HeaderEnum.DROPOFF.DisplayName(), value, headers),
                    Duration = HeaderParser.GetStringValue(HeaderEnum.DURATION.DisplayName(), value, headers),
                    Pay = HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderParser.GetDecimalValue(HeaderEnum.TIPS.DisplayName(), value, headers),
                    Bonus = HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    OdometerStart = HeaderParser.GetDecimalValue(HeaderEnum.ODOMETER_START.DisplayName(), value, headers),
                    OdometerEnd = HeaderParser.GetDecimalValue(HeaderEnum.ODOMETER_END.DisplayName(), value, headers),
                    Distance = HeaderParser.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
                    Name = HeaderParser.GetStringValue(HeaderEnum.NAME.DisplayName(), value, headers),
                    StartAddress = HeaderParser.GetStringValue(HeaderEnum.ADDRESS_START.DisplayName(), value, headers),
                    EndAddress = HeaderParser.GetStringValue(HeaderEnum.ADDRESS_END.DisplayName(), value, headers),
                    EndUnit = HeaderParser.GetStringValue(HeaderEnum.UNIT_END.DisplayName(), value, headers),
                    OrderNumber = HeaderParser.GetStringValue(HeaderEnum.ORDER_NUMBER.DisplayName(), value, headers),
                    Region = HeaderParser.GetStringValue(HeaderEnum.REGION.DisplayName(), value, headers),
                    Note = HeaderParser.GetStringValue(HeaderEnum.NOTE.DisplayName(), value, headers),
                    AmountPerTime = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_TIME.DisplayName(), value, headers),
                    AmountPerDistance = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName(), value, headers),
                    Saved = true
                };

                trips.Add(trip);
            }
            return trips;
        }
        public static IList<IList<object>> MapToRangeData(List<TripEntity> trips, IList<object> tripHeaders)
        {
            var rangeData = new List<IList<object>>();

            foreach (var trip in trips)
            {
                var objectList = new List<object>();

                foreach (var header in tripHeaders)
                {
                    var headerEnum = header.ToString().Trim().GetValueFromName<HeaderEnum>();

                    switch (headerEnum)
                    {
                        case HeaderEnum.DATE:
                            objectList.Add(trip.Date);
                            break;
                        case HeaderEnum.SERVICE:
                            objectList.Add(trip.Service);
                            break;
                        case HeaderEnum.NUMBER:
                            objectList.Add(trip.Number);
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
                            objectList.Add(trip.Pay);
                            break;
                        case HeaderEnum.TIPS:
                            objectList.Add(trip.Tip);
                            break;
                        case HeaderEnum.BONUS:
                            objectList.Add(trip.Bonus);
                            break;
                        case HeaderEnum.CASH:
                            objectList.Add(trip.Cash);
                            break;
                        case HeaderEnum.ODOMETER_START:
                            objectList.Add(trip.OdometerStart);
                            break;
                        case HeaderEnum.ODOMETER_END:
                            objectList.Add(trip.OdometerEnd);
                            break;
                        case HeaderEnum.DISTANCE:
                            objectList.Add(trip.Distance);
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

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.TripSheet;

            sheet.Headers = new List<SheetCellModel>();

            // Date
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DATE.DisplayName(),
                Note = "Format: YYYY-MM-DD",
                Format = FormatEnum.DATE
            });
            var dateRange = sheet.GetLocalRange(HeaderEnum.DATE);
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
                Note = "Shift Number 1-9 Leave blank if there is only shift for that service for that day."
            });
            // X
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.EXCLUDE.DisplayName(),
                Note = "Exclude this trip from being included in shift.",
                Validation = ValidationEnum.BOOLEAN
            });
            // Type
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TYPE.DisplayName(),
                Note = "Pickup, Shop, Order, Curbside, Canceled",
                Validation = ValidationEnum.RANGE_TYPE
            });
            // Place
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.PLACE.DisplayName(),
                Note = "Location of pickup (delivery).",
                Validation = ValidationEnum.RANGE_PLACE
            });
            // Pickup
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.PICKUP.DisplayName(),
                Note = "Time when request/ride picked up."
            });
            // Dropoff
            sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.DROPOFF.DisplayName() });
            // Duration
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DURATION.DisplayName(),
                Note = "Hours/Minutes task took to complete.",
                Format = FormatEnum.DURATION
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
            // Total
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.TOTAL.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.TOTAL.DisplayName()}\",ISBLANK({dateRange}), \"\",true,{sheet.GetLocalRange(HeaderEnum.PAY)}+{sheet.GetLocalRange(HeaderEnum.TIPS)}+{sheet.GetLocalRange(HeaderEnum.BONUS)}))",
                Format = FormatEnum.ACCOUNTING
            });
            // Cash
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.CASH.DisplayName(),
                Format = FormatEnum.ACCOUNTING
            });
            // Odo Start
            sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.ODOMETER_START.DisplayName() });
            // Odo End
            sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.ODOMETER_END.DisplayName() });
            // Distance
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DISTANCE.DisplayName(),
                Note = "How many miles/km the request took."
            });
            // Name
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.NAME.DisplayName(),
                Validation = ValidationEnum.RANGE_NAME
            });
            // Start Address
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.ADDRESS_START.DisplayName(),
                Validation = ValidationEnum.RANGE_ADDRESS
            });
            // End Address
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.ADDRESS_END.DisplayName(),
                Validation = ValidationEnum.RANGE_ADDRESS
            });
            // End Unit
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.UNIT_END.DisplayName(),
                Note = "Apartment, Unit, Room, Suite"
            });
            // Order Number
            sheet.Headers.AddColumn(new SheetCellModel { Name = HeaderEnum.ORDER_NUMBER.DisplayName() });
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
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.KEY.DisplayName()}\",ISBLANK({sheet.GetLocalRange(HeaderEnum.SERVICE)}), \"\",true,IF({sheet.GetLocalRange(HeaderEnum.EXCLUDE)},{dateRange} & \"-X-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE)},IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.NUMBER)}), {dateRange} & \"-0-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE)}, {dateRange} & \"-\" & {sheet.GetLocalRange(HeaderEnum.NUMBER)} & \"-\" & {sheet.GetLocalRange(HeaderEnum.SERVICE)}))))",
                Note = "Used to connect trips to shifts."
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
            // Amt/Time
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_TIME.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_TIME.DisplayName()}\",ISBLANK({sheet.GetLocalRange(HeaderEnum.DURATION)}), \"\", true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.DURATION)}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.DURATION)}=0,1,({sheet.GetLocalRange(HeaderEnum.DURATION)}*24)))))",
                Format = FormatEnum.ACCOUNTING
            });
            // Amt/Dist
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName()}\",ISBLANK({sheet.GetLocalRange(HeaderEnum.DISTANCE)}), \"\", true,IF(ISBLANK({sheet.GetLocalRange(HeaderEnum.DISTANCE)}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)}/IF({sheet.GetLocalRange(HeaderEnum.DISTANCE)}=0,1,{sheet.GetLocalRange(HeaderEnum.DISTANCE)}))))",
                Format = FormatEnum.ACCOUNTING
            });

            return sheet;
        }
    }
}