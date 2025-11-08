using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Place mapper for Place sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper<PlaceEntity> directly.
/// </summary>
/// <summary>
/// Place mapper for Place sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper<PlaceEntity> directly.
/// </summary>
public static class PlaceMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.PlaceSheet;
        sheet.Headers.UpdateColumns();

        var tripSheet = TripMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.PLACE.GetDescription());
        var tripKeyRange = tripSheet.GetRange(HeaderEnum.PLACE.GetDescription());

        // Configure common aggregation patterns (for trip-based data)
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(
            sheet, 
            keyRange, 
            tripSheet, 
            tripKeyRange,
            countTrips: true);  // Count individual trip occurrences
        
        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to PlaceMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();
            
            switch (headerEnum)
            {
                case HeaderEnum.PLACE:
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFilteredSorted(HeaderEnum.PLACE.GetDescription(), tripSheet.GetRange(HeaderEnum.PLACE.GetDescription(), 2));
                    break;
                case HeaderEnum.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), 
                        SheetEnum.TRIPS.GetDescription(), 
                        tripSheet.GetColumn(HeaderEnum.DATE.GetDescription()), 
                        tripSheet.GetColumn(HeaderEnum.PLACE.GetDescription()), true);
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), 
                        SheetEnum.TRIPS.GetDescription(), 
                        tripSheet.GetColumn(HeaderEnum.DATE.GetDescription()), 
                        tripSheet.GetColumn(HeaderEnum.PLACE.GetDescription()), false);
                    header.Format = FormatEnum.DATE;
                    break;
            }
        });

        return sheet;
    }
}

