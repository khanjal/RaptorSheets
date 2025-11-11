using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Name mapper for Name sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper<NameEntity> directly.
/// </summary>
public static class NameMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.NameSheet;
        sheet.Headers.UpdateColumns();

        var tripSheet = TripMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.NAME.GetDescription());
        var tripKeyRange = tripSheet.GetRange(HeaderEnum.NAME.GetDescription());

        // Configure common aggregation patterns (for trip-based data)
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(
            sheet, 
            keyRange, 
            tripSheet, 
            tripKeyRange,
            countTrips: true);  // Count individual trip occurrences
        
        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to NameMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();
            
            switch (headerEnum)
            {
                case HeaderEnum.NAME:
                    MapperFormulaHelper.ConfigureUniqueValueHeader(header, tripSheet.GetRange(HeaderEnum.NAME.GetDescription(), 2));
                    break;
                case HeaderEnum.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), 
                        SheetEnum.TRIPS.GetDescription(), 
                        tripSheet.GetColumn(HeaderEnum.DATE.GetDescription()), 
                        tripSheet.GetColumn(HeaderEnum.NAME.GetDescription()), true);
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), 
                        SheetEnum.TRIPS.GetDescription(), 
                        tripSheet.GetColumn(HeaderEnum.DATE.GetDescription()), 
                        tripSheet.GetColumn(HeaderEnum.NAME.GetDescription()), false);
                    header.Format = FormatEnum.DATE;
                    break;
            }
        });

        return sheet;
    }
}

