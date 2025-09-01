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
    public static class TypeMapper
    {
        public static List<TypeEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var types = new List<TypeEntity>();
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

                TypeEntity type = new()
                {
                    RowId = id,
                    Type = HeaderHelpers.GetStringValue(HeaderEnum.TYPE.GetDescription(), value, headers),
                    Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                    Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                    Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                    Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                    Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                    Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                    Distance = HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                    FirstTrip = HeaderHelpers.GetStringValue(HeaderEnum.VISIT_FIRST.GetDescription(), value, headers),
                    LastTrip = HeaderHelpers.GetStringValue(HeaderEnum.VISIT_LAST.GetDescription(), value, headers),
                    Saved = true
                };

                types.Add(type);
            }
            return types;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.TypeSheet;
            sheet.Headers.UpdateColumns();

            var tripSheet = TripMapper.GetSheet();
            var keyRange = sheet.GetLocalRange(HeaderEnum.TYPE.GetDescription());
            var tripKeyRange = tripSheet.GetRange(HeaderEnum.TYPE.GetDescription(), 2);

            sheet.Headers.ForEach(header =>
            {
                var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();
                
                switch (headerEnum)
                {
                    case HeaderEnum.TYPE:
                        header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUnique(HeaderEnum.TYPE.GetDescription(), tripSheet.GetRange(HeaderEnum.TYPE.GetDescription(), 2));
                        break;
                    case HeaderEnum.TRIPS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(keyRange, HeaderEnum.TRIPS.GetDescription(), tripKeyRange);
                        header.Format = FormatEnum.NUMBER;
                        break;
                    case HeaderEnum.PAY:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.PAY.GetDescription(), tripKeyRange, tripSheet.GetRange(HeaderEnum.PAY.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TIPS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TIPS.GetDescription(), tripKeyRange, tripSheet.GetRange(HeaderEnum.TIPS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.BONUS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.BONUS.GetDescription(), tripKeyRange, tripSheet.GetRange(HeaderEnum.BONUS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TOTAL:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(keyRange, HeaderEnum.TOTAL.GetDescription(), sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()), sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.CASH:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.CASH.GetDescription(), tripKeyRange, tripSheet.GetRange(HeaderEnum.CASH.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.AMOUNT_PER_TRIP:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(keyRange, HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.DISTANCE:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.DISTANCE.GetDescription(), tripKeyRange, tripSheet.GetRange(HeaderEnum.DISTANCE.GetDescription()));
                        header.Format = FormatEnum.DISTANCE;
                        break;
                    case HeaderEnum.AMOUNT_PER_DISTANCE:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(keyRange, HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.VISIT_FIRST:
                        header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), SheetEnum.TRIPS.GetDescription(), tripSheet.GetColumn(HeaderEnum.DATE.GetDescription()), tripSheet.GetColumn(HeaderEnum.TYPE.GetDescription()), true);
                        header.Format = FormatEnum.DATE;
                        break;
                    case HeaderEnum.VISIT_LAST:
                        header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), SheetEnum.TRIPS.GetDescription(), tripSheet.GetColumn(HeaderEnum.DATE.GetDescription()), tripSheet.GetColumn(HeaderEnum.TYPE.GetDescription()), false);
                        header.Format = FormatEnum.DATE;
                        break;
                    default:
                        break;
                }
            });

            return sheet;
        }
    }
}