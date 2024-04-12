using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Mappers
{
    public static class NameMapper
    {
        public static List<NameEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var names = new List<NameEntity>();
            var headers = new Dictionary<int, string>();
            values = values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
            var id = 0;

            foreach (List<object> value in values)
            {
                id++;
                if (id == 1)
                {
                    headers = HeaderParser.ParserHeader(value);
                    continue;
                }

                if (value.Count < headers.Count)
                {
                    value.AddItems(headers.Count - value.Count);
                };

                NameEntity name = new()
                {
                    Id = id,
                    Name = HeaderParser.GetStringValue(HeaderEnum.NAME.DisplayName(), value, headers),
                    Visits = HeaderParser.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Pay = HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderParser.GetDecimalValue(HeaderEnum.TIP.DisplayName(), value, headers),
                    Bonus = HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    Distance = HeaderParser.GetIntValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
                };

                names.Add(name);
            }
            return names;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.NameSheet;

            var tripSheet = TripMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonTripGroupSheetHeaders(tripSheet, HeaderEnum.NAME);

            return sheet;
        }
    }
}