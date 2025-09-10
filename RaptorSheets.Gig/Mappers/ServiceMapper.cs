using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

public static class ServiceMapper
{
    public static List<ServiceEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var services = new List<ServiceEntity>();
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

            ServiceEntity service = new()
            {
                RowId = id,
                Service = HeaderHelpers.GetStringValue(HeaderEnum.SERVICE.GetDescription(), value, headers),
                Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIPS.GetDescription(), value, headers),
                Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                Distance = HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                FirstTrip = HeaderHelpers.GetStringValue(HeaderEnum.VISIT_FIRST.GetDescription(), value, headers),
                LastTrip = HeaderHelpers.GetStringValue(HeaderEnum.VISIT_LAST.GetDescription(), value, headers),
                Saved = true
            };

            services.Add(service);
        }
        return services;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.ServiceSheet;
        sheet.Headers.UpdateColumns();

        var shiftSheet = ShiftMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription());
        var shiftKeyRange = shiftSheet.GetRange(HeaderEnum.SERVICE.GetDescription());

        // Configure common aggregation patterns
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, shiftSheet, shiftKeyRange);
        
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