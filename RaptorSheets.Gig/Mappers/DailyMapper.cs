using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;
using HeaderEnum = RaptorSheets.Gig.Enums.HeaderEnum;

namespace RaptorSheets.Gig.Mappers
{
    public static class DailyMapper
    {
        public static List<DailyEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var dailyList = new List<DailyEntity>();
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

                DailyEntity daily = new()
                {
                    RowId = id,
                    Date = HeaderHelpers.GetStringValue(HeaderEnum.DATE.GetDescription(), value, headers),
                    Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                    Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                    Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                    Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                    Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                    Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                    AmountPerTrip = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), value, headers),
                    Distance = HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                    AmountPerDistance = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), value, headers),
                    Time = HeaderHelpers.GetStringValue(HeaderEnum.TIME_TOTAL.GetDescription(), value, headers),
                    AmountPerTime = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_TIME.GetDescription(), value, headers),
                    Day = HeaderHelpers.GetStringValue(HeaderEnum.DAY.GetDescription(), value, headers),
                    Week = HeaderHelpers.GetStringValue(HeaderEnum.WEEK.GetDescription(), value, headers),
                    Month = HeaderHelpers.GetStringValue(HeaderEnum.MONTH.GetDescription(), value, headers),
                };

                dailyList.Add(daily);
            }
            return dailyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.DailySheet;
            sheet.Headers.UpdateColumns();

            var shiftSheet = ShiftMapper.GetSheet();
            var sheetKeyRange = sheet.GetLocalRange(HeaderEnum.DATE.GetDescription());

            sheet.Headers.ForEach(header =>
            {
                var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();

                switch (headerEnum)
                {
                    case HeaderEnum.DATE:
                        header.Format = FormatEnum.DATE;
                        break;
                    case HeaderEnum.TRIPS:
                        header.Format = FormatEnum.NUMBER;
                        break;
                    case HeaderEnum.PAY:
                    case HeaderEnum.TIPS:
                    case HeaderEnum.BONUS:
                    case HeaderEnum.TOTAL:
                    case HeaderEnum.CASH:
                    case HeaderEnum.AMOUNT_PER_TRIP:
                    case HeaderEnum.AMOUNT_PER_TIME:
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.DISTANCE:
                        header.Format = FormatEnum.DISTANCE;
                        break;
                    case HeaderEnum.AMOUNT_PER_DISTANCE:
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TIME_TOTAL:
                        header.Format = FormatEnum.DURATION;
                        break;
                    case HeaderEnum.DAY:
                        header.Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.DAY.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,WEEKDAY({sheetKeyRange},2)))";
                        header.Format = FormatEnum.NUMBER;
                        break;
                    case HeaderEnum.WEEKDAY:
                        header.Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.WEEKDAY.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,WEEKDAY({sheetKeyRange},1)))";
                        header.Format = FormatEnum.WEEKDAY;
                        break;
                    case HeaderEnum.WEEK:
                        header.Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.WEEK.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,WEEKNUM({sheetKeyRange},2)&\"-\"&YEAR({sheetKeyRange})))";
                        break;
                    case HeaderEnum.MONTH:
                        header.Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.MONTH.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,MONTH({sheetKeyRange})&\"-\"&YEAR({sheetKeyRange})))";
                        break;
                    default:
                        break;
                }
            });

            // Get data from shift sheet using GigSheetHelpers functionality
            var commonHeaders = GigSheetHelpers.GetCommonShiftGroupSheetHeaders(shiftSheet, HeaderEnum.DATE);
            
            // Apply formulas from common headers to the corresponding sheet headers
            for (int i = 0; i < Math.Min(sheet.Headers.Count, commonHeaders.Count); i++)
            {
                if (!string.IsNullOrEmpty(commonHeaders[i].Formula) && string.IsNullOrEmpty(sheet.Headers[i].Formula))
                {
                    sheet.Headers[i].Formula = commonHeaders[i].Formula;
                }
            }

            return sheet;
        }
    }
}