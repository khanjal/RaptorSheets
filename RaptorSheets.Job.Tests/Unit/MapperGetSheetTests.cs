using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Helpers;
using RaptorSheets.Job.Sheets;

namespace RaptorSheets.Job.Tests.Unit;

/// <summary>
/// Verifies every sheet can build without errors (no circular dependencies), the
/// registry knows every configured sheet, and the key calculated formulas/validations are wired.
/// </summary>
public class MapperGetSheetTests
{
    [Fact]
    public void AllMappers_GetSheet_ShouldSucceed()
    {
        Assert.Null(Record.Exception(() => ApplicationSheet.GetSheet()));
        Assert.Null(Record.Exception(() => InterviewSheet.GetSheet()));
        Assert.Null(Record.Exception(() => CompanySheet.GetSheet()));
        Assert.Null(Record.Exception(() => PositionSheet.GetSheet()));
        Assert.Null(Record.Exception(() => SiteSheet.GetSheet()));
        Assert.Null(Record.Exception(() => DecisionSheet.GetSheet()));
        Assert.Null(Record.Exception(() => InterviewTypeSheet.GetSheet()));
        Assert.Null(Record.Exception(() => InterviewOutcomeSheet.GetSheet()));
        Assert.Null(Record.Exception(() => ScheduleSheet.GetSheet()));
        Assert.Null(Record.Exception(() => SetupSheet.GetSheet()));
    }

    [Fact]
    public void Registry_HasEveryConfiguredSheet()
    {
        foreach (var name in SheetsConfig.SheetUtilities.GetAllSheetNames())
        {
            Assert.True(JobSheetHelpers.Registry.IsRegistered(name), $"Sheet '{name}' is not registered");
            Assert.NotNull(JobSheetHelpers.GetSheetLayout(name));
        }
    }

    [Fact]
    public void ApplicationSheet_HasKeyFormula_AndCompanyDropdown()
    {
        var sheet = ApplicationSheet.GetSheet();

        var key = sheet.Headers.Single(h => h.Name == SheetsConfig.HeaderNames.Key);
        Assert.Contains("ARRAYFORMULA", key.Formula);

        var company = sheet.Headers.Single(h => h.Name == SheetsConfig.HeaderNames.Company);
        Assert.Equal(SheetsConfig.ValidationNames.RangeCompany, company.Validation);
    }

    [Fact]
    public void ReferenceSheet_ValueColumn_IsUniqueFormula()
    {
        var sheet = InterviewTypeSheet.GetSheet();
        var value = sheet.Headers.Single(h => h.Name == SheetsConfig.HeaderNames.InterviewType);

        Assert.False(string.IsNullOrEmpty(value.Formula));
        Assert.Contains("UNIQUE", value.Formula);
    }

    [Fact]
    public void MultipleMapperCalls_AreIdempotent()
    {
        var sheet1 = ApplicationSheet.GetSheet();
        var sheet2 = ApplicationSheet.GetSheet();

        Assert.Equal(sheet1.Name, sheet2.Name);
        Assert.Equal(sheet1.Headers.Count, sheet2.Headers.Count);
    }

    [Fact]
    public void Registry_GetDependents_DetectsRealCrossSheetFormulaGraph()
    {
        // SheetRegistry.GetDependents derives dependencies dynamically by scanning each mapper's
        // built formulas for other sheets' quoted range pattern ('Name'!) - no manual declaration.
        // This confirms it actually finds Job's real dependency graph (Companies/Positions/Sites/
        // Decisions/Schedules depend on both Applications and Interviews; InterviewTypes/
        // InterviewOutcomes depend on Interviews only), not just synthetic fixtures, so
        // RefreshDependentSheetsAsync rewrites the right sheets in production.
        var applicationDependents = JobSheetHelpers.Registry.GetDependents([SheetsConfig.SheetNames.Applications]);

        Assert.Contains(SheetsConfig.SheetNames.Companies, applicationDependents);
        Assert.Contains(SheetsConfig.SheetNames.Positions, applicationDependents);
        Assert.Contains(SheetsConfig.SheetNames.Sites, applicationDependents);
        Assert.Contains(SheetsConfig.SheetNames.Decisions, applicationDependents);
        Assert.Contains(SheetsConfig.SheetNames.Schedules, applicationDependents);
        Assert.DoesNotContain(SheetsConfig.SheetNames.InterviewTypes, applicationDependents);
        Assert.DoesNotContain(SheetsConfig.SheetNames.InterviewOutcomes, applicationDependents);

        var interviewDependents = JobSheetHelpers.Registry.GetDependents([SheetsConfig.SheetNames.Interviews]);

        Assert.Contains(SheetsConfig.SheetNames.InterviewTypes, interviewDependents);
        Assert.Contains(SheetsConfig.SheetNames.InterviewOutcomes, interviewDependents);
        Assert.Contains(SheetsConfig.SheetNames.Companies, interviewDependents);
    }
}
