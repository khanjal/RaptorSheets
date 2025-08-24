using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using System.Diagnostics.CodeAnalysis;
using HeaderEnum = RaptorSheets.Gig.Enums.HeaderEnum;

namespace RaptorSheets.Gig.Constants;

[ExcludeFromCodeCoverage]
public static class SheetsConfig
{
    public static SheetModel AddressSheet => new()
    {
        Name = Enums.SheetEnum.ADDRESSES.GetDescription(),
        CellColor = ColorEnum.LIGHT_CYAN,
        TabColor = ColorEnum.CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.ADDRESS.GetDescription() },
            .. CommonTripSheetHeaders
        ]
    };

    public static SheetModel DailySheet => new()
    {
        Name = Enums.SheetEnum.DAILY.GetDescription(),
        TabColor = ColorEnum.LIGHT_GREEN,
        CellColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.DATE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TRIPS.GetDescription() },
            .. CommonIncomeHeaders,
            .. CommonTravelHeaders,
            new SheetCellModel { Name = HeaderEnum.TIME_TOTAL.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DAY.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.WEEKDAY.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.WEEK.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.MONTH.GetDescription() }
        ]
    };

    public static SheetModel ExpenseSheet => new()
    {
        Name = Enums.SheetEnum.EXPENSES.GetDescription(),
        TabColor = ColorEnum.RED,
        CellColor = ColorEnum.LIGHT_RED,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.DATE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.NAME.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DESCRIPTION.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.CATEGORY.GetDescription() }
        ]
    };

    public static SheetModel MonthlySheet => new()
    {
        Name = Enums.SheetEnum.MONTHLY.GetDescription(),
        TabColor = ColorEnum.LIGHT_GREEN,
        CellColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.MONTH.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TRIPS.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DAYS.GetDescription() },
            .. CommonIncomeHeaders,
            .. CommonTravelHeaders,
            .. CommonPeriodicHeaders,
            new SheetCellModel { Name = HeaderEnum.NUMBER.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.YEAR.GetDescription() }
        ]
    };

    public static SheetModel NameSheet => new()
    {
        Name = Enums.SheetEnum.NAMES.GetDescription(),
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.NAME.GetDescription() },
            .. CommonTripSheetHeaders
        ]
    };

    public static SheetModel PlaceSheet => new()
    {
        Name = Enums.SheetEnum.PLACES.GetDescription(),
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.PLACE.GetDescription() },
            .. CommonTripSheetHeaders
        ]
    };

    public static SheetModel RegionSheet => new()
    {
        Name = Enums.SheetEnum.REGIONS.GetDescription(),
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.REGION.GetDescription() },
            .. CommonTripSheetHeaders
        ]
    };

    public static SheetModel ServiceSheet => new()
    {
        Name = Enums.SheetEnum.SERVICES.GetDescription(),
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.SERVICE.GetDescription() },
            .. CommonTripSheetHeaders
        ]
    };

    public static SheetModel ShiftSheet => new()
    {
        Name = Enums.SheetEnum.SHIFTS.GetDescription(),
        TabColor = ColorEnum.RED,
        CellColor = ColorEnum.LIGHT_RED,
        FontColor = ColorEnum.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.DATE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TIME_START.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TIME_END.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.SERVICE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.NUMBER.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TIME_ACTIVE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TIME_TOTAL.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TIME_OMIT.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TRIPS.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.PAY.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TIPS.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.BONUS.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.CASH.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DISTANCE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.REGION.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.NOTE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.KEY.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TOTAL_TIME.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TOTAL_TRIPS.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TOTAL_PAY.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TOTAL_TIPS.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TOTAL_BONUS.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TOTAL_GRAND.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TOTAL_CASH.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TRIP.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TOTAL_DISTANCE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TRIPS_PER_HOUR.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DAY.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.MONTH.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.YEAR.GetDescription() }
        ]
    };

    public static SheetModel TripSheet => new()
    {
        Name = Enums.SheetEnum.TRIPS.GetDescription(),
        TabColor = ColorEnum.DARK_YELLOW,
        CellColor = ColorEnum.LIGHT_YELLOW,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.DATE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.SERVICE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.NUMBER.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.EXCLUDE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TYPE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.PLACE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.PICKUP.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DROPOFF.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DURATION.GetDescription() },
            .. CommonIncomeHeaders,
            new SheetCellModel { Name = HeaderEnum.ODOMETER_START.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.ODOMETER_END.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DISTANCE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.NAME.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.ADDRESS_START.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.ADDRESS_END.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.UNIT_END.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.ORDER_NUMBER.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.REGION.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.NOTE.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.KEY.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DAY.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.MONTH.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.YEAR.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription() }
       ]
    };

    public static SheetModel TypeSheet => new()
    {
        Name = Enums.SheetEnum.TYPES.GetDescription(),
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.TYPE.GetDescription() },
            .. CommonTripSheetHeaders
        ]
    };

    public static SheetModel WeekdaySheet => new()
    {
        Name = Enums.SheetEnum.WEEKDAYS.GetDescription(),
        TabColor = ColorEnum.LIGHT_GREEN,
        CellColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.DAY.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.WEEKDAY.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TRIPS.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DAYS.GetDescription() },
            .. CommonIncomeHeaders,
            .. CommonTravelHeaders,
            new SheetCellModel { Name = HeaderEnum.TIME_TOTAL.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_DAY.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_CURRENT.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PREVIOUS.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.GetDescription() }
        ]
    };

    public static SheetModel WeeklySheet => new()
    {
        Name = Enums.SheetEnum.WEEKLY.GetDescription(),
        TabColor = ColorEnum.LIGHT_GREEN,
        CellColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.WEEK.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TRIPS.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DAYS.GetDescription() },
            .. CommonIncomeHeaders,
            .. CommonTravelHeaders,
            .. CommonPeriodicHeaders,
            new SheetCellModel { Name = HeaderEnum.NUMBER.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.YEAR.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DATE_BEGIN.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DATE_END.GetDescription() }
        ]
    };

    public static SheetModel YearlySheet => new()
    {
        Name = Enums.SheetEnum.YEARLY.GetDescription(),
        TabColor = ColorEnum.LIGHT_GREEN,
        CellColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.YEAR.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TRIPS.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.DAYS.GetDescription() },
            .. CommonIncomeHeaders,
            .. CommonTravelHeaders,
            .. CommonPeriodicHeaders
        ]
    };

    private static List<SheetCellModel> CommonTripSheetHeaders =>
    [
        new SheetCellModel { Name = HeaderEnum.TRIPS.GetDescription() },
        .. CommonIncomeHeaders,
        .. CommonTravelHeaders,
        new SheetCellModel { Name = HeaderEnum.VISIT_FIRST.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.VISIT_LAST.GetDescription() }
    ];

    private static List<SheetCellModel> CommonIncomeHeaders => [
        new SheetCellModel { Name = HeaderEnum.PAY.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.TIPS.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.BONUS.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.TOTAL.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.CASH.GetDescription() }
    ];

    private static List<SheetCellModel> CommonPeriodicHeaders => [
        new SheetCellModel { Name = HeaderEnum.TIME_TOTAL.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TIME.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_DAY.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.AVERAGE.GetDescription() }
    ];

    private static List<SheetCellModel> CommonTravelHeaders => [
        new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TRIP.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.DISTANCE.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription() }
    ];
}
