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
        var keyRange = sheet.GetLocalRange(HeaderEnum.REGION.GetDescription());
        var shiftKeyRange = shiftSheet.GetRange(HeaderEnum.REGION.GetDescription());

        // Configure common aggregation patterns (eliminates ~80% of duplication)
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, shiftSheet, shiftKeyRange, useShiftTotals: true);
        
        // Configure common ratio calculations  
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to RegionMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();
            
            switch (headerEnum)
            {
                case HeaderEnum.REGION:
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueCombinedFiltered(
                        HeaderEnum.REGION.GetDescription(), 
                        TripMapper.GetSheet().GetRange(HeaderEnum.REGION.GetDescription(), 2), 
                        ShiftMapper.GetSheet().GetRange(HeaderEnum.REGION.GetDescription(), 2));
                    break;
                case HeaderEnum.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), 
                        SheetEnum.SHIFTS.GetDescription(), 
                        shiftSheet.GetColumn(HeaderEnum.DATE.GetDescription()), 
                        shiftSheet.GetColumn(HeaderEnum.REGION.GetDescription()), true);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), 
                        SheetEnum.SHIFTS.GetDescription(), 
                        shiftSheet.GetColumn(HeaderEnum.DATE.GetDescription()), 
                        shiftSheet.GetColumn(HeaderEnum.REGION.GetDescription()), false);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = FormatEnum.DATE;
                    break;
            }
        });

        return sheet;
    }
}

