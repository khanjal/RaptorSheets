using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Service mapper for Service sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper<ServiceEntity> directly.
/// </summary>
/// <summary>
/// Service mapper for Service sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper<ServiceEntity> directly.
/// </summary>
public static class ServiceMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.ServiceSheet;
        sheet.Headers.UpdateColumns();

        var shiftSheet = ShiftMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription());
        var shiftKeyRange = shiftSheet.GetRange(HeaderEnum.SERVICE.GetDescription());

        // Configure common aggregation patterns
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, shiftSheet, shiftKeyRange, useShiftTotals: true);
        
        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to ServiceMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();
            
            switch (headerEnum)
            {
                case HeaderEnum.SERVICE:
                    // Combine services from both trips and shifts
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueCombined(
                        HeaderEnum.SERVICE.GetDescription(), 
                        TripMapper.GetSheet().GetRange(HeaderEnum.SERVICE.GetDescription(), 2), 
                        ShiftMapper.GetSheet().GetRange(HeaderEnum.SERVICE.GetDescription(), 2));
                    break;
                case HeaderEnum.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_FIRST.GetDescription(), 
                        SheetEnum.SHIFTS.GetDescription(), 
                        shiftSheet.GetColumn(HeaderEnum.DATE.GetDescription()), 
                        shiftSheet.GetColumn(HeaderEnum.SERVICE.GetDescription()), true);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, HeaderEnum.VISIT_LAST.GetDescription(), 
                        SheetEnum.SHIFTS.GetDescription(), 
                        shiftSheet.GetColumn(HeaderEnum.DATE.GetDescription()), 
                        shiftSheet.GetColumn(HeaderEnum.SERVICE.GetDescription()), false);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = FormatEnum.DATE;
                    break;
            }
        });

        return sheet;
    }
}

