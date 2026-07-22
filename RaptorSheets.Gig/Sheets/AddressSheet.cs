using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Sheets;

/// <summary>
/// Address sheet definition - layout and formulas for the Addresses sheet.
/// For data mapping operations, use GenericSheetMapper&lt;AddressEntity&gt; directly.
/// </summary>
public static class AddressSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Addresses,
        CellColor = SheetColor.LIGHT_CYAN,
        TabColor = SheetColor.CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<AddressEntity>()
    };

    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;
        sheet.Headers.UpdateColumns();

        var tripSheet = TripSheet.GetSheet();
        var keyRange = sheet.GetLocalRange(Header.ADDRESS.GetDescription());

        // Configure common aggregation patterns for address-based trip analysis
        // Note: AddressSheet uses trip data with end address as the key
        var tripStartAddressRange = tripSheet.GetRange(Header.ADDRESS_END.GetDescription());
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(
            sheet,
            keyRange,
            tripSheet,
            tripStartAddressRange);

        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to AddressSheet
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.ADDRESS:
                    // Combine start and end addresses from trips
                    MapperFormulaHelper.ConfigureCombinedUniqueValueHeader(header,
                        tripSheet.GetRange(Header.ADDRESS_END.GetDescription(), 2),
                        tripSheet.GetRange(Header.ADDRESS_START.GetDescription(), 2));
                    break;
                case Header.TRIPS:
                    // Count trips that start OR end at this address (override common helper)
                    MapperFormulaHelper.ConfigureDualCountHeader(header, keyRange,
                        tripSheet.GetRange(Header.ADDRESS_START.GetDescription()),
                        tripSheet.GetRange(Header.ADDRESS_END.GetDescription()));
                    break;
                case Header.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildDualFieldVisitLookup(
                        keyRange,
                        Header.VISIT_FIRST.GetDescription(),
                        SheetName.TRIPS.GetDescription(),
                        tripSheet.GetColumn(Header.DATE.GetDescription()),
                        tripSheet.GetColumn(Header.ADDRESS_START.GetDescription()),
                        tripSheet.GetColumn(Header.ADDRESS_END.GetDescription()),
                        (tripSheet.GetIndex(Header.DATE.GetDescription()) + 1).ToString(),
                        true
                    );
                    header.Format = Format.DATE;
                    break;
                case Header.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildDualFieldVisitLookup(
                        keyRange,
                        Header.VISIT_LAST.GetDescription(),
                        SheetName.TRIPS.GetDescription(),
                        tripSheet.GetColumn(Header.DATE.GetDescription()),
                        tripSheet.GetColumn(Header.ADDRESS_START.GetDescription()),
                        tripSheet.GetColumn(Header.ADDRESS_END.GetDescription()),
                        (tripSheet.GetIndex(Header.DATE.GetDescription()) + 1).ToString(),
                        false
                    );
                    header.Format = Format.DATE;
                    break;
            }
        });

        return sheet;
    }
}
