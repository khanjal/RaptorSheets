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
public static class ServiceMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.ServiceSheet;
        sheet.Headers.UpdateColumns();

        var shiftSheet = ShiftMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(Header.SERVICE.GetDescription());
        var shiftKeyRange = shiftSheet.GetRange(Header.SERVICE.GetDescription());

        // Configure common aggregation patterns
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, shiftSheet, shiftKeyRange, useShiftTotals: true);
        
        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to ServiceMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();
            
            switch (headerEnum)
            {
                case Header.SERVICE:
                    // Combine services from both trips and shifts
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueCombined(
                        Header.SERVICE.GetDescription(), 
                        TripMapper.GetSheet().GetRange(Header.SERVICE.GetDescription(), 2), 
                        ShiftMapper.GetSheet().GetRange(Header.SERVICE.GetDescription(), 2));
                    break;
                case Header.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_FIRST.GetDescription(), 
                        SheetName.SHIFTS.GetDescription(), 
                        shiftSheet.GetColumn(Header.DATE.GetDescription()), 
                        shiftSheet.GetColumn(Header.SERVICE.GetDescription()), true);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = Format.DATE;
                    break;
                case Header.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_LAST.GetDescription(), 
                        SheetName.SHIFTS.GetDescription(), 
                        shiftSheet.GetColumn(Header.DATE.GetDescription()), 
                        shiftSheet.GetColumn(Header.SERVICE.GetDescription()), false);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = Format.DATE;
                    break;
            }
        });

        return sheet;
    }
}

