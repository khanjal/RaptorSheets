using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

public static class NameMapper
{
    public static List<NameEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var names = new List<NameEntity>();
        var headers = new Dictionary<int, string>();
        values = values!.Where(x => x.Count > 0 && !string.IsNullOrEmpty(x[0]?.ToString())).ToList();
        var id = 0;

        foreach (List<object> value in values)
        {
            id++;
            if (id == 1)
            {
                headers = HeaderHelpers.ParserHeader(value);
                continue;
            }

            if (value.Count < headers.Count)
            {
                value.AddItems(headers.Count - value.Count);
            };

            NameEntity name = new()
            {
                RowId = id,
                Name = HeaderHelpers.GetStringValue(HeaderEnum.NAME.GetDescription(), value, headers),
                Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                Distance = HeaderHelpers.GetIntValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                FirstTrip = HeaderHelpers.GetStringValue(HeaderEnum.VISIT_FIRST.GetDescription(), value, headers),
                LastTrip = HeaderHelpers.GetStringValue(HeaderEnum.VISIT_LAST.GetDescription(), value, headers),
                Saved = true
            };

            names.Add(name);
        }
        return names;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.NameSheet;
        sheet.Headers.UpdateColumns();

        var tripSheet = TripMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.NAME.GetDescription());
        var tripKeyRange = tripSheet.GetRange(HeaderEnum.NAME.GetDescription(), 2);

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