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
        var keyRange = sheet.GetLocalRange(Header.NAME.GetDescription());
        var tripKeyRange = tripSheet.GetRange(Header.NAME.GetDescription());

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
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();
            
            switch (headerEnum)
            {
                case Header.NAME:
                    MapperFormulaHelper.ConfigureUniqueValueHeader(header, tripSheet.GetRange(Header.NAME.GetDescription(), 2));
                    break;
                case Header.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_FIRST.GetDescription(), 
                        SheetName.TRIPS.GetDescription(), 
                        tripSheet.GetColumn(Header.DATE.GetDescription()), 
                        tripSheet.GetColumn(Header.NAME.GetDescription()), true);
                    header.Format = Format.DATE;
                    break;
                case Header.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_LAST.GetDescription(), 
                        SheetName.TRIPS.GetDescription(), 
                        tripSheet.GetColumn(Header.DATE.GetDescription()), 
                        tripSheet.GetColumn(Header.NAME.GetDescription()), false);
                    header.Format = Format.DATE;
                    break;
            }
        });

        return sheet;
    }
}

