using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Type mapper for Type sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper&lt;TypeEntity&gt; directly.
/// </summary>
public static class TypeMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.TypeSheet;
        sheet.Headers.UpdateColumns();

        var tripSheet = TripMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.TYPE.GetDescription());
        var tripKeyRange = tripSheet.GetRange(HeaderEnum.TYPE.GetDescription());

        // Configure common aggregation patterns (for trip-based data)
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, tripSheet, tripKeyRange, countTrips: true);
        
        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to TypeMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();
            
            switch (headerEnum)
            {
                case HeaderEnum.TYPE:
                    MapperFormulaHelper.ConfigureUniqueValueHeader(header, tripSheet.GetRange(HeaderEnum.TYPE.GetDescription(), 2));
                    break;
                case HeaderEnum.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), 
                        SheetEnum.TRIPS.GetDescription(), 
                        tripSheet.GetColumn(HeaderEnum.DATE.GetDescription()), 
                        tripSheet.GetColumn(HeaderEnum.TYPE.GetDescription()), true);
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), 
                        SheetEnum.TRIPS.GetDescription(), 
                        tripSheet.GetColumn(HeaderEnum.DATE.GetDescription()), 
                        tripSheet.GetColumn(HeaderEnum.TYPE.GetDescription()), false);
                    header.Format = FormatEnum.DATE;
                    break;
            }
        });

        return sheet;
    }
}

