using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;
using HeaderEnum = RaptorSheets.Gig.Enums.HeaderEnum;

namespace RaptorSheets.Gig.Mappers;

public static class WeekdayMapper
{
    public static List<WeekdayEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var weekdays = new List<WeekdayEntity>();
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

            // Console.Write(JsonSerializer.Serialize(value));
            WeekdayEntity weekday = new()
            {
                RowId = id,
                Day = HeaderHelpers.GetIntValue(HeaderEnum.DAY.GetDescription(), value, headers),
                Weekday = HeaderHelpers.GetStringValue(HeaderEnum.WEEKDAY.GetDescription(), value, headers),
                Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                Distance = HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                Time = HeaderHelpers.GetStringValue(HeaderEnum.TIME_TOTAL.GetDescription(), value, headers),
                Days = HeaderHelpers.GetIntValue(HeaderEnum.DAYS.GetDescription(), value, headers),
                DailyAverage = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_DAY.GetDescription(), value, headers),
                PreviousDailyAverage = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.GetDescription(), value, headers),
                CurrentAmount = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_CURRENT.GetDescription(), value, headers),
                PreviousAmount = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PREVIOUS.GetDescription(), value, headers),
            };

            weekdays.Add(weekday);
        }
        return weekdays;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.WeekdaySheet;
        var dailySheet = DailyMapper.GetSheet();

        // Use the GigSheetHelpers to generate the correct headers with formulas
        sheet.Headers = GigSheetHelpers.GetCommonTripGroupSheetHeaders(dailySheet, HeaderEnum.DAY);

        // Update column indexes to ensure proper assignment
        sheet.Headers.UpdateColumns();

        var sheetKeyRange = sheet.GetLocalRange(HeaderEnum.DAY.GetDescription());

        // Current Amount - using new gig-specific formula builder approach
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_CURRENT.GetDescription(),
            Formula = GigFormulaBuilder.BuildArrayFormulaCurrentAmount(
                sheetKeyRange,
                HeaderEnum.AMOUNT_CURRENT.GetDescription(),
                sheetKeyRange,
                Enums.SheetEnum.DAILY.GetDescription(),
                dailySheet.GetColumn(HeaderEnum.DATE.GetDescription()),
                dailySheet.GetColumn(HeaderEnum.TOTAL.GetDescription()),
                (dailySheet.GetIndex(HeaderEnum.TOTAL.GetDescription()) + 1).ToString()
            ),
            Format = FormatEnum.ACCOUNTING
        });

        // Previous Amount - using new gig-specific formula builder approach
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PREVIOUS.GetDescription(),
            Formula = GigFormulaBuilder.BuildArrayFormulaPreviousAmount(
                sheetKeyRange,
                HeaderEnum.AMOUNT_PREVIOUS.GetDescription(),
                sheetKeyRange,
                Enums.SheetEnum.DAILY.GetDescription(),
                dailySheet.GetColumn(HeaderEnum.DATE.GetDescription()),
                dailySheet.GetColumn(HeaderEnum.TOTAL.GetDescription()),
                (dailySheet.GetIndex(HeaderEnum.TOTAL.GetDescription()) + 1).ToString()
            ),
            Format = FormatEnum.ACCOUNTING
        });

        // Previous Day Average - using new gig-specific formula builder approach
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.GetDescription(),
            Formula = GigFormulaBuilder.BuildArrayFormulaPreviousDayAverage(
                sheetKeyRange,
                HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.GetDescription(),
                sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()),
                sheet.GetLocalRange(HeaderEnum.AMOUNT_PREVIOUS.GetDescription()),
                sheet.GetLocalRange(HeaderEnum.DAYS.GetDescription())
            ),
            Format = FormatEnum.ACCOUNTING
        });

        // Update columns again after adding new headers
        sheet.Headers.UpdateColumns();

        return sheet;
    }
}