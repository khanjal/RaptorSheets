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
            var sheet = new SheetModel
            {
                Name = SheetEnum.NAMES.DisplayName(),
                TabColor = ColorEnum.CYAN,
                CellColor = ColorEnum.LIGHT_CYAN,
                FreezeColumnCount = 1,
                FreezeRowCount = 1,
                ProtectSheet = true
            };

            var tripSheet = TripMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonTripGroupSheetHeaders(tripSheet, HeaderEnum.NAME);

            return sheet;
        }
    }
}