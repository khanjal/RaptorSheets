using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Entities;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RaptorSheets.Home.Constants;

/// <summary>
/// Configuration constants and models for Google Sheets in the Home domain
/// (home maintenance and inventory tracking).
/// </summary>
[ExcludeFromCodeCoverage]
public static class SheetsConfig
{
    /// <summary>
    /// Sheet names. The explicit ordering in <see cref="SheetUtilities"/> is the source of truth
    /// for tab order.
    /// </summary>
    public static class SheetNames
    {
        // Inventory / log sheets (primary data entry)
        public const string Appliances = "Appliances & Electronics";
        public const string Projects = "Projects";
        public const string Maintenance = "Maintenance Log";
        public const string Doors = "Doors";
        public const string Paints = "Paints";
        public const string Power = "Power";

        // Reference sheets (feed dropdowns on other sheets)
        public const string Rooms = "Rooms";
        public const string Contacts = "Contacts";

        // Property facts
        public const string Stats = "Stats";
    }

    /// <summary>
    /// Header names shared across the Home sheets. Reference these constants exclusively.
    /// </summary>
    public static class HeaderNames
    {
        // Shared
        public const string Type = "Type";
        public const string Location = "Location";
        public const string Model = "Model";
        public const string SerialNumber = "Serial #";
        public const string Brand = "Brand";
        public const string Color = "Color";
        public const string Name = "Name";
        public const string Number = "Number";
        public const string Notes = "Notes";
        public const string Description = "Description";

        // Appliances & Electronics
        public const string Manufacturer = "Manufacturer";
        public const string ManufactureDate = "Mfg Date";
        public const string EnergySource = "Energy Src";
        public const string AverageUsage = "Avg. Usage";
        public const string Capacity = "Capacity";
        public const string Filter = "Filter";
        public const string FilterDate = "Filter Date";
        public const string ReplacementMonths = "Rpl. Mth";
        public const string NextFilter = "Next Filter";
        public const string Other = "Other";
        public const string OriginalPrice = "Original Price";

        // Projects
        public const string Task = "Task";
        public const string Area = "Area";
        public const string Details = "Details";
        public const string Started = "Started";
        public const string Completed = "Completed";
        public const string ApproximateCost = "Approx. Cost";

        // Maintenance Log
        public const string Date = "Date";
        public const string Problem = "Problem";
        public const string CompanyPerson = "Company/Person";
        public const string Solution = "Solution";
        public const string Amount = "Amount";

        // Contacts
        public const string AltNumber = "Alt Number";
        public const string Address = "Address";
        public const string Retired = "Retired";

        // Rooms
        public const string Room = "Room";
        public const string RoomLength = "L";
        public const string RoomWidth = "W";
        public const string SquareFeet = "Sq. Ft";
        public const string Level = "Level";

        // Doors
        public const string Width = "Width";
        public const string Height = "Height";
        public const string Depth = "Depth";
        public const string Hinge = "Hinge";
        public const string Installed = "Installed";

        // Paints
        public const string Remaining = "Remaining";
        public const string Size = "Size";

        // Power
        public const string Position = "Position";
        public const string Amps = "AMPs";
        public const string Grounded = "Grounded";
        public const string GFI = "GFI";

        // Stats (name/value)
        public const string Value = "Value";
    }

    /// <summary>
    /// Validation pattern name constants that map to <see cref="Enums.ValidationEnum"/> members.
    /// Used in <c>[Column(validationPattern: ...)]</c> attributes.
    /// </summary>
    public static class ValidationNames
    {
        public const string Boolean = "BOOLEAN";
        public const string RangeRoom = "RANGE_ROOM";
        public const string RangeContact = "RANGE_CONTACT";
        public const string RangeSelf = "RANGE_SELF";
    }

