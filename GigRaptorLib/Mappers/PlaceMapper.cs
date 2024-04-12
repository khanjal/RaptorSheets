using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Mappers
{
    public static class PlaceMapper
    {
        public static List<PlaceEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var places = new List<PlaceEntity>();
            var headers = new Dictionary<int, string>();
            values = values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
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

                PlaceEntity place = new()
                {
                    Id = id,
                    Place = HeaderParser.GetStringValue(HeaderEnum.PLACE.DisplayName(), value, headers),
                    Trips = HeaderParser.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Pay = HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderParser.GetDecimalValue(HeaderEnum.TIP.DisplayName(), value, headers),
                    Bonus = HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    Distance = HeaderParser.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
                };

                places.Add(place);
            }

            return places;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.PlaceSheet;

            var tripSheet = TripMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonTripGroupSheetHeaders(tripSheet, HeaderEnum.PLACE);

            // // Days/Visit
            // sheet.Headers.Add(new SheetCellModel{Name = HeaderEnum.DAYS_PER_VISIT.DisplayName(),
            //     Formula = $"=ARRAYFORMULA(IFS(ROW($A:$A)=1,\"{HeaderEnum.DAYS_PER_VISIT.DisplayName()}\",ISBLANK($A:$A), \"\", true, DAYS(K:K,J:J)/B:B))",
            //     Note = $"Average days between visits.{(char)10}{(char)10}Doesn't take into account active time vs all time."});
            // // Since
            // sheet.Headers.Add(new SheetCellModel{Name = HeaderEnum.DAYS_SINCE_VISIT.DisplayName(),
            //     Formula = $"=ARRAYFORMULA(IFS(ROW($A:$A)=1,\"{HeaderEnum.DAYS_SINCE_VISIT.DisplayName()}\",ISBLANK($A:$A), \"\", true, DAYS(TODAY(),K:K)))",
            //     Note = "Days since last visit."});

            return sheet;
        }

    }
}