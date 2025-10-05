using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Entities;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RaptorSheets.Gig.Constants;

/// <summary>
/// Configuration constants and models for Google Sheets in the Gig domain.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SheetsConfig
{
    /// <summary>
    /// Sheet names with implicit ordering based on declaration order
    /// </summary>
    public static class SheetNames
    {
        // Primary data entry sheets (declared first for highest priority)
        public const string Trips = "Trips";
        public const string Shifts = "Shifts";
        public const string Expenses = "Expenses";
        
        // Reference data sheets (depend on primary data)
        public const string Addresses = "Addresses";
        public const string Names = "Names";
        public const string Places = "Places";
        public const string Regions = "Regions";
        public const string Services = "Services";
        public const string Types = "Types";
        
        // Analysis/summary sheets (depend on primary and reference data)
        public const string Daily = "Daily";
        public const string Weekdays = "Weekdays";
        public const string Weekly = "Weekly";
        public const string Monthly = "Monthly";
        public const string Yearly = "Yearly";
        
        // Administrative sheets (lowest priority, declared last)
        public const string Setup = "Setup";
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
        /// <summary>
        /// All sheet names in explicit order. This is the definitive source of truth for sheet ordering.
        /// Order represents the desired tab sequence in Google Sheets.
        /// </summary>
        private static readonly List<string> _allSheetNames = new()
        {
            // Primary data entry sheets (leftmost tabs for easy access)
            SheetNames.Trips,
            SheetNames.Shifts,
            SheetNames.Expenses,
            
            // Reference data sheets (middle tabs)
            SheetNames.Addresses,
            SheetNames.Names,
            SheetNames.Places,
            SheetNames.Regions,
            SheetNames.Services,
            SheetNames.Types,
            
            // Analysis/summary sheets (right-side tabs)
            SheetNames.Daily,
            SheetNames.Weekdays,
            SheetNames.Weekly,
            SheetNames.Monthly,
            SheetNames.Yearly,
            
            // Administrative sheets (rightmost tabs)
            SheetNames.Setup
        };

        /// <summary>
        /// Gets all sheet names in explicit order.
        /// This is library-safe and doesn't rely on reflection.
        /// </summary>
        public static List<string> GetAllSheetNames() => new(_allSheetNames);
        
        /// <summary>
        /// Validates that a sheet name is recognized by the system
        /// </summary>
        public static bool IsValidSheetName(string name) =>
            _allSheetNames.Any(sheet => string.Equals(sheet, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets the order index of a sheet name (zero-based).
        /// </summary>
        /// <param name="sheetName">Sheet name to get index for</param>
        /// <returns>Zero-based index, or -1 if not found</returns>
        public static int GetSheetIndex(string sheetName) =>
            _allSheetNames.FindIndex(sheet => string.Equals(sheet, sheetName, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Validates that all provided sheet names are valid.
        /// </summary>
        /// <param name="sheetNames">Sheet names to validate</param>
        /// <returns>List of validation errors (empty if valid)</returns>
        public static List<string> ValidateSheetNames(IEnumerable<string> sheetNames)
        {
            var validNames = _allSheetNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return sheetNames
                .Where(sheetName => !validNames.Contains(sheetName))
                .Select(sheetName => $"Sheet name '{sheetName}' is not defined in SheetNames constants")
                .ToList();
        }

        /// <summary>
        /// Gets all sheet names in uppercase for case-insensitive switch statements
        /// </summary>
        public static class UpperCase
        {
            public static string Addresses => SheetNames.Addresses.ToUpperInvariant();
            public static string Daily => SheetNames.Daily.ToUpperInvariant();
            public static string Expenses => SheetNames.Expenses.ToUpperInvariant();
            public static string Monthly => SheetNames.Monthly.ToUpperInvariant();
            public static string Names => SheetNames.Names.ToUpperInvariant();
            public static string Places => SheetNames.Places.ToUpperInvariant();
            public static string Regions => SheetNames.Regions.ToUpperInvariant();
            public static string Services => SheetNames.Services.ToUpperInvariant();
            public static string Setup => SheetNames.Setup.ToUpperInvariant();
            public static string Shifts => SheetNames.Shifts.ToUpperInvariant();
            public static string Trips => SheetNames.Trips.ToUpperInvariant();
            public static string Types => SheetNames.Types.ToUpperInvariant();
            public static string Weekdays => SheetNames.Weekdays.ToUpperInvariant();
            public static string Weekly => SheetNames.Weekly.ToUpperInvariant();
            public static string Yearly => SheetNames.Yearly.ToUpperInvariant();
        }

        /// <summary>
        /// Validates that the explicit sheet order array contains all constants and no extras.
        /// This should be called in unit tests to ensure synchronization.
        /// </summary>
        public static List<string> ValidateSheetOrderCompleteness()
        {
            var errors = new List<string>();
            
            // Get all constant values using reflection (safe for validation, not ordering)
            var constantValues = typeof(SheetNames)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => f.GetValue(null)?.ToString())
                .Where(v => v != null)
                .ToHashSet()!;

            var explicitOrderSet = _allSheetNames.ToHashSet();

            // Check for missing sheets in explicit order
            var missingFromExplicit = constantValues.Except(explicitOrderSet);
            foreach (var missing in missingFromExplicit)
            {
                errors.Add($"Sheet '{missing}' exists in SheetNames constants but is missing from explicit order array");
            }

            // Check for extra sheets in explicit order
            var extraInExplicit = explicitOrderSet.Except(constantValues);
            foreach (var extra in extraInExplicit)
            {
                errors.Add($"Sheet '{extra}' exists in explicit order array but is missing from SheetNames constants");
            }

            return errors;
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
