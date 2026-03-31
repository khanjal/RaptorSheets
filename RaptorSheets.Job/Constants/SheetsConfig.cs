using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Entities;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Job.Constants;

/// <summary>
/// Configuration constants and models for Google Sheets in the Job domain.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SheetsConfig
{
    /// <summary>
    /// Sheet names with explicit ordering in _allSheetNames array
    /// </summary>
    public static class SheetNames
    {
        // Primary data entry sheets (leftmost tabs)
        public const string Applications = "Applications";
        public const string Interviews = "Interviews";

        // Optional input sheets for additional details
        public const string CompanyDetails = "Company Details";
        public const string PositionDetails = "Position Details";

        // Reference data sheets (calculated from primary data)
        public const string Companies = "Companies";
        public const string Positions = "Positions";

        // Validation reference sheets
        public const string Sites = "Sites";
        public const string Decisions = "Decisions";
        public const string InterviewTypes = "Interview Types";
        public const string InterviewOutcomes = "Interview Outcomes";
        public const string Schedules = "Schedules";

        // Analysis/summary sheets
        public const string Weekly = "Weekly";
        public const string Monthly = "Monthly";
        public const string Summary = "Summary";

        // Administrative sheets (rightmost tabs)
        public const string Setup = "Setup";
    }

    /// <summary>
    /// Explicit ordering - this is the definitive source of truth for sheet tab order
    /// </summary>
    private static readonly List<string> _allSheetNames = new()
    {
        // Primary data entry sheets (leftmost tabs)
        SheetNames.Applications,
        SheetNames.Interviews,

        // Optional input sheets
        SheetNames.CompanyDetails,
        SheetNames.PositionDetails,

        // Reference data sheets (middle tabs)
        SheetNames.Companies,
        SheetNames.Positions,

        // Validation reference sheets
        SheetNames.Sites,
        SheetNames.Decisions,
        SheetNames.InterviewTypes,
        SheetNames.InterviewOutcomes,
        SheetNames.Schedules,

        // Analysis/summary sheets
        SheetNames.Weekly,
        SheetNames.Monthly,
        SheetNames.Summary,

        // Administrative sheets (rightmost tabs)
        SheetNames.Setup
    };

    /// <summary>
    /// Header names for all sheets
    /// </summary>
    public static class HeaderNames
    {
        // Common headers
        public const string Key = "Key";
        public const string Date = "Date";
        public const string Company = "Company";
        public const string JobTitle = "Job Title";
        public const string Notes = "Notes";

        // Application headers
        public const string Posting = "Posting";
        public const string Site = "Site";
        public const string InterviewCount = "Interviews";
        public const string Decision = "Decision";
        public const string DecisionDate = "Decision Date";
        public const string DaysActive = "Days Active";
        public const string PayLow = "Pay - Low";
        public const string PayHigh = "Pay - High";
        public const string PayAvg = "Pay - Avg";
        public const string Location = "Location";
        public const string Schedule = "Schedule";

        // Interview headers
        public const string StartTime = "Start Time";
        public const string EndTime = "End Time";
        public const string Duration = "Duration";
        public const string InterviewType = "Interview Type";
        public const string InterviewRound = "Round";
        public const string RecruiterName = "Recruiter";
        public const string RecruiterContact = "Contact";
        public const string Attendees = "Attendees";
        public const string Outcome = "Outcome";

        // Company headers
        public const string Industry = "Industry";
        public const string Website = "Website";
        public const string ApplicationCount = "Applications";

        // Position headers
        public const string Position = "Position";
        public const string Category = "Category";
        public const string Seniority = "Seniority";

        // Analytics headers
        public const string Week = "Week";
        public const string Month = "Month";
        public const string Total = "Total";
        public const string Count = "Count";
        public const string Rate = "Rate";
        public const string Average = "Average";
        public const string Interviews = "Interviews";
        public const string InterviewRate = "Interview Rate";
        public const string ResponseRate = "Response Rate";
        public const string AcceptanceRate = "Acceptance Rate";
        public const string Accepted = "Accepted";
        public const string Rejected = "Rejected";
        public const string Pending = "Pending";
    }

    /// <summary>
    /// Validation range names for data validation
    /// </summary>
    public static class ValidationNames
    {
        public const string RangeCompany = "Companies!$A$2:$A";
        public const string RangePosition = "Positions!$A$2:$A";
        public const string RangeSite = "Sites!$A$2:$A";
        public const string RangeDecision = "Decisions!$A$2:$A";
        public const string RangeSchedule = "Schedules!$A$2:$A";
        public const string RangeInterviewType = "Interview Types!$A$2:$A";
        public const string RangeInterviewOutcome = "Interview Outcomes!$A$2:$A";
        public const string Boolean = "TRUE,FALSE";
    }

    /// <summary>
    /// Sheet utilities for accessing sheet names and validation
    /// </summary>
    public static class SheetUtilities
    {
        /// <summary>
        /// Gets all sheet names in their defined order
        /// </summary>
        public static List<string> GetAllSheetNames() => new(_allSheetNames);

        /// <summary>
        /// Gets the index of a sheet by name (case-insensitive)
        /// </summary>
        public static int GetSheetIndex(string sheetName)
        {
            var index = _allSheetNames.FindIndex(s => 
                s.Equals(sheetName, StringComparison.OrdinalIgnoreCase));

            if (index == -1)
                throw new ArgumentException($"Sheet '{sheetName}' not found in configuration", nameof(sheetName));

            return index;
        }

        /// <summary>
        /// Validates that the sheet name exists in configuration
        /// </summary>
        public static bool IsValidSheetName(string sheetName)
        {
            return _allSheetNames.Any(s => 
                s.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
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
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
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

    // Sheet Configurations - Entity-Driven Approach

    public static SheetModel ApplicationSheet => new()
    {
        Name = SheetNames.Applications,
        CellColor = ColorEnum.LIGHT_CYAN,
        TabColor = ColorEnum.BLUE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ApplicationEntity>()
    };

    public static SheetModel InterviewSheet => new()
    {
        Name = SheetNames.Interviews,
        CellColor = ColorEnum.LIGHT_GREEN,
        TabColor = ColorEnum.GREEN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<InterviewEntity>()
    };

    public static SheetModel CompanySheet => new()
    {
        Name = SheetNames.Companies,
        CellColor = ColorEnum.LIGHT_CYAN,
        TabColor = ColorEnum.CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<CompanyEntity>()
    };

    public static SheetModel PositionSheet => new()
    {
        Name = SheetNames.Positions,
        CellColor = ColorEnum.LIGHT_CYAN,
        TabColor = ColorEnum.CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<PositionEntity>()
    };

    public static SheetModel CompanyDetailSheet => new()
    {
        Name = SheetNames.CompanyDetails,
        CellColor = ColorEnum.LIGHT_PURPLE,
        TabColor = ColorEnum.PURPLE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = false,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<CompanyDetailEntity>()
    };

    public static SheetModel PositionDetailSheet => new()
    {
        Name = SheetNames.PositionDetails,
        CellColor = ColorEnum.LIGHT_PURPLE,
        TabColor = ColorEnum.PURPLE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = false,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<PositionDetailEntity>()
    };

    public static SheetModel SiteSheet => new()
    {
        Name = SheetNames.Sites,
        CellColor = ColorEnum.LIGHT_GRAY,
        TabColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 0,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<SiteEntity>()
    };

    public static SheetModel DecisionSheet => new()
    {
        Name = SheetNames.Decisions,
        CellColor = ColorEnum.LIGHT_GRAY,
        TabColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 0,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<DecisionEntity>()
    };

    public static SheetModel InterviewTypeSheet => new()
    {
        Name = SheetNames.InterviewTypes,
        CellColor = ColorEnum.LIGHT_GRAY,
        TabColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 0,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<InterviewTypeEntity>()
    };

    public static SheetModel InterviewOutcomeSheet => new()
    {
        Name = SheetNames.InterviewOutcomes,
        CellColor = ColorEnum.LIGHT_GRAY,
        TabColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 0,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<InterviewOutcomeEntity>()
    };

    public static SheetModel ScheduleSheet => new()
    {
        Name = SheetNames.Schedules,
        CellColor = ColorEnum.LIGHT_GRAY,
        TabColor = ColorEnum.LIGHT_GRAY,
        FreezeColumnCount = 0,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ScheduleEntity>()
    };

    public static SheetModel SetupSheet => new()
    {
        Name = SheetNames.Setup,
        TabColor = ColorEnum.ORANGE,
        CellColor = ColorEnum.LIGHT_YELLOW,
        FreezeColumnCount = 0,
        FreezeRowCount = 1,
        ProtectSheet = false,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<SetupEntity>()
    };
}
