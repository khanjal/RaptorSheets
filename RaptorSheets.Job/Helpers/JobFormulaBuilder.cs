using RaptorSheets.Core.Helpers;

namespace RaptorSheets.Job.Helpers;

/// <summary>
/// Formula builder helpers for Job domain sheets.
/// Provides reusable formula generation methods following the ARRAYFORMULA pattern.
/// Uses GoogleFormulaBuilder.BuildArrayFormula from Core for the base pattern.
/// </summary>
public static class JobFormulaBuilder
{
    /// <summary>
    /// Builds Key formula: Company-JobTitle-<duplicate>
    /// If duplicateRange is empty the key will end in -0
    /// </summary>
    public static string BuildKeyFormula(string keyRange, string header, string companyRange, string jobTitleRange, string? duplicateRange = null)
    {
        // If duplicateRange provided, append '-' & duplicateRange; otherwise append '-0'
        var duplicatePart = string.IsNullOrEmpty(duplicateRange)
            ? "\"-0\""
            : "\"-\"&" + duplicateRange;

        var formula = $"{companyRange}&\"-\"&{jobTitleRange}&" + duplicatePart;
        return GoogleFormulaBuilder.BuildArrayFormula(keyRange, header, formula);
    }

    /// <summary>
    /// Builds Interview Count formula: counts interviews matching the application key using a provided interview lookup range.
    /// keyRange = blank-check range (e.g., Date column on Applications)
    /// applicationKeyRange = the application key cell/range in Applications
    /// interviewLookupRange = the key range in Interviews (e.g., "Interviews!F2:F")
    /// </summary>
    public static string BuildInterviewCountFormula(string keyRange, string header, string applicationKeyRange, string interviewLookupRange)
    {
        var formula = $"IFERROR(COUNTIF({interviewLookupRange},{applicationKeyRange}),0)";
        return GoogleFormulaBuilder.BuildArrayFormula(keyRange, header, formula);
    }

    /// <summary>
    /// Builds Days Active formula: calculates days between application and decision (or today)
    /// </summary>
    public static string BuildDaysActiveFormula(string keyRange, string header, string dateRange, string decisionDateRange)
    {
        var formula = $"IF(ISBLANK({decisionDateRange}),TODAY()-{dateRange},{decisionDateRange}-{dateRange})";
        return GoogleFormulaBuilder.BuildArrayFormula(keyRange, header, formula);
    }

    /// <summary>
    /// Builds Pay Average formula: average of low and high pay, with fallbacks
    /// </summary>
    public static string BuildPayAverageFormula(string keyRange, string header, string payLowRange, string payHighRange)
    {
        var formula = $@"IF(ISNUMBER({payLowRange}),
                            IF(ISNUMBER({payHighRange}),({payLowRange}+{payHighRange})/2,{payLowRange}),
                            IF(ISNUMBER({payHighRange}),{payHighRange},""""))";
        return GoogleFormulaBuilder.BuildArrayFormula(keyRange, header, formula);
    }

    /// <summary>
    /// Builds Interview Round formula: counts interviews with same key up to current row
    /// </summary>
    public static string BuildInterviewRoundFormula(string keyRange, string header, string interviewKeyRange)
    {
        var formula = $"COUNTIFS({interviewKeyRange},{interviewKeyRange},ROW({interviewKeyRange}),\"<=\"&ROW({interviewKeyRange}))";
        return GoogleFormulaBuilder.BuildArrayFormula(keyRange, header, formula);
    }

    /// <summary>
    /// Builds Duplicate count formula: counts occurrences of company+jobtitle up to current row
    /// </summary>
    public static string BuildDuplicateCountFormula(string keyRange, string header, string companyRange, string jobTitleRange)
    {
        // COUNTIFS(companyRange, companyRange, jobTitleRange, jobTitleRange, ROW(companyRange), "<="&ROW(companyRange))
        var formula = $"COUNTIFS({companyRange},{companyRange},{jobTitleRange},{jobTitleRange},ROW({companyRange}),\"<=\"&ROW({companyRange}))";
        return GoogleFormulaBuilder.BuildArrayFormula(keyRange, header, formula);
    }
}
