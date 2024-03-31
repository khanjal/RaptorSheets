using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Mappers
{
    public static class ServiceMapper
    {
        public static List<ServiceEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var services = new List<ServiceEntity>();
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

                ServiceEntity service = new()
                {
                    Id = id,
                    Service = HeaderParser.GetStringValue(HeaderEnum.SERVICE.DisplayName(), value, headers),
                    Trips = HeaderParser.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Pay = HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderParser.GetDecimalValue(HeaderEnum.TIPS.DisplayName(), value, headers),
                    Bonus = HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    Distance = HeaderParser.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
                };

                services.Add(service);
            }
            return services;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.ServiceSheet;

            var shiftSheet = ShiftMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonShiftGroupSheetHeaders(shiftSheet, HeaderEnum.SERVICE);

            return sheet;
        }
    }
}