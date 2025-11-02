using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Trip mapper using GenericSheetMapper for data mapping operations.
/// Provides formula configuration specific to the Trips sheet.
/// </summary>
public static class TripMapper
{
    /// <summary>
    /// Maps Google Sheets range data to TripEntity list.
    /// </summary>
    public static List<TripEntity> MapFromRangeData(IList<IList<object>> values)
    {
        return GenericSheetMapper<TripEntity>.MapFromRangeData(values);
    }

    /// <summary>
    /// Maps TripEntity list to Google Sheets range data (simple object arrays).
    /// </summary>
    public static IList<IList<object?>> MapToRangeData(List<TripEntity> trips, IList<object> tripHeaders)
    {
        return GenericSheetMapper<TripEntity>.MapToRangeData(trips, tripHeaders);
    }

    /// <summary>
    /// Maps TripEntity list to Google Sheets RowData (structured format with types).
    /// </summary>
    public static IList<RowData> MapToRowData(List<TripEntity> tripEntities, IList<object> headers)
    {
        return GenericSheetMapper<TripEntity>.MapToRowData(tripEntities, headers);
    }

    /// <summary>
    /// Creates a format row for applying number formats to the Trips sheet.
    /// </summary>
    public static RowData MapToRowFormat(IList<object> headers)
    {
        return GenericSheetMapper<TripEntity>.MapToRowFormat(headers);
    }

    /// <summary>
    /// Gets the configured Trips sheet with formulas, validations, and formatting.
    /// </summary>
    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<TripEntity>.GetSheet(
            SheetsConfig.TripSheet,
            ConfigureTripFormulas
        );
    }

    /// <summary>
    /// Configures formulas, validations, notes, and formatting specific to the Trips sheet.
    /// </summary>
    private static void ConfigureTripFormulas(SheetModel sheet)
    {
        var dateRange = sheet.GetLocalRange(HeaderEnum.DATE.GetDescription());

        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.DATE:
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.SERVICE:
                    header.Validation = ValidationEnum.RANGE_SERVICE.GetDescription();
                    break;
                case HeaderEnum.NUMBER:
                    header.Note = ColumnNotes.ShiftNumber;
                    break;
                case HeaderEnum.EXCLUDE:
                    header.Note = ColumnNotes.Exclude;
                    header.Validation = ValidationEnum.BOOLEAN.GetDescription();
                    break;
                case HeaderEnum.TYPE:
                    header.Note = ColumnNotes.Types;
                    header.Validation = ValidationEnum.RANGE_TYPE.GetDescription();
                    break;
                case HeaderEnum.PLACE:
                    header.Note = ColumnNotes.Place;
                    header.Validation = ValidationEnum.RANGE_PLACE.GetDescription();
                    break;
                case HeaderEnum.PICKUP:
                    header.Note = ColumnNotes.Pickup;
                    header.Format = FormatEnum.TIME;
                    break;
                case HeaderEnum.DROPOFF:
                    header.Format = FormatEnum.TIME;
                    break;
                case HeaderEnum.DURATION:
                    header.Note = ColumnNotes.Duration;
                    header.Format = FormatEnum.DURATION;
                    break;
                case HeaderEnum.PAY:
                case HeaderEnum.TIPS:
                case HeaderEnum.BONUS:
                case HeaderEnum.CASH:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TOTAL:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(dateRange, HeaderEnum.TOTAL.GetDescription(), sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()), sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.DISTANCE:
                    header.Note = ColumnNotes.TripDistance;
                    break;
                case HeaderEnum.NAME:
                    header.Validation = ValidationEnum.RANGE_NAME.GetDescription();
                    break;
                case HeaderEnum.ADDRESS_START:
                case HeaderEnum.ADDRESS_END:
                    header.Validation = ValidationEnum.RANGE_ADDRESS.GetDescription();
                    break;
                case HeaderEnum.UNIT_END:
                    header.Note = ColumnNotes.UnitTypes;
                    break;
                case HeaderEnum.REGION:
                    header.Validation = ValidationEnum.RANGE_REGION.GetDescription();
                    break;
                case HeaderEnum.KEY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTripKey(dateRange, HeaderEnum.KEY.GetDescription(), dateRange, sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription()), sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription()), sheet.GetLocalRange(HeaderEnum.EXCLUDE.GetDescription()));
                    header.Note = ColumnNotes.TripKey;
                    break;
                case HeaderEnum.DAY:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaDay(dateRange, HeaderEnum.DAY.GetDescription(), dateRange);
                    break;
                case HeaderEnum.MONTH:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaMonth(dateRange, HeaderEnum.MONTH.GetDescription(), dateRange);
                    break;
                case HeaderEnum.YEAR:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaYear(dateRange, HeaderEnum.YEAR.GetDescription(), dateRange);
                    break;
                case HeaderEnum.AMOUNT_PER_TIME:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(dateRange, HeaderEnum.AMOUNT_PER_TIME.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.DURATION.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_PER_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(dateRange, HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                default:
                    break;
            }
        });
    }
}