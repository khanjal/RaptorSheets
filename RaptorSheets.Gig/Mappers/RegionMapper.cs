using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

public static class RegionMapper
{
    public static List<RegionEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var regions = new List<RegionEntity>();
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

            RegionEntity region = new()
            {
                RowId = id,
                Region = HeaderHelpers.GetStringValue(HeaderEnum.REGION.GetDescription(), value, headers),
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

            regions.Add(region);
        }
        return regions;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.RegionSheet;
        sheet.Headers.UpdateColumns();

        var shiftSheet = ShiftMapper.GetSheet();
        var tripSheet = TripMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.REGION.GetDescription());
        var shiftKeyRange = shiftSheet.GetRange(HeaderEnum.REGION.GetDescription());

        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();
            
            switch (headerEnum)
            {
                case HeaderEnum.REGION:
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueCombined(HeaderEnum.REGION.GetDescription(), TripMapper.GetSheet().GetRange(HeaderEnum.REGION.GetDescription(), 2), ShiftMapper.GetSheet().GetRange(HeaderEnum.REGION.GetDescription(), 2));
                    break;
                case HeaderEnum.TRIPS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TRIPS.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TRIPS.GetDescription()));
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.PAY:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.PAY.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_PAY.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TIPS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TIPS.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TIPS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.BONUS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.BONUS.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_BONUS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TOTAL:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(keyRange, HeaderEnum.TOTAL.GetDescription(), sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()), sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.CASH:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.CASH.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_CASH.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_PER_TRIP:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(keyRange, HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.DISTANCE:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.DISTANCE.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_DISTANCE.GetDescription()));
                    header.Format = FormatEnum.DISTANCE;
                    break;
                case HeaderEnum.AMOUNT_PER_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(keyRange, HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), SheetEnum.SHIFTS.GetDescription(), shiftSheet.GetColumn(HeaderEnum.DATE.GetDescription()), shiftSheet.GetColumn(HeaderEnum.REGION.GetDescription()), true);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), SheetEnum.SHIFTS.GetDescription(), shiftSheet.GetColumn(HeaderEnum.DATE.GetDescription()), shiftSheet.GetColumn(HeaderEnum.REGION.GetDescription()), false);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = FormatEnum.DATE;
                    break;
                default:
                    break;
            }
        });

        return sheet;
    }
}