using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Region mapper for Region sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper<RegionEntity> directly.
/// </summary>
public static class RegionMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.RegionSheet;
        sheet.Headers.UpdateColumns();

        var shiftSheet = ShiftMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(Header.REGION.GetDescription());
        var shiftKeyRange = shiftSheet.GetRange(Header.REGION.GetDescription());

        // Configure common aggregation patterns (eliminates ~80% of duplication)
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, shiftSheet, shiftKeyRange, useShiftTotals: true);
        
        // Configure common ratio calculations  
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to RegionMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();
            
            switch (headerEnum)
            {
                case Header.REGION:
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueCombinedFiltered(
                        Header.REGION.GetDescription(), 
                        TripMapper.GetSheet().GetRange(Header.REGION.GetDescription(), 2), 
                        ShiftMapper.GetSheet().GetRange(Header.REGION.GetDescription(), 2));
                    break;
                case Header.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_FIRST.GetDescription(), 
                        SheetName.SHIFTS.GetDescription(), 
                        shiftSheet.GetColumn(Header.DATE.GetDescription()), 
                        shiftSheet.GetColumn(Header.REGION.GetDescription()), true);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = Format.DATE;
                    break;
                case Header.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_LAST.GetDescription(), 
                        SheetName.SHIFTS.GetDescription(), 
                        shiftSheet.GetColumn(Header.DATE.GetDescription()), 
                        shiftSheet.GetColumn(Header.REGION.GetDescription()), false);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = Format.DATE;
                    break;
            }
        });

        return sheet;
    }
}

