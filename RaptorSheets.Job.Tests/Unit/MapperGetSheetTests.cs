using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Helpers;
using RaptorSheets.Job.Mappers;

namespace RaptorSheets.Job.Tests.Unit;

/// <summary>
/// Verifies every mapper can build its sheet without errors (no circular dependencies), the
/// registry knows every configured sheet, and the key calculated formulas/validations are wired.
/// </summary>
public class MapperGetSheetTests
{
    [Fact]
    public void AllMappers_GetSheet_ShouldSucceed()
    {
        Assert.Null(Record.Exception(() => ApplicationMapper.GetSheet()));
        Assert.Null(Record.Exception(() => InterviewMapper.GetSheet()));
        Assert.Null(Record.Exception(() => CompanyMapper.GetSheet()));
        Assert.Null(Record.Exception(() => PositionMapper.GetSheet()));
        Assert.Null(Record.Exception(() => ValidationMapper.GetSiteSheet()));
        Assert.Null(Record.Exception(() => ValidationMapper.GetDecisionSheet()));
        Assert.Null(Record.Exception(() => ValidationMapper.GetInterviewTypeSheet()));
        Assert.Null(Record.Exception(() => ValidationMapper.GetInterviewOutcomeSheet()));
        Assert.Null(Record.Exception(() => ValidationMapper.GetScheduleSheet()));
        Assert.Null(Record.Exception(() => ValidationMapper.GetSetupSheet()));
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
        var sheet = ApplicationMapper.GetSheet();

        var key = sheet.Headers.Single(h => h.Name == SheetsConfig.HeaderNames.Key);
        Assert.Contains("ARRAYFORMULA", key.Formula);

        var company = sheet.Headers.Single(h => h.Name == SheetsConfig.HeaderNames.Company);
        Assert.Equal(SheetsConfig.ValidationNames.RangeCompany, company.Validation);
    }

    [Fact]
    public void ReferenceSheet_ValueColumn_IsUniqueFormula()
    {
        var sheet = ValidationMapper.GetInterviewTypeSheet();
        var value = sheet.Headers.Single(h => h.Name == SheetsConfig.HeaderNames.InterviewType);

        Assert.False(string.IsNullOrEmpty(value.Formula));
        Assert.Contains("UNIQUE", value.Formula);
    }

    [Fact]
    public void MultipleMapperCalls_AreIdempotent()
    {
        var sheet1 = ApplicationMapper.GetSheet();
        var sheet2 = ApplicationMapper.GetSheet();

        Assert.Equal(sheet1.Name, sheet2.Name);
        Assert.Equal(sheet1.Headers.Count, sheet2.Headers.Count);
    }
}
