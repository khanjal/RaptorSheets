using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

public static class AddressMapper
{
    public static List<AddressEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var addresses = new List<AddressEntity>();
        var headers = new Dictionary<int, string>();
        values = values!.Where(x => x.Count > 0 && !string.IsNullOrEmpty(x[0]?.ToString())).ToList();
        var id = 0;

        foreach (List<object> value in values.Cast<List<object>>())
        {
            id++;
            if (id == 1)
            {
                headers = HeaderHelpers.ParserHeader(value);
                continue;
            }

            AddressEntity address = new()
            {
                RowId = id,
                Address = HeaderHelpers.GetStringValue(HeaderEnum.ADDRESS.GetDescription(), value, headers),
                Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                Distance = HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                FirstTrip = HeaderHelpers.GetStringValue(HeaderEnum.VISIT_FIRST.GetDescription(), value, headers),
                LastTrip = HeaderHelpers.GetStringValue(HeaderEnum.VISIT_LAST.GetDescription(), value, headers),
                Saved = true
            };

            addresses.Add(address);
        }
        return addresses;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.AddressSheet;
        sheet.Headers.UpdateColumns();

        var tripSheet = TripMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.ADDRESS.GetDescription());

        // Configure common aggregation patterns for address-based trip analysis
        // Note: AddressMapper uses trip data with start address as the key
        var tripStartAddressRange = tripSheet.GetRange(HeaderEnum.ADDRESS_START.GetDescription());
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, tripSheet, tripStartAddressRange, useShiftTotals: false);
        
        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to AddressMapper  
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.ADDRESS:
                    // Combine start and end addresses from trips
                    MapperFormulaHelper.ConfigureCombinedUniqueValueHeader(header,
                        tripSheet.GetRange(HeaderEnum.ADDRESS_END.GetDescription(), 2),
                        tripSheet.GetRange(HeaderEnum.ADDRESS_START.GetDescription(), 2));
                    break;
                case HeaderEnum.TRIPS:
                    // Count trips that start OR end at this address (override common helper)
                    MapperFormulaHelper.ConfigureDualCountHeader(header, keyRange,
                        tripSheet.GetRange(HeaderEnum.ADDRESS_START.GetDescription()),
                        tripSheet.GetRange(HeaderEnum.ADDRESS_END.GetDescription()));
                    break;
                case HeaderEnum.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildDualFieldVisitLookup(
                        keyRange,
                        HeaderEnum.VISIT_FIRST.GetDescription(),
                        SheetEnum.TRIPS.GetDescription(),
                        tripSheet.GetColumn(HeaderEnum.DATE.GetDescription()),
                        tripSheet.GetColumn(HeaderEnum.ADDRESS_START.GetDescription()),
                        tripSheet.GetColumn(HeaderEnum.ADDRESS_END.GetDescription()),
                        (tripSheet.GetIndex(HeaderEnum.DATE.GetDescription()) + 1).ToString(),
                        true
                    );
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildDualFieldVisitLookup(
                        keyRange,
                        HeaderEnum.VISIT_LAST.GetDescription(),
                        SheetEnum.TRIPS.GetDescription(),
                        tripSheet.GetColumn(HeaderEnum.DATE.GetDescription()),
                        tripSheet.GetColumn(HeaderEnum.ADDRESS_START.GetDescription()),
                        tripSheet.GetColumn(HeaderEnum.ADDRESS_END.GetDescription()),
                        (tripSheet.GetIndex(HeaderEnum.DATE.GetDescription()) + 1).ToString(),
                        false
                    );
                    header.Format = FormatEnum.DATE;
                    break;
            }
        });

        return sheet;
    }
}