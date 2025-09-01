using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers
{
    public static class YearlyMapper
    {
        public static List<YearlyEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var yearlyList = new List<YearlyEntity>();
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

                YearlyEntity yearly = new()
                {
                    RowId = id,
                    Year = HeaderHelpers.GetIntValue(HeaderEnum.YEAR.GetDescription(), value, headers),
                    Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                    Days = HeaderHelpers.GetIntValue(HeaderEnum.DAYS.GetDescription(), value, headers),
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
                    Average = HeaderHelpers.GetDecimalValue(HeaderEnum.AVERAGE.GetDescription(), value, headers),
                    AmountPerDay = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_DAY.GetDescription(), value, headers),
                };

                yearlyList.Add(yearly);
            }
            return yearlyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.YearlySheet;
            sheet.Headers.UpdateColumns();

            var monthlySheet = MonthlyMapper.GetSheet();
            var keyRange = sheet.GetLocalRange(HeaderEnum.YEAR.GetDescription());
            var monthlyKeyRange = monthlySheet.GetRange(HeaderEnum.YEAR.GetDescription());

            sheet.Headers.ForEach(header =>
            {
                var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

                switch (headerEnum)
                {
                    case HeaderEnum.YEAR:
                        header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(HeaderEnum.YEAR.GetDescription(), monthlySheet.GetRange(HeaderEnum.YEAR.GetDescription(), 2));
                        break;
                    case HeaderEnum.TRIPS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TRIPS.GetDescription(), monthlyKeyRange, monthlySheet.GetRange(HeaderEnum.TRIPS.GetDescription()));
                        header.Format = FormatEnum.NUMBER;
                        break;
                    case HeaderEnum.DAYS:
                        // For yearly, we sum days instead of counting
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.DAYS.GetDescription(), monthlyKeyRange, monthlySheet.GetRange(HeaderEnum.DAYS.GetDescription()));
                        header.Format = FormatEnum.NUMBER;
                        break;
                    case HeaderEnum.PAY:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.PAY.GetDescription(), monthlyKeyRange, monthlySheet.GetRange(HeaderEnum.PAY.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TIPS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TIPS.GetDescription(), monthlyKeyRange, monthlySheet.GetRange(HeaderEnum.TIPS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.BONUS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.BONUS.GetDescription(), monthlyKeyRange, monthlySheet.GetRange(HeaderEnum.BONUS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TOTAL:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(keyRange, HeaderEnum.TOTAL.GetDescription(), sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()), sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.CASH:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.CASH.GetDescription(), monthlyKeyRange, monthlySheet.GetRange(HeaderEnum.CASH.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.AMOUNT_PER_TRIP:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(keyRange, HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.DISTANCE:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.DISTANCE.GetDescription(), monthlyKeyRange, monthlySheet.GetRange(HeaderEnum.DISTANCE.GetDescription()));
                        header.Format = FormatEnum.DISTANCE;
                        break;
                    case HeaderEnum.AMOUNT_PER_DISTANCE:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(keyRange, HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TIME_TOTAL:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TIME_TOTAL.GetDescription(), monthlyKeyRange, monthlySheet.GetRange(HeaderEnum.TIME_TOTAL.GetDescription()));
                        header.Format = FormatEnum.DURATION;
                        break;
                    case HeaderEnum.AMOUNT_PER_TIME:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(keyRange, HeaderEnum.AMOUNT_PER_TIME.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.AMOUNT_PER_DAY:
                        header.Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{HeaderEnum.AMOUNT_PER_DAY.GetDescription()}\",ISBLANK({keyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())} = 0, 0,true,{sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())}/IF({sheet.GetLocalRange(HeaderEnum.DAYS.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.DAYS.GetDescription())})))";
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.AVERAGE:
                        header.Formula = "=ARRAYFORMULA(IFS(ROW(" + keyRange + ")=1,\"" + HeaderEnum.AVERAGE.GetDescription() + "\",ISBLANK(" + keyRange + "), \"\",true, DAVERAGE(transpose({" + sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()) + ",TRANSPOSE(if(ROW(" + sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()) + ") <= TRANSPOSE(ROW(" + sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()) + "))," + sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()) + ",))}),sequence(rows(" + sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()) + "),1),{if(,,);if(,,)})))";
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    default:
                        break;
                }
            });

            return sheet;
        }
    }
}