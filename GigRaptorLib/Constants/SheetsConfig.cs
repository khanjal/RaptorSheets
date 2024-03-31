using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Constants
{
    public static class SheetsConfig
    {
        public static SheetModel AddressSheet => new()
        {
            Name = SheetEnum.ADDRESSES.DisplayName(),
            CellColor = ColorEnum.LIGHT_CYAN,
            TabColor = ColorEnum.CYAN,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.ADDRESS.DisplayName() },
                .. CommonTripSheetHeaders
            ]
        };

        public static SheetModel DailySheet => new()
        {
            Name = SheetEnum.DAILY.DisplayName(),
            TabColor = ColorEnum.LIGHT_GREEN,
            CellColor = ColorEnum.LIGHT_GRAY,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.DATE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TRIPS.DisplayName() },
                .. CommonIncomeHeaders,
                .. CommonTravelHeaders,
                new SheetCellModel { Name = HeaderEnum.TIME_TOTAL.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TIME.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DAY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.WEEKDAY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.WEEK.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.MONTH.DisplayName() }
            ]
        };

        public static SheetModel MonthlySheet => new()
        {
            Name = SheetEnum.MONTHLY.DisplayName(),
            TabColor = ColorEnum.LIGHT_GREEN,
            CellColor = ColorEnum.LIGHT_GRAY,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.MONTH.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TRIPS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DAYS.DisplayName() },
                .. CommonIncomeHeaders,
                .. CommonTravelHeaders,
                .. CommonPeriodicHeaders,
                new SheetCellModel { Name = HeaderEnum.NUMBER.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.YEAR.DisplayName() }
            ]
        };

        public static SheetModel NameSheet => new()
        {
            Name = SheetEnum.NAMES.DisplayName(),
            TabColor = ColorEnum.CYAN,
            CellColor = ColorEnum.LIGHT_CYAN,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.NAME.DisplayName() },
                .. CommonTripSheetHeaders
            ]
        };

        public static SheetModel PlaceSheet => new()
        {
            Name = SheetEnum.PLACES.DisplayName(),
            TabColor = ColorEnum.CYAN,
            CellColor = ColorEnum.LIGHT_CYAN,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.PLACE.DisplayName() },
                .. CommonTripSheetHeaders
            ]
        };

        public static SheetModel RegionSheet => new()
        {
            Name = SheetEnum.REGIONS.DisplayName(),
            TabColor = ColorEnum.CYAN,
            CellColor = ColorEnum.LIGHT_CYAN,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.REGION.DisplayName() },
                .. CommonTripSheetHeaders
            ]
        };

        public static SheetModel ServiceSheet => new()
        {
            Name = SheetEnum.SERVICES.DisplayName(),
            TabColor = ColorEnum.CYAN,
            CellColor = ColorEnum.LIGHT_CYAN,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.SERVICE.DisplayName() },
                .. CommonTripSheetHeaders
            ]
        };

        public static SheetModel ShiftSheet => new()
        {
            Name = SheetEnum.SHIFTS.DisplayName(),
            TabColor = ColorEnum.RED,
            CellColor = ColorEnum.LIGHT_RED,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.DATE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TIME_START.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TIME_END.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.SERVICE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.NUMBER.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TIME_ACTIVE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TIME_TOTAL.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TIME_OMIT.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TRIPS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.PAY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TIPS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.BONUS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.CASH.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DISTANCE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.REGION.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.NOTE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.KEY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TOTAL_TIME_ACTIVE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TOTAL_TIME.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TOTAL_TRIPS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TOTAL_PAY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TOTAL_TIPS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TOTAL_BONUS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TOTAL_GRAND.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TOTAL_CASH.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TRIP.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TIME.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TOTAL_DISTANCE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TRIPS_PER_HOUR.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DAY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.MONTH.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.YEAR.DisplayName() }
            ]
        };

        public static SheetModel TripSheet => new()
        {
            Name = SheetEnum.TRIPS.DisplayName(),
            TabColor = ColorEnum.DARK_YELLOW,
            CellColor = ColorEnum.LIGHT_YELLOW,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.DATE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.SERVICE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.NUMBER.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.EXCLUDE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TYPE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.PLACE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.PICKUP.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DROPOFF.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DURATION.DisplayName() },
                .. CommonIncomeHeaders,
                new SheetCellModel { Name = HeaderEnum.ODOMETER_START.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.ODOMETER_END.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DISTANCE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.NAME.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.ADDRESS_START.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.ADDRESS_END.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.UNIT_END.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.ORDER_NUMBER.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.REGION.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.NOTE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.KEY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DAY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.MONTH.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.YEAR.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TIME.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName() }
           ]
        };

        public static SheetModel TypeSheet => new()
        {
            Name = SheetEnum.TYPES.DisplayName(),
            TabColor = ColorEnum.CYAN,
            CellColor = ColorEnum.LIGHT_CYAN,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.TYPE.DisplayName() },
                .. CommonTripSheetHeaders
            ]
        };

        public static SheetModel WeekdaySheet => new()
        {
            Name = SheetEnum.WEEKDAYS.DisplayName(),
            TabColor = ColorEnum.LIGHT_GREEN,
            CellColor = ColorEnum.LIGHT_GRAY,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.DAY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.WEEKDAY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TRIPS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DAYS.DisplayName() },
                .. CommonIncomeHeaders,
                .. CommonTravelHeaders,
                new SheetCellModel { Name = HeaderEnum.TIME_TOTAL.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TIME.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_DAY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_CURRENT.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PREVIOUS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.DisplayName() }
            ]
        };

        public static SheetModel WeeklySheet => new()
        {
            Name = SheetEnum.WEEKLY.DisplayName(),
            TabColor = ColorEnum.LIGHT_GREEN,
            CellColor = ColorEnum.LIGHT_GRAY,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.WEEK.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TRIPS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DAYS.DisplayName() },
                .. CommonIncomeHeaders,
                .. CommonTravelHeaders,
                .. CommonPeriodicHeaders,
                new SheetCellModel { Name = HeaderEnum.NUMBER.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.YEAR.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DATE_BEGIN.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DATE_END.DisplayName() }
            ]
        };

        public static SheetModel YearlySheet => new()
        {
            Name = SheetEnum.YEARLY.DisplayName(),
            TabColor = ColorEnum.LIGHT_GREEN,
            CellColor = ColorEnum.LIGHT_GRAY,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.YEAR.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TRIPS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DAYS.DisplayName() },
                .. CommonIncomeHeaders,
                .. CommonTravelHeaders,
                .. CommonPeriodicHeaders
            ]
        };

        private static List<SheetCellModel> CommonTripSheetHeaders =>
        [
            new SheetCellModel { Name = HeaderEnum.TRIPS.DisplayName() },
            .. CommonIncomeHeaders,
            .. CommonTravelHeaders,
            new SheetCellModel { Name = HeaderEnum.VISIT_FIRST.DisplayName() },
            new SheetCellModel { Name = HeaderEnum.VISIT_LAST.DisplayName() }
        ];

        private static List<SheetCellModel> CommonIncomeHeaders => [
            new SheetCellModel { Name = HeaderEnum.PAY.DisplayName() },
            new SheetCellModel { Name = HeaderEnum.TIPS.DisplayName() },
            new SheetCellModel { Name = HeaderEnum.BONUS.DisplayName() },
            new SheetCellModel { Name = HeaderEnum.TOTAL.DisplayName() },
            new SheetCellModel { Name = HeaderEnum.CASH.DisplayName() }
        ];

        private static List<SheetCellModel> CommonPeriodicHeaders => [
            new SheetCellModel { Name = HeaderEnum.TIME_TOTAL.DisplayName() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TIME.DisplayName() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_DAY.DisplayName() },
            new SheetCellModel { Name = HeaderEnum.AVERAGE.DisplayName() }
        ];

        private static List<SheetCellModel> CommonTravelHeaders => [
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TRIP.DisplayName() },
            new SheetCellModel { Name = HeaderEnum.DISTANCE.DisplayName() },
            new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName() }
        ];
    }
}
