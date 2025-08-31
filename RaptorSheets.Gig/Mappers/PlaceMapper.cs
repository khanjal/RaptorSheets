using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

public static class PlaceMapper
{
    public static List<PlaceEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var places = new List<PlaceEntity>();
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

            PlaceEntity place = new()
            {
                RowId = id,
                Place = HeaderHelpers.GetStringValue(HeaderEnum.PLACE.GetDescription(), value, headers),
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

            places.Add(place);
        }

        return places;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.PlaceSheet;

        var tripSheet = TripMapper.GetSheet();

        // Use the GigSheetHelpers to generate the correct headers with formulas
        sheet.Headers = GigSheetHelpers.GetCommonTripGroupSheetHeaders(tripSheet, HeaderEnum.PLACE);

        // Update column indexes to ensure proper assignment
        sheet.Headers.UpdateColumns();

        // Example: If we wanted to use the new GoogleFormulaBuilder instead of GigSheetHelpers
        // This shows how we could refactor to use the new constants:
        /*
        var placeRange = sheet.GetLocalRange(HeaderEnum.PLACE.GetDescription());
        var tripKeyRange = tripSheet.GetRange(HeaderEnum.PLACE.GetDescription());

        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();
            switch (headerEnum)
            {
                case HeaderEnum.PLACE:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaUnique(placeRange, HeaderEnum.PLACE.GetDescription(), tripSheet.GetRange(HeaderEnum.PLACE.GetDescription(), 2));
                    break;
                case HeaderEnum.TRIPS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(placeRange, HeaderEnum.TRIPS.GetDescription(), tripKeyRange);
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.PAY:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(placeRange, HeaderEnum.PAY.GetDescription(), tripKeyRange, tripSheet.GetRange(HeaderEnum.PAY.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                // ... etc for other headers
            }
        });
        */

        return sheet;
    }

}