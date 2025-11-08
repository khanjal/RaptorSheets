using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Address mapper for Address sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper<AddressEntity> directly.
/// </summary>
/// <summary>
/// Address mapper for Address sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper<AddressEntity> directly.
/// </summary>
public static class AddressMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.AddressSheet;
        sheet.Headers.UpdateColumns();

        var tripSheet = TripMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.ADDRESS.GetDescription());

        // Configure common aggregation patterns for address-based trip analysis
        // Note: AddressMapper uses trip data with end address as the key
        var tripStartAddressRange = tripSheet.GetRange(HeaderEnum.ADDRESS_END.GetDescription());
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(
            sheet, 
            keyRange, 
            tripSheet, 
            tripStartAddressRange);

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

