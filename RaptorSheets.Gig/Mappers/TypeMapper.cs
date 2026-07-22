using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Type mapper for configuring the Type sheet with formulas, validations, and formatting.
/// This mapper is tailored for trip-based data aggregation and ratio calculations.
/// </summary>
public static class TypeMapper
{
    /// <summary>
    /// Retrieves the configured Type sheet.
    /// Includes formulas, validations, and formatting specific to the Type sheet.
    /// </summary>
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.TypeSheet;
        sheet.Headers.UpdateColumns();

        var tripSheet = TripMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(Header.TYPE.GetDescription());
        var tripKeyRange = tripSheet.GetRange(Header.TYPE.GetDescription());

        // Configure common aggregation patterns for trip-based data.
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, tripSheet, tripKeyRange, countTrips: true);
        
        // Configure common ratio calculations.
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to TypeMapper.
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();
            
            switch (headerEnum)
            {
                case Header.TYPE:
                    // Formula to configure unique values for the Type column.
                    MapperFormulaHelper.ConfigureUniqueValueHeader(header, tripSheet.GetRange(Header.TYPE.GetDescription(), 2));
                    break;
                case Header.VISIT_FIRST:
                    // Formula to find the first visit date for each type.
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_FIRST.GetDescription(), 
                        SheetName.TRIPS.GetDescription(), 
                        tripSheet.GetColumn(Header.DATE.GetDescription()), 
                        tripSheet.GetColumn(Header.TYPE.GetDescription()), true);
                    header.Format = Format.DATE;
                    break;
                case Header.VISIT_LAST:
                    // Formula to find the last visit date for each type.
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_LAST.GetDescription(), 
                        SheetName.TRIPS.GetDescription(), 
                        tripSheet.GetColumn(Header.DATE.GetDescription()), 
                        tripSheet.GetColumn(Header.TYPE.GetDescription()), false);
                    header.Format = Format.DATE;
                    break;

                // Additional cases for other headers can be added here.
            }
        });

        return sheet;
    }
}

