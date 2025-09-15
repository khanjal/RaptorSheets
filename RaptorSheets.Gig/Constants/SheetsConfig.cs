using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Entities;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Constants;

/// <summary>
/// Configuration constants and models for Google Sheets in the Gig domain.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SheetsConfig
{
    /// <summary>
    /// Sheet names
    /// </summary>
    public static class SheetNames
    {
        public const string Addresses = "Addresses";
        public const string Daily = "Daily";
        public const string Expenses = "Expenses";
        public const string Monthly = "Monthly";
        public const string Names = "Names";
        public const string Places = "Places";
        public const string Regions = "Regions";
        public const string Services = "Services";
        public const string Setup = "Setup";
        public const string Shifts = "Shifts";
        public const string Trips = "Trips";
        public const string Types = "Types";
        public const string Weekdays = "Weekdays";
        public const string Weekly = "Weekly";
        public const string Yearly = "Yearly";
    }

    /// <summary>
    /// Header names
    /// </summary>
    public static class HeaderNames
    {
        public const string Address = "Address";
        public const string AddressStart = "Start Address";
        public const string AddressEnd = "End Address";
        public const string Amount = "Amount";
        public const string AmountCurrent = "Curr Amt";
        public const string AmountPrevious = "Prev Amt";
        public const string AmountPerDay = "Amt/Day";
        public const string AmountPerDistance = "Amt/Dist";
        public const string AmountPerPreviousDay = "Amt/Prev";
        public const string AmountPerTime = "Amt/Hour";
        public const string AmountPerTrip = "Amt/Trip";
        public const string Average = "Average";
        public const string Bonus = "Bonus";
        public const string Cash = "Cash";
        public const string Category = "Category";
        public const string Date = "Date";
        public const string DateBegin = "Begin";
        public const string DateEnd = "End";
        public const string Day = "Day";
        public const string Days = "Days";
        public const string DaysPerVisit = "D/V";
        public const string DaysSinceVisit = "Since";
        public const string Description = "Description";
        public const string Distance = "Dist";
        public const string Dropoff = "Dropoff";
        public const string Duration = "Duration";
        public const string Exclude = "X";
        public const string Key = "Key";
        public const string Month = "Month";
        public const string Name = "Name";
        public const string Note = "Note";
        public const string Number = "#";
        public const string NumberOfDays = "# Days";
        public const string OdometerEnd = "Odo End";
        public const string OdometerStart = "Odo Start";
        public const string OrderNumber = "Order #";
        public const string Pay = "Pay";
        public const string Pickup = "Pickup";
        public const string Place = "Place";
        public const string Region = "Region";
        public const string Service = "Service";
        public const string TaxDeductible = "Tax Deductible";
        public const string TimeActive = "Active";
        public const string TimeEnd = "Finish";
        public const string TimeOmit = "O";
        public const string TimeStart = "Start";
        public const string TimeTotal = "Time";
        public const string Tip = "Tip";
        public const string Tips = "Tips";
        public const string Total = "Total";
        public const string TotalBonus = "T Bonus";
        public const string TotalCash = "T Cash";
        public const string TotalDistance = "T Dist";
        public const string TotalGrand = "G Total";
        public const string TotalPay = "T Pay";
        public const string TotalTime = "T Time";
        public const string TotalTimeActive = "T Active";
        public const string TotalTips = "T Tips";
        public const string TotalTrips = "T Trips";
        public const string Trips = "Trips";
        public const string TripsPerDay = "Trips/Day";
        public const string TripsPerHour = "Trips/Hour";
        public const string Type = "Type";
        public const string UnitEnd = "End Unit";
        public const string VisitFirst = "First Trip";
        public const string VisitLast = "Last Trip";
        public const string Visits = "Visits";
        public const string Week = "Week";
        public const string Weekday = "Weekday";
        public const string Year = "Year";
    }

    /// <summary>
    /// Utility methods for working with sheet names
    /// </summary>
    public static class SheetUtilities
    {
        public static List<string> GetAllSheetNames() =>
            EntitySheetOrderHelper.GetSheetOrderFromEntity<SheetEntity>();
        
        public static bool IsValidSheetName(string name) =>
            GetAllSheetNames().Any(sheet => string.Equals(sheet, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets all sheet names in uppercase for case-insensitive switch statements
        /// </summary>
        public static class UpperCase
        {
            public const string Addresses = "ADDRESSES";
            public const string Daily = "DAILY";
            public const string Expenses = "EXPENSES";
            public const string Monthly = "MONTHLY";
            public const string Names = "NAMES";
            public const string Places = "PLACES";
            public const string Regions = "REGIONS";
            public const string Services = "SERVICES";
            public const string Setup = "SETUP";
            public const string Shifts = "SHIFTS";
            public const string Trips = "TRIPS";
            public const string Types = "TYPES";
            public const string Weekdays = "WEEKDAYS";
            public const string Weekly = "WEEKLY";
            public const string Yearly = "YEARLY";
        }
    }

    public static SheetModel AddressSheet => new()
    {
        Name = SheetNames.Addresses,
        CellColor = ColorEnum.LIGHT_CYAN,
        TabColor = ColorEnum.CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<AddressEntity>()
    };

    public static SheetModel DailySheet => new()
    {
        Name = SheetNames.Daily,
        TabColor = ColorEnum.LIGHT_GREEN,
        CellColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<DailyEntity>()
    };

    public static SheetModel ExpenseSheet => new()
    {
        Name = SheetNames.Expenses,
        TabColor = ColorEnum.ORANGE,
        CellColor = ColorEnum.LIGHT_RED,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ExpenseEntity>()
    };

    public static SheetModel MonthlySheet => new()
    {
        Name = SheetNames.Monthly,
        TabColor = ColorEnum.LIGHT_GREEN,
        CellColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<MonthlyEntity>()
    };

    public static SheetModel NameSheet => new()
    {
        Name = SheetNames.Names,
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<NameEntity>()
    };

    public static SheetModel PlaceSheet => new()
    {
        Name = SheetNames.Places,
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<PlaceEntity>()
    };

    public static SheetModel RegionSheet => new()
    {
        Name = SheetNames.Regions,
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<RegionEntity>()
    };

    public static SheetModel ServiceSheet => new()
    {
        Name = SheetNames.Services,
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ServiceEntity>()
    };

    public static SheetModel ShiftSheet => new()
    {
        Name = SheetNames.Shifts,
        TabColor = ColorEnum.RED,
        CellColor = ColorEnum.LIGHT_RED,
        FontColor = ColorEnum.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ShiftEntity>()
    };

    public static SheetModel TripSheet => new()
    {
        Name = SheetNames.Trips,
        TabColor = ColorEnum.DARK_YELLOW,
        CellColor = ColorEnum.LIGHT_YELLOW,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TripEntity>()
    };

    public static SheetModel TypeSheet => new()
    {
        Name = SheetNames.Types,
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TypeEntity>()
    };

    public static SheetModel WeekdaySheet => new()
    {
        Name = SheetNames.Weekdays,
        TabColor = ColorEnum.LIGHT_GREEN,
        CellColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<WeekdayEntity>()
    };

    public static SheetModel WeeklySheet => new()
    {
        Name = SheetNames.Weekly,
        TabColor = ColorEnum.LIGHT_GREEN,
        CellColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<WeeklyEntity>()
    };

    public static SheetModel YearlySheet => new()
    {
        Name = SheetNames.Yearly,
        TabColor = ColorEnum.LIGHT_GREEN,
        CellColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<YearlyEntity>()
    };
}