    /// <summary>
    /// Utility methods for working with sheet names.
    /// </summary>
    public static class SheetUtilities
    {
        /// <summary>
        /// All sheet names in explicit order - the definitive source of truth for tab order.
        /// </summary>
        private static readonly List<string> _allSheetNames = new()
        {
            // Inventory / log sheets (leftmost tabs)
            SheetNames.Appliances,
            SheetNames.Projects,
            SheetNames.Maintenance,
            SheetNames.Doors,
            SheetNames.Paints,
            SheetNames.Power,

            // Reference sheets
            SheetNames.Rooms,
            SheetNames.Contacts,

            // Property facts (rightmost tab)
            SheetNames.Stats
        };

        public static List<string> GetAllSheetNames() => new(_allSheetNames);

        public static bool IsValidSheetName(string name) =>
            _allSheetNames.Any(sheet => string.Equals(sheet, name, StringComparison.OrdinalIgnoreCase));

        public static int GetSheetIndex(string sheetName) =>
            _allSheetNames.FindIndex(sheet => string.Equals(sheet, sheetName, StringComparison.OrdinalIgnoreCase));

        public static List<string> ValidateSheetNames(IEnumerable<string> sheetNames)
        {
            var validNames = _allSheetNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return sheetNames
                .Where(sheetName => !validNames.Contains(sheetName))
                .Select(sheetName => $"Sheet name '{sheetName}' is not defined in SheetNames constants")
                .ToList();
        }

        /// <summary>
        /// Validates that the explicit sheet order array contains all constants and no extras.
        /// Call from unit tests to ensure synchronization.
        /// </summary>
        public static List<string> ValidateSheetOrderCompleteness()
        {
            var errors = new List<string>();

            var constantValues = typeof(SheetNames)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => f.GetValue(null)?.ToString())
                .Where(v => v != null)
                .ToHashSet()!;

            var explicitOrderSet = _allSheetNames.ToHashSet();

            foreach (var missing in constantValues.Except(explicitOrderSet))
            {
                errors.Add($"Sheet '{missing}' exists in SheetNames constants but is missing from explicit order array");
            }

            foreach (var extra in explicitOrderSet.Except(constantValues))
            {
                errors.Add($"Sheet '{extra}' exists in explicit order array but is missing from SheetNames constants");
            }

            return errors;
        }
    }

    public static SheetModel ApplianceSheet => new()
    {
        Name = SheetNames.Appliances,
        TabColor = ColorEnum.BLUE,
        CellColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ApplianceEntity>()
    };

    public static SheetModel ProjectSheet => new()
    {
        Name = SheetNames.Projects,
        TabColor = ColorEnum.GREEN,
        CellColor = ColorEnum.LIGHT_GREEN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ProjectEntity>()
    };

    public static SheetModel MaintenanceSheet => new()
    {
        Name = SheetNames.Maintenance,
        TabColor = ColorEnum.ORANGE,
        CellColor = ColorEnum.LIGHT_RED,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<MaintenanceEntity>()
    };

    public static SheetModel DoorSheet => new()
    {
        Name = SheetNames.Doors,
        TabColor = ColorEnum.DARK_YELLOW,
        CellColor = ColorEnum.LIGHT_YELLOW,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<DoorEntity>()
    };

    public static SheetModel PaintSheet => new()
    {
        Name = SheetNames.Paints,
        TabColor = ColorEnum.MAGENTA,
        CellColor = ColorEnum.PINK,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<PaintEntity>()
    };

    public static SheetModel PowerSheet => new()
    {
        Name = SheetNames.Power,
        TabColor = ColorEnum.RED,
        CellColor = ColorEnum.LIGHT_RED,
        FontColor = ColorEnum.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<PowerEntity>()
    };

    public static SheetModel RoomSheet => new()
    {
        Name = SheetNames.Rooms,
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<RoomEntity>()
    };

    public static SheetModel ContactSheet => new()
    {
        Name = SheetNames.Contacts,
        TabColor = ColorEnum.CYAN,
        CellColor = ColorEnum.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ContactEntity>()
    };

    public static SheetModel StatSheet => new()
    {
        Name = SheetNames.Stats,
        TabColor = ColorEnum.PURPLE,
        CellColor = ColorEnum.LIGHT_PURPLE,
        FontColor = ColorEnum.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<StatEntity>()
    };
}
